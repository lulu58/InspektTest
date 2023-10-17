/*
 * Program settings class
 * Visutronik/Hamann
 */

using System;
using System.ComponentModel;			// wichtig für Benutzung PropertyGrid!
using System.IO;
using System.Diagnostics;
using Visutronik.Commons;               // XmlAppSettings
using System.Xml;


namespace Visutronik.InspektTest
{
	/// <summary>
	/// Singleton-Klasse für benutzerdefinierte Einstellungen BKP_LinerMessung
	/// </summary>
	public sealed class ProgSettings
	{
		#region ----- constants ------

		private const string XML_SETTINGSFILE = "ProgSettings.xml";
		//private const string SQL_CONNECTFILE  = "DatabaseParameter.dat";

		#endregion

		#region ----- properties -----

		/// <summary>
		/// Verzeichnis für Logdateien (Programm, DB, Profildateien)
		/// </summary>
		[Description("Logdateiverzeichnis"), Category("Dateien")]
		public string LogFolder { get; set; } = @"D:\Temp\JUS\Log";

        /// <summary>
        /// Verzeichnis für Logdateien (Programm, DB, Profildateien)
        /// </summary>
        [Description("Instruktionsverzeichnis"), Category("Dateien")]
        public string InstructionFolder { get; set; } = @"D:\Temp\JUS\Instruktion";

        /// <summary>
        /// Verzeichnis für Bilddateien (Programm, DB, Profildateien)
        /// </summary>
        [Description("Bildverzeichnis"), Category("Dateien")]
        public string ImageFolder { get; set; } = @"D:\Temp\JUS\Kamerabilder";

        [Description("Letztes Bild"), Category("Dateien")]
        public string LastImage { get; set; } = "";

        [Description("Letzte Liste"), Category("Dateien")]
        public string LastInstruction { get; set; } = "";


        // --- Window size and location ---
        [Description("Fullscreenmodus"), Category("Allgemein")]
		public bool Fullscreen { get; set; }

		[Description("WindowSize"), Category("Allgemein")]
		public int WindowSizeX { get; set; }
		public int WindowSizeY { get; set; }

		[Description("WindowLocation"), Category("Allgemein")]
		public int WindowLocX { get; set; }
		public int WindowLocY { get; set; }

		// ---------------------------------


		// --- Hardware ------------------------------

		/// <summary>
		/// COM-Port des EXDUL-342 DIO Moduls
		/// </summary>
		[Description("IO-Modul-Port"), Category("Hardware")]
		public string DIO_Port { get; set; } = "COM1";

		//-----------------------------------------------------------------------
		// Freischaltung von ungetestetem Programmcode
		//-----------------------------------------------------------------------

		[Description("TestMode1"), Category("Anlage")]
		public bool TestMode1 { get; set; } = false;

		[Description("TestMode2"), Category("Anlage")]
		public bool TestMode2 { get; set; } = true;

		[Description("TestMode3"), Category("Anlage")]
		public bool TestMode3 { get; set; } = false;


		#endregion

		#region ----- internal vars -----------

		public string strMessage = "";
        private string AppSettingsPath = "";
		private string XmlSettingsFile = "";


		// Instantiierung
		private static ProgSettings _instance = new ProgSettings();
		public static ProgSettings Instance { get { return _instance; } }

		#endregion

		#region ----- methods --------

		/// <summary>
		/// Dateipfad setzen
		/// </summary>
		/// <param name="path">Path to settings file or empty for default path</param>
		public bool SetSettingsPath(string path = "")
		{
			//if (path.Length == 0)	AppSettingsPath = DEFAULT_PATH;
			//else					AppSettingsPath = path;
			AppSettingsPath = path;
			Debug.WriteLine("  settings path = " + AppSettingsPath);

			XmlSettingsFile = Path.Combine(AppSettingsPath, XML_SETTINGSFILE);
			return true;
		}


		/// <summary>
		/// Programmeinstellungen aus Datei laden
		/// </summary>
		/// <returns><c>true</c> bei Erfolg</returns>
		public bool LoadSettings()
		{
			Debug.WriteLine("--- ProgSettings.LoadSettings() " + XmlSettingsFile + " ---");

			XmlAppSettings xmlset = null;
			bool result = true;

			try
			{
				xmlset = new XmlAppSettings(XmlSettingsFile, false);
				LogFolder		= xmlset.Read("Log-Verzeichnis", LogFolder);
                InstructionFolder = xmlset.Read("Prüfanweisungs-Verzeichnis", InstructionFolder);
                ImageFolder     = xmlset.Read("Bild-Verzeichnis", ImageFolder);
                LastImage       = xmlset.Read("LetztesBild", LastImage);
                LastInstruction = xmlset.Read("LetztesListe", LastInstruction);

                // Window               
                Fullscreen = xmlset.Read("Fullscreen", this.Fullscreen);
				WindowSizeX 	= xmlset.Read("WindowSizeX", this.WindowSizeX);
				WindowSizeY 	= xmlset.Read("WindowSizeY", this.WindowSizeY);
				WindowLocX 		= xmlset.Read("WindowPosX", this.WindowLocX);
				WindowLocY 		= xmlset.Read("WindowPosY", this.WindowLocY);

				DIO_Port		= xmlset.Read("DIO-Port", this.DIO_Port);

				TestMode1		= xmlset.Read("TestMode1", TestMode1);
				TestMode2		= xmlset.Read("TestMode2", TestMode2);
				TestMode3		= xmlset.Read("TestMode3", TestMode3);
			}
			catch (System.IO.FileNotFoundException fnfex)
            {
                this.strMessage = fnfex.Message;
				result = false;
			}
			catch (Exception ex)
			{
				this.strMessage = ex.Message;
				result = false;
			}
			return result;
		}


		/// <summary>
		/// Programmeinstellungen in Datei speichern
		/// </summary>
		/// <returns><c>true</c> bei Erfolg</returns>
		public bool SaveSettings()
		{
			Debug.WriteLine("--- ProgSettings.Save() " + XmlSettingsFile + " ---");
			bool result = true;
			XmlAppSettings xmlset = null;

			try
			{
				xmlset = new XmlAppSettings(XmlSettingsFile, false);

				// Dateipfade	
				xmlset.Write("Log-Verzeichnis", this.LogFolder);
                xmlset.Write("Prüfanweisungs-Verzeichnis", this.InstructionFolder);
                xmlset.Write("Bild-Verzeichnis", this.ImageFolder);
                xmlset.Write("LetztesBild", LastImage);
                xmlset.Write("LetztesListe", LastInstruction);

                //Window
                xmlset.Write("Fullscreen", this.Fullscreen);
				xmlset.Write("WindowSizeX", this.WindowSizeX);
				xmlset.Write("WindowSizeY", this.WindowSizeY);
				xmlset.Write("WindowPosX", this.WindowLocX);
				xmlset.Write("WindowPosY", this.WindowLocY);

				// Anlage
				xmlset.Write("DIO-Port", this.DIO_Port);

				xmlset.Write("TestMode1", TestMode1);
				xmlset.Write("TestMode2", TestMode2);
				xmlset.Write("TestMode3", TestMode3);

				xmlset.Save();
			}
			catch (System.IO.FileNotFoundException fnfex)
			{
				this.strMessage = fnfex.Message;
				result = false;
			}
			catch (Exception ex)
			{
				this.strMessage = ex.Message;
				result = false;
			}
			return result;
		}

		
		/// <summary>
		/// Programmeinstellungen im Debugfenster ausgeben:
		/// </summary>
        public void DebugSettings()
        {
			Debug.WriteLine("--- DEBUG Benutzereinstellungen ---");

			Debug.WriteLine("Logdatei-Verzeichnis :      " + LogFolder);
            Debug.WriteLine("Prüfanweisungs-Verzeichnis: " + InstructionFolder);
            Debug.WriteLine("Bild-Verzeichnis:           " + ImageFolder);
            Debug.WriteLine("Letztes Bild:               " + LastImage);
            Debug.WriteLine("Letzte Liste:               " + LastInstruction);

            Debug.WriteLine("Fullscreen : " + this.Fullscreen);
			Debug.WriteLine("WindowSizeX: " + this.WindowSizeX);
			Debug.WriteLine("WindowSizeY: " + this.WindowSizeY);
			Debug.WriteLine("WindowPosX : " + this.WindowLocX);
			Debug.WriteLine("WindowPosY : " + this.WindowLocY);

			Debug.WriteLine("DIO-Port   : " + this.DIO_Port);

			Debug.WriteLine("TestMode1  : " + this.TestMode1);
			Debug.WriteLine("TestMode2  : " + this.TestMode2);
			Debug.WriteLine("TestMode3  : " + this.TestMode3);

			Debug.WriteLine("");
			Debug.WriteLine("--------------------------------");
		}

		#endregion

		#region ----- private methods -----

		/// <summary>
		/// Konstruktion / setzt defaults
		/// </summary>
		private ProgSettings()
		{
			// Defaults setzen

			// Window
			this.Fullscreen = false;
			this.WindowSizeX = 1200;
			this.WindowSizeY = 800;
			this.WindowLocX = 10;
			this.WindowLocY = 10;

			// Hardware
			DIO_Port = "COM1";

			TestMode1 = false;
			TestMode2 = false;
			TestMode3 = false;
		}

		#endregion

	}
}
