// Projekt BKP
// Klasse ErrorTools
// schreibt Fehler in Fehlerlogdatei
// schreibt Fehler in SQLite-Tabelle, löscht nach Quittierung
// https://www.smartcsharp.de/2017/07/07/sqlite-mit-c-eine-einfuehrung/

// 28.02.2019	add SQLite reference
// 08.04.2019	chg table creation
// 31.01.2020	chg ohne SQLite

using System;
using System.IO;
//using System.Data.SQLite;
using System.Diagnostics;


// nicht mehr TODO sqLite-DB komplett einbinden in ErrorLog
// benutzt TextLog-Klasse aus Projekt ggp

namespace Visutronik.Commons
{
	
	/// <summary>
	/// Singleton class ErrorLog.
	/// </summary>
	public sealed class ErrorTools
	{
		// SQL stuff:
		const string DBNAME 		= "errors.db";
		const string TABLENAME 		= "ERRORS";
		const string TABLESTRUCT 	= "idx INT NOT NULL, zeit TEXT, level INT, quit INT, error INT, msg TEXT";
		
		/// <summary>
		/// Fehlercode SQL-Operation
		/// </summary>
		public int SqlErrorCode { get; internal set; }

		/// <summary>
		/// Ordner der TextLog-Datei
		/// </summary>
		public string TxtFolder
		{
			get { return this.txtlogger.GetFolder(); }
			set { this.txtlogger.SetFolder(value); }
		}
		
		Visutronik.Commons.TextLogger txtlogger = new Visutronik.Commons.TextLogger();

		//		SQLiteConnection 	sqlite_conn = null;
		//		SQLiteCommand 		sqlite_cmd = null;
		//		string 				sqlconn = "";
		//		string 				sqlquery = "";
		//		int 				sqlresult = 0;
		//		string 				SqlFolder = "";

		// ctor
		public ErrorTools()
		{
			Debug.WriteLine("ErrorTools.ErrorTools()");
			TxtFolder = "";
		}
/*
		/// <summary>
		/// Initiate SQLite table
		/// </summary>
		/// <param name="drop">drop table if exist</param>
		/// <returns>true if success</returns>
		public bool InitSqlTable(bool drop)
		{
			Debug.WriteLine("ErrorLog.InitSqlTable()");
			
			SqlErrorCode = 0;
			try
			{
				// create connection / create database file if not available
				string dbpath = Path.Combine(SqlFolder, DBNAME);
				sqlconn = string.Format(@"Data Source={0}; Version=3;", dbpath);
				sqlite_conn = new SQLiteConnection(sqlconn);

				// open the connection:
				Debug.WriteLine(" - open connection: " + sqlconn);
				sqlite_conn.Open();
				
				if (drop)
				{
					sqlquery = "drop table if exists " + TABLENAME;
					sqlite_cmd = new SQLiteCommand(sqlquery, sqlite_conn);
					sqlresult = sqlite_cmd.ExecuteNonQuery();
					Debug.WriteLine(" - drop anzahl = " + sqlresult);
				}
				
				sqlquery = string.Format("create table if not exists {0} ({1})", TABLENAME, TABLESTRUCT);
				sqlite_cmd = new SQLiteCommand(sqlquery, sqlite_conn);
				sqlresult = sqlite_cmd.ExecuteNonQuery();
				Debug.WriteLine(" - create anzahl = " + sqlresult);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(" - exception: " + ex.Message);
			}
			return true;
		}
*/		

		/// <summary>
		/// Schreibt msg ohne Zeitangabe in Log-Datei
		/// </summary>
		/// <param name="msg">String</param>
		public void Write(string msg)
		{
			try
			{
				this.txtlogger.LogIt(msg);
			}
			catch (Exception ex)
			{
				// keine Fehlerbehandlung
				Debug.WriteLine(ex.Message);
			}
		}
		
		/// <summary>
		/// Schreibt msg mit Zeitangabe in Log-Datei
		/// </summary>
		/// <param name="msg">String</param>
		public void WriteNow(string msg)
		{
			string strTime = DateTime.Now.ToString(TextLogger.STR_DATE_SECOND_FORMAT);
			try
			{
				string s = String.Format("{0} : {1}", strTime, msg);
				this.txtlogger.LogIt(s);
			}
			catch (Exception ex)
			{
				// keine Fehlerbehandlung
				Debug.WriteLine(ex.Message);
			}
		}

		/// <summary>
		/// Schreibt Fehler in Datenbank und Log-Datei
		/// zeit TEXT, level INT, quit INT, error INT, msg TEXT
		/// </summary>
		/// <param name="errorcode">Fehlercode</param>
		/// <param name="errortext">Fehlertext</param>
		/// <param name="remark">Bemerkung</param>
		/// <returns>Datenbank-Index</returns>
		public int Append(int errorlevel, int errorcode, string errortext, string remark = "")
		{
			int logidx = -1;
			string strTime = DateTime.Now.ToString(TextLogger.STR_DATE_SECOND_FORMAT);
			
			// TODO Binding an control ... über callback
			// TODO schreiben in DB
			
			
			// Schreiben in Textdatei
			string s = String.Format("{0}; {1}; {2}; {3}; {4}", strTime, errorlevel, errorcode, errortext, remark);
			this.txtlogger.LogIt(s);
			return logidx;
		}
		
		
		#region ==== BAUSTELLE =====

		// "zeit TEXT, level INT, quit INT, error INT, msg TEXT"
		bool InsertDB(string Text1, string Text2, int count)
		{
			// TODO
			Debug.WriteLine("ErrorTools: " + Text1 + Text2 + count);
			return false;
		}
		
		bool ReadDB(string Text1, string Text2, out int count)
		{
			count = 0;
			try
			{
				/*
								using (var conn = new SQLiteConnection(sqlconn))
								{
									conn.Open();
									sqlquery = String.Format("SELECT Username,Password FROM {0} WHERE Username='@username' AND Password = '@password'", TABLENAME);
									using (var cmd = new SQLiteCommand(sqlquery, conn))
									{
										cmd.Parameters.AddWithValue("@username", Text1);
										cmd.Parameters.AddWithValue("@password", Text2);
										using (var reader = cmd.ExecuteReader())
										{
											while (reader.Read())
											{
												count += 1;
											}
											if (count == 1)
											{

											}
											else if (count == 0)
											{
												//flatAlertBox1.kind = FlatUI.FlatAlertBox._Kind.Error;
												//flatAlertBox1.Text = "data not right";
											}
										}
									}
								}
				*/
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
				return false;
			}
			return true;
		}
		
		/// <summary>
		/// löscht Fehler aus Datenbank (setze Flag + Update record)
		/// </summary>
		/// <param name="logidx"></param>
		/// <returns></returns>
		public bool Delete(int logidx)
		{
			// TODO löschen in DB
			return false;
		}

		#endregion ==== BAUSTELLE =====

	}
}
