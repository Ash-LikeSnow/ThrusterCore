using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThrusterCore
{
    internal class ThrustDefinition
    {
        internal readonly float ForceMagnitude;
        internal readonly float SlowdownFactor;
        internal readonly float MaxPowerConsumption;
        internal readonly float MinPowerConsumption;

        public ThrustDefinition(ThrustValues values)
        {
            ForceMagnitude = values.ForceMagnitude;
            SlowdownFactor = values.SlowdownFactor;
            MaxPowerConsumption = values.MaxPowerConsumption;
            MinPowerConsumption = values.MinPowerConsumption;
        }

    }
}
