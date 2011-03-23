// Probe.cs
// Author: Adam Nathan
// Date: 5/11/2003
// For more information, see http://blogs.gotdotnet.com/anathan
using System.Windows.Forms;

namespace ClrSpy
{
	public enum ProbeType
	{
		Setting,
		Error,
		Failure,
		Warning,
		Info
	}

	public struct Probe
	{
		public static Probe [] GlobalSettingsList =
		{
			// Setting probes
			new Probe("Probes Enabled", "CDP.AllowDebugProbes", ProbeType.Setting),
			new Probe("Break on Error Messages", "CDP.AllowDebugBreak", ProbeType.Setting)
		};

		public static Probe [] ProbeList = 
		{
			// Error probes
			new Probe("Collected Delegate", "CDP.CollectedDelegate", ProbeType.Error),
			new Probe("Invalid IUnknown", "CDP.InvalidIUnknown", ProbeType.Error),
			new Probe("Invalid VARIANT", "CDP.InvalidVariant", ProbeType.Error),
			new Probe("PInvoke Calling Convention Mismatch", "CDP.PInvokeCallConvMismatch", ProbeType.Error),

			// Failure probes
			new Probe("Buffer Overrun", "CDP.BufferOverrun", ProbeType.Failure),
			new Probe("Object Not Kept Alive", "CDP.ObjNotKeptAlive", ProbeType.Failure),

			// Warning probes
			new Probe("Disconnected Context", "CDP.DisconnectedContext", ProbeType.Warning),
			new Probe("QueryInterface Failure", "CDP.FailedQI", ProbeType.Warning),
			new Probe("Thread Changing Apartment State", "CDP.Apartment", ProbeType.Warning),
			new Probe("Unmarshalable Interface", "CDP.NotMarshalable", ProbeType.Warning),

			// Info probes
			new Probe("Marshaling", "CDP.Marshaling", ProbeType.Info),
			new Probe("Marshaling Filter", "CDP.Marshaling.Filter", ProbeType.Info)
		};

		public Probe(string displayName, string configName, ProbeType type)
		{
			DisplayName = displayName;
			ConfigName = configName;
			Type = type;
			CheckBox = null;
		}

		public string DisplayName;
		public string ConfigName;
		public ProbeType Type;
		public CheckBox CheckBox;
	}
}