using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRageMath;

namespace ThrusterCore
{
    /// <summary>
    /// Stores data from ship controllers
    /// </summary>
    internal class Control
    {
        internal readonly IMyShipController Controller;
        internal readonly MyBlockOrientation Orientation;

        internal MyCubeGrid Grid;
        internal Vector3 PreviousMove;

        internal readonly Dictionary<byte, int> Mapping = new Dictionary<byte, int>();

        public Control(IMyShipController controller)
        {
            Controller = controller;
            Orientation = controller.Orientation;

            Mapping[(byte)Orientation.Forward] = 3;
            Mapping[(byte)Orientation.Up] = 2;
            Mapping[(byte)Orientation.Left] = 1;

            foreach (var key in Mapping.Keys.ToList())
            {
                var even = key % 2 == 0;
                var newKey = even ? key + 1 : key - 1;
                Mapping[(byte)newKey] = Mapping[key];
            }

            Grid = controller.CubeGrid as MyCubeGrid;
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
            PreviousMove = Vector3.Zero;
        }

    }
}
