/* Klassenbibliothek "TextLog"
 *  Stand: 08.07.2012
 *  Erstellt von: A. Pouwels (APouwels85[at]gmail[dot]com
 *  
 *  Ihr könnt die Klassenbibliothek gerne euren eigenen Wünschen anpassen!
 *  Bei Fragen oder Anregungen schreibt mir einfach eine Mail!
 *  
 * 2015-11-06 mod by Lulu:
 * 2023-01-06 mod by Lulu:
 *              complete refacturing
 *              append ist always default, 
 *              add LogItMonatlich(), add LogItMinutenweise(), chg LogToFile()
 */

using System;
using System.IO;
using System.Globalization;
using System.Diagnostics;

namespace Visutronik.Commons
{
    public enum FilenameTimeFormat
    {
        Monat, KW, Tag, Stunde, Minute
    };

    /// <summary>
    /// Class to write simple textlog files
    /// </summary>
    public class Logger
    {
        /// <summary>
        /// flag to enable / disable logging
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Flag to enable / disable debug output
        /// </summary>
        public bool Diag { get; set; } = false;

        /// <summary>
        /// file extension of logfile
        /// </summary>
        public string FileExtension { get; set; } = "log";

        /// <summary>
        /// error string
        /// </summary>
        public string LastError { get; set; } = "";

        private string _folder = Environment.CurrentDirectory;

        // --- add by Lulu ---
        private const string STR_MONTHLY_FORMAT = "yyyy-MM";
        private const string STR_YEARLY_FORMAT = "yyyy";
        private const string STR_DAILY_FORMAT = "yyyy-MM-dd";
        private const string STR_HOURLY_FORMAT = "yyyy-MM-dd_hh";
        private const string STR_MINUTELY_FORMAT = "yyyy-MM-dd_hh-mm";

        private readonly CultureInfo cultureInfo;
        private readonly Calendar calendar;
        private readonly CalendarWeekRule myCWR;
        private readonly DayOfWeek myFirstDOW;

        FilenameTimeFormat ftfmt = FilenameTimeFormat.Tag;

        /// <summary>
        /// ctor
        /// </summary>
        public Logger()
        {
            // Gets the DTFI properties required by GetWeekOfYear.
            cultureInfo = CultureInfo.CurrentCulture;
            calendar = cultureInfo.Calendar;
            myCWR = cultureInfo.DateTimeFormat.CalendarWeekRule;
            myFirstDOW = cultureInfo.DateTimeFormat.FirstDayOfWeek;

            if (Diag)
            {
                // Displays the number of the current week relative to the beginning of the year.
                Debug.WriteLine("The CalendarWeekRule used is {0}.", myCWR);        // FirstFourDayWeek
                Debug.WriteLine("The FirstDayOfWeek used is {0}.", myFirstDOW);     // Monday
                Debug.WriteLine("The current week is Week {0}.", calendar.GetWeekOfYear(DateTime.Now, myCWR, myFirstDOW));
            }
        }

        /// <summary>
        /// Set other log folder then current directory
        /// </summary>
        /// <param name="strFolder">folder path</param>
        /// <returns>true if successfull</returns>
        public bool SetFolder(string strFolder)
        {
            try
            {
                if (!Directory.Exists(strFolder)) Directory.CreateDirectory(strFolder);
                _folder = strFolder;
            }
            catch (Exception ex)
            {
                Debug.WriteLineIf(Diag, "Logger.SetFolder(): " + ex.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Setze Format für Zeitperiode in Dateinamen
        /// </summary>
        /// <param name="fmt"></param>
        public void SetFilenameTimeFormat(FilenameTimeFormat fmt)
        {
            ftfmt = fmt;
        }

        /// <summary>
        /// Erstellt eine Log-Datei im Log-Ordner für die aktuell festgelegte Periode
        /// und schreibt Nachricht mit Zeitangabe
        /// </summary>
        /// <param name="message"></param>
        /// <param name="append"></param>
        /// <returns></returns>
        public bool Log(string message, bool append = true)
        {
            bool result = true;
            if (Enabled)
            {
                DateTime dt = DateTime.Now;
                string datestr = STR_DAILY_FORMAT;
                switch (ftfmt)
                {
                    case FilenameTimeFormat.Monat: datestr = dt.ToString(STR_MONTHLY_FORMAT); break;
                    case FilenameTimeFormat.KW:
                        int currentWeek = calendar.GetWeekOfYear(dt, myCWR, myFirstDOW);
                        datestr = string.Format("{0:D4}_W{1:D2}", dt.Year, currentWeek);
                        break;
                    case FilenameTimeFormat.Tag: datestr = dt.ToString(STR_DAILY_FORMAT); break;
                    case FilenameTimeFormat.Stunde: datestr = dt.ToString(STR_HOURLY_FORMAT); break;
                    case FilenameTimeFormat.Minute: datestr = dt.ToString(STR_MINUTELY_FORMAT); break;
                }

                try
                {
                    string fileName = string.Format(@"{0}.{1}", datestr, FileExtension);
                    string path = Path.Combine(this._folder, fileName);
                    using (StreamWriter writer = new StreamWriter(path, append))
                    {
                        string line = string.Format($"{dt} - {message}");
                        writer.WriteLine(line);
                        writer.Flush();
                        writer.Close();
                    }
                }
                catch (Exception ex)
                {
                    LastError = "Logger.Log(): " + ex.Message;
                    Debug.WriteLineIf(Diag, LastError);
                    result = false;
                }
            }
            return result;
        }

        /// <summary>
        /// Erstellt eine Log-Datei im Log-Ordner für die aktuelle Kalenderwoche.
        /// </summary>
        /// <param name="message">Die zu speichernde Nachricht</param>
        /// <param name="append">Flag, ob Nachricht an vorhandene Datei angehängt werden soll</param>
        /// <returns><c>true</c> wenn erfolgreich, andernfalls <c>false</c></returns>
        public bool LogItWochenweise(string message, bool append = true)
        {
            if (!Enabled) return true;

            DateTime dt = DateTime.Now;
            int currentWeek = calendar.GetWeekOfYear(dt, myCWR, myFirstDOW);
            string datestr = dt.ToString("yyyy") + "_W" + currentWeek.ToString("D2");
            string fileName = string.Format(@"Log_{0}.txt", datestr);
            return LogToFile(message, fileName, append);
        }

        /// <summary>
        /// Erstellt eine Log-Datei im Log-Ordner mit aktueller Zeit in Minuten.
        /// Je nach Formatstring wird sekündlich / minütlich / stündlich eine neue Logdatei erzeugt
        /// </summary>
        /// <param name="message">Die zu speichernde Nachricht</param>
        /// <param name="append">Flag, ob Nachricht an vorhandene Datei angehängt werden soll</param>
        /// <returns><c>true</c> wenn erfolgreich, andernfalls <c>false</c></returns>
        public bool LogItMinutenweise(string message, bool append = true)
        {
            if (!Enabled) return true;
            DateTime dt = DateTime.Now;
            string datestr = dt.ToString(STR_MINUTELY_FORMAT);
            string fileName = string.Format(@"Log_{0}.txt", datestr);
            return LogToFile(message, fileName, append);
        }

        /// <summary>
        /// Erstellt eine Log-Datei im Log-Ordner mit aktueller Zeit.
        /// Je nach Formatstring wird sekündlich / minütlich / stündlich eine neue Logdatei erzeugt
        /// </summary>
        /// <param name="message">Die zu speichernde Nachricht</param>
        /// <param name="append">Flag, ob Nachricht an vorhandene Datei angehängt werden soll</param>
        /// <returns><c>true</c> wenn erfolgreich, andernfalls <c>false</c></returns>
        public bool LogItStundenweise(string message, bool append = true)
        {
            DateTime dt = DateTime.Now;
            string datestr = dt.ToString(STR_HOURLY_FORMAT);
            string fileName = string.Format(@"Log_{0}.txt", datestr);
            return LogToFile(message, fileName, append);
        }

        /// <summary>
        /// Erstellt eine Tages-Log-Datei im Log-Ordner.
        /// </summary>
        /// <param name="message">Die zu speichernde Nachricht</param>
        /// <param name="append">Flag, ob Nachricht an vorhandene Datei angehängt werden soll</param>
        /// <returns><c>true</c> wenn erfolgreich, andernfalls <c>false</c></returns>
        public bool LogIt(string message, bool append = true)
        {
            DateTime dt = DateTime.Now;
            string datestr = dt.ToString(STR_DAILY_FORMAT);
            string fileName = string.Format(@"Log_{0}.txt", datestr);
            return LogToFile(message, fileName, append);
        }

        /// <summary>
        /// Erstellt eine Monats-Log-Datei im Log-Ordner.
        /// </summary>
        /// <param name="message">Die zu speichernde Nachricht</param>
        /// <param name="append">Gibt an ob eine bereits vorhandene Log-Datei mit dem gleichen Namen überschrieben werden soll, oder ob der Text angehangen werden soll
        /// <c>true</c> wenn der Text zu der Log-Datei hinzugefügt werden soll, <c>false</c> wenn die bestehende Log-Datei überschrieben werden soll</param>
        /// <returns><c>true</c> wenn erfolgreich, andernfalls <c>false</c></returns>
        public bool LogItMonatlich(string message, bool append = true)
        {
            DateTime dt = DateTime.Now;
            string datestr = dt.ToString(STR_MONTHLY_FORMAT);
            string fileName = string.Format(@"Log_{0}.txt", datestr);
            return LogToFile(message, fileName, append);
        }

        /// <summary>
        /// Erstellt eine Log-Datei mit einem bestimmten Namen im Log-Ordner
        /// </summary>
        /// <param name="message">Die zu speichernde Nachricht</param>
        /// <param name="logFileName">Name der Log-Datei</param>
        /// <param name="append">Flag, ob Nachricht an vorhandene Datei angehängt werden soll</param>
        /// <returns><c>true</c> wenn erfolgreich, andernfalls <c>false</c></returns>
        public bool LogToFile(string message, string fileName, bool append = true)
        {
            return LogToFile(message, this._folder, fileName, append);
        }

        /// <summary>
        /// Erstellt eine Log-Datei mit einem bestimmten Namen in einem bestimmten Ordner
        /// </summary>
        /// <param name="message">Die zu speichernde Nachricht</param>
        /// <param name="folder">Pfad des Ordner in welchem die Log-Datei erstellt werden soll</param>
        /// <param name="fileName">Name der Log-Datei</param>
        /// <param name="append">Flag, ob Nachricht an vorhandene Datei angehängt werden soll</param>
        /// <returns><c>true</c> wenn erfolgreich, andernfalls <c>false</c></returns>
        public bool LogToFile(string message, string folder, string fileName, bool append = true)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(string.Format(@"{0}\{1}", folder, fileName), append))
                {
                    writer.WriteLine(message);
                    writer.Flush();
                    writer.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                LastError = "Logger.LogToFile(): " + ex.Message;
                Debug.WriteLineIf(Diag, LastError);
                return false;
            }
        }

    }
}
