using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using System;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;
using VRage.Utils;
using Sandbox.Game.Entities;
using ObjectBuilders.SafeZone;
using SpaceEngineers.Game.ModAPI;
using VRage.Game.Entity;
using Sandbox.Game.Entities.Cube;
using System.Collections.Generic;
using Sandbox.Game.EntityComponents;
using VRage.Game.ModAPI.Interfaces;

namespace ThrusterCore
{
    public partial class Session
    {

        internal void CompLoop()
        {

            for (int i = 0; i < GridList.Count; i++)
            {
                var gridComp = GridList[i];

                for (int j = 0; j < gridComp.ThrustComps.Count; j++)
                {
                    var thrustComp = gridComp.ThrustComps[j];

                }

            }

        }

        internal void StartComps()
        {
            try
            {
                _startGrids.ApplyAdditions();
                if (_startGrids.Count > 0)
                {
                    for (int i = 0; i < _startGrids.Count; i++)
                    {
                        var grid = _startGrids[i];

                        if ((grid as MyCubeGrid).IsPreview)
                            continue;

                        var gridComp = _gridCompPool.Count > 0 ? _gridCompPool.Pop() : new GridComp();
                        gridComp.Init(grid);

                        GridList.Add(gridComp);
                        GridMap[grid] = gridComp;
                        grid.OnClose += OnGridClose;
                    }
                    _startGrids.ClearImmediate();
                }

                _startBlocks.ApplyAdditions();
                for (int i = 0; i < _startBlocks.Count; i++)
                {
                    var block = _startBlocks[i];

                    if (block?.CubeGrid?.Physics == null || !GridMap.ContainsKey(block.CubeGrid))
                        continue;

                    if ((block.CubeGrid as MyCubeGrid).IsPreview)
                        continue;

                    var gridComp = GridMap[block.CubeGrid];

                    ThrustDefinition def;
                    if (!DefinitionMap.TryGetValue(block.BlockDefinition, out def))
                        continue;

                    var comp = new ThrustComp(block, def, this);
                    ThrustMap[block.EntityId] = comp;

                }
                _startBlocks.ClearImmediate();
            }
            catch (Exception ex)
            {
                Logs.WriteLine($"Exception in StartComps: {ex}");
            }

        }
    }
}
