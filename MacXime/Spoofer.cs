using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace MacXime
{
	// http://www.windows-commandline.com/get-mac-address-command-line/

	// http://www.irongeek.com/i.php?page=security/madmacs-mac-spoofer
	// http://stackoverflow.com/questions/22310464/how-to-spoof-mac-address-via-c-sharp-code
	// http://www.codeproject.com/Articles/10493/MAC-Address-Changer-for-Windows-XP

	public class Spoofer
	{
		private const string BaseReg = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}\";

		public static bool SetMac(string nicid, string newmac)
		{
			var ret = false;
			using (var bkey = GetBaseKey())
			using (var key = bkey.OpenSubKey(BaseReg + nicid))
			{
				if (key == null) return false;
				key.SetValue("NetworkAddress", newmac, RegistryValueKind.String);

				var mos = new ManagementObjectSearcher(
					new SelectQuery("SELECT * FROM Win32_NetworkAdapter WHERE Index = " + nicid));

				foreach (var o in mos.Get().OfType<ManagementObject>())
				{
					o.InvokeMethod("Disable", null);
					o.InvokeMethod("Enable", null);
					ret = true;
				}
			}
			return ret;
		}

		public static IEnumerable<string> GetNicIds()
		{
			using (var bkey = GetBaseKey())
			using (var key = bkey.OpenSubKey(BaseReg))
			{
				if (key == null) yield break;
				foreach (var name in key.GetSubKeyNames().Where(n => !n.Equals("Properties")))
				{
					using (var sub = key.OpenSubKey(name))
					{
						if (sub != null)
						{
							var busType = sub.GetValue("BusType");
							var busStr = busType != null ? busType.ToString() : String.Empty;
							if (!String.IsNullOrEmpty(busStr))
								yield return name;

							//var busType = sub.GetValue("BusType");
							//if (busType != null)
							//	yield return name;
						}
					}
				}
			}
		}

		public static RegistryKey GetBaseKey()
		{
			return RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,
			    Environment.Is64BitOperatingSystem
			    ? RegistryView.Registry64 : RegistryView.Registry32);
		}

		public static string GenerateMacAddress()
		{
			var random = new Random();
			var buffer = new byte[6];
			random.NextBytes(buffer);
			var result = String.Concat(buffer.Select(x => x.ToString("X2") + ':'));
			return result.TrimEnd(':');
		}
	}
}
