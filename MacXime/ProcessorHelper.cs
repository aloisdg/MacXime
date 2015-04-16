using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MacXime
{
	// http://stackoverflow.com/a/336729/1248177

	public static class ProcessorHelper
	{
		static readonly bool Is64BitProcess = (IntPtr.Size == 8);
		static bool _is64BitOperatingSystem = Is64BitProcess || InternalCheckIsWow64();

		[DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool IsWow64Process(
		    [In] IntPtr hProcess,
		    [Out] out bool wow64Process
		);

		public static bool InternalCheckIsWow64()
		{
			if ((Environment.OSVersion.Version.Major != 5 || Environment.OSVersion.Version.Minor < 1)
				&& Environment.OSVersion.Version.Major < 6) return false;
			using (var p = Process.GetCurrentProcess())
			{
				bool retVal;
				return IsWow64Process(p.Handle, out retVal) && retVal;
			}
		}
	}
}
