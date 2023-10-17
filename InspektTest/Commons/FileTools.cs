//============================================================================================================
// Klasse für Zugriff auf Verzeichnisse und Dateien
// Hamann / Visutronik GmbH
//============================================================================================================
// 30.01.2023

// 2023-01-27	fix Exception in CheckFileExist, wenn strFilePath leer ist
// 2023-01-30	add EnsureFolderExistAsync
//
// http://stackoverflow.com/questions/726602/how-to-prevent-timeout-when-inspecting-unavailable-network-share-c-sharp


using System;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Visutronik.Commons
{
	/// <summary>
	/// Description of FileTools.
	/// </summary>
	public static class FileTools
	{
		// ctor
		//public FileTools()				{ }

        #region === FOLDER ACCESS TOOLS ===

        /// <summary>
        /// Prüft mit Zeitüberwachung, ob eine Datei existiert
        /// </summary>
        /// <param name="strFilePath">Datei-Pfad</param>
        /// <param name="waittime">Wartezeit in ms</param>
        /// <returns>true wenn Datei existiert</returns>
        public static bool CheckFileExist(string strFilePath, int waittime = 200)
		{
            if (strFilePath == "") return false;
            
			// clip the waittime 
            if ((waittime < 10) || (waittime > 10000)) waittime = 100;
			//
			var task = new Task<bool>(() => 
			{
				try
				{
					var fi = new System.IO.FileInfo(strFilePath);
					return fi.Exists;
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine("CheckFileExist(): " + ex.Message);
					return false;
				}
			});	// uri.LocalPath
			task.Start();
			bool taskresult = task.Wait(waittime) && task.Result;
			return taskresult;
		}


		/// <summary>
		/// Prüft mit Zeitüberwachung, ob ein Verzeichnis existiert
		/// </summary>
		/// <param name="strFolder">Verzeichnis-Pfad</param>
		/// <param name="waittime">Wartezeit in ms</param>
		/// <returns>true wenn Verzeichnis existiert</returns>
		public static bool CheckFolderExist(string strFolder, int waittime = 200)
		{
			// clip the waittime 
			if ((waittime < 10) || (waittime > 10000)) waittime = 100;
			//
			var task = new Task<bool>(() => 
			{
			   	//var fi = new System.IO.FileInfo(strFolder);
			   	//return fi.Exists;
			   	var di = new System.IO.DirectoryInfo(strFolder);
			   	return di.Exists;
			});	// uri.LocalPath
			task.Start();
			bool taskresult = task.Wait(waittime) && task.Result;
			return taskresult;
		}


        /// <summary>
        /// Erzeugt bei Bedarf den angegebenen Ordner (async)
        /// </summary>
        /// <param name="strFolder">Ordner-Pfad</param>
        /// <param name="waittime">Timeout [ms]</param>
        /// <returns>true wenn Ordner existiert</returns>
        public static bool EnsureFolderExistAsync(string strFolder, int waittime = 200)
        {
            // clip the waittime
            if ((waittime < 10) || (waittime > 10000)) waittime = 100;
            //
            var task = new Task<bool>(() =>
            {
                bool result = true;
                try
                {
                    if (!System.IO.Directory.Exists(strFolder))
                    {
                        System.IO.DirectoryInfo di = System.IO.Directory.CreateDirectory(strFolder);
                        Debug.WriteLine(di.FullName + " created.");
                    }
                    else
                    {
                        Debug.WriteLine("Directory exists: " + strFolder);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("EnsureFolderExistAsync(): " + ex.Message);
                    result = false;
                }
                return result;
            });
            task.Start();
            bool taskresult = task.Wait(waittime) && task.Result;
            return taskresult;
        }


        /// <summary>
        /// Erzeugt bei Bedarf den angegebenen Ordner
        /// </summary>
        /// <param name="strFolder">Ordner-Pfad</param>
        /// <returns>true wenn Ordner existiert</returns>
        public static bool EnsureFolderExist(string strFolder)
		{
			try
			{
				if (!System.IO.Directory.Exists(strFolder))
				{
					System.IO.DirectoryInfo di = System.IO.Directory.CreateDirectory(strFolder);
                    Debug.WriteLine("EnsureFolderExist: " + di.FullName + " created.");
                }
            }
			catch (Exception ex)
			{
                Debug.WriteLine("EnsureFolderExist: " + ex.Message);
                return false;
			}
			return true;
		}
		
		#endregion

		#region === FOLDERNAME AND FILENAME TOOLS ===
		
		/// <summary>
		/// Konvertiert ungültige in gültige Dateinamen
		/// </summary>
		/// <param name="strInvalidName">roher Dateiname</param>
		/// <returns>gültiger Dateiname</returns>
		public static string GetValidFileName(string strInvalid)
		{
			string strValid = strInvalid;
			char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
			int pos;
			while ((pos = strValid.IndexOfAny(invalidChars)) > -1)
			{
				strValid = strValid.Replace(strValid[pos], '_');
			}
			Debug.WriteLine("  invalid filename :" + strInvalid);
			Debug.WriteLine("  valid filename   :" + strValid);
			return strValid;
		}

		/// <summary>
		/// Konvertiert ungültiges in gültiges Verzeichnis
		/// </summary>
		/// <param name="strInvalidName">roher Verzeichnisname</param>
		/// <returns>gültiger Verzeichnisname</returns>
		public static string GetValidPath(string strInvalid)
		{
			string strValid = strInvalid;
			char[] invalidChars = System.IO.Path.GetInvalidPathChars();
			int pos;			
			while ((pos = strValid.IndexOfAny(invalidChars)) > -1)
			{
				strValid = strValid.Replace(strValid[pos], '_');
			}
			Debug.WriteLine("  invalid path :" + strInvalid);
			Debug.WriteLine("  valid path   :" + strValid);
			return strValid;
		}
		
		#endregion
	}
	
}
