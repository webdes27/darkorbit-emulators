﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ow.Game.Objects;
using Ow.Game.Objects.Players;
using Ow.Game.Objects.Players.Managers;
using Ow.Game.Movements;
using Ow.Game.Objects.Stations;
using Ow.Managers;
using Ow.Net;
using Ow.Net.netty;
using Ow.Net.netty.commands;
using Ow.Net.netty.handlers;
using Ow.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ow.Game.Objects.Collectables;
using Ow.Game.Objects.Mines;
using System.Collections.Concurrent;
using Ow.Game.Ticks;

namespace Ow.Game
{
    class OptionsBase
    {
        public bool StarterMap { get; set; }
        public bool PvpMap { get; set; }
        public bool RangeDisabled { get; set; }
        public bool CloakBlocked { get; set; }
        public bool LogoutBlocked { get; set; }
        public bool DeathLocationRepair { get; set; }
    }

    class Spacemap : Tick
    {
        public ConcurrentDictionary<int, Character> Characters = new ConcurrentDictionary<int, Character>();
        public ConcurrentDictionary<int, Activatable> Activatables = new ConcurrentDictionary<int, Activatable>();
        public ConcurrentDictionary<string, Collectable> Collectables = new ConcurrentDictionary<string, Collectable>();
        public ConcurrentDictionary<string, Mine> Mines = new ConcurrentDictionary<string, Mine>();
        public ConcurrentDictionary<string, POI> POIs = new ConcurrentDictionary<string, POI>();

        public int TickId { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public int FactionId { get; set; }
        public string StationsJSON { get; set; }
        public Position[] Limits { get; private set; }
        public OptionsBase Options { get; set; }

        private List<PortalBase> PortalBase { get; set; }
        private List<StationBase> StationBase { get; set; }

        public Spacemap(int mapID, string name, int factionID, List<PortalBase> portals, List<StationBase> stations, OptionsBase options)
        {
            Id = mapID;
            Name = name;
            FactionId = factionID;
            PortalBase = portals;
            StationBase = stations;
            Options = options;
            ParseLimits();
            LoadObjects();

            var tickId = -1;
            Program.TickManager.AddTick(this, out tickId);
            TickId = tickId;
        }

        public void Tick()
        {
            foreach (var thisCharacter in Characters.Values)
            {
                foreach (var otherCharacter in Characters.Values)
                {
                    if (!thisCharacter.Equals(otherCharacter))
                    {
                        if (thisCharacter.InRange(otherCharacter, otherCharacter.RenderRange))
                        {
                            thisCharacter.AddInRangeCharacter(otherCharacter);
                        }
                        else if (thisCharacter.SelectedCharacter == null || (thisCharacter.SelectedCharacter != null && !thisCharacter.SelectedCharacter.Equals(otherCharacter)))
                        {
                            thisCharacter.RemoveInRangeCharacter(otherCharacter);
                        }
                    }
                }
            }
        }

        private void ParseLimits()
        {
            Limits = new Position[] { null, null };
            Limits[0] = new Position(0, 0);
            if (Id == 16 || Id == 29)
                Limits[1] = new Position(41800, 26000);
            else Limits[1] = new Position(20800, 12800);
        }

        public void LoadObjects()
        {
            /*
             * TEK HARİTA SEBEBİ İLE KAPATILDI
            if (StationBase != null && StationBase.Count >= 1) {
                foreach (var station in StationBase)
                {
                    switch (station.TypeId)
                    {
                        case AssetTypeModule.BASE_COMPANY:
                            var stationPosition = new Position(station.Position[0], station.Position[1]);
                            new HomeStation(this, station.FactionId, stationPosition, null);
                            break;
                    }
                }
            }
            */


            if (Id == 16)
            {
                new HomeStation(this, 1, Position.MMOPosition, null);
                new HomeStation(this, 2, Position.EICPosition, null);
                new HomeStation(this, 3, Position.VRUPosition, null);
            }

            /*
             * TEK HARİTA SEBEBİ İLE KAPATILDI
            if (PortalBase != null && PortalBase.Count >= 1)
            {
                foreach (var portal in PortalBase)
                {
                    var portalPosition = new Position(portal.Position[0], portal.Position[1]);
                    var targetPosition = new Position(portal.TargetPosition[0], portal.TargetPosition[1]);
                    new Portal(this, portalPosition, targetPosition, portal.TargetSpaceMapId, portal.GraphicId, portal.FactionId, portal.Visible, portal.Working);
                }
            }
            */

            /*
            if (Id == 14)
            {
                var battleStation = new BattleStation(this, 1, new Position(18500, 600), GameManager.GetClan(1));
            }
            */

            //if (Id != 101 && Id != 121 && Id != 42)
            if (Id == 16)
            {
                for (int i = 0; i <= 85; i++)
                    new BonusBox(AssetTypeModule.BOXTYPE_BONUS_BOX, Position.Random(this, 1000, 19800, 1000, 11800), this, true);

               // for (int i = 0; i <= 15; i++)
               //     new GreenBooty(AssetTypeModule.BOXTYPE_PIRATE_BOOTY, Position.Random(this, 1000, 19800, 1000, 11800), this, true);
            }

            if (Id == 101)
            {
                var poi = new POI("jackpot_poi", POITypes.RADIATION, POIDesigns.SIMPLE, POIShapes.CIRCLE, new List<Position> { new Position(5000, 3200), new Position(1500, 200) }, true, true);
                POIs.TryAdd("jackpot_poi", poi);
            }

            if (Id == 121)
            {
                var poi1 = new POI("uba_poi1", POITypes.NO_ACCESS, POIDesigns.SIMPLE, POIShapes.RECTANGLE, new List<Position> { new Position(5000, 3000), new Position(4000, 4000), new Position(4000, 2000), new Position(6000, 2000) }, true, true);
                var poi2 = new POI("uba_poi2", POITypes.NO_ACCESS, POIDesigns.SIMPLE, POIShapes.CIRCLE, new List<Position> { new Position(4400, 3600), new Position(200, 100) }, true, true);
                var poi3 = new POI("uba_poi3", POITypes.NO_ACCESS, POIDesigns.SIMPLE, POIShapes.CIRCLE, new List<Position> { new Position(5600, 2400), new Position(200, 100) }, true, true);

                POIs.TryAdd("uba_poi1", poi1);
                POIs.TryAdd("uba_poi2", poi2);
                POIs.TryAdd("uba_poi3", poi3);
            }
        }

        public void OnPlayerMovement(Player player)
        {
            CheckCollectables(player);
            CheckMines(player);
            bool inRadiationChanged = CheckRadiation(player);
            bool assetsChanged = CheckActivatables(player);
            if (inRadiationChanged || assetsChanged)
                player.SendCommand(player.GetBeaconCommand());
        }

        private void CheckCollectables(Player player)
        {
            foreach (var collectable in Collectables.Values)
            {
                if (collectable.ToPlayer == null || (collectable.ToPlayer != null && player == collectable.ToPlayer))
                {
                    if (player.Position.DistanceTo(collectable.Position) <= 1250)
                    {
                        if (!player.Storage.InRangeCollectables.ContainsKey(collectable.Hash))
                        {
                            player.Storage.InRangeCollectables.TryAdd(collectable.Hash, collectable);
                            player.SendCommand(collectable.GetCollectableCreateCommand());
                        }
                    }
                    else
                    {
                        if (player.Storage.InRangeCollectables.ContainsKey(collectable.Hash))
                        {
                            var outCollectable = collectable;
                            player.Storage.InRangeCollectables.TryRemove(collectable.Hash, out outCollectable);
                            player.SendCommand(DisposeBoxCommand.write(collectable.Hash, true));
                        }
                    }
                }
            }
        }

        private void CheckMines(Player player)
        {
            foreach (var mine in Mines.Values)
            {
                if (mine.Player == player || player.Storage.DuelOpponent == null || (player.Storage.DuelOpponent != null && mine.Player == player.Storage.DuelOpponent))
                {
                    if (player.Position.DistanceTo(mine.Position) <= 1250)
                    {
                        if (!player.Storage.InRangeMines.ContainsKey(mine.Hash))
                        {
                            player.Storage.InRangeMines.TryAdd(mine.Hash, mine);
                            player.SendCommand(mine.GetMineCreateCommand());
                        }
                        else
                        {
                            if (mine.Active)
                                if (mine.activationTime.AddSeconds(Mine.ACTIVATION_TIME) < DateTime.Now)
                                    if (player.Position.DistanceTo(mine.Position) < Mine.RANGE)
                                    {
                                        mine.Remove();
                                        mine.Explode();
                                    }
                        }
                    }
                    else
                    {
                        if (player.Storage.InRangeMines.ContainsKey(mine.Hash))
                        {
                            var outMine = mine;
                            player.Storage.InRangeMines.TryRemove(mine.Hash, out outMine);
                            player.SendPacket("0|" + ServerCommands.REMOVE_ORE + "|" + mine.Hash);
                        }
                    }
                }
            }
        }

        private bool CheckActivatables(Player Player)
        {
            bool isInSecureZone = false;
            bool inEquipZone = false;
            bool onBlockedMinePosition = false;

            foreach (var entity in Activatables.Values)
            {
                bool inRange = Player.Position.DistanceTo(entity.Position) <= 500;

                short status = inRange ? MapAssetActionAvailableCommand.ON : MapAssetActionAvailableCommand.OFF;

                if (entity is HomeStation)
                {
                    var homeStation = (HomeStation)entity;

                    if (Player.Position.DistanceTo(homeStation.Position) <= HomeStation.SECURE_ZONE_RANGE)
                    {
                        onBlockedMinePosition = true;
                        if (homeStation.FactionId == Player.FactionId)
                        {
                            if (!Player.LastAttackTime(5))
                            {
                                isInSecureZone = true;
                                inEquipZone = true;
                            }
                        }
                    } else onBlockedMinePosition = false;
                }
                else if (entity is RepairStation)
                {
                    if(Player.CurrentHitPoints == Player.MaxHitPoints || Player.FactionId != entity.FactionId)
                        inRange = false;
                }

                bool activateButton = Player.UpdateActivatable(entity, inRange);

                if (activateButton)
                {

                    var tooltipItemBars = new List<ClientUITooltipModule>();

                    var class521_localized_1 =
                     new ClientUITooltipTextFormatModule(ClientUITooltipTextFormatModule.LOCALIZED);

                    var slotBarItemStatusTooltip_1 =
                    new ClientUITooltipModule(class521_localized_1, ClientUITooltipModule.STANDARD, "q2_condition_JUMP", new List<ClientUITextReplacementModule>());

                    tooltipItemBars.Add(slotBarItemStatusTooltip_1);

                    var assetAction =
                            MapAssetActionAvailableCommand.write(entity.Id,
                                                               status,
                                                               inRange,
                                                               new ClientUITooltipsCommand(
                                                                       entity is Portal ? tooltipItemBars : new List<ClientUITooltipModule>()),
                                                               new class_h45()
                            );
                    Player.SendCommand(assetAction);
                }
            }

            
            Player.Storage.OnBlockedMinePosition = onBlockedMinePosition;

            if (Player.Settings.InGameSettings.inEquipZone != inEquipZone)
            {
                Player.Settings.InGameSettings.inEquipZone = inEquipZone;
                QueryManager.SavePlayer.Settings(Player, "inGameSettings", Player.Settings.InGameSettings);
                Player.SendCommand(EquipReadyCommand.write(inEquipZone));

                if (Player.Settings.InGameSettings.inEquipZone)
                    Player.SendPacket("0|A|STM|msg_equip_ready");
                else
                    Player.SendPacket("0|A|STM|msg_equip_not_ready");
            }

            if (Player.Storage.IsInDemilitarizedZone != isInSecureZone)
            {
                Player.Storage.IsInDemilitarizedZone = isInSecureZone;
                return true;
            }
            return false;
        }

        private bool CheckRadiation(Player Player)
        {
            int positionX = Player.Position.X;
            int positionY = Player.Position.Y;

            bool inRadiationZone = false;

            if (Id == 16 || Id == 42)
                inRadiationZone = positionX < 0 || positionX > 41800 || positionY < 0 || positionY > 26000;
            else
                inRadiationZone = positionX < 0 || positionX > 20900 || positionY < 0 || positionY > 13000;

            foreach (var poi in POIs.Values)
            {
                if (poi.Type == POITypes.RADIATION)
                {
                    if (Player.Position.DistanceTo(poi.ShapeCords[0]) > poi.ShapeCords[1].X)
                        inRadiationZone = true;
                }
            }

            if (Player.Storage.IsInRadiationZone != inRadiationZone)
            {
                Player.Storage.IsInRadiationZone = inRadiationZone;
                return true;
            }
            return false;
        }

        public void SendObjects(Player player)
        {
            foreach (Activatable activatableStationary in Activatables.Values)
                player.SendCommand(activatableStationary.GetAssetCreateCommand());
            foreach (var poi in POIs.Values)
                player.SendCommand(poi.GetPOICreateCommand());
        }

        public void SendPlayers(Player Player)
        {
            foreach (var character in Characters.Values)
            {
                Player.InRangeCharacters.Clear();
                if (Player.InRange(character, character.RenderRange))
                    Player.AddInRangeCharacter(character);
            }
        }

        public void AddAndInitPlayer(Player player)
        {
            AddCharacter(player);
            LoginRequestHandler.SendSettings(player);
            LoginRequestHandler.SendPlayerItems(player, false);
        }

        public Activatable GetActivatableMapEntity(int pAssetId)
        {
            return !Activatables.ContainsKey(pAssetId) ? null : Activatables[pAssetId];
        }

        public class CharacterArgs : EventArgs
        {
            public Character Character { get; }
            public CharacterArgs(Character character)
            {
                Character = character;
            }
        }

        public event EventHandler<CharacterArgs> CharacterRemoved;
        public event EventHandler<CharacterArgs> CharacterAdded;

        public bool AddCharacter(Character character)
        {
            var success = Characters.TryAdd(character.Id, character);
            if (success)
            {
                CharacterAdded?.Invoke(this, new CharacterArgs(character));
            }
            return success;
        }

        public bool RemoveCharacter(Character character)
        {
            var success = Characters.TryRemove(character.Id, out character);
            if (success)
            {
                CharacterRemoved?.Invoke(this, new CharacterArgs(character));
                foreach (var otherCharacter in Characters.Values)
                    otherCharacter.RemoveInRangeCharacter(character);
            }
            return success;
        }
    }
}
