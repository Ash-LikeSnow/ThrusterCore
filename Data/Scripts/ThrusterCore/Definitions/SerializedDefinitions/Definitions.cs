using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using VRage.ObjectBuilders;

namespace ThrusterCore
{
	public class Definitions
    {
		public Definition[] CubeBlocks;
		public Component[] Components;
		public PhysicalItem[] PhysicalItems;
		public Definition[] Definition;
	}

	
	public class PhysicalItem : Definition
	{

	}
	
	public class Component : Definition
	{

	}
	
	public class Definition
	{
		public SerializableDefinitionId Id;
		public ThrustValues ThrustValues;

	}

	public class ThrustValues
	{
        public float ForceMagnitude = 1f;
        public float SlowdownFactor = 1f;
        public float MaxPowerConsumption = 1f;
        public float MinPowerConsumption = 0f;
    }


}
