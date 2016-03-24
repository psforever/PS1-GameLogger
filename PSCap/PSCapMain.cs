using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PSCap
{ 
    public partial class PSCapMain : Form
    {
        //states for attaching to the process
        enum DisableProcessSelectionReason
        {
            NoInstances,
            Attached,
        }

        //states for the application in reference to its attachment to the process
        enum UIState
        {
            Detached,
            Detaching,
            Attached,
            Attaching,
            Capturing,
        }


        // bump these when editing DllMessages or capture records
        public const byte GAME_LOGGER_MAJOR_VERSION = 1;
        public const byte GAME_LOGGER_MINOR_VERSION = 1;

        const string NO_INSTANCE_PLACEHOLDER = "No instances";
        ProcessScanner scanner = new ProcessScanner("PlanetSide");
        // manages the state of the logger and the transitions from detached attached etc
        CaptureLogic captureLogic = new CaptureLogic("PSLogServer" + Program.LoggerId, Path.Combine(Environment.CurrentDirectory, "pslog.dll"));
        CaptureFile captureFile = null;
        ulong estimatedCaptureSize = 0;

        int lastSelectedInstanceIndex = 0;
        bool followLast = true;
        int loggerId = 0;

        private UIState currentUIState = UIState.Detached;

        /// <summary>
        ///     Main entry point to the application
        /// </summary>
        /// <param name="loggerId">
        ///     A unique mutex identification number.
        /// </param>
        public PSCapMain(int loggerId)
        {
            this.loggerId = loggerId;
            InitializeComponent();

            // required for hotkey hooking with key modifiers
            this.KeyPreview = true;
            splitContainer1.PerformLayout();
        }

        /// <summary>
        ///     Sets the intial conditions of the application
        /// </summary>
        /// <param name="sender">
        ///     Event source
        /// </param>
        /// <param name="e">
        ///     Details regarding the event
        /// </param>
        private void Form1_Load(object sender, EventArgs e)
        {
            // set the logger ID
            this.toolStripLoggerID.Text = "Logger ID " + loggerId;
            // start with a log ready to be written
            try
            {
                Log.logFile = new StreamWriter("PSGameLogger" + loggerId + "_log.txt", false);
                Log.logFile.AutoFlush = true;
                Log.logFile.WriteLine("PS1 GameLogger Logging started at " + DateTime.Now);
            }
            catch(IOException ex)
            {
                MessageBox.Show("Failed to create log file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //wire attached process to application's logging routines via callbacks
            scanner.ProcessListUpdate += new EventHandler<Process []>(processList_update);
            captureLogic.AttachedProcessExited += new AttachedProcessExited(attachedProcessExited);
            captureLogic.NewEvent += new NewEventCallback(newUIEvent);
            captureLogic.OnNewRecord += new NewGameRecord(newRecord);

            // start off detached and with no open game instances
            enterUIState(UIState.Detached);

            setCaptureFile(captureFile);
            initListView();
        }

        /// <summary>
        ///     What happens when the application closes
        /// </summary>
        /// <param name="sender">
        ///     Event source
        /// </param>
        /// <param name="e">
        ///     Details regarding the event
        /// </param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //stable exit
            if (e.CloseReason == CloseReason.ApplicationExitCall)
                return;

            // TODO: handle the cases where we are attached, capturing, or have an unsaved capture
            if (captureFile != null && captureFile.isModified())
            {
                DialogResult result = MessageBox.Show("You have an unsaved capture file. Would you like to save it before exiting?",
                    "Save capture file", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    if (captureLogic.isCapturing())
                        captureLogic.stopCapture();

                    bool canceled;
                    saveCaptureFile(out canceled);

                    if (canceled)
                    {
                        e.Cancel = true;
                    }

                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }

            if(e.Cancel)
            {
                Log.Info("Form close cancelled");
                return;
            }

            Log.Info("Form closing");
        }

        /// <summary>
        ///     Process user input from keyboard
        /// </summary>
        /// <param name="msg">
        ///     dunno
        /// </param>
        /// <param name="keyData">
        ///     Identification of the keypress
        /// </param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch(keyData)
            {
                case Keys.F9:
                    if (capturePauseButton.Enabled)
                        capturePauseButton_Click(this, new EventArgs());
                    return true;
                default:
                    return base.ProcessCmdKey(ref msg, keyData);
            }
        }

        /// <summary>
        ///     Format the UI of themain list of captured packet events
        /// </summary>
        private void initListView()
        {
            listView1.VirtualMode = true;
            listView1.VirtualListSize = 0;

            listView1.View = View.Details;
            listView1.FullRowSelect = true;
            listView1.EnableDoubleBuffer();

            //Add column header
            listView1.Columns.Add("Event", 150);
            listView1.Columns.Add("Time", 100);
            listView1.Columns.Add("Type", 150);
            listView1.Columns.Add("Size", 30);

            eventImageList.Images.Add(global::PSCap.Properties.Resources.arrow_Up_16xLG_green);
            eventImageList.Images.Add(global::PSCap.Properties.Resources.arrow_Down_16xLG_red);
            eventImageList.Images.Add(global::PSCap.Properties.Resources.lock_16xLG);

            listView1.SmallImageList = eventImageList;
        }

        /// <summary>
        ///     Add a record or series of records to the list of captured packet events
        /// </summary>
        /// <param name="recs">
        ///     The new record(s) to be added
        /// </param>
        private void newRecord(List<GameRecord> recs)
        {
            List<Record> newItems = new List<Record>(recs.Count);
            /*
            for each individual new gameRec:
            1. For packets ...
            1.a. Cast the gameRec into a GameRecordPacket type
            1.b. Cast the rec into a RecordGame type
            1.c. Transfer data from the gameRec/GameRecordPacket into the rec/RecordGame
            1.d. Remove sensitive data from the transferred data
            2. Do nothing to anything else; note lack of handled format
            */
            foreach (GameRecord gameRec in recs)
            {
                Record rec = Record.Factory.Create(RecordType.GAME);

                switch(gameRec.type)
                {
                    case GameRecordType.PACKET:
                        GameRecordPacket record = gameRec as GameRecordPacket;
                        RecordGame gameRecord = rec as RecordGame;
                        this.sensitiveDataScrubbing(record);
                        gameRecord.setRecord(record);
                        break;
                    default:
                        Trace.Assert(false, string.Format("NewRecord: Unhandled record type {0}", gameRec.type));
                        break;
                }

                addRecordSizeEstimate(rec.size);
                captureFile.addRecord(rec);
            }

            setRecordCount(captureFile.getNumRecords());
        }

        /// <summary>
        ///     Nasty hack to prevent password disclosures
        /// </summary>
        /// <param name="record">
        ///     The record that owns the packet being scrubbed
        /// </param>
        private void sensitiveDataScrubbing(GameRecordPacket record) {
            byte[] sensitive = { 0x00, 0x09, 0x00, 0x00, 0x01, 0x03 };
            int i = 0;
            List<Byte> packet = record.packet;
            for(i = 0; i < packet.Count && i < sensitive.Length; i++) {
                if(packet[i] != sensitive[i])
                    break;
            }

            if(i == sensitive.Length) {
                Log.Info("Found sensitive login packet. Scrubbing from the record");
                packet.Clear();
                packet.AddRange(sensitive);
            }
        }

        /// <summary>
        ///     What to do when the process to which this application is attached ends while being attached
        /// </summary>
        /// <param name="p">
        ///     Process that has ended
        /// </param>
        private void attachedProcessExited(Process p)
        {
            Log.Warning("attached process has exited");

            // XXX: this is shit. We have to "stop capture" then detach. Weird state
            // to be in, but it saves code at the expense of breaking some models
            //enterUIState(UIState.Attached);
            //enterUIState(UIState.Detaching);
            //enterUIState(UIState.Detached);

            // TODO: add more messagebox types for capturing/not capturing

            /*this.SafeInvoke(delegate
            {
                if (p.ExitCode == 0)
                    MessageBox.Show("The attached process has exited safely.", "Process Exited",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else
                    MessageBox.Show(string.Format("The attached process has crashed with exit code 0x{0:X}.", p.ExitCode),
                        "Process Crashed",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
            });*/
        }

        /// <summary>
        ///     Handle the UI functionality when no further process attachment is possible from our current state
        /// </summary>
        /// <param name="reason">
        ///     The reason the process selection option is being disabled
        /// </param>
        void disableProcessSelection(DisableProcessSelectionReason reason)
        {
            switch(reason)
            {
                case DisableProcessSelectionReason.NoInstances:
                    Log.Debug("Disabling process selection due to no instances");
                    this.SafeInvoke(delegate
                    {
                        toolStripInstance.Items.Clear();
                        toolStripInstance.Enabled = false;
                        toolStripInstance.Items.Add(NO_INSTANCE_PLACEHOLDER);
                        toolStripInstance.SelectedIndex = 0;
#if !WITHOUT_GAME
                        toolStripAttachButton.Enabled = false;
#endif
                    });
                    break;
                case DisableProcessSelectionReason.Attached:
                    Log.Debug("Disabling process selection because we're attached");
                    this.SafeInvoke(delegate
                    {
                        toolStripInstance.Enabled = false;
                    });
                    break;
            }
        }

        /// <summary>
        ///     Handle the UI functionality when the application is capable of attaching to a process
        /// </summary>
        void enableProcessSelection()
        {
            this.SafeInvoke(delegate
            {
                toolStripInstance.Enabled = true;
                toolStripAttachButton.Enabled = true;
            });
        }

        /// <summary>
        ///     Handle a changing list of available processes to which the application can be attached
        /// </summary>
        /// <param name="from">
        ///     unused
        /// </param>
        /// <param name="list">
        ///     Available processes
        /// </param>
        private void processList_update(object from, Process[] list)
        {
            this.SafeInvoke(delegate
            {
                if (list.Length == 0)
                {
                    disableProcessSelection(DisableProcessSelectionReason.NoInstances);
                    return;
                }

                // we have at least one element
                // clear list, refill it, select first item, enable selection
                toolStripInstance.Items.Clear();

                foreach (Process p in list)
                {
                    toolStripInstance.Items.Add(new ProcessCollectable(p));
                }

                // select the last item as a convienience
                if (lastSelectedInstanceIndex < list.Length && lastSelectedInstanceIndex >= 0)
                    toolStripInstance.SelectedIndex = lastSelectedInstanceIndex;
                else
                    toolStripInstance.SelectedIndex = 0;

                enableProcessSelection();
            });
        }

        /// <summary>
        ///     Log a message on the status bar
        /// </summary>
        /// <param name="status">
        ///     The message to be logged
        /// </param>
        private void setStatus(string status)
        {
            this.SafeInvoke(delegate
            {
                toolStripStatus.Text = status;
            });
        }

        /// <summary>
        ///     Set the total size of the records
        /// </summary>
        /// <param name="bytes">
        ///     The size of the records
        /// </param>
        private void setRecordSizeEstimate(ulong bytes)
        {
            estimatedCaptureSize = bytes;
        }

        /// <summary>
        ///     Increment the total size of the records
        /// </summary>
        /// <param name="bytes">
        ///     The size of the records to increment
        /// </param>
        private void addRecordSizeEstimate(ulong bytes)
        {
            estimatedCaptureSize += bytes;
        }

        /// <summary>
        ///     Update these features of the GUI as the condition of the capture progresses
        /// </summary>
        private void updateCaptureFileState()
        {
            this.SafeInvoke(delegate
            {
                if (captureFile == null)
                {
                    saveToolStripMenuItem.Enabled = false;
                    saveAsToolStripMenuItem.Enabled = false;
                    copyToolStripMenuItem.Enabled = false;
                    openToolStripMenuItem.Enabled = true;
                    this.displayPacketData(null);

                    setCaptureFileName("");
                    return;
                }

                if (currentUIState == UIState.Capturing)
                {
                    saveToolStripMenuItem.Enabled = false;
                    saveAsToolStripMenuItem.Enabled = false;
                    copyToolStripMenuItem.Enabled = true;
                    openToolStripMenuItem.Enabled = false;
                    this.displayPacketData(null);
                    return;
                }

                string filename = Path.GetFileName(captureFile.getCaptureFilename());

                if (captureFile.isModified())
                {
                    saveToolStripMenuItem.Enabled = true;
                    filename += " (modified)";
                }
                else
                {
                    saveToolStripMenuItem.Enabled = false;
                }

                setCaptureFileName(filename);

                saveAsToolStripMenuItem.Enabled = true;
                copyToolStripMenuItem.Enabled = true;
                openToolStripMenuItem.Enabled = true;
            });
        }

        /// <summary>
        ///     Load a new capture file (and reset some UI elements)
        /// </summary>
        /// <param name="cap">
        ///     The file object
        /// </param>
        private void setCaptureFile(CaptureFile cap)
        {
            this.SafeInvoke(delegate
            {
                // must be set before
                captureFile = cap;

                if (cap == null)
                {
                    // set the estimate before updating the record count
                    setRecordSizeEstimate(0);
                    setRecordCount(0);
                }
                else
                {
                    ulong estimatedSize = 0;

                    foreach (Record r in cap.getRecords())
                        estimatedSize += r.size;

                    setRecordSizeEstimate(estimatedSize);
                    setRecordCount(cap.getNumRecords());
                }
                this.updateCaptureFileState();
                this.displayPacketData(null);
            });
        }

        /// <summary>
        ///     Set the total count of the records
        /// </summary>
        /// <param name="count">
        ///     The number of records
        /// </param>
        private void setRecordCount(int count)
        {
            //the count is not maintained by a basic counter but by the state of Form Controls
            this.SafeInvoke(delegate
            {
                listView1.SetVirtualListSize(count);
                if (followLast)
                    scrollToEnd();

                if (count == 0)
                {
                    recordCountLabel.Visible = false;
                    toolStripStatus.BorderSides = ToolStripStatusLabelBorderSides.None;
                }
                else
                {
                    recordCountLabel.Text = string.Format("{0} record{1}{2}",
                        count, count == 1 ? "" : "s",
                        estimatedCaptureSize == 0 ? "" :
                        string.Format(" ({0})", Util.BytesToString((long)estimatedCaptureSize)));
                    recordCountLabel.Visible = true;
                    toolStripStatus.BorderSides = ToolStripStatusLabelBorderSides.Right;
                }
            });
        }

        /// <summary>
        ///     Display the name of current capture file
        /// </summary>
        /// <param name="name">
        ///     The name
        /// </param>
        private void setCaptureFileName(string name)
        {
            this.SafeInvoke(delegate
            {
                if (string.Empty == name)
                    captureFileLabel.Text = "No capture file";
                else
                    captureFileLabel.Text = name;
            });
        }

        /// <summary>
        ///     Initialize the different conditions of the UI
        /// </summary>
        /// <param name="state">
        ///     The state to which the application will transition
        /// </param>
        void enterUIState(UIState state)
        {
            currentUIState = state;

            switch (state)
            {
                case UIState.Detached:
                    Log.Info("UIState Detached");

                    this.SafeInvoke(delegate
                    {
                        disableProcessSelection(DisableProcessSelectionReason.NoInstances);
                        capturePauseButton.Enabled = false;

                        toolStripAttachButton.Text = "Attach";
                        toolStripAttachButton.Enabled = true;
                        // in the detached state, the scanner task and selection callback control
                        // the enabled state of the Attach/Detach button

                        // status bar
                        statusStrip1.BackColor = SystemColors.Highlight;
                        setStatus("Detached");

                        // start scanning for our target process
                        scanner.startScanning();
                    });
                    break;
                case UIState.Detaching:
                    Log.Info("UIState Detaching");

                    this.SafeInvoke(delegate
                    {
                        toolStripAttachButton.Text = "Detaching";
                        toolStripAttachButton.Enabled = false;
                    });
                    break;
                case UIState.Attached:
                    Log.Info("UIState Attached");

                    this.SafeInvoke(delegate
                    {
                        // no need to scan anymore
                        scanner.stopScanning();
                        disableProcessSelection(DisableProcessSelectionReason.Attached);

                        // save the last selected instance ID to come back to
                        lastSelectedInstanceIndex = toolStripInstance.SelectedIndex;

                        capturePauseButton.Image = Properties.Resources.StatusAnnotations_Play_16xLG_color;
                        capturePauseButton.Text = "Capture";
                        capturePauseButton.Enabled = true;
                        
                        toolStripAttachButton.Text = "Detach";
                        toolStripAttachButton.Enabled = true;

                        updateCaptureFileState();

                        openToolStripMenuItem.Enabled = true;

                        // status bar
                        statusStrip1.BackColor = Color.DarkGreen;
                        setStatus("Ready to capture");
                    });
                    break;
                case UIState.Attaching:
                    Log.Info("UIState Attaching");

                    this.SafeInvoke(delegate
                    {
                        toolStripAttachButton.Text = "Attaching";
                        toolStripAttachButton.Enabled = false;
                    });
                    break;
                case UIState.Capturing:
                    Log.Info("UIState Capturing");

                    this.SafeInvoke(delegate
                    {
                        capturePauseButton.Image = Properties.Resources.StatusAnnotations_Stop_16xLG_color;
                        capturePauseButton.Text = "Stop Capture";
                        capturePauseButton.Enabled = true;

                        toolStripAttachButton.Text = "Detach";
                        toolStripAttachButton.Enabled = false;

                        updateCaptureFileState();

                        // status bar
                        statusStrip1.BackColor = Color.DarkRed;
                        setStatus("Capturing...");
                    });
                    break;
                default:
                    Trace.Assert(false, "Unhandled UIState " + state.ToString());
                    break;
            }
        }

        /// <summary>
        ///     Scroll the list of packet items to the last item
        /// </summary>
        private void scrollToEnd()
        {
            if(listView1.Items.Count > 0)
                this.SafeInvoke(delegate
                {
                    listView1.EnsureVisible(listView1.Items.Count - 1);
                });
        }

        /// <summary>
        ///     Parse data to be loaded into the list of captured packets
        /// </summary>
        /// <param name="sender">
        ///     Event source
        /// </param>
        /// <param name="e">
        ///     Details regarding the event
        /// </param>
        private void listView1_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            RecordGame i = captureFile.getRecord(e.ItemIndex) as RecordGame;
            double time = i.getSecondsSinceStart((uint)captureFile.getStartTime());

            GameRecordPacket record = i.Record as GameRecordPacket;
            string eventName = record.packetDestination == GameRecordPacket.PacketDestination.Client ? "Received Packet" : "Sent Packet";
            string bytes = ((PlanetSideMessageType)record.packet[0]).ToString();
            string[] row = new string[] {
                eventName,
                string.Format("{0:0.000000}", time),
                bytes,
                record.packet.Count.ToString()
            };
            e.Item = new ListViewItem(row);
            e.Item.ImageIndex = record.packetDestination == GameRecordPacket.PacketDestination.Server ? 0 : 1;
        }

        /// <summary>
        ///     Determine whether to follow the last captured packet entry in the list
        /// </summary>
        /// <param name="sender">
        ///     Event source
        /// </param>
        /// <param name="e">
        ///     Details regarding the event
        /// </param>
        private void listView1_OnScroll(object sender, ScrollEventArgs e)
        {
            int itemHeight;
            if (listView1.VirtualListSize == 0)
                itemHeight = 0;
            else
                itemHeight = listView1.GetItemRect(0).Height;

            if (itemHeight == 0) // bad!
                return;

            // mad hax: an estimation of how many items should be visible
            int itemsDisplayed = listView1.DisplayRectangle.Height / itemHeight;
            followLast = (e.NewValue + itemsDisplayed) >= listView1.VirtualListSize;
        }
        
        // Start synchronized caret manipulation processing here
        // Key presses ...
        //
        // Record the initial position of a mouse button press or a keyboard key press
        private int selectionControlStart = -1;

        /// <summary>
        ///     When the user selects character data in the display, highlight the related hexadecimal data.
        /// </summary>
        /// <param name="sender">
        ///     Event source
        /// </param>
        /// <param name="e">
        ///     Details regarding the event
        /// </param>
        private void hexCharDisplay_onKeyPress(object sender, KeyEventArgs e) {
            TextBox box = (TextBox)sender;
            bool selection = Control.ModifierKeys == Keys.Shift;
            int scStart = this.selectionControlStart, boxStart = box.SelectionStart, boxLength = box.SelectionLength;
            // 1: Do fire if the key was pressed this turn
            // 2: Don't fire if the caret is already on a previous selection start or previous selction end
            if((!selection || e.KeyCode == Keys.ShiftKey) &&
                boxLength > 0 && scStart != boxStart && scStart != boxStart+boxLength) {
                this.selectionControlStart = boxStart;
            }

            Tuple<int, int> caretPos = this.displayKeyPressCaretPos(box, e, 18);
            if(caretPos == null)
                return; // See default case of displayKeyPressCaretPos(...)
            int start = caretPos.Item1;
            int length = caretPos.Item2;
            bool caretToStart = start != box.SelectionStart;
            if(!selection) { // We only just moved the caret, we're not selecting any text
                if(!caretToStart)
                    start += length;
                length = 0;
            }
            this.hexCharDisplay_onSelectionUpdate(start, start+length, !caretToStart);
            this.doTextBoxCaretScroll(box, start, length, !caretToStart);
            e.Handled = true; // Do not propagate
        }

        /// <summary>
        ///     When the user selects hexadecimal data in the display, highlight the related character data.
        /// </summary>
        /// <param name="sender">
        ///     Event source
        /// </param>
        /// <param name="e">
        ///     Details regarding the event
        /// </param>
        private void hexDisplay_onKeyPress(object sender, KeyEventArgs e) {
            TextBox box = (TextBox)sender;
            bool selection = Control.ModifierKeys == Keys.Shift;
            int scStart = this.selectionControlStart, boxStart = box.SelectionStart, boxLength = box.SelectionLength;
            // 1: Do fire if the key was pressed this turn
            // 2: Don't fire if the caret is already on a previous selection start or previous selction end
            if((!selection || e.KeyCode == Keys.ShiftKey) &&
                boxLength > 0 && scStart != boxStart && scStart != boxStart+boxLength) {
                this.selectionControlStart = boxStart;
            }

            Tuple<int, int> caretPos = this.displayKeyPressCaretPos(box, e, 50);
            if(caretPos == null)
                return;  // See default case of displayKeyPressCaretPos(...)
            int start = caretPos.Item1;
            int length = caretPos.Item2;
            bool caretToStart = start != box.SelectionStart;
            if(!selection) { // We only just moved the caret; we're not selecting any text
                if(!caretToStart)
                    start += length;
                length = 0;
            }

            this.hexDisplay_onSelectionUpdate(start, start+length, !caretToStart);
            this.doTextBoxCaretScroll(box, start, length, !caretToStart);
            e.Handled = true; // Do not propagate
        }

        /// <summary>
        ///     Calculate the caret position change on the Control due to a key press event.
        /// </summary>
        /// <param name="box">
        ///     Event source as a Textbox Control
        /// </param>
        /// <param name="e">
        ///     Details regarding the key press event
        /// </param>
        /// <param name="lineLen">
        ///     Length of the a single line of text, including he length of the newline string (2)
        /// </param>
        /// <returns>
        ///     A Tuple containing the starting position and length for the selection.
        /// </returns>
        private Tuple<int, int> displayKeyPressCaretPos(TextBox box, KeyEventArgs e, int lineLen) {
            int start = box.SelectionStart;
            int length = box.SelectionLength;
            String text = box.Text;
            char word; //see cases Keys.Right and Keys.Left

            switch(e.KeyCode) {
                case Keys.Right:
                    if(start >= this.selectionControlStart) { //s...caret>>
                        if(start + length < text.Length) {
                            word = text[start + length];
                            length = Math.Min(text.Length, length + 1);
                            if(word == '\r')
                                length += 2; //newlines are \r\n; if \r, move to first character on next line
                        } // Return a value because a caret position change was detected
                    }
                    else { //caret>>...s
                        word = box.Text[start];
                        int diff = start;
                        start = Math.Min(text.Length, start + 1);
                        length = Math.Max(0, length - (start - diff));
                        if(word == '\r') {
                            start += 2;
                            length -= 2;
                        }
                    }
                    break;
                case Keys.Left: //s...<<caret
                    if(start >= this.selectionControlStart && length > 0) {
                        word = text[start+length-1];
                        length = Math.Max(0, length - 1);
                        if(word == '\n')  //newlines are \r\n; if \n, move to last character on previous line
                            length -= 2;
                    }
                    else { //<<caret...s
                        if(start - 1 >= 0) {
                            word = text[start-1];
                            int diff = start;
                            start = Math.Max(0, start - 1);
                            length = Math.Min(text.Length, length + (diff - start));
                            if(word == '\n') {
                                start -= 2;
                                length += 2;
                            }
                        } // Return a value because a caret position change was detected
                    }
                    break;
                case Keys.Down:
                    if(start >= this.selectionControlStart) { //s...caret>>
                        length = Math.Min(text.Length, length + lineLen);
                    }
                    else if(start + lineLen >= this.selectionControlStart) { //[...caret>>...s...
                        int diff = start;
                        start = this.selectionControlStart;
                        length = Math.Max(0, (diff + lineLen) - start);
                    }
                    else { //caret>>...]...s
                        start = Math.Min(text.Length, start + lineLen);
                        length = Math.Max(0, length - lineLen);
                    }
                    break;
                case Keys.Up:
                    if(start < this.selectionControlStart) { //<<caret...s
                        int diff = start;
                        start = Math.Max(0, start - lineLen);
                        length = Math.Min(text.Length, length + (diff - start));
                    }
                    else if(length - lineLen < 0) { //...s...<<caret...]
                        start = Math.Max(0, start + (length - lineLen));
                        length = this.selectionControlStart - start;
                    }
                    else { //s...<<caret
                        length = Math.Max(0, length - lineLen);
                    }
                    break;
                default:
                    return null; // Not a position manipulation so there is no return
            }
            return Tuple.Create(start, length);
        }
        //
        // Mouse controls ...
        //
        /// <summary>
        ///     Consume mouse wheel events without guilt
        /// </summary>
        /// <param name="sender">
        ///     Event source
        /// </param>
        /// <param name="e">
        ///     Details regarding the event
        /// </param>
        private void ignoreMouseWheel(object sender, EventArgs e) {
            HandledMouseEventArgs ee = (HandledMouseEventArgs)e;
            ee.Handled = true; //om nom
        }

        /// <summary>
        ///     Manually toggle the ScrollBar on the Control and manages shared text selection activities
        /// </summary>
        /// <param name="sender">
        ///     Event source
        /// </param>
        /// <param name="e">
        ///     Details regarding the event
        /// </param>
        private void hexCharDisplay_TextChanged(object sender, EventArgs e) {
            TextBox control = (TextBox)sender;

            //scrollbar control
            int controlHeight = control.Height;
            int lines = control.Lines.Length;
            int fontHeight = control.Font.Height;
            bool hasScrollBars = control.ScrollBars != ScrollBars.None;
            if(lines * fontHeight > controlHeight && !hasScrollBars) {
                control.Width += SystemInformation.VerticalScrollBarWidth;
                control.ScrollBars = ScrollBars.Vertical;
            }
            else if(hasScrollBars) {
                control.Width -= SystemInformation.VerticalScrollBarWidth;
                control.ScrollBars = ScrollBars.None;
            }
        }

        // Leverage which Control has a working MouseMove event
        private Control selectionControl = null;

        /// <summary>
        ///     Flag for related data selection activity
        /// </summary>
        /// <param name="sender">
        ///     Event source
        /// </param>
        /// <param name="e">
        ///     Details regarding the event
        /// </param>
        private void selection_MouseDown(object sender, EventArgs e) {
            TextBox control = (TextBox)sender;
            this.selectionControl = control;
            this.selectionControlStart = control.SelectionStart;
        }

        /// <summary>
        ///     Remove flag for related data selection activity and clear unnecessary selections
        /// </summary>
        /// <param name="sender">
        ///     Event source
        /// </param>
        /// <param name="e">
        ///     Details regarding the event
        /// </param>
        private void selection_MouseUp(object sender, EventArgs e) {
            if(this.selectionControl != null) {
                if(((TextBox)this.selectionControl).SelectionLength == 0)
                    this.clearSelectionData();
                this.selectionControl = null;
            }
        }

        /// <summary>
        ///     Perform checks for validity of onSelection handler.
        /// </summary>
        /// <param name="sender">
        ///     Event source
        /// </param>
        /// <param name="e">
        ///     Details regarding the event
        /// </param>
        private void hexCharDisplay_onSelection(object sender, EventArgs e) {
            TextBox control = (TextBox)sender;

            if(this.selectionControl == control) {
                int initial = control.SelectionStart;
                int final = initial + control.SelectionLength;
                bool alignment = initial >= this.selectionControlStart;
                this.hexCharDisplay_onSelectionUpdate(initial, final, alignment);
            }
        }

        /// <summary>
        ///     Update the hex line number position and the hexadecimal display position with selection data
        /// </summary>
        /// <param name="initial">
        ///     Where the selection starts in the hexadecimal character display Control
        /// </param>
        /// <param name="length">
        ///     Where the selection ends in the hexadecimal character display Control
        /// </param>
        /// <param name="alignment">
        ///     True, if the text should necessarily show the initial text position; false, if the final position
        /// </param>
        private void hexCharDisplay_onSelectionUpdate(int initial, int final, bool alignment) {
            Tuple<int, int> caretPos;

            // Line numbers control
            caretPos = this.hexLineNumbersCaretPosition(initial, final);
            this.doTextBoxCaretScroll(this.hexLineNumbers, caretPos.Item1, caretPos.Item2, alignment);
            // Hex display control
            caretPos = this.hexDisplayCaretPosition(initial, final);
            this.doTextBoxCaretScroll(this.hexDisplay, caretPos.Item1, caretPos.Item2, alignment);
            // Return caller focus
            this.hexCharDisplay.Focus();
        }

        /// <summary>
        ///     Perform checks for validity of onSelection handler.
        /// </summary>
        /// <param name="sender">
        ///     Event source
        /// </param>
        /// <param name="e">
        ///     Details regarding the event
        /// </param>
        private void hexDisplay_onSelection(object sender, EventArgs e) {
            TextBox control = (TextBox)sender;

            if(this.selectionControl == control) {
                int initial = control.SelectionStart;
                int final = initial + control.SelectionLength;
                bool alignment = initial >= this.selectionControlStart;
                this.hexDisplay_onSelectionUpdate(initial, final, alignment);
            }
        }

        /// <summary>
        ///     Update the hex line number position and the hexadecimal character position with selection data
        /// </summary>
        /// <param name="initial">
        ///     Where the selection starts in the hexadecimal display Control
        /// </param>
        /// <param name="length">
        ///     Where the selection ends in the hexadecimal display Control
        /// </param>
        /// <param name="alignment">
        ///     True, if the text should necessarily show the initial text position; false, if the final position
        /// </param>
        private void hexDisplay_onSelectionUpdate(int initial, int final, bool alignment) {
            Tuple<int, int> caretPos;

            // Hex char display control
            caretPos = this.hexCharDisplayCaretPosition(initial, final);
            this.doTextBoxCaretScroll(this.hexCharDisplay, caretPos.Item1, caretPos.Item2, alignment);
            // Line numbers control
            caretPos = this.hexLineNumbersCaretPosition(caretPos.Item1, (caretPos.Item1 + caretPos.Item2));
            this.doTextBoxCaretScroll(this.hexLineNumbers, caretPos.Item1, caretPos.Item2, alignment);
            // Return caller focus
            this.hexDisplay.Focus();
        }

        /// <summary>
        ///     Scroll to the most significant caret position during a selection
        /// </summary>
        /// <param name="box">
        ///     The Control being manipulated
        /// </param>
        /// <param name="initial">
        ///     The index where the selection of text begins
        /// </param>
        /// <param name="length">
        ///     The length of the selection of text
        /// </param>
        /// <param name="gotoEnd">
        ///     When true, the Control scrolls to show the selection's initial; when false, the selection's end
        /// </param>
        private void doTextBoxCaretScroll(TextBox box, int initial, int length, bool gotoEnd) {
            box.Focus();
            box.SelectionStart = initial;
            if(gotoEnd) { //show end of selection
                box.SelectionLength = length;
                box.ScrollToCaret();
            }
            else { //show start of selection
                box.SelectionLength = 0;
                box.ScrollToCaret();
                box.SelectionLength = length;
            }
        }

        /// <summary>
        ///     Remove highlighted selected data from the given Controls
        /// </summary>
        private void clearSelectionData() {
            this.hexDisplay.SelectionLength = 0;
            this.hexCharDisplay.SelectionLength = 0;
        }

        /*
        HexLineNumbers
            XXXXXXXXXrn
            0         1
            01234567890
            XXXXXXXXXrn
                     2
            12345678901
            XXXXXXXXX
                    3
            234567890
        HexDisplay
            XX XX XX XX XX XX XX XX  XX XX XX XX XX XX XX XXrn
            0         1         2         3         4
            01234567890123456789012345678901234567890123456789
            XX XX XX XX XX XX XX XX  XX XX XX XX XX XX XX XXrn
            5         6         7         8         9
            01234567890123456789012345678901234567890123456789
            XX XX
            /
            01234
        HexCharDisplay
            XXXXXXXXXXXXXXXXrn
            0         1
            012345678901234567
            XXXXXXXXXXXXXXXXrn
              2         3
            890123456789012345
            XX

            67
        */

        /// <summary>
        ///     Calculate the caret position on the line number control using caret on the character display Control.
        /// </summary>
        /// <param name="hexCharDisplayInitial">
        ///     The starting position of the caret in the character Control.
        /// </param>
        /// <param name="hexCharDisplayFinal">
        ///     The final position of the caret in the character display Control.
        /// </param>
        /// <returns>
        ///     A Tuple containing the starting position and length for the line number Control selection.
        /// </returns>
        private Tuple<int, int> hexLineNumbersCaretPosition(int hexCharDisplayInitial, int hexCharDisplayFinal) {
            int initial = hexCharDisplayInitial/18 * 10; // Move after beginning of line
            int final = hexCharDisplayFinal/18 * 10;
            int length = final - initial;
            return Tuple.Create(initial, Math.Max(0, length));
        }

        /// <summary>
        ///     Calculate the caret position on the hexadecimal control using caret on the character Control.
        /// </summary>
        /// <param name="hexCharDisplayInitial">
        ///     The starting position of the caret in the character Control.
        /// </param>
        /// <param name="hexCharDisplayFinal">
        ///     The final position of the caret in the character Control.
        /// </param>
        /// <returns>
        ///     A Tuple containing the starting position and length for the hex Control selection.
        /// </returns>
        private Tuple<int, int> hexDisplayCaretPosition(int hexCharDisplayInitial, int hexCharDisplayFinal) {
            int newLine = System.Environment.NewLine.Length;
            int lines; // Number of lines
            int elements; // Number of counted values preceding and including

            // Initial
            lines = hexCharDisplayInitial / 18;
            elements = hexCharDisplayInitial - lines * newLine;
            int initial = elements * 3; // Resize: 1-length value -> 2-length value + 1-length non-value
            initial += lines * 2; // Two extra non-values every line
            initial += Convert.ToInt32(elements%16 >= 8 || (lines + 1) * 16 == elements); // Extra non-value if last line is more than half-full

            // Length
            lines = hexCharDisplayFinal / 18;
            elements = hexCharDisplayFinal - lines * newLine;
            int length = elements * 3;
            length += lines * 2;
            length += Convert.ToInt32(elements%16 > 8 || (lines + 1) * 16 == elements); // Note the conditional
            length -= 1; // Remove the non-value character at the end of every triple
            length -= initial; // Difference
            return Tuple.Create(initial, Math.Max(0, length));
        }

        /// <summary>
        ///     Calculate the caret position on the character control using caret on the hexadecimal Control.
        /// </summary>
        /// <param name="hexDisplayInitial">
        ///     The starting position of the caret in the hexadecimal Control.
        /// </param>
        /// <param name="hexDisplayFinal">
        ///     The final position of the caret in the hexadecimal Control.
        /// </param>
        /// <returns>
        ///     A Tuple containing the starting position and length for the hexadecimal Control selection.
        /// </returns>
        private Tuple<int, int> hexCharDisplayCaretPosition(int hexDisplayInitial, int hexDisplayFinal) {
            int newLine = System.Environment.NewLine.Length;
            int lines; // Number of lines
            int region; // Index of the triple on which the caret is positioned

            // Initial
            lines = hexDisplayInitial / 50;
            int initial = hexDisplayInitial - lines * newLine; // Remove two non-values per line (new lines)
            initial -= Convert.ToInt32(hexDisplayInitial - lines * 50 >= 24); // Account for extra spacer non-value
            region = initial%3; // Character of the triple on which the caret is positioned
            if(region == 1)
                initial -= 1; // Move to the first position of the triple
            else if(region == 2)
                initial += 1; // Move to the next value
            initial /= 3; // Reduce from three characters per value to one
            initial += initial/16 * newLine; // Add filler for new lines

            // Length
            int length = hexDisplayFinal - hexDisplayInitial; // Initial test
            if(length > 0 && !(region == 2 && length == 1)) { // If region=2 and length=1, no highlight
                lines = hexDisplayFinal / 50;
                length = hexDisplayFinal - lines * newLine;
                length -= Convert.ToInt32(hexDisplayFinal - lines * 50 >= 24);
                region = length%3;
                if(region == 1) // Advance to the next value
                    length += 2;
                else if(region == 2)
                    length += 1;
                length /= 3;
                length += lines * newLine; // Important: do not add a newline if only at end of a line
                length -= initial;
            }
            else
                length = 0;
            return Tuple.Create(initial, Math.Max(0, length));
        }

        /// <summary>
        ///     Format and display detailed data of a selected capture entry
        /// </summary>
        /// <param name="sender">
        ///     Event source
        /// </param>
        /// <param name="e">
        ///     Details regarding the event
        /// </param>
        private void listView1_SelectedIndexChanged(object sender, EventArgs e) {
            /*foreach(var s in listView1.SelectedIndices)
            {
                Console.WriteLine("New selection " + s.ToString());
            }*/

            richTextBox1.Clear();
            richTextBox1.SelectionColor = Color.Green;
            richTextBox1.AppendText("  "); // dont remove this if you want colors

            if(listView1.SelectedIndices.Count == 0) {
                this.displayPacketData(null);
                richTextBox1.Text = ("No selection");
            }
            else {
                RecordGame record = captureFile.getRecord(listView1.SelectedIndices[0]) as RecordGame;
                GameRecordPacket gameRecord = record.Record as GameRecordPacket;

                string name = ((PlanetSideMessageType)gameRecord.packet[0]).ToString();
                string bytes = this.displayPacketData(gameRecord);

                richTextBox1.AppendText(name + "\n");
                richTextBox1.AppendText(bytes);

            }
        }

        /// <summary>
        ///     Parse and display hexadecimal strings and character strings from the packet's byte data.
        /// </summary>
        /// <param name="gameRecord">
        ///     The game record packet.
        /// </param>
        /// <returns>
        ///     The hexadecimal data as a string
        /// </returns>
        private string displayPacketData(GameRecordPacket gameRecord) {
            // If there is no record, or no packet data in the record, blank the fields.
            if(gameRecord == null || gameRecord.packet == null || gameRecord.packet.Count == 0) {
                this.hexLineNumbers.Text =
                this.hexDisplay.Text =
                this.hexCharDisplay.Text = "";
                return "";
            }

            // "lineNumbers" keeps track of the number of lines in the "formatted" and "converted" data.
            // "normal" string is a simple spaced display of each byte turned into hexadecimal.
            // "formatted" string is the display form of the "normal" string.  It's mostly the "normal" string.
            // "converted" string is what the byte array looks like when converted to char data.
            StringBuilder lineNumbers = new StringBuilder();
            StringBuilder normal = new StringBuilder();
            StringBuilder formatted = new StringBuilder();
            StringBuilder converted = new StringBuilder();
            String conversion = "{0:X2}";

            // Iterate over packet data bytes.
            List<byte> array = gameRecord.packet;
            string newLine = System.Environment.NewLine;
            for(int entry = 0, byteIndex = 0, byteLength = array.Count, lineNo = 0; byteIndex < byteLength; ) {
                byte b = array[byteIndex];
                string decoded = string.Format(conversion, b);
                // See ByteViewer.DrawDump
                char c = Convert.ToChar(b);
                normal.Append(decoded);
                formatted.Append(decoded);
                if(CharIsPrintable(c))
                    converted.Append(c);
                else
                    converted.Append(".");

                entry++;
                if(++byteIndex < byteLength) { //loop counter increment
                    if(entry == 16) {
                        formatted.Append(newLine);
                        converted.Append(newLine);
                        lineNumbers.Append((lineNo.ToString().PadLeft(8, '0')) + newLine);
                        lineNo += 10;
                        entry = 0;
                    }
                    else if(entry == 8)
                        formatted.Append("  ");
                    else
                        formatted.Append(" ");
                }
                else if(byteIndex == byteLength)
                    lineNumbers.Append((lineNo.ToString().PadLeft(8, '0')));
            }

            // Display resulting data in appropriate fields, and return.
            this.hexLineNumbers.Text = lineNumbers.ToString();
            this.hexDisplay.Text = formatted.ToString();
            this.hexCharDisplay.Text = converted.ToString();
            return normal.ToString();
        }

        /// <summary>
        ///     Check if char data meets a set of criteria which defines that it can be properly displayed.
        /// </summary>
        /// <param name="c">
        ///     A character to be tested.
        /// </param>
        /// <see cref="System.ComponentModel.Design.ByteViewer.CharIsPrintable"/>
        /// <returns>
        ///     True, if the character can be displayed; false, otherwise.
        /// </returns>
        private static bool CharIsPrintable(char c) {
            UnicodeCategory uc = Char.GetUnicodeCategory(c);
            return (!(uc == UnicodeCategory.Control) || (uc == UnicodeCategory.Format) ||
                    (uc == UnicodeCategory.LineSeparator) || (uc == UnicodeCategory.ParagraphSeparator) ||
                    (uc == UnicodeCategory.OtherNotAssigned));
        }

        /// <summary>
        ///     Close this application
        /// </summary>
        /// <param name="sender">
        ///     Event source
        /// </param>
        /// <param name="e">
        ///     Details regarding the event
        /// </param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        ///     Handle what happens when the record/pause button is pressed
        /// </summary>
        /// <param name="sender">
        ///     Event source
        /// </param>
        /// <param name="e">
        ///     Details regarding the event
        /// </param>
        private void capturePauseButton_Click(object sender, EventArgs e)
        {
            if(captureLogic.isCapturing())
            {
                capturePauseButton.Enabled = false;
                captureLogic.stopCapture();
            }
            else
            {
                if(captureFile != null && captureFile.isModified())
                {
                    DialogResult result = MessageBox.Show("You have an unsaved capture file. Would you like to save it before starting a new capture?",
                        "Save capture file", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        if (!saveCaptureFile())
                            return;
                    }
                    else if (result == DialogResult.Cancel)
                        return;
                }

                capturePauseButton.Enabled = false;

                // create a new capture file
                setCaptureFile(CaptureFile.Factory.New());
                captureLogic.capture();
            }
        }

        /// <summary>
        ///     Update the state of the UI based on the type of event received
        /// </summary>
        /// <param name="evt">
        ///     The event
        /// </param>
        /// <param name="timeout">
        ///     Whether this event encountered a timeout or occurred because of one
        /// </param>
        private void newUIEvent(EventNotification evt, bool timeout)
        {
            Log.Info("Got new UIEvent " + evt.ToString());

            if(timeout)
            {
                Log.Info("UIEvent timed out");
                return;
            }

            switch (evt)
            {
                case EventNotification.Attached:
                    enterUIState(UIState.Attached);
                    break;
                case EventNotification.Attaching:
                    enterUIState(UIState.Attaching);
                    break;
                case EventNotification.Detached:
                    enterUIState(UIState.Detached);
                    break;
                case EventNotification.Detaching:
                    enterUIState(UIState.Detaching);
                    break;
                case EventNotification.CaptureStarted:
                    enterUIState(UIState.Capturing);
                    break;
                case EventNotification.CaptureStarting:
                    break;
                case EventNotification.CaptureStopping:
                    break;
                case EventNotification.CaptureStopped:
                    this.SafeInvoke((asd) =>
                    {
                        captureFile.finalize();
                    });

                    enterUIState(UIState.Attached);
                    break;
            }
        }

        /// <summary>
        ///     Guard against any strange behavior based on whether the process exists
        /// </summary>
        /// <returns>
        ///     A perfectly valid response to indicate whether to consider the process is selected
        /// </returns>
        private bool isProcessSelected()
        {
#if !WITHOUT_GAME
            return toolStripInstance.SelectedItem != null &&
                toolStripInstance.Enabled &&
                toolStripInstance.SelectedItem.ToString() != NO_INSTANCE_PLACEHOLDER;
#else
            return true;
#endif
        }

        /// <summary>
        ///     Save the capture data to file
        /// </summary>
        /// <returns>
        ///     True, if the capture file was saved; false, otherwise
        /// </returns>
        private bool saveCaptureFile()
        {
            bool cancelled;
            return saveCaptureFile(out cancelled);
        }

        /// <summary>
        ///     Save the capture data to file
        /// </summary>
        /// <param name="filename">
        ///     The name of the file to be created/saved
        /// </param>
        /// <returns>
        ///     True, if the capture file was saved; false, otherwise
        /// </returns>
        private bool doSaveCaptureFile(string filename)
        {
            try
            {
                CaptureFile.Factory.ToFile(captureFile, filename);
                setCaptureFile(captureFile);

                return true;
            }
            catch (IOException e)
            {
                Log.Debug("Failed to save capture file: {0}", e.Message);
                MessageBox.Show(e.Message,
                    "Failed to save capture file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        ///     Save the capture data to file
        /// </summary>
        /// <param name="canceled">
        ///     Whether the process was canceled
        /// </param>
        /// <returns>
        ///     True, if the capture file was saved; false, otherwise
        /// </returns>
        private bool saveCaptureFile(out bool canceled)
        {
            Log.Info("Save capture file");

            if (captureFile.isFirstSave())
                if (!showEditMetadata())
                {
                    canceled = true;
                    return false;
                }

            SaveFileDialog saveFile = new SaveFileDialog();

            saveFile.FileName = captureFile.getCaptureFilename();
            saveFile.Filter = "Game Capture Files (*.gcap) | *.gcap";
            saveFile.AddExtension = true;
            saveFile.DefaultExt = ".gcap";

            canceled = false;

            DialogResult result = saveFile.ShowDialog();

            if (result == DialogResult.OK)
            {
                return doSaveCaptureFile(saveFile.FileName);
            }
            else if(result == DialogResult.Cancel)
            {
                canceled = true;
            }

            return false;
        }

        /// <summary>
        ///     Handles the logic of pressing on the "Attach" button
        /// </summary>
        /// <param name="sender">
        ///     Event source
        /// </param>
        /// <param name="e">
        ///     Details regarding the event
        /// </param>
        private async void toolStripAttachButton_Click(object sender, EventArgs e)
        {
            if(captureLogic.isAttached())
            {
                enterUIState(UIState.Detaching);

                await Task.Factory.StartNew(() =>
                {
                    captureLogic.detach();
                });
            }
            else
            {
                if (!isProcessSelected())
                {
                    Trace.Assert(false, "Attemped to attach without first selecting a process");
                    return;
                }
#if !WITHOUT_GAME
                ProcessCollectable targetProcess = toolStripInstance.SelectedItem as ProcessCollectable;
#else
                ProcessCollectable targetProcess = new ProcessCollectable(Process.GetCurrentProcess());
#endif

                if (targetProcess == null)
                {
                    MessageBox.Show("Target process target was NULL", "Unknown Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                enterUIState(UIState.Attaching);

                captureLogic.attach(targetProcess,
                    (okay, attachResult, message) =>
                    {
                        if (okay)
                        {
                            enterUIState(UIState.Attached);
                            return;
                        }

                        enterUIState(UIState.Detached);

                        if (attachResult == AttachResult.PipeServerStartup)
                        {
                            DialogResult res = MessageBox.Show(message + Environment.NewLine + "Would you like to end the offending process?"
                                , "Failed to Attach", MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                            if (res == DialogResult.Yes)
                            {
                                targetProcess.Process.Refresh();

                                if(!targetProcess.Process.HasExited)
                                {
                                    Log.Info("Sending close to process {0} (this may fail)", targetProcess);
                                    targetProcess.Process.CloseMainWindow();
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show(message, "Failed to Attach", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    });
                    
            }
        }

        /// <summary>
        ///     Handle selecting the menu item to saving the capture data to file
        /// </summary>
        /// <param name="sender">
        ///     Event source
        /// </param>
        /// <param name="evt">
        ///     Details regarding the event
        /// </param>
        private void saveToolStripMenuItem_Click(object sender, EventArgs evt)
        {
            if (captureFile.isFirstSave())
                saveCaptureFile();
            else
                doSaveCaptureFile(captureFile.getCaptureFilename());
        }

        /// <summary>
        ///     Handle selecting the menu item that opens an existing capture file
        /// </summary>
        /// <param name="sender">
        ///     Event source
        /// </param>
        /// <param name="evt">
        ///     Details regarding the event
        /// </param>
        private async void openToolStripMenuItem_Click(object sender, EventArgs evt)
        {
            if (captureFile != null && captureFile.isModified())
            {
                DialogResult result = MessageBox.Show("You have an unsaved capture file. Would you like to save it before opening capture?",
                    "Save capture file", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    if (!saveCaptureFile())
                        return;
                }
                else if (result == DialogResult.Cancel)
                    return;
            }

            Log.Info("Open capture");

            OpenFileDialog openFile = new OpenFileDialog();
            openFile.AddExtension = true;
            openFile.Filter = "Game Capture Files (*.gcap)|*.gcap|All Files (*.*)|*.*";

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                BackgroundWorker worker = new BackgroundWorker();
                ProgressDialog progress = new ProgressDialog("Loading capture file");
                progress.ProgressTemplate("Loading records {value}/{max}...");

                // NOTE: hack alert. BeginInvoke merely posts this to the message pump
                this.BeginInvoke(new Action(() => progress.ShowDialog()));

                await Task.Run(delegate
                {
                    try
                    {
                        CaptureFile newCapFile = CaptureFile.Factory.FromFile(openFile.FileName, this, progress);
                        setCaptureFile(newCapFile);
                    }
                    catch (InvalidCaptureFileException e)
                    {
                        Log.Debug("Failed to open capture file: {0}", e.Message);
                        MessageBox.Show(e.Message, "Could not open capture file",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    this.SafeInvoke((a) => progress.Done());
                });
            }
        }

        /// <summary>
        ///     Handle selecting the menu item to saving the capture data to a new file
        /// </summary>
        /// <param name="sender">
        ///     Event source
        /// </param>
        /// <param name="evt">
        ///     Details regarding the event
        /// </param>
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveCaptureFile();
        }

        /// <summary>
        ///     Handle selecting the menu item that opens the About window for application information
        /// </summary>
        /// <param name="sender">
        ///     Event source
        /// </param>
        /// <param name="e">
        ///     Details regarding the event
        /// </param>
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Trace.Assert(false, "test");
            AboutBox about = new AboutBox();
            about.ShowDialog();
        }

        /// <summary>
        ///     Opens the window for save/file metadata
        /// </summary>
        /// <param name="sender">
        ///     Event source
        /// </param>
        /// <param name="e">
        ///     Details regarding the event
        /// </param>
        /// <returns>
        ///     True, if the window is to be opened; false, if not
        /// </returns>
        private bool showEditMetadata()
        {
            Trace.Assert(captureFile != null, "Capture file is null");

            EditMetadata editMeta = new EditMetadata(captureFile);
            DialogResult result = editMeta.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                captureFile.setCaptureName(editMeta.CaptureNameResult);
                captureFile.setCaptureDescription(editMeta.DescriptionResult);

                updateCaptureFileState();

                return true;
            }

            return false;
        }

        /// <summary>
        ///     Handle selecting the menu item that opens the window for save/file metadata
        /// </summary>
        /// <param name="sender">
        ///     Event source
        /// </param>
        /// <param name="e">
        ///     Details regarding the event
        /// </param>
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showEditMetadata();
        }

        /// <summary>
        ///     Handle selecting the menu item that displays information about hot keys
        /// </summary>
        /// <param name="sender">
        ///     Event source
        /// </param>
        /// <param name="e">
        ///     Details regarding the event
        /// </param>
        private void hotkeysToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HotkeysDialog dialog = new HotkeysDialog();
            dialog.ShowDialog();
        }
    }
}
