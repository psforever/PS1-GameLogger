using System;
using System.Collections.Generic;

namespace PSCap
{

    public sealed class PlanetSideGamePacketOpcode
    {
        private readonly string name;
        private readonly byte opcode;
        private static readonly Dictionary<byte, PlanetSideGamePacketOpcode> instance = new Dictionary<byte, PlanetSideGamePacketOpcode>();

        /*
         Generated from GamePacketOpcode.scala using vim

         %!awk '{print $2}'
         %s/OPCODE\n//
         %s/^\n//
         Macro: 0yypj"zYddk4ell"zPldef""zpldt"
         Format:
           1. public static ... NAME = new ... (NUM, "NAME");
           2. NAME_1
        */

        public static readonly PlanetSideGamePacketOpcode Unknown0 = new PlanetSideGamePacketOpcode(0, "Unknown0");
        public static readonly PlanetSideGamePacketOpcode LoginMessage = new PlanetSideGamePacketOpcode(1, "LoginMessage");
        public static readonly PlanetSideGamePacketOpcode LoginRespMessage = new PlanetSideGamePacketOpcode(2, "LoginRespMessage");
        public static readonly PlanetSideGamePacketOpcode ConnectToWorldRequestMessage = new PlanetSideGamePacketOpcode(3, "ConnectToWorldRequestMessage");
        public static readonly PlanetSideGamePacketOpcode ConnectToWorldMessage = new PlanetSideGamePacketOpcode(4, "ConnectToWorldMessage");
        public static readonly PlanetSideGamePacketOpcode VNLWorldStatusMessage = new PlanetSideGamePacketOpcode(5, "VNLWorldStatusMessage");
        public static readonly PlanetSideGamePacketOpcode UnknownMessage6 = new PlanetSideGamePacketOpcode(6, "UnknownMessage6");
        public static readonly PlanetSideGamePacketOpcode UnknownMessage7 = new PlanetSideGamePacketOpcode(7, "UnknownMessage7");
        public static readonly PlanetSideGamePacketOpcode PlayerStateMessage = new PlanetSideGamePacketOpcode(8, "PlayerStateMessage");
        public static readonly PlanetSideGamePacketOpcode HitMessage = new PlanetSideGamePacketOpcode(9, "HitMessage");
        public static readonly PlanetSideGamePacketOpcode HitHint = new PlanetSideGamePacketOpcode(10, "HitHint");
        public static readonly PlanetSideGamePacketOpcode DamageMessage = new PlanetSideGamePacketOpcode(11, "DamageMessage");
        public static readonly PlanetSideGamePacketOpcode DestroyMessage = new PlanetSideGamePacketOpcode(12, "DestroyMessage");
        public static readonly PlanetSideGamePacketOpcode ReloadMessage = new PlanetSideGamePacketOpcode(13, "ReloadMessage");
        public static readonly PlanetSideGamePacketOpcode MountVehicleMsg = new PlanetSideGamePacketOpcode(14, "MountVehicleMsg");
        public static readonly PlanetSideGamePacketOpcode DismountVehicleMsg = new PlanetSideGamePacketOpcode(15, "DismountVehicleMsg");
        public static readonly PlanetSideGamePacketOpcode UseItemMessage = new PlanetSideGamePacketOpcode(16, "UseItemMessage");
        public static readonly PlanetSideGamePacketOpcode MoveItemMessage = new PlanetSideGamePacketOpcode(17, "MoveItemMessage");
        public static readonly PlanetSideGamePacketOpcode ChatMsg = new PlanetSideGamePacketOpcode(18, "ChatMsg");
        public static readonly PlanetSideGamePacketOpcode CharacterNoRecordMessage = new PlanetSideGamePacketOpcode(19, "CharacterNoRecordMessage");
        public static readonly PlanetSideGamePacketOpcode CharacterInfoMessage = new PlanetSideGamePacketOpcode(20, "CharacterInfoMessage");
        public static readonly PlanetSideGamePacketOpcode UnknownMessage21 = new PlanetSideGamePacketOpcode(21, "UnknownMessage21");
        public static readonly PlanetSideGamePacketOpcode BindPlayerMessage = new PlanetSideGamePacketOpcode(22, "BindPlayerMessage");
        public static readonly PlanetSideGamePacketOpcode ObjectCreateMessage_Duplicate = new PlanetSideGamePacketOpcode(23, "ObjectCreateMessage_Duplicate");
        public static readonly PlanetSideGamePacketOpcode ObjectCreateMessage = new PlanetSideGamePacketOpcode(24, "ObjectCreateMessage");
        public static readonly PlanetSideGamePacketOpcode ObjectDeleteMessage = new PlanetSideGamePacketOpcode(25, "ObjectDeleteMessage");
        public static readonly PlanetSideGamePacketOpcode PingMsg = new PlanetSideGamePacketOpcode(26, "PingMsg");
        public static readonly PlanetSideGamePacketOpcode VehicleStateMessage = new PlanetSideGamePacketOpcode(27, "VehicleStateMessage");
        public static readonly PlanetSideGamePacketOpcode FrameVehicleStateMessage = new PlanetSideGamePacketOpcode(28, "FrameVehicleStateMessage");
        public static readonly PlanetSideGamePacketOpcode GenericObjectStateMsg = new PlanetSideGamePacketOpcode(29, "GenericObjectStateMsg");
        public static readonly PlanetSideGamePacketOpcode ChildObjectStateMessage = new PlanetSideGamePacketOpcode(30, "ChildObjectStateMessage");
        public static readonly PlanetSideGamePacketOpcode ActionResultMessage = new PlanetSideGamePacketOpcode(31, "ActionResultMessage");
        public static readonly PlanetSideGamePacketOpcode UnknownMessage32 = new PlanetSideGamePacketOpcode(32, "UnknownMessage32");
        public static readonly PlanetSideGamePacketOpcode ActionProgressMessage = new PlanetSideGamePacketOpcode(33, "ActionProgressMessage");
        public static readonly PlanetSideGamePacketOpcode ActionCancelMessage = new PlanetSideGamePacketOpcode(34, "ActionCancelMessage");
        public static readonly PlanetSideGamePacketOpcode ActionCancelAcknowledgeMessage = new PlanetSideGamePacketOpcode(35, "ActionCancelAcknowledgeMessage");
        public static readonly PlanetSideGamePacketOpcode SetEmpireMessage = new PlanetSideGamePacketOpcode(36, "SetEmpireMessage");
        public static readonly PlanetSideGamePacketOpcode EmoteMsg = new PlanetSideGamePacketOpcode(37, "EmoteMsg");
        public static readonly PlanetSideGamePacketOpcode UnuseItemMessage = new PlanetSideGamePacketOpcode(38, "UnuseItemMessage");
        public static readonly PlanetSideGamePacketOpcode ObjectDetachMessage = new PlanetSideGamePacketOpcode(39, "ObjectDetachMessage");
        public static readonly PlanetSideGamePacketOpcode CreateShortcutMessage = new PlanetSideGamePacketOpcode(40, "CreateShortcutMessage");
        public static readonly PlanetSideGamePacketOpcode ChangeShortcutBankMessage = new PlanetSideGamePacketOpcode(41, "ChangeShortcutBankMessage");
        public static readonly PlanetSideGamePacketOpcode ObjectAttachMessage = new PlanetSideGamePacketOpcode(42, "ObjectAttachMessage");
        public static readonly PlanetSideGamePacketOpcode UnknownMessage43 = new PlanetSideGamePacketOpcode(43, "UnknownMessage43");
        public static readonly PlanetSideGamePacketOpcode PlanetsideAttributeMessage = new PlanetSideGamePacketOpcode(44, "PlanetsideAttributeMessage");
        public static readonly PlanetSideGamePacketOpcode RequestDestroyMessage = new PlanetSideGamePacketOpcode(45, "RequestDestroyMessage");
        public static readonly PlanetSideGamePacketOpcode UnknownMessage46 = new PlanetSideGamePacketOpcode(46, "UnknownMessage46");
        public static readonly PlanetSideGamePacketOpcode CharacterCreateRequestMessage = new PlanetSideGamePacketOpcode(47, "CharacterCreateRequestMessage");
        public static readonly PlanetSideGamePacketOpcode CharacterRequestMessage = new PlanetSideGamePacketOpcode(48, "CharacterRequestMessage");
        public static readonly PlanetSideGamePacketOpcode LoadMapMessage = new PlanetSideGamePacketOpcode(49, "LoadMapMessage");
        public static readonly PlanetSideGamePacketOpcode SetCurrentAvatarMessage = new PlanetSideGamePacketOpcode(50, "SetCurrentAvatarMessage");
        public static readonly PlanetSideGamePacketOpcode ObjectHeldMessage = new PlanetSideGamePacketOpcode(51, "ObjectHeldMessage");
        public static readonly PlanetSideGamePacketOpcode WeaponFireMessage = new PlanetSideGamePacketOpcode(52, "WeaponFireMessage");
        public static readonly PlanetSideGamePacketOpcode AvatarJumpMessage = new PlanetSideGamePacketOpcode(53, "AvatarJumpMessage");
        public static readonly PlanetSideGamePacketOpcode PickupItemMessage = new PlanetSideGamePacketOpcode(54, "PickupItemMessage");
        public static readonly PlanetSideGamePacketOpcode DropItemMessage = new PlanetSideGamePacketOpcode(55, "DropItemMessage");
        public static readonly PlanetSideGamePacketOpcode InventoryStateMessage = new PlanetSideGamePacketOpcode(56, "InventoryStateMessage");
        public static readonly PlanetSideGamePacketOpcode ChangeFireStateMessage_Start = new PlanetSideGamePacketOpcode(57, "ChangeFireStateMessage_Start");
        public static readonly PlanetSideGamePacketOpcode ChangeFireStateMessage_Stop = new PlanetSideGamePacketOpcode(58, "ChangeFireStateMessage_Stop");
        public static readonly PlanetSideGamePacketOpcode UnknownMessage59 = new PlanetSideGamePacketOpcode(59, "UnknownMessage59");
        public static readonly PlanetSideGamePacketOpcode GenericCollisionMsg = new PlanetSideGamePacketOpcode(60, "GenericCollisionMsg");
        public static readonly PlanetSideGamePacketOpcode QuantityUpdateMessage = new PlanetSideGamePacketOpcode(61, "QuantityUpdateMessage");
        public static readonly PlanetSideGamePacketOpcode ArmorChangedMessage = new PlanetSideGamePacketOpcode(62, "ArmorChangedMessage");
        public static readonly PlanetSideGamePacketOpcode ProjectileStateMessage = new PlanetSideGamePacketOpcode(63, "ProjectileStateMessage");
        public static readonly PlanetSideGamePacketOpcode MountVehicleCargoMsg = new PlanetSideGamePacketOpcode(64, "MountVehicleCargoMsg");
        public static readonly PlanetSideGamePacketOpcode DismountVehicleCargoMsg = new PlanetSideGamePacketOpcode(65, "DismountVehicleCargoMsg");
        public static readonly PlanetSideGamePacketOpcode CargoMountPointStatusMessage = new PlanetSideGamePacketOpcode(66, "CargoMountPointStatusMessage");
        public static readonly PlanetSideGamePacketOpcode BeginZoningMessage = new PlanetSideGamePacketOpcode(67, "BeginZoningMessage");
        public static readonly PlanetSideGamePacketOpcode ItemTransactionMessage = new PlanetSideGamePacketOpcode(68, "ItemTransactionMessage");
        public static readonly PlanetSideGamePacketOpcode ItemTransactionResultMessage = new PlanetSideGamePacketOpcode(69, "ItemTransactionResultMessage");
        public static readonly PlanetSideGamePacketOpcode ChangeFireModeMessage = new PlanetSideGamePacketOpcode(70, "ChangeFireModeMessage");
        public static readonly PlanetSideGamePacketOpcode ChangeAmmoMessage = new PlanetSideGamePacketOpcode(71, "ChangeAmmoMessage");
        public static readonly PlanetSideGamePacketOpcode TimeOfDayMessage = new PlanetSideGamePacketOpcode(72, "TimeOfDayMessage");
        public static readonly PlanetSideGamePacketOpcode UnknownMessage73 = new PlanetSideGamePacketOpcode(73, "UnknownMessage73");
        public static readonly PlanetSideGamePacketOpcode SpawnRequestMessage = new PlanetSideGamePacketOpcode(74, "SpawnRequestMessage");
        public static readonly PlanetSideGamePacketOpcode DeployRequestMessage = new PlanetSideGamePacketOpcode(75, "DeployRequestMessage");
        public static readonly PlanetSideGamePacketOpcode UnknownMessage76 = new PlanetSideGamePacketOpcode(76, "UnknownMessage76");
        public static readonly PlanetSideGamePacketOpcode RepairMessage = new PlanetSideGamePacketOpcode(77, "RepairMessage");
        public static readonly PlanetSideGamePacketOpcode ServerVehicleOverrideMsg = new PlanetSideGamePacketOpcode(78, "ServerVehicleOverrideMsg");
        public static readonly PlanetSideGamePacketOpcode LashMessage = new PlanetSideGamePacketOpcode(79, "LashMessage");
        public static readonly PlanetSideGamePacketOpcode TargetingInfoMessage = new PlanetSideGamePacketOpcode(80, "TargetingInfoMessage");
        public static readonly PlanetSideGamePacketOpcode TriggerEffectMessage = new PlanetSideGamePacketOpcode(81, "TriggerEffectMessage");
        public static readonly PlanetSideGamePacketOpcode WeaponDryFireMessage = new PlanetSideGamePacketOpcode(82, "WeaponDryFireMessage");
        public static readonly PlanetSideGamePacketOpcode DroppodLaunchRequestMessage = new PlanetSideGamePacketOpcode(83, "DroppodLaunchRequestMessage");
        public static readonly PlanetSideGamePacketOpcode HackMessage = new PlanetSideGamePacketOpcode(84, "HackMessage");
        public static readonly PlanetSideGamePacketOpcode DroppodLaunchResponseMessage = new PlanetSideGamePacketOpcode(85, "DroppodLaunchResponseMessage");
        public static readonly PlanetSideGamePacketOpcode GenericObjectActionMessage = new PlanetSideGamePacketOpcode(86, "GenericObjectActionMessage");
        public static readonly PlanetSideGamePacketOpcode AvatarVehicleTimerMessage = new PlanetSideGamePacketOpcode(87, "AvatarVehicleTimerMessage");
        public static readonly PlanetSideGamePacketOpcode AvatarImplantMessage = new PlanetSideGamePacketOpcode(88, "AvatarImplantMessage");
        public static readonly PlanetSideGamePacketOpcode UnknownMessage89 = new PlanetSideGamePacketOpcode(89, "UnknownMessage89");
        public static readonly PlanetSideGamePacketOpcode DelayedPathMountMsg = new PlanetSideGamePacketOpcode(90, "DelayedPathMountMsg");
        public static readonly PlanetSideGamePacketOpcode OrbitalShuttleTimeMsg = new PlanetSideGamePacketOpcode(91, "OrbitalShuttleTimeMsg");
        public static readonly PlanetSideGamePacketOpcode AIDamage = new PlanetSideGamePacketOpcode(92, "AIDamage");
        public static readonly PlanetSideGamePacketOpcode DeployObjectMessage = new PlanetSideGamePacketOpcode(93, "DeployObjectMessage");
        public static readonly PlanetSideGamePacketOpcode FavoritesRequest = new PlanetSideGamePacketOpcode(94, "FavoritesRequest");
        public static readonly PlanetSideGamePacketOpcode FavoritesResponse = new PlanetSideGamePacketOpcode(95, "FavoritesResponse");
        public static readonly PlanetSideGamePacketOpcode FavoritesMessage = new PlanetSideGamePacketOpcode(96, "FavoritesMessage");
        public static readonly PlanetSideGamePacketOpcode ObjectDetectedMessage = new PlanetSideGamePacketOpcode(97, "ObjectDetectedMessage");
        public static readonly PlanetSideGamePacketOpcode SplashHitMessage = new PlanetSideGamePacketOpcode(98, "SplashHitMessage");
        public static readonly PlanetSideGamePacketOpcode SetChatFilterMessage = new PlanetSideGamePacketOpcode(99, "SetChatFilterMessage");
        public static readonly PlanetSideGamePacketOpcode AvatarSearchCriteriaMessage = new PlanetSideGamePacketOpcode(100, "AvatarSearchCriteriaMessage");
        public static readonly PlanetSideGamePacketOpcode AvatarSearchResponse = new PlanetSideGamePacketOpcode(101, "AvatarSearchResponse");
        public static readonly PlanetSideGamePacketOpcode WeaponJammedMessage = new PlanetSideGamePacketOpcode(102, "WeaponJammedMessage");
        public static readonly PlanetSideGamePacketOpcode LinkDeadAwarenessMsg = new PlanetSideGamePacketOpcode(103, "LinkDeadAwarenessMsg");
        public static readonly PlanetSideGamePacketOpcode DroppodFreefallingMessage = new PlanetSideGamePacketOpcode(104, "DroppodFreefallingMessage");
        public static readonly PlanetSideGamePacketOpcode AvatarFirstTimeEventMessage = new PlanetSideGamePacketOpcode(105, "AvatarFirstTimeEventMessage");
        public static readonly PlanetSideGamePacketOpcode AggravatedDamageMessage = new PlanetSideGamePacketOpcode(106, "AggravatedDamageMessage");
        public static readonly PlanetSideGamePacketOpcode TriggerSoundMessage = new PlanetSideGamePacketOpcode(107, "TriggerSoundMessage");
        public static readonly PlanetSideGamePacketOpcode LootItemMessage = new PlanetSideGamePacketOpcode(108, "LootItemMessage");
        public static readonly PlanetSideGamePacketOpcode VehicleSubStateMessage = new PlanetSideGamePacketOpcode(109, "VehicleSubStateMessage");
        public static readonly PlanetSideGamePacketOpcode SquadMembershipRequest = new PlanetSideGamePacketOpcode(110, "SquadMembershipRequest");
        public static readonly PlanetSideGamePacketOpcode SquadMembershipResponse = new PlanetSideGamePacketOpcode(111, "SquadMembershipResponse");
        public static readonly PlanetSideGamePacketOpcode SquadMemberEvent = new PlanetSideGamePacketOpcode(112, "SquadMemberEvent");
        public static readonly PlanetSideGamePacketOpcode PlatoonEvent = new PlanetSideGamePacketOpcode(113, "PlatoonEvent");
        public static readonly PlanetSideGamePacketOpcode FriendsRequest = new PlanetSideGamePacketOpcode(114, "FriendsRequest");
        public static readonly PlanetSideGamePacketOpcode FriendsResponse = new PlanetSideGamePacketOpcode(115, "FriendsResponse");
        public static readonly PlanetSideGamePacketOpcode TriggerEnvironmentalDamageMessage = new PlanetSideGamePacketOpcode(116, "TriggerEnvironmentalDamageMessage");
        public static readonly PlanetSideGamePacketOpcode TrainingZoneMessage = new PlanetSideGamePacketOpcode(117, "TrainingZoneMessage");
        public static readonly PlanetSideGamePacketOpcode DeployableObjectsInfoMessage = new PlanetSideGamePacketOpcode(118, "DeployableObjectsInfoMessage");
        public static readonly PlanetSideGamePacketOpcode SquadState = new PlanetSideGamePacketOpcode(119, "SquadState");
        public static readonly PlanetSideGamePacketOpcode OxygenStateMessage = new PlanetSideGamePacketOpcode(120, "OxygenStateMessage");
        public static readonly PlanetSideGamePacketOpcode TradeMessage = new PlanetSideGamePacketOpcode(121, "TradeMessage");
        public static readonly PlanetSideGamePacketOpcode UnknownMessage122 = new PlanetSideGamePacketOpcode(122, "UnknownMessage122");
        public static readonly PlanetSideGamePacketOpcode DamageFeedbackMessage = new PlanetSideGamePacketOpcode(123, "DamageFeedbackMessage");
        public static readonly PlanetSideGamePacketOpcode DismountBuildingMsg = new PlanetSideGamePacketOpcode(124, "DismountBuildingMsg");
        public static readonly PlanetSideGamePacketOpcode UnknownMessage125 = new PlanetSideGamePacketOpcode(125, "UnknownMessage125");
        public static readonly PlanetSideGamePacketOpcode UnknownMessage126 = new PlanetSideGamePacketOpcode(126, "UnknownMessage126");
        public static readonly PlanetSideGamePacketOpcode AvatarStatisticsMessage = new PlanetSideGamePacketOpcode(127, "AvatarStatisticsMessage");
        public static readonly PlanetSideGamePacketOpcode GenericObjectAction2Message = new PlanetSideGamePacketOpcode(128, "GenericObjectAction2Message");
        public static readonly PlanetSideGamePacketOpcode DestroyDisplayMessage = new PlanetSideGamePacketOpcode(129, "DestroyDisplayMessage");
        public static readonly PlanetSideGamePacketOpcode TriggerBotAction = new PlanetSideGamePacketOpcode(130, "TriggerBotAction");
        public static readonly PlanetSideGamePacketOpcode SquadWaypointRequest = new PlanetSideGamePacketOpcode(131, "SquadWaypointRequest");
        public static readonly PlanetSideGamePacketOpcode SquadWaypointEvent = new PlanetSideGamePacketOpcode(132, "SquadWaypointEvent");
        public static readonly PlanetSideGamePacketOpcode OffshoreVehicleMessage = new PlanetSideGamePacketOpcode(133, "OffshoreVehicleMessage");
        public static readonly PlanetSideGamePacketOpcode ObjectDeployedMessage = new PlanetSideGamePacketOpcode(134, "ObjectDeployedMessage");
        public static readonly PlanetSideGamePacketOpcode ObjectDeployedCountMessage = new PlanetSideGamePacketOpcode(135, "ObjectDeployedCountMessage");
        public static readonly PlanetSideGamePacketOpcode WeaponDelayFireMessage = new PlanetSideGamePacketOpcode(136, "WeaponDelayFireMessage");
        public static readonly PlanetSideGamePacketOpcode BugReportMessage = new PlanetSideGamePacketOpcode(137, "BugReportMessage");
        public static readonly PlanetSideGamePacketOpcode PlayerStasisMessage = new PlanetSideGamePacketOpcode(138, "PlayerStasisMessage");
        public static readonly PlanetSideGamePacketOpcode UnknownMessage139 = new PlanetSideGamePacketOpcode(139, "UnknownMessage139");
        public static readonly PlanetSideGamePacketOpcode OutfitMembershipRequest = new PlanetSideGamePacketOpcode(140, "OutfitMembershipRequest");
        public static readonly PlanetSideGamePacketOpcode OutfitMembershipResponse = new PlanetSideGamePacketOpcode(141, "OutfitMembershipResponse");
        public static readonly PlanetSideGamePacketOpcode OutfitRequest = new PlanetSideGamePacketOpcode(142, "OutfitRequest");
        public static readonly PlanetSideGamePacketOpcode OutfitEvent = new PlanetSideGamePacketOpcode(143, "OutfitEvent");
        public static readonly PlanetSideGamePacketOpcode OutfitMemberEvent = new PlanetSideGamePacketOpcode(144, "OutfitMemberEvent");
        public static readonly PlanetSideGamePacketOpcode OutfitMemberUpdate = new PlanetSideGamePacketOpcode(145, "OutfitMemberUpdate");
        public static readonly PlanetSideGamePacketOpcode PlanetsideStringAttributeMessage = new PlanetSideGamePacketOpcode(146, "PlanetsideStringAttributeMessage");
        public static readonly PlanetSideGamePacketOpcode DataChallengeMessage = new PlanetSideGamePacketOpcode(147, "DataChallengeMessage");
        public static readonly PlanetSideGamePacketOpcode DataChallengeMessageResp = new PlanetSideGamePacketOpcode(148, "DataChallengeMessageResp");
        public static readonly PlanetSideGamePacketOpcode WeatherMessage = new PlanetSideGamePacketOpcode(149, "WeatherMessage");
        public static readonly PlanetSideGamePacketOpcode SimDataChallenge = new PlanetSideGamePacketOpcode(150, "SimDataChallenge");
        public static readonly PlanetSideGamePacketOpcode SimDataChallengeResp = new PlanetSideGamePacketOpcode(151, "SimDataChallengeResp");
        public static readonly PlanetSideGamePacketOpcode OutfitListEvent = new PlanetSideGamePacketOpcode(152, "OutfitListEvent");
        public static readonly PlanetSideGamePacketOpcode EmpireIncentivesMessage = new PlanetSideGamePacketOpcode(153, "EmpireIncentivesMessage");
        public static readonly PlanetSideGamePacketOpcode InvalidTerrainMessage = new PlanetSideGamePacketOpcode(154, "InvalidTerrainMessage");
        public static readonly PlanetSideGamePacketOpcode SyncMessage = new PlanetSideGamePacketOpcode(155, "SyncMessage");
        public static readonly PlanetSideGamePacketOpcode DebugDrawMessage = new PlanetSideGamePacketOpcode(156, "DebugDrawMessage");
        public static readonly PlanetSideGamePacketOpcode SoulMarkMessage = new PlanetSideGamePacketOpcode(157, "SoulMarkMessage");
        public static readonly PlanetSideGamePacketOpcode UplinkPositionEvent = new PlanetSideGamePacketOpcode(158, "UplinkPositionEvent");
        public static readonly PlanetSideGamePacketOpcode HotSpotUpdateMessage = new PlanetSideGamePacketOpcode(159, "HotSpotUpdateMessage");
        public static readonly PlanetSideGamePacketOpcode BuildingInfoUpdateMessage = new PlanetSideGamePacketOpcode(160, "BuildingInfoUpdateMessage");
        public static readonly PlanetSideGamePacketOpcode FireHintMessage = new PlanetSideGamePacketOpcode(161, "FireHintMessage");
        public static readonly PlanetSideGamePacketOpcode UplinkRequest = new PlanetSideGamePacketOpcode(162, "UplinkRequest");
        public static readonly PlanetSideGamePacketOpcode UplinkResponse = new PlanetSideGamePacketOpcode(163, "UplinkResponse");
        public static readonly PlanetSideGamePacketOpcode WarpgateRequest = new PlanetSideGamePacketOpcode(164, "WarpgateRequest");
        public static readonly PlanetSideGamePacketOpcode WarpgateResponse = new PlanetSideGamePacketOpcode(165, "WarpgateResponse");
        public static readonly PlanetSideGamePacketOpcode DamageWithPositionMessage = new PlanetSideGamePacketOpcode(166, "DamageWithPositionMessage");
        public static readonly PlanetSideGamePacketOpcode GenericActionMessage = new PlanetSideGamePacketOpcode(167, "GenericActionMessage");
        public static readonly PlanetSideGamePacketOpcode ContinentalLockUpdateMessage = new PlanetSideGamePacketOpcode(168, "ContinentalLockUpdateMessage");
        public static readonly PlanetSideGamePacketOpcode AvatarGrenadeStateMessage = new PlanetSideGamePacketOpcode(169, "AvatarGrenadeStateMessage");
        public static readonly PlanetSideGamePacketOpcode UnknownMessage170 = new PlanetSideGamePacketOpcode(170, "UnknownMessage170");
        public static readonly PlanetSideGamePacketOpcode UnknownMessage171 = new PlanetSideGamePacketOpcode(171, "UnknownMessage171");
        public static readonly PlanetSideGamePacketOpcode ReleaseAvatarRequestMessage = new PlanetSideGamePacketOpcode(172, "ReleaseAvatarRequestMessage");
        public static readonly PlanetSideGamePacketOpcode AvatarDeadStateMessage = new PlanetSideGamePacketOpcode(173, "AvatarDeadStateMessage");
        public static readonly PlanetSideGamePacketOpcode CSAssistMessage = new PlanetSideGamePacketOpcode(174, "CSAssistMessage");
        public static readonly PlanetSideGamePacketOpcode CSAssistCommentMessage = new PlanetSideGamePacketOpcode(175, "CSAssistCommentMessage");
        public static readonly PlanetSideGamePacketOpcode VoiceHostRequest = new PlanetSideGamePacketOpcode(176, "VoiceHostRequest");
        public static readonly PlanetSideGamePacketOpcode VoiceHostKill = new PlanetSideGamePacketOpcode(177, "VoiceHostKill");
        public static readonly PlanetSideGamePacketOpcode VoiceHostInfo = new PlanetSideGamePacketOpcode(178, "VoiceHostInfo");
        public static readonly PlanetSideGamePacketOpcode BattleplanMessage = new PlanetSideGamePacketOpcode(179, "BattleplanMessage");
        public static readonly PlanetSideGamePacketOpcode BattleExperienceMessage = new PlanetSideGamePacketOpcode(180, "BattleExperienceMessage");
        public static readonly PlanetSideGamePacketOpcode TargetingImplantRequest = new PlanetSideGamePacketOpcode(181, "TargetingImplantRequest");
        public static readonly PlanetSideGamePacketOpcode ZonePopulationUpdateMessage = new PlanetSideGamePacketOpcode(182, "ZonePopulationUpdateMessage");
        public static readonly PlanetSideGamePacketOpcode DisconnectMessage = new PlanetSideGamePacketOpcode(183, "DisconnectMessage");
        public static readonly PlanetSideGamePacketOpcode ExperienceAddedMessage = new PlanetSideGamePacketOpcode(184, "ExperienceAddedMessage");
        public static readonly PlanetSideGamePacketOpcode OrbitalStrikeWaypointMessage = new PlanetSideGamePacketOpcode(185, "OrbitalStrikeWaypointMessage");
        public static readonly PlanetSideGamePacketOpcode KeepAliveMessage = new PlanetSideGamePacketOpcode(186, "KeepAliveMessage");
        public static readonly PlanetSideGamePacketOpcode MapObjectStateBlockMessage = new PlanetSideGamePacketOpcode(187, "MapObjectStateBlockMessage");
        public static readonly PlanetSideGamePacketOpcode SnoopMsg = new PlanetSideGamePacketOpcode(188, "SnoopMsg");
        public static readonly PlanetSideGamePacketOpcode PlayerStateMessageUpstream = new PlanetSideGamePacketOpcode(189, "PlayerStateMessageUpstream");
        public static readonly PlanetSideGamePacketOpcode PlayerStateShiftMessage = new PlanetSideGamePacketOpcode(190, "PlayerStateShiftMessage");
        public static readonly PlanetSideGamePacketOpcode ZipLineMessage = new PlanetSideGamePacketOpcode(191, "ZipLineMessage");
        public static readonly PlanetSideGamePacketOpcode CaptureFlagUpdateMessage = new PlanetSideGamePacketOpcode(192, "CaptureFlagUpdateMessage");
        public static readonly PlanetSideGamePacketOpcode VanuModuleUpdateMessage = new PlanetSideGamePacketOpcode(193, "VanuModuleUpdateMessage");
        public static readonly PlanetSideGamePacketOpcode FacilityBenefitShieldChargeRequestMessage = new PlanetSideGamePacketOpcode(194, "FacilityBenefitShieldChargeRequestMessage");
        public static readonly PlanetSideGamePacketOpcode ProximityTerminalUseMessage = new PlanetSideGamePacketOpcode(195, "ProximityTerminalUseMessage");
        public static readonly PlanetSideGamePacketOpcode QuantityDeltaUpdateMessage = new PlanetSideGamePacketOpcode(196, "QuantityDeltaUpdateMessage");
        public static readonly PlanetSideGamePacketOpcode ChainLashMessage = new PlanetSideGamePacketOpcode(197, "ChainLashMessage");
        public static readonly PlanetSideGamePacketOpcode ZoneInfoMessage = new PlanetSideGamePacketOpcode(198, "ZoneInfoMessage");
        public static readonly PlanetSideGamePacketOpcode LongRangeProjectileInfoMessage = new PlanetSideGamePacketOpcode(199, "LongRangeProjectileInfoMessage");
        public static readonly PlanetSideGamePacketOpcode WeaponLazeTargetPositionMessage = new PlanetSideGamePacketOpcode(200, "WeaponLazeTargetPositionMessage");
        public static readonly PlanetSideGamePacketOpcode ModuleLimitsMessage = new PlanetSideGamePacketOpcode(201, "ModuleLimitsMessage");
        public static readonly PlanetSideGamePacketOpcode OutfitBenefitMessage = new PlanetSideGamePacketOpcode(202, "OutfitBenefitMessage");
        public static readonly PlanetSideGamePacketOpcode EmpireChangeTimeMessage = new PlanetSideGamePacketOpcode(203, "EmpireChangeTimeMessage");
        public static readonly PlanetSideGamePacketOpcode ClockCalibrationMessage = new PlanetSideGamePacketOpcode(204, "ClockCalibrationMessage");
        public static readonly PlanetSideGamePacketOpcode DensityLevelUpdateMessage = new PlanetSideGamePacketOpcode(205, "DensityLevelUpdateMessage");
        public static readonly PlanetSideGamePacketOpcode ActOfGodMessage = new PlanetSideGamePacketOpcode(206, "ActOfGodMessage");
        public static readonly PlanetSideGamePacketOpcode AvatarAwardMessage = new PlanetSideGamePacketOpcode(207, "AvatarAwardMessage");
        public static readonly PlanetSideGamePacketOpcode UnknownMessage208 = new PlanetSideGamePacketOpcode(208, "UnknownMessage208");
        public static readonly PlanetSideGamePacketOpcode DisplayedAwardMessage = new PlanetSideGamePacketOpcode(209, "DisplayedAwardMessage");
        public static readonly PlanetSideGamePacketOpcode RespawnAMSInfoMessage = new PlanetSideGamePacketOpcode(210, "RespawnAMSInfoMessage");
        public static readonly PlanetSideGamePacketOpcode ComponentDamageMessage = new PlanetSideGamePacketOpcode(211, "ComponentDamageMessage");
        public static readonly PlanetSideGamePacketOpcode GenericObjectActionAtPositionMessage = new PlanetSideGamePacketOpcode(212, "GenericObjectActionAtPositionMessage");
        public static readonly PlanetSideGamePacketOpcode PropertyOverrideMessage = new PlanetSideGamePacketOpcode(213, "PropertyOverrideMessage");
        public static readonly PlanetSideGamePacketOpcode WarpgateLinkOverrideMessage = new PlanetSideGamePacketOpcode(214, "WarpgateLinkOverrideMessage");
        public static readonly PlanetSideGamePacketOpcode EmpireBenefitsMessage = new PlanetSideGamePacketOpcode(215, "EmpireBenefitsMessage");
        public static readonly PlanetSideGamePacketOpcode ForceEmpireMessage = new PlanetSideGamePacketOpcode(216, "ForceEmpireMessage");
        public static readonly PlanetSideGamePacketOpcode BroadcastWarpgateUpdateMessage = new PlanetSideGamePacketOpcode(217, "BroadcastWarpgateUpdateMessage");
        public static readonly PlanetSideGamePacketOpcode UnknownMessage218 = new PlanetSideGamePacketOpcode(218, "UnknownMessage218");
        public static readonly PlanetSideGamePacketOpcode SquadMainTerminalMessage = new PlanetSideGamePacketOpcode(219, "SquadMainTerminalMessage");
        public static readonly PlanetSideGamePacketOpcode SquadMainTerminalResponseMessage = new PlanetSideGamePacketOpcode(220, "SquadMainTerminalResponseMessage");
        public static readonly PlanetSideGamePacketOpcode SquadOrderMessage = new PlanetSideGamePacketOpcode(221, "SquadOrderMessage");
        public static readonly PlanetSideGamePacketOpcode SquadOrderResponse = new PlanetSideGamePacketOpcode(222, "SquadOrderResponse");
        public static readonly PlanetSideGamePacketOpcode ZoneLockInfoMessage = new PlanetSideGamePacketOpcode(223, "ZoneLockInfoMessage");
        public static readonly PlanetSideGamePacketOpcode SquadBindInfoMessage = new PlanetSideGamePacketOpcode(224, "SquadBindInfoMessage");
        public static readonly PlanetSideGamePacketOpcode AudioSequenceMessage = new PlanetSideGamePacketOpcode(225, "AudioSequenceMessage");
        public static readonly PlanetSideGamePacketOpcode SquadFacilityBindInfoMessage = new PlanetSideGamePacketOpcode(226, "SquadFacilityBindInfoMessage");
        public static readonly PlanetSideGamePacketOpcode ZoneForcedCavernConnectionsMessage = new PlanetSideGamePacketOpcode(227, "ZoneForcedCavernConnectionsMessage");
        public static readonly PlanetSideGamePacketOpcode MissionActionMessage = new PlanetSideGamePacketOpcode(228, "MissionActionMessage");
        public static readonly PlanetSideGamePacketOpcode MissionKillTriggerMessage = new PlanetSideGamePacketOpcode(229, "MissionKillTriggerMessage");
        public static readonly PlanetSideGamePacketOpcode ReplicationStreamMessage = new PlanetSideGamePacketOpcode(230, "ReplicationStreamMessage");
        public static readonly PlanetSideGamePacketOpcode SquadDefinitionActionMessage = new PlanetSideGamePacketOpcode(231, "SquadDefinitionActionMessage");
        public static readonly PlanetSideGamePacketOpcode SquadDetailDefinitionUpdateMessage = new PlanetSideGamePacketOpcode(232, "SquadDetailDefinitionUpdateMessage");
        public static readonly PlanetSideGamePacketOpcode TacticsMessage = new PlanetSideGamePacketOpcode(233, "TacticsMessage");
        public static readonly PlanetSideGamePacketOpcode RabbitUpdateMessage = new PlanetSideGamePacketOpcode(234, "RabbitUpdateMessage");
        public static readonly PlanetSideGamePacketOpcode SquadInvitationRequestMessage = new PlanetSideGamePacketOpcode(235, "SquadInvitationRequestMessage");
        public static readonly PlanetSideGamePacketOpcode CharacterKnowledgeMessage = new PlanetSideGamePacketOpcode(236, "CharacterKnowledgeMessage");
        public static readonly PlanetSideGamePacketOpcode GameScoreUpdateMessage = new PlanetSideGamePacketOpcode(237, "GameScoreUpdateMessage");
        public static readonly PlanetSideGamePacketOpcode UnknownMessage238 = new PlanetSideGamePacketOpcode(238, "UnknownMessage238");
        public static readonly PlanetSideGamePacketOpcode OrderTerminalBugMessage = new PlanetSideGamePacketOpcode(239, "OrderTerminalBugMessage");
        public static readonly PlanetSideGamePacketOpcode QueueTimedHelpMessage = new PlanetSideGamePacketOpcode(240, "QueueTimedHelpMessage");
        public static readonly PlanetSideGamePacketOpcode MailMessage = new PlanetSideGamePacketOpcode(241, "MailMessage");
        public static readonly PlanetSideGamePacketOpcode GameVarUpdate = new PlanetSideGamePacketOpcode(242, "GameVarUpdate");
        public static readonly PlanetSideGamePacketOpcode ClientCheatedMessage = new PlanetSideGamePacketOpcode(243, "ClientCheatedMessage");

        private PlanetSideGamePacketOpcode(byte opcode, string name)
        {
            this.opcode = opcode;
            this.name = name;
            instance[opcode] = this;
        }

        public override string ToString()
        {
            return name;
        }

        public static explicit operator PlanetSideGamePacketOpcode(byte opcode)
        {
            PlanetSideGamePacketOpcode res;

            if (instance.TryGetValue(opcode, out res))
                return res;
            else
                throw new InvalidCastException();
        }
    }
}