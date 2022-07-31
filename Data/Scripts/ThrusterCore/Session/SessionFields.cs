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
    public partial class Session : MySessionComponentBase
    {
        private const string PATH = "Data\\ThrusterCoreFiles.txt";

        private readonly MyObjectBuilderType _sorterType = new MyObjectBuilderType(typeof(MyObjectBuilder_ConveyorSorter));

        private readonly List<MyThrustDefinition> _vanillaDefs = new List<MyThrustDefinition>();

        private readonly Stack<GridComp> _gridCompPool = new Stack<GridComp>(128);

        private readonly ConcurrentCachingList<IMyUpgradeModule> _startBlocks = new ConcurrentCachingList<IMyUpgradeModule>();
        private readonly ConcurrentCachingList<IMyCubeGrid> _startGrids = new ConcurrentCachingList<IMyCubeGrid>();

        internal readonly Dictionary<SerializableDefinitionId, ThrustDefinition> DefinitionMap = new Dictionary<SerializableDefinitionId, ThrustDefinition>();

        internal readonly List<GridComp> GridList = new List<GridComp>();
        internal readonly ConcurrentDictionary<IMyCubeGrid, GridComp> GridMap = new ConcurrentDictionary<IMyCubeGrid, GridComp>();

        internal readonly Dictionary<long, ThrustComp> ThrustMap = new Dictionary<long, ThrustComp>();

        public Session()
        {
            
        }

        private void Clean()
        {
            _vanillaDefs.Clear();
            _gridCompPool.Clear();
            _startBlocks.ClearImmediate();
            _startGrids.ClearImmediate();

            DefinitionMap.Clear();

            GridList.Clear();
            GridMap.Clear();
            ThrustMap.Clear();

        }

    }
}
