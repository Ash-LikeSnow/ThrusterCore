using ObjectBuilders.SafeZone;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.GameSystems;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
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
    internal class ThrustComp
    {
        private readonly Session _session;

        internal readonly IMyFunctionalBlock Thrust;
        internal MyCubeGrid Grid;
        internal GridComp GridComp;

        internal IMyTerminalControlOnOffSwitch ShowInToolbarSwitch;

        internal ThrustComp(IMyFunctionalBlock block, ThrustDefinition def, Session session)
        {
            _session = session;

            Thrust = block;
            Grid = block.CubeGrid as MyCubeGrid;


        }

        internal void Init()
        {

        }

        internal void Close()
        {


            Clean();
        }

        internal void Clean()
        {
            Grid = null;
            GridComp = null;

            ShowInToolbarSwitch = null;
        }

    }
}
