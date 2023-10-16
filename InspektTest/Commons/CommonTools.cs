/// <summary>
/// Klassen für allgemein brauchbare Tools
/// </summary>
///
/// Klasse NetCommons enthält
///		- Methoden für .Net Version
/// Klasse Tools enthält
///		- ENUM TOOLS, DATE & TIME TOOLS, Conversions, Embedded Resources ...
/// Hamann / Visutronik GmbH
/// 2015-11-06
/// 2023-01-06 add GetRuntimeInformation(), IsWOW64(), chg Namespace
/// 

using System;
using System.Globalization;     // CultureInfo
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Reflection;        // embedded resources
using System.Runtime.InteropServices;


namespace Visutronik.Commons
{

	#region === .NET Version Tools ===

	static class NetCommons
	{
		/// <summary>
		/// Get some runtime information from .NET
		/// </summary>
		public static string GetRuntimeInformation()
		{
			StringBuilder sb = new StringBuilder("RuntimeInformation:\n");
			string crlf = Environment.NewLine;
			sb.Append("  OSDescription        = " + RuntimeInformation.OSDescription + crlf);
			sb.Append("  OSArchitecture       = " + RuntimeInformation.OSArchitecture + crlf);
			sb.Append("  FrameworkDescription = " + RuntimeInformation.FrameworkDescription + crlf);
			sb.Append("  ProcessArchitecture  = " + RuntimeInformation.ProcessArchitecture + crlf);
			string info = sb.ToString();
			Debug.WriteLine(info);
			return info;
		}

        /// <summary>
        /// Get installed .NET Versions from registry
        /// </summary>
        /// <returns>Collection<Version></returns>
        public static System.Collections.ObjectModel.Collection<Version> InstalledDotNetVersions()
		{
			System.Collections.ObjectModel.Collection<Version> versions = new System.Collections.ObjectModel.Collection<Version>();
			Microsoft.Win32.RegistryKey NDPKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP");
			if (NDPKey != null)
			{
				string[] subkeys = NDPKey.GetSubKeyNames();
				foreach (string subkey in subkeys)
				{
					GetDotNetVersion(NDPKey.OpenSubKey(subkey), subkey, versions);
					GetDotNetVersion(NDPKey.OpenSubKey(subkey).OpenSubKey("Client"), subkey, versions);
					GetDotNetVersion(NDPKey.OpenSubKey(subkey).OpenSubKey("Full"), subkey, versions);
				}
			}
			return versions;
		}

		private static void GetDotNetVersion(Microsoft.Win32.RegistryKey parentKey, string subVersionName, System.Collections.ObjectModel.Collection<Version> versions)
		{
			if (parentKey != null)
			{
				string installed = Convert.ToString(parentKey.GetValue("Install"));
				if (installed == "1")
				{
					string version = Convert.ToString(parentKey.GetValue("Version"));
					if (string.IsNullOrEmpty(version))
					{
						if (subVersionName.StartsWith("v"))
							version = subVersionName.Substring(1);
						else
							version = subVersionName;
					}

					Version ver = new Version(version);

					if (!versions.Contains(ver))
						versions.Add(ver);
				}
			}
		}

		#region --- misc ---

		[DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool IsWow64Process(IntPtr hProcess, out bool lpSystemInfo);
		// C++: BOOL IsWow64Process([in] HANDLE hProcess, [out] PBOOL Wow64Process);

		[DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool IsWow64Process2(IntPtr hProcess, out ushort processMachine, out ushort nativeMachine);
		// C++: BOOL IsWow64Process2( [in] HANDLE hProcess,  [out] USHORT* pProcessMachine,  [out, optional] USHORT* pNativeMachine);

		// C++: _Post_equals_last_error_ DWORD GetLastError();
		[DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		[return: MarshalAs(UnmanagedType.U8)]
		public static extern ulong GetLastError();

		[DllImport("kernel32.dll")]
		private static extern IntPtr GetCurrentProcess();

		/// <summary>
		/// 
		/// </summary>
		/// <returns>True if WOW64 subsystem for 32bit App is used</returns>
		public static bool IsWOW64()
		{
			// WOW64(Windows - On - Windows 64 - bit) ist ein Subsystem des Windows-Betriebssystems,
			// das in der Lage ist, 32-Bit-Anwendungen auszuführen.
			// WOW64 ist in allen 64-Bit-Versionen von Windows seit Windows 2000 und Windows XP enthalten.
			// WOW64 berücksichtigt sämtliche Unterschiede zwischen 32-Bit- und 64-Bit-Windows,
			// insbesondere strukturelle Änderungen an Windows selbst. 

			bool retVal;
			IntPtr pProcess = GetCurrentProcess();
			//if (!IsWow64Process(Process.GetCurrentProcess().Handle, out retVal))
			if (IsWow64Process(pProcess, out retVal))
			{
				// retval:
				// A pointer to a value that is set to TRUE if the process is running under WOW64 on an Intel64 or x64 processor.
				// If the process is running under 32-bit Windows, the value is set to FALSE.
				// If the process is a 32-bit application running under 64-bit Windows 10 on ARM, the value is set to FALSE.
				// If the process is a 64-bit application running under 64-bit Windows, the value is also set to FALSE.
				Debug.WriteLine($"IsWow64Process(): {retVal}");
				// IsWow64Process(): False
			}
			else
			{
				ulong err = GetLastError();
				Debug.WriteLine("IsWow64Process(): LastError = " + err);
			}

			ushort processMachine, nativeMachine;
			if (IsWow64Process2(pProcess, out processMachine, out nativeMachine))
			{
				Debug.WriteLine($"IsWow64Process2(): 0x{processMachine:X4}, 0x{nativeMachine:X4}");
				// IsWow64Process2(): 0 (IMAGE_FILE_MACHINE_UNKNOWN), 34404
				// IsWow64Process2(): 0x0000, 0x8664 (AMD64)
				// pProcessMachine returns a pointer to an IMAGE_FILE_MACHINE_*value.
				//   The value will be IMAGE_FILE_MACHINE_UNKNOWN if the target process is not a WOW64 process;
				//   otherwise, it will identify the type of WoW process.
				// pNativeMachine returns a pointer to a possible IMAGE_FILE_MACHINE_* value identifying the
				//   native architecture of host system.
			}
			else
			{
				ulong err = GetLastError();
				Debug.WriteLine("IsWow64Process2(): LastError = " + err);
			}
			return retVal;
		}

		#endregion

	}

    #endregion

    #region === Common Tools ===	

    public static class Tools
	{
		#region === ENUM TOOLS ===

		// Test, ob Enumeration einen int-Wert enthält
		public static bool EnumContains(int intval, Enum enumval)
		{
			int b = Convert.ToInt32(enumval);
			return ((intval & b) == b);
		}

		public static int EnumToInt(Enum enumval)
		{
			return Convert.ToInt32(enumval);
		}

		#endregion

		#region === DATE & TIME TOOLS ===

		// Umwandlung string -> DateTime
		// Achtung: wirft Ausnahme bei Fehler!!!
		public static DateTime ParseDateTimeStringDE(string DTString)
		{
			CultureInfo MyCultureInfo = new CultureInfo("de-DE");
			//DTString = "12 Juni 2008";
			DateTime myDateTime;
			DateTimeStyles styles = DateTimeStyles.AssumeLocal;
			try
			{
				if (!DateTime.TryParse(DTString, MyCultureInfo, styles, out myDateTime))
				{
					throw (new Exception("ERROR parseDateTime(" + DTString + ")"));
				}
			}
			catch(Exception ex)
			{
				throw new Exception("DateTime parsing exception:", ex);	// weiterreichen ...
			}
			return myDateTime;
		}


		/// <summary>
		/// Gibt aktuelle Zeit als formatierten String zurück (24h):
		/// </summary>
		/// <returns>date & time string with format "yyyy-MM-dd_HH-mm-ss"</returns>
		public static string GetCurrentDateTimeString()
        {
        	//return DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        	return DateTime.Now.ToString("yyyy.MM.dd HH:mm");
        }

		/// <summary>
		/// Gibt aktuelle Zeit als formatierten String zurück (24h):
		/// </summary>
		/// <returns>time string with format "HH:mm"</returns>
		public static string GetCurrentTimeString()
        {
        	//return DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        	return DateTime.Now.ToString("HH:mm");
        }
  
		
		#endregion
	
		#region === CONVERSION TOOLS ===

		/// <summary>
		/// Wandelt String in Double um mit Default falls es nicht möglich ist
		/// </summary>
		/// <param name="In"></param>
		/// <param name="Default"></param>
		/// <returns></returns>
		/// http://dotnet-snippets.de/snippet/string-in-double-umwandeln-mit-invariantculture/839
		///
		public static double ToDouble(string In, double Default)
		{
			double dblOut;
			In = In.Replace(",", ".");
			try
			{
				dblOut = double.Parse(In, System.Globalization.CultureInfo.InvariantCulture);
			}
			catch
			{
				dblOut = Default;
			}
			return dblOut;
		}
		
		#endregion === CONVERSION TOOLS ===
		
		#region === MATH TOOLS ===
		
		/// <summary>
		/// Vergleich von Gleitkomma-Zahlenwerten
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="epsilon"></param>
		/// <returns></returns>
		/// aus: http://www.mycsharp.de/wbb2/thread.php?threadid=71995
		/// 
		public static bool NearlyEqual(double a, double b, double epsilon)
		{
		    if (a == b) // shortcut, handles infinities
		        return true;

		    double diff = Math.Abs(a - b);
		    if (a == 0 || b == 0)
    		{	
        		// a or b is zero
        		// relative error is less meaningful here
        		return diff < epsilon;
    		}
		    else
		    {
        		double absA = Math.Abs(a);
		        double absB = Math.Abs(b);
        		// use relative error
        		return (diff / (absA + absB)) < epsilon;
		        // alternativ stattdessen Math.Max(absA, absB)
    		}
		}
		
		
		// swap objects:
		public static void Swap<T>(ref T val1, ref T val2)
		{
			T tmp = val1; val1 = val2; val2 = tmp;
		}

		// swap integers if val1 greater val2
		private static void SwapIfGreaterInt(ref int val1, ref int val2)
		{
			if (val1 > val2)
			{
				int tmp = val1; val1 = val2; val2 = tmp;
			}
		}
		
		// swap doubles if val1 greater val2
		private static void SwapIfGreaterDbl(ref double val1, ref double val2)
		{
			if (val1 > val2)
			{
				double tmp = val1; val1 = val2; val2 = tmp;
			}
		}
		#endregion
	
		#region === Statistics Tools ===

		private static double Mittelwert(double[] values)
		{
			double mw = 0;
			int l = values.Length;
			if (l > 0)
			{
				for (int i = 0; i < l; i++)
				{
					mw += values[i];	// Summe der Werte
				}
				mw = mw / (double) l;
			}
			return mw;
		}

		private static double Varianz(double[] values, double mw)
		{
			double var = 0;
			int l = values.Length;
			if (l > 1)
			{
				for (int i = 0; i < l; i++)
				{
					var += ((values[i] - mw) * (values[i] - mw));	// Summe der quadratischen Abstände
				}
				var = var / (double) l;
			}
			return var;
		}
		
		#endregion
	
		#region === Embedded Resources Tools ===

		/*
		So fügen Sie dem Projekt ein Bild als eingebettete Ressource hinzu:
	    Klicken Sie im Menü Projekt auf Vorhandenes Element hinzufügen.
    	Navigieren Sie zu dem Bild, das Sie dem Projekt hinzufügen möchten. 
		Klicken Sie auf die Schaltfläche Öffnen, um das Bild in die Projektdateiliste einzufügen.
	    Klicken Sie in der Projektdateiliste mit der rechten Maustaste auf das Bild, und wählen Sie Eigenschaften. 
		Das Eigenschaftenfenster wird angezeigt.
    	Suchen Sie im Eigenschaftenfenster die Build Action-Eigenschaft. 
		Ändern Sie ihren Wert in Eingebettete Ressource.
		Eventuell fügen Sie einen Resourcennamen zu...
		Sie können das Projekt jetzt erstellen. Das Bild wird in die Assembly Ihres Projekts kompiliert.
		*/
	
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		/// http://www.csharp411.com/embedded-image-resources/
		///	Stream imageStream = myAssembly.GetManifestResourceStream("Logo");
		///	pictureBox1.Image = new Bitmap(imageStream);
		///	imageStream.Dispose();
		/// 
		static string[] GetEmbeddedResourceNames()
		{
			Debug.WriteLine("GetEmbeddedResourceNames()");
		
			Assembly myAssembly = Assembly.GetExecutingAssembly();
			string[] names = myAssembly.GetManifestResourceNames();
			foreach (string name in names)
			{
	   			Debug.WriteLine("Embedded ManifestResource: " + name);
			}
			return names;
		}
		
		#endregion

	}
	
	#endregion
	
}
