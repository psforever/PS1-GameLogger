using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PSCap {
    /// <summary>
    ///     Textbox that allows for synchronized scrolling of other textboxes
    /// </summary>
    public class SharedScrollTextBox : TextBox {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        private List<TextBox> buddies;

        public SharedScrollTextBox() {
            this.buddies = new List<TextBox>();
        }

        public bool linkTextbox(TextBox box) {
            if(!this.buddies.Contains(box)) {
                this.buddies.Add(box);
                return true;
            }
            return false;
        }

        public bool removeTextbox(TextBox box) {
            return this.buddies.Remove(box) == true;
        }

        /// <summary>
        ///     Coordinate TextBox scrolling with buddies
        /// </summary>
        /// <param name="m">
        ///     The incoming Message
        /// </param>
        // stackoverflow.com/questions/3823188/how-can-i-sync-the-scrolling-of-two-multiline-textboxes
        protected override void WndProc(ref Message m) {
            base.WndProc(ref m);
            if((m.Msg == 0x115 || m.Msg == 0x20a) && this.buddies.Count > 0) {
                foreach(TextBox tbox in this.buddies)
                    SendMessage(tbox.Handle, m.Msg, m.WParam, m.LParam);
            }
        }
    }
}
