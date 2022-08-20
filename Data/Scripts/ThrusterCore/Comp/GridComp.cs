using ObjectBuilders.SafeZone;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.EntityComponents;
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
        private Session _session;

        internal MyCubeGrid Grid;
        internal Control ActiveControl;
        internal MyResourceDistributorComponent Distributor;
        internal MyResourceSinkComponent ElectricSink;
        internal MyConveyorSorter SinkBlock;

        internal readonly List<ThrustComp> ThrustComps = new List<ThrustComp>();
        internal readonly ConcurrentDictionary<byte, DirectionData> DirectionMap = new ConcurrentDictionary<byte, DirectionData>();
        internal readonly ConcurrentDictionary<IMyShipController, Control> Controllers = new ConcurrentDictionary<IMyShipController, Control>();

        internal MyPlanet ClosestPlanet;

        internal float AirDensity;
        internal float Power;

        internal long CompTick60;
        internal long CompTick20;

        internal bool UnderControl;
        internal bool Dirty;
        internal bool AirDirty;

        internal void Init(MyCubeGrid grid, Session session)
        {
            _session = session;

            Grid = grid;

            CompTick20 = Grid.EntityId % 20;
            CompTick60 = Grid.EntityId % 60;

            Grid.OnBlockAdded += BlockAdded;
            Grid.OnBlockRemoved += BlockRemoved;

            Grid.OnFatBlockAdded += FatBlockAdded;
            Grid.OnFatBlockRemoved += FatBlockRemoved;

        }

        internal void FatBlockAdded(MyCubeBlock block)
        {
            if (block is MyConveyorSorter)
            {
                ThrustComp comp;
                if (_session.ThrustMap.TryGetValue(block.EntityId, out comp))
                {
                    ThrustComps.Add(comp);

                    var data = DirectionMap.GetOrAdd((byte)block.Orientation.Forward, new DirectionData());
                    data.ThrustComps.Add(comp);

                    if (SinkBlock == null && comp.Electric)
                        SinkInit(comp.Block);
                }
            }
        }

        private void FatBlockRemoved(MyCubeBlock block)
        {
            if (block is MyConveyorSorter)
            {
                ThrustComp comp;
                if (_session.ThrustMap.TryGetValue(block.EntityId, out comp))
                {
                    ThrustComps.Remove(comp);

                    DirectionData data;
                    if (DirectionMap.TryGetValue((byte)block.Orientation.Forward, out data))
                        data.ThrustComps.Remove(comp);
                    else Logs.WriteLine("FatBlockRemoved() - direction data not found!");

                    if (SinkBlock == block && ThrustComps.Count > 0)
                        SinkInit(ThrustComps[0].Block);
                }
            }
        }

        private void BlockAdded(IMySlimBlock slim)
        {
            Dirty = true;
        }

        private void BlockRemoved(IMySlimBlock slim)
        {
            Dirty = true;
        }

        internal void SinkInit(MyConveyorSorter sinkBlock)
        {
            var sinkInfo = new MyResourceSinkInfo()
            {
                MaxRequiredInput = 0,
                RequiredInputFunc = PowerFunc,
                ResourceTypeId = MyResourceDistributorComponent.ElectricityId
            };

            if (SinkBlock != null && ElectricSink != null)
            {
                ElectricSink.SetRequiredInputFuncByType(sinkInfo.ResourceTypeId, () => 0f);
            }

            SinkBlock = sinkBlock;

            ElectricSink = SinkBlock.Components?.Get<MyResourceSinkComponent>();
            if (ElectricSink != null)
            {
                //ElectricSink.RemoveType(ref sinkInfo.ResourceTypeId);
                //ElectricSink.AddType(ref sinkInfo);
                ElectricSink.SetRequiredInputFuncByType(sinkInfo.ResourceTypeId, PowerFunc);
                Logs.WriteLine("sink found");
            }
            else
            {
                ElectricSink = new MyResourceSinkComponent();
                ElectricSink.Init(MyStringHash.GetOrCompute("Thrust"), sinkInfo);
                SinkBlock.Components.Add(ElectricSink);
                Logs.WriteLine("sink added");
            }

            Distributor = (Grid as IMyCubeGrid).ResourceDistributor as MyResourceDistributorComponent;
            if (Distributor != null)
                Distributor.AddSink(ElectricSink);
            else
                Logs.WriteLine($"GridComp.SinkInit() - Distributor null");

            ElectricSink.Update();
        }

        private float PowerFunc()
        {
            return Power;
        }

        internal void Clean()
        {
            Grid.OnBlockAdded -= BlockAdded;
            Grid.OnBlockRemoved -= BlockRemoved;

            Grid.OnFatBlockAdded -= FatBlockAdded;
            Grid.OnFatBlockRemoved -= FatBlockRemoved;

            Grid = null;
            ActiveControl = null;
            Distributor = null;
            ElectricSink = null;
            SinkBlock = null;
            ClosestPlanet = null;

            ThrustComps.Clear();
            Controllers.Clear();
            DirectionMap.Clear();
        }
    }

    internal class DirectionData
    {
        internal List<ThrustComp> ThrustComps = new List<ThrustComp>();

        internal float MaxThrust;
        internal float ThrustElectric;
        internal float ThrustGas;
    }

}
