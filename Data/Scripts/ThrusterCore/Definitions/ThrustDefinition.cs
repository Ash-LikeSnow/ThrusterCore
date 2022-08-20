using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;

namespace ThrusterCore
{
    /// <summary>
    /// Stored values from cubeblock definitions plus precalculated constants
    /// </summary>
    internal class ThrustDefinition
    {
        internal readonly MyFuelConverterInfo FuelInfo;
        internal readonly float ForceMagnitude;
        internal readonly float SlowdownFactor;
        internal readonly float MaxPowerConsumption;
        internal readonly float MinPowerConsumption;

        internal readonly float MinPlanetaryInfluence;
        internal readonly float MaxPlanetaryInfluence;
        internal readonly float EffectivenessAtMinInfluence;
        internal readonly float EffectivenessAtMaxInfluence;
        internal readonly bool NeedsAtmosphereForInfluence;

        internal readonly float InfluenceRangeInv;

        public ThrustDefinition(ThrustValues values)
        {
            FuelInfo = values.FuelInfo;
            ForceMagnitude = values.ForceMagnitude;
            SlowdownFactor = values.SlowdownFactor;
            MaxPowerConsumption = values.MaxPowerConsumption;
            MinPowerConsumption = values.MinPowerConsumption;

            MinPlanetaryInfluence = values.MinPlanetaryInfluence;
            MaxPlanetaryInfluence = values.MaxPlanetaryInfluence;
            EffectivenessAtMinInfluence = values.EffectivenessAtMinInfluence;
            EffectivenessAtMaxInfluence = values.EffectivenessAtMaxInfluence;
            NeedsAtmosphereForInfluence = values.NeedsAtmosphereForInfluence;

            InfluenceRangeInv = 1 / (MaxPlanetaryInfluence - MinPlanetaryInfluence);

            Logs.WriteLine($"Definition loaded: {ForceMagnitude} {SlowdownFactor} {MaxPowerConsumption} {MinPowerConsumption}");
        }

    }
}
