/*
 * Projekt  : Visutronik.Commons
 * Datei    : TextTools.cs
 * Benutzer : Lulu
 * 
 * Benutzung von einfachen Textdateien zur sprachabhängigen Textausgabe
 * 
 * 12.04.2019 initial
 *				TODO Verweis auf CONSTANTS
 * 08.01.2020	add SchreibeTextdatei(), ClearTexte(),
 * TODO: DefaultTextDE() entfernen!!!		
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
//using BKP.Commons;	// CONSTANTS

namespace Visutronik.Commons
{
	/// <summary>
	/// Description of TextTools.
	/// </summary>
	public class TextTools
	{
		public const int ERROR_LANGFILE_READ  = 2;        // Fehler beim Laden der Sprachdatei
		public const int ERROR_LANGFILE_WRITE = 2;        // Fehler beim Schreiben der Sprachdatei

		/// <summary>
		/// interne Textliste nach Zeilenschema "nnnnn=Das ist mein Text nnnnn\n"
		/// </summary>
		private List<string> txtListe = new List<string>();
		
		/// <summary>
		/// Trennzeichen nach Textcode nnnnn
		/// </summary>
		private char delimiter = '=';

		/// <summary>
		/// Fehlercodegruppe
		/// </summary>
		private int TxtErrorGroup = 0;

		/// <summary>
		/// Fehlercode der Klasse
		/// </summary>
		private int TxtErrorCode = 0;

		/// <summary>
		/// Rückgabe des Fehlercodes vo
		/// </summary>
		/// <returns></returns>
		public int GetTxtErrorCode()
		{ 
			//return Constants.APP_ERRORGROUP + TxtErrorCode;
			return TxtErrorGroup + TxtErrorCode;
		}
		

		
		/// <summary>
		/// Konstruktion
		/// </summary>
		public TextTools()
		{
		}

		public void SetErrorGroup(int errorGroup)
		{
			TxtErrorGroup = errorGroup;
		}


		/// <summary>
		/// TODO Textdatei in Liste laden
		/// </summary>
		/// <param name="pfad"></param>
		/// <returns>true bei Erfolg</returns>
		public bool LadeTextdatei(string pfad)
		{
			Debug.WriteLine("TextTools.LadeTextdatei(): " + pfad); 	

			// Textarray löschen
			txtListe.Clear();

			// Read the file line by line and add lines to list
			try
			{
				using (StreamReader reader = new StreamReader(pfad))
				{
					string line;
					while ((line = reader.ReadLine()) != null)
					{
						txtListe.Add(line); 			// Add to list.
					}
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("TextTools: " + ex.Message);
				TxtErrorCode = ERROR_LANGFILE_READ;
				return false;
			}
			return true;
		}

		/// <summary>
		/// Textdatei aus Liste schreiben
		/// </summary>
		/// <param name="pfad"></param>
		/// <returns></returns>
		public bool SchreibeTextdatei(string pfad)
		{
			Debug.WriteLine("TextTools.SchreibeTextdatei(): " + pfad);

			// Read line by line from list & write to file
			try
			{
				using (StreamWriter writer = new StreamWriter(pfad, false, System.Text.Encoding.UTF8))
				{
					foreach (string line in txtListe)
					{
						writer.WriteLine(line);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("TextTools: " + ex.Message);
				TxtErrorCode = ERROR_LANGFILE_WRITE;
				return false;
			}
			return true;
		}

		/// <summary>
		/// Löscht bereits geladene Texte (zur Neuerzeugung per Programm)
		/// </summary>
		/// <returns></returns>
		public void ClearTexte()
		{
			if (txtListe.Count > 0)
				txtListe.Clear();
		}

		/// <summary>
		/// Erweitert Textarray um neue einträge
		/// </summary>
		/// <param name="txtcode"></param>
		/// <param name="txt"></param>
		/// <returns></returns>
		public bool AddText(int txtcode, string txt)
		{
			bool result = true;
			string start = String.Format("{0:D5}{1}", txtcode, delimiter);

			foreach (string line in txtListe)
			{
				if (line.StartsWith(start))
				{
					Debug.WriteLine("AddText(): " + line);
					result = false;		// Textcode bereits benutzt
					break;
				}
			}
			if (result)
			{
				string newline = start + txt;
				txtListe.Add(newline);
				Debug.WriteLine("AddText(): " + newline);
			}
			return result;
		}

		/// <summary>
		/// Sucht nach Code in Textliste und gibt Text zurück
		/// </summary>
		/// <param name="txtcode">Code</param>
		/// <returns>Text</returns>
		public string TextString(int txtcode)
		{
			string ausgabe = "";
			string start = String.Format("{0:D5}{1}", txtcode, delimiter);
			//Debug.WriteLine("Suche nach: " + start);
			
			foreach (string line in txtListe)
			{
				if (line.StartsWith(start))
				{
					//Debug.WriteLine(line);
					int pos = line.IndexOf(delimiter);
					ausgabe = line.Substring(pos + 1);
				}
			}
			
			if (ausgabe.Length == 0)
			{
				ausgabe = "missing text: " + txtcode.ToString();
				Debug.WriteLine("TextTools.TextString(): " + ausgabe);
			}
			return ausgabe;
		}

		/// <summary>
		/// Wichtigste Textausgaben für den Notfall in Textliste schreiben 
		/// </summary>
		public void DefaultTextDe()
		{
			Debug.WriteLine("TextTools.DefaultTextDe()");
			ClearTexte();
			AddText(10000, "");
			AddText(10001, "Fehler beim Laden der Programmeinstellungen!");
			AddText(10002, "Fehler beim Laden der Sprachdatei!");
			AddText(10003, "Fehler beim Sichern der Programmeinstellungen!");
			AddText(10004, "Programm durch Benutzer beendet");
			AddText(10005, "Programm durch Betriebssystem beendet");
			AddText(10009, "Programm beenden?");
			// ...

			AddText(10010, "Passwort OK");
			AddText(10011, "Passwort falsch");
			AddText(10012, "Bitte Passwort eingeben!");
			AddText(10013, "Keine Anmeldung");
			AddText(10014, "Standard");
			AddText(10015, "Operator");
			AddText(10016, "Admin");

			// ...

			AddText(10006, "SQL-Tabelle ERRORS nicht initialisiert");
			AddText(10007, "SQL-Tabelle USERS nicht initialisiert");
			// ...

		}
	}
}
