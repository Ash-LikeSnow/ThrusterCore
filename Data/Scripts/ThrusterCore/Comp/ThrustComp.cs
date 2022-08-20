using ObjectBuilders.SafeZone;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.EntityComponents;
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
    /// <summary>
    /// Holds all thrust block data
    /// </summary>
    internal class ThrustComp : MyEntityComponentBase
    {
        private readonly Session _session;

        internal readonly MyConveyorSorter Block;
        internal MyCubeGrid Grid;
        internal GridComp GridComp;

        internal IMyTerminalControlOnOffSwitch ShowInToolbarSwitch;

        internal readonly Dictionary<ushort, float> Multipliers = new Dictionary<ushort, float>();

        internal readonly ThrustDefinition Definition;
        internal readonly bool Electric;
        internal readonly bool AffectedByPlanet;

        internal bool Enabled;
        internal bool Functional;
        internal bool Powered;
        internal bool Overridden;
        internal bool NeedsFuel;

        internal float Override;
        internal float Strength;
        internal float Effectiveness = 1f;
        internal float StoredFuel;

        internal ThrustComp(MyConveyorSorter block, ThrustDefinition def, Session session)
        {
            _session = session;

            Block = block;
            Grid = block.CubeGrid;

            Definition = def;

            Electric = def.FuelInfo == null;
            AffectedByPlanet = def.EffectivenessAtMaxInfluence != def.EffectivenessAtMinInfluence;

            var iBlock = (IMyConveyorSorter)Block;
            iBlock.EnabledChanged += EnabledChanged;
            iBlock.IsWorkingChanged += IsWorkingChanged;

            Enabled = iBlock.Enabled;
            Functional = iBlock.IsFunctional;

        }

        private void EnabledChanged(IMyTerminalBlock block)
        {
            Enabled = (block as IMyFunctionalBlock).Enabled;
        }

        private void IsWorkingChanged(IMyCubeBlock block)
        {
            Functional = block.IsFunctional;
        }

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
        }

        public override void OnAddedToScene()
        {
            base.OnAddedToScene();
        }

        public override void OnBeforeRemovedFromContainer()
        {
            base.OnBeforeRemovedFromContainer();

            Close();
        }

        public override bool IsSerialized()
        {
            return false;
        }

        internal void Init()
        {
            Block.Components.Add(this);

            SinkInit();
        }

        internal void SinkInit()
        {

            var sink = Block.Components?.Get<MyResourceSinkComponent>();
            if (sink != null)
            {
                var sinkInfo = new MyResourceSinkInfo()
                {
                    MaxRequiredInput = 0,
                    RequiredInputFunc = () => 0f,
                    ResourceTypeId = MyResourceDistributorComponent.ElectricityId
                };

                sink.SetRequiredInputFuncByType(sinkInfo.ResourceTypeId, () => 0f);

                //sink.RemoveType(ref sinkInfo.ResourceTypeId);
                //sink.AddType(ref sinkInfo);
                Logs.WriteLine("sink reset");
            }
        }

        internal void Close()
        {
            var iBlock = (IMyConveyorSorter)Block;
            iBlock.EnabledChanged -= EnabledChanged;
            iBlock.IsWorkingChanged -= IsWorkingChanged;

            Clean();
        }

        internal void Clean()
        {
            Grid = null;
            GridComp = null;

            ShowInToolbarSwitch = null;

            Multipliers.Clear();
        }

        public override string ComponentTypeDebugString => "ThrusterCore";
    }
}
