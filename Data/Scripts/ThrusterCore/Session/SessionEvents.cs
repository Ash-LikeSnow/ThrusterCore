using ObjectBuilders.SafeZone;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.GameSystems;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Input;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;
using IMyControllableEntity = VRage.Game.ModAPI.Interfaces.IMyControllableEntity;

namespace ThrusterCore
{
    public partial class Session : MySessionComponentBase
    {
        private void OnEntityCreate(MyEntity entity)
        {
            var grid = entity as MyCubeGrid;
            if (grid != null)
            {
                grid.AddedToScene += addToStart => _startGrids.Add(grid);
            }

            var sorter = entity as MyConveyorSorter;
            if (sorter != null && DefinitionMap.ContainsKey(sorter.BlockDefinition.Id))
            {
                sorter.AddedToScene += addToStart => _startBlocks.Add(sorter);
            }

            var controller = entity as MyShipController;
            if (controller != null)
            {
                controller.AddedToScene += addToStart => _startBlocks.Add(controller);
            }

        }

        private void OnGridClose(IMyEntity entity)
        {
            var grid = entity as MyCubeGrid;

            GridComp comp;
            if (GridMap.TryRemove(grid, out comp))
            {
                GridList.Remove(comp);

                comp.Clean();
                _gridCompPool.Push(comp);
            }
        }

        private void PlayerConnected(long id)
        {
            try
            {
                Logs.WriteLine($"PlayerConnected() - {id}");
                if (PlayerMap.ContainsKey(id)) return;
                MyAPIGateway.Multiplayer.Players.GetPlayers(null, myPlayer => FindPlayer(myPlayer, id));
            }
            catch (Exception ex) { Logs.WriteLine($"Exception in PlayerConnected: {ex}"); }
        }

        private void PlayerDisconnected(long id)
        {
            try
            {
                Logs.WriteLine($"PlayerDisconnected() - {id}");
                IMyPlayer player;
                if (PlayerMap.TryRemove(id, out player))
                {
                    player.Controller.ControlledEntityChanged -= OnPlayerController;
                }
            }
            catch (Exception ex) { Logs.WriteLine($"Exception in PlayerDisconnected: {ex}"); }
        }

        private bool FindPlayer(IMyPlayer player, long id)
        {
            if (player.IdentityId == id)
            {
                Logs.WriteLine($"Player found - {id}");
                PlayerMap.TryAdd(id, player);

                var controller = player.Controller;
                if (controller != null)
                {
                    controller.ControlledEntityChanged += OnPlayerController;
                    OnPlayerController(null, controller.ControlledEntity);
                }

                // Send enforcement
                //if (IsServer)
                //{
                //    SendServerStartup(player.SteamUserId);
                //}
            }
            return false;
        }

        private void OnPlayerController(IMyControllableEntity exitController, IMyControllableEntity enterController)
        {
            try
            {
                Logs.WriteLine("OnPlayerController()");
                GridComp gridComp;
                var exitEntity = exitController as MyEntity;
                if (exitEntity != null && enterController?.ControllerInfo != null)
                {
                    var controller = exitEntity as IMyShipController;
                    if (controller != null)
                    {
                        if (GridMap.TryGetValue(controller.CubeGrid, out gridComp))
                        {
                            gridComp.UnderControl = false;
                        }
                    }
                }

                var enterEntity = enterController as MyEntity;
                if (enterEntity != null && enterController.ControllerInfo != null)
                {
                    var controller = enterEntity as IMyShipController;

                    if (controller != null)
                    {
                        if (GridMap.TryGetValue(controller.CubeGrid, out gridComp))
                        {
                            Logs.WriteLine("OnPlayerController() success");
                            gridComp.UnderControl = true;
                            gridComp.ActiveControl = gridComp.Controllers[controller];
                        }
                    }
                }
            }
            catch (Exception ex) { Logs.WriteLine($"Exception in OnPlayerController: {ex}"); }
        }

    }
}
