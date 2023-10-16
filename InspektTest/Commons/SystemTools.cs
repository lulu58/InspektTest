/*
 * Projekt  : Allgemein
 * Datei    : SystemTools.cs - Systemfunktionen Win32 API, ...
 * Benutzer : Visutronik / Hamann
 * 
 * http://www.nullskull.com/a/777/net-lock-logoff-reboot-shutdown-hibernate-standby.aspx.
 * https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-exitwindowsex
 * https://social.msdn.microsoft.com/Forums/vstudio/en-US/2a8d9c82-08b2-4a64-97fd-fc9a4d9d59b5/how-to-shutdown-and-restart-windows-from-within-a-c-program?forum=csharpgeneral
 * 
 * auch:
 * System.Diagnostics.Process.Start("ShutDown", "/s");	// to shutdown
 * System.Diagnostics.Process.Start("ShutDown", "/r");	// to restart
 * 
 * List of all arguments are as following:
 *		-r		Shutdown and restart the computer
 *		-s		Shutdown the computer
 *		-t xx	Set timeout for shutdown to xx seconds
 *		-a		Abort a system shutdown
 *		-f		Forces all windows to close
 *		-i		Display GUI interface
 *		-l		Log off
 * 
 * 08.05.2019	initial
 * 				add Lock, Shutdown, Reboot, Hibernate, Standby for the workstation 
 * 15.05.2019	add	GetComputerName(), GetIPV4Addresses()
 * 07.07.2020	add Shutdown()
 * 30.09.2020	add SetCueText()
 * 03.02.2023	add GetPowerStatus()
 */

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Net;
using System.Collections.Generic;
using System.Diagnostics;


namespace Visutronik.Commons
{
	/// <summary>
	/// Description of SystemTools.
	/// </summary>
	public class SystemTools
	{
		public SystemTools()
		{
		}

		[DllImport("user32.dll")]
		public static extern void LockWorkStation();
		
		// flags (logoff, shutdown, reboot, etc.,)
		// reason for this action(maintenance, software update, etc.,)
		[DllImport("user32.dll")]
		static extern int ExitWindowsEx(int uFlags, int dwReason);

		//Imports the user32.dll
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam);

        [DllImport("kernel32.dll")]
        public static extern bool GetSystemPowerStatus(ref SystemPowerStatus systemPowerStatus);


        // Log Off
        // Shuts down all processes running in the logon session of the process that called the ExitWindowsEx function.
        // Then it logs the user off.
        // This flag can be used only by processes running in an interactive user's logon session.
        public static void LogOff()
		{
			ExitWindowsEx(0, 0);
		}
		
		public static void Shutdown()
		{
			ExitWindowsEx(1, 0);
		}

		public static void Reboot()
		{
			ExitWindowsEx(2, 0);
		}

		// To force processes to terminate while logging off, change the flag to 4 in the function as below
		public static void LogOffForced()
		{
			ExitWindowsEx(4, 0);
		}

		// Shuts down the system and turns off the power. The system must support the power-off feature.
		// The calling process must have the SE_SHUTDOWN_NAME privilege. For more
		public static void PowerOff()
		{
			ExitWindowsEx(8, 0);
		}
		
		// ===============================================================================================
		// Hibernate and Standby
		// ===============================================================================================
		// To put the system in hibernate and standby modes, we are going to use Application class's SetSuspendState method.
		// There are three arguments for this function, power state, force and disable wake event.
		// The first argument, power state is where we mention the state of the system (hibernate/suspend)
		// https://docs.microsoft.com/de-de/dotnet/api/system.windows.forms.application.setsuspendstate?view=netframework-4.8
		// using System.Windows.Forms;
		
		// force
		// true: 	Standbymodus sofort erzwingen
		// false: 	Windows sendet an jede Anwendung eine Unterbrechungsanforderung
		const bool force = true;
		
		// disableWakeEvent
		// true: 	Wiederherstellung eines aktiven Systemenergiezustands wird deaktiviert
		// false: 	Wiederherstellung eines aktiven Systemenergiezustands wird aktiviert
		const bool disableWakeEvent = true;
		
		public static void Hibernate()
		{
			Application.SetSuspendState(PowerState.Hibernate, force, disableWakeEvent);
		}
		
		public static void Standby()
		{
			Application.SetSuspendState(PowerState.Suspend, force, disableWakeEvent);
		}


        public struct SystemPowerStatus
        {
            public byte ACLineStatus;		// -> enumACLineStatus
            public byte batteryFlag;		// -> enumBatteryFlag
            public byte batteryLifePercent;
            public byte reserved1;
            public int batteryLifeTime;
            public int batteryFullLifeTime;
        }

        enum enumACLineStatus : byte
        {
            Offline = 0,
            Online = 1,
            Unknown = 255,
        }

        enum enumBatteryFlag : byte
        {
            High = 1,
            Low = 2,
            Critical = 4,
            Charging = 8,
            NoSystemBattery = 128,
            Unknown = 255,
        }

        /*
        public SystemPowerStatus powerStatus = new Visutronik.Commons.SystemTools.SystemPowerStatus();
      	/// <summary>
      	/// PowerStatus der Maschine ermitteln
      	/// </summary>
      	private void YouGotThePowerStatus()
      	{
      		int freq = 1000;
      		int dur = 1000;
      		if (GetSystemPowerStatus(ref powerStatus))
      		{
      			Debug.WriteLine(powerStatus.ACLineStatus.ToString());
      			switch (powerStatus.ACLineStatus)
      			{
      				case (byte) enumACLineStatus.Online:
      					listbox1.Items.Add("Power online - Netzbetrieb");
      					freq = 400; 	dur = 1000; Beep(freq, dur);	
      					break;

      				case (byte) enumACLineStatus.Offline:
	      				listbox1.Items.Add("Power offline - Batteriebetrieb");
      					freq = 1600; 	dur = 1000; Beep(freq, dur);	
      					break;

      				case (byte) enumACLineStatus.Unknown:
	      				listbox1.Items.Add("Power-Status unbekannt");
      					DoTheBeep(); 
      					break;
      			}
      		}
      		else
      		{
      			listbox1.Items.Add("Power-Status Fehler!!!");
      		}
      	}

		*/

        // ===============================================================================================
        // Konfigurationsdaten des Computers
        // ===============================================================================================

        // Ermittelt Maschinennamen des lokalen Computers
        public static string GetComputerName()
		{
			string machinename = Environment.MachineName; //"my Computer";
			System.Diagnostics.Debug.WriteLine("This computer's name is: " + machinename);
			return machinename;
		}
			
		// Ermittelt alle IP-Adressen IPV4 des lokalen Computers
		// Dns.GetHostAddresses(Environment.MachineName)[0].ToString();
		public static List<string> GetIPV4Adresses()
		{
			// etwas besser:
			IPAddress[] ipaddresses = Dns.GetHostAddresses(Environment.MachineName);
			List<string> ipV4Addr = new List<string>();
			
			foreach(IPAddress ip in ipaddresses)
			{
				System.Diagnostics.Debug.WriteLine(ip.AddressFamily);
				System.Diagnostics.Debug.WriteLine(ip.ToString());
				
				if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
				{
					ipV4Addr.Add(ip.ToString());
					Debug.WriteLine("IP V4 address: " + ip);
				}
			}
			return ipV4Addr;
		}

		#region --- PC Shutdown ---

		/// <summary>
		/// Shutdown the computer immediate
		/// https://stackoverflow.com/questions/102567/how-to-shut-down-the-computer-from-c-sharp
		/// https://dotnet-snippets.de/snippet/windows-herrunterfahren-ausloggen-neustarten/455
		/// </summary>
		public static void ProcessShutdown()
		{
			//shutdown: Der PC wird sofort heruntergefahren.
			//shutdown –s –t 60: Der PC wird in 60 Sekunden heruntergefahren.
			//shutdown –r: Der PC wird neugestartet.
			//shutdown –a: Das Herunterfahren wird abgebrochen.
			//shutdown –i: Der Remote-PC wird heruntergefahren.
			//shutdown –s: Der lokale PC wird heruntergefahren.
			//shutdown –l: Der aktuelle Benutzer wird abgemeldet.
			//shutdown –f: Laufende Anwendungen werden zum Schließen gezwungen.
			//shutdown -c "Kommentar": Es wird zusätzlich ein Kommentar vermerkt. 
			System.Diagnostics.Process.Start("shutdown", "/s /t 10");
		}

		#endregion

		/// <summary>
		/// Setzt Cue-Text für Textboxen - ab Windows Vista
		/// </summary>
		/// <param name="box"></param>
		/// <param name="cuetxt"></param>
		public static void SetCueText(System.Windows.Forms.TextBox box, string cuetxt)
        {
			SendMessage(box.Handle, 0x1500 + 1, IntPtr.Zero, cuetxt);
		}

	}
}
