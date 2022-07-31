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
using System.Collections.Concurrent;
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

namespace ThrusterCore
{
    /// <summary>
    /// Holds all grid specific data
    /// </summary>
    internal class GridComp
    {
        private IMyCubeGrid _grid;

        internal readonly List<ThrustComp> ThrustComps = new List<ThrustComp>();

        internal void Init(IMyCubeGrid grid)
        {
            _grid = grid;

            _grid.OnBlockAdded += BlockAdded;
            _grid.OnBlockRemoved += BlockRemoved;

        }

        private void BlockAdded(IMySlimBlock slim)
        {

        }

        private void BlockRemoved(IMySlimBlock slim)
        {

        }

        internal void Clean()
        {
            _grid.OnBlockAdded -= BlockAdded;
            _grid.OnBlockRemoved -= BlockRemoved;

            _grid = null;

            ThrustComps.Clear();
        }
    }

}
