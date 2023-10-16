// class XmlAppSettings
// Hilfsklasse zur persistente Speicherung von Einstellungen als XML-File
// http://www.mycsharp.de/wbb2/thread.php?threadid=6439
// User "Burgpflanze"
// created on 02.08.2006 at 16:06
// Lulu 2008/04/16	: add type Boolean
// Lulu 2017/07/19	: allow empty strings in xml - do not use defaults
// Lulu 2017/10/23	: throw exception if XML file not found to read
// Lulu 2020/03/12	: chg: set class public
// Lulu 2021/02/24	: add: type Decimal
// Lulu 2021/11/15  : chg: Save() because of XML file filled with zeros after shutdown / crash
// Lulu 2022/12/03  : chg: add regions 
// Lulu 2023/01/27  : chg: Save() always returns bool, doesn't throw exceptions

using System;
using System.IO;
using System.Xml;
using System.Diagnostics;

namespace Visutronik.Commons
{
	public class XmlAppSettings
	{
		private readonly XmlDocument xmlDoc;
		private readonly XmlElement xmlRoot = null;
		private readonly string xmlFilename;
		private bool flagModified = false;				// add by Lulu

		/// <summary>
		/// Constructor creates XML document
		/// </summary>
		/// <param name="file">filename or complete path to settings file</param>
		/// <param name="createNew"></param>
		public XmlAppSettings(string file, bool createNew)
		{
			xmlFilename = file;
			xmlDoc = new XmlDocument();

			if (File.Exists(xmlFilename) && !createNew)
			{
				// read existing xml file to xml doc
				try
				{
					xmlDoc.Load(file);
					xmlRoot = xmlDoc.DocumentElement;
				}
				catch (Exception ex)
				{
					Debug.WriteLine("XMLAppSettings: " + ex.Message);
					throw ex;
				}
			}
			else
			{
				// create xml doc without elements and save to file
				Debug.WriteLine("create xml doc without elements and save to file");

				XmlDeclaration xmlDec = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
				xmlDoc.AppendChild(xmlDec);
				if (xmlRoot == null)
				{
					xmlRoot = xmlDoc.CreateElement("Settings");
					xmlDoc.AppendChild(xmlRoot);
				}
				xmlDoc.Save(xmlFilename);
                // add 2017
                throw (new Exception("Empty XML Document"));
            }
        }

        public string GetXmlFilename()
		{
			return this.xmlFilename;
		}

		/*
			// doesn't work in compact framework. why??? 
			private XmlNode FindNode (string key, bool createNew)
			{
				if (key != null)
					if (key[0] == '/') 
						key = key.Remove (0, 1);
		
				if (key != null)
					if (key[key.Length - 1] == '/')
						key = key.Remove (key.Length - 1, 1);
				
				char[] splitChars = {'/'};
				string[] parts = key.Split (splitChars);

				XmlNode node = root;
				XmlNode child = null;
		
				foreach (string entry in parts)
				{
					child = node.FirstChild;
					do
					{
						if (child == null || child.Name == entry)
							break;
				
						child = child.NextSibling;
					}
					while (child == null);
			
					if (child == null)
					{
						child = xmlDoc.CreateElement (entry);
						node.AppendChild (child);
					}
					node = child;
				}
				return node;
			}
		*/

		// mod by lulu
		// - removed unused parameter "createNew"
		// - removed split key strings for subnodes
		private XmlNode FindNode(string key)
		{
			XmlNode root = xmlRoot;		// this.xmlDoc.FirstChild;
			XmlNode child = null;

			if (root.HasChildNodes)
			{
				for (int i = 0; i < root.ChildNodes.Count; i++)
				{
					child = root.ChildNodes[i];
					if (child.Name == key) break;
				}
			}

			if ((child == null) || (child.Name != key))
			{

				child = xmlDoc.CreateElement(key);
				root.AppendChild(child);
				//Debug.WriteLine(" appending xml node: " + child.Name);
			}
			return child;
		}


		public void Remove(string key)
		{
			XmlNode parent;
			XmlNode node = FindNode(key);

			while (node != null && node.ChildNodes.Count == 0)
			{
				parent = node.ParentNode;
				parent.RemoveChild(node);
				if (parent == xmlRoot) break;
				node = parent;
			}
			flagModified = true;			// by Lulu
			//xmlDoc.Save (xmlFilename);
		}

		// add by Lulu
		/// <summary>
		/// Save settings file 
		/// </summary>
		/// <returns>true if file is written or wasn't modified</returns>
		public bool Save()
		{
			bool result = true;
			if (flagModified == true)
			{
				try
				{
					// Schreiben mit FileStream
                    // Verhindert, das beim Shutdown Nullen in XML-Datei geschrieben werden
                    // 11/2021
                    // https://stackoverflow.com/questions/4380424/will-xmlserializer-ever-create-empty-documents/8858975#8858975
                    using (Stream file = new FileStream(xmlFilename,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.None,
                        0x1000,
                        FileOptions.WriteThrough))      // no caching
                    {
                        xmlDoc.Save(file);
                    }
                }
                catch (XmlException xmlex)
				{
					Debug.WriteLine("XMLAppSettings: " + xmlex.Message);
					result = false;
					//throw xmlex;
				}
				catch (Exception ex)
				{
					Debug.WriteLine("XMLAppSettings: " + ex.Message);
					result = false;
					//throw ex;
				}
			}
			return result;
		}

        #region --- write diverse var types ---

        //=======================================================================
        // WRITE values
        //=======================================================================

        public void Write(string key, string newValue)
		{
			//Debug.WriteLine("Write XML key: " + key + " = " + newValue);

			XmlNode node = FindNode(key);
			node.InnerText = newValue;
			flagModified = true;				// add by Lulu
			// xmlDoc.Save (xmlFilename);		// rem by Lulu
		}

		public void Write(string key, bool newValue)		// add by Lulu
		{
			Write(key, newValue.ToString());
		}

		public void Write(string key, char newValue)
		{
			Write(key, newValue.ToString());
		}

		public void Write(string key, byte newValue)
		{
			Write(key, newValue.ToString());
		}

		public void Write(string key, short newValue)
		{
			Write(key, newValue.ToString());
		}

		public void Write(string key, int newValue)
		{
			Write(key, newValue.ToString());
		}

		public void Write(string key, long newValue)
		{
			Write(key, newValue.ToString());
		}

		public void Write(string key, ushort newValue)
		{
			Write(key, newValue.ToString());
		}

		public void Write(string key, uint newValue)
		{
			Write(key, newValue.ToString());
		}

		public void Write(string key, ulong newValue)
		{
			Write(key, newValue.ToString());
		}

		public void Write(string key, float newValue)
		{
			Write(key, newValue.ToString());
		}

		public void Write(string key, double newValue)
		{
			Write(key, newValue.ToString());
		}

		public void Write(string key, decimal newValue)
		{
			Write(key, newValue.ToString());
		}

		public void Write(string key, DateTime newValue)
		{
			Write(key, newValue.Ticks.ToString());
		}

        #endregion

        #region --- read diverse var types ---

        //=======================================================================
        // READ values
        //=======================================================================

        public string Read(string key, string defValue)
		{
			XmlNode node = FindNode(key);

			//Debug.Write("Read XML key: " + key);

			if ((node != null)
				&& (node.InnerText != null))
				//&& (node.InnerText != ""))		// add, 2017 removed by Lulu to allow empty string
			{
				//Debug.WriteLine(" = " + node.InnerText);
				return node.InnerText;
			}
			else
			{
				//Debug.WriteLine(" = " + defValue + " (default!)");
				return defValue;
			}
		}

		public bool Read(string key, bool defValue)
		{
			string result = Read(key, defValue.ToString());
			try
			{
				return Convert.ToBoolean(result);
			}
			catch
			{
				return defValue;
			}
		}

		public char Read(string key, char defValue)
		{
			string result = Read(key, defValue.ToString());
			try
			{
				return Convert.ToChar(result);
			}
			catch
			{
				return defValue;
			}
		}

		public byte Read(string key, byte defValue)
		{
			string result = Read(key, defValue.ToString());
			try
			{
				return Convert.ToByte(result);
			}
			catch
			{
				return defValue;
			}
		}

		public short Read(string key, short defValue)
		{
			string result = Read(key, defValue.ToString());
			try
			{
				return Convert.ToInt16(result);
			}
			catch
			{
				return defValue;
			}
		}

		public int Read(string key, int defValue)
		{
			string result = Read(key, defValue.ToString());
			try
			{
				return Convert.ToInt32(result);
			}
			catch
			{
				return defValue;
			}
		}

		public long Read(string key, long defValue)
		{
			string result = Read(key, defValue.ToString());
			try
			{
				return Convert.ToInt64(result);
			}
			catch
			{
				return defValue;
			}
		}

		public ushort Read(string key, ushort defValue)
		{
			string result = Read(key, defValue.ToString());
			try
			{
				return Convert.ToUInt16(result);
			}
			catch
			{
				return defValue;
			}
		}

		public uint Read(string key, uint defValue)
		{
			string result = Read(key, defValue.ToString());
			try
			{
				return Convert.ToUInt32(result);
			}
			catch
			{
				return defValue;
			}
		}

		public ulong Read(string key, ulong defValue)
		{
			string result = Read(key, defValue.ToString());
			try
			{
				return Convert.ToUInt64(result);
			}
			catch
			{
				return defValue;
			}
		}

		public float Read(string key, float defValue)
		{
			string result = Read(key, defValue.ToString());
			try
			{
				return Convert.ToSingle(result);
			}
			catch
			{
				return defValue;
			}
		}

		public double Read(string key, double defValue)
		{
			string result = Read(key, defValue.ToString());
			try
			{
				return Convert.ToDouble(result);
			}
			catch
			{
				return defValue;
			}
		}

		public decimal Read(string key, decimal defValue)
		{
			string result = Read(key, defValue.ToString());
			try
			{
				return Convert.ToDecimal(result);
			}
			catch
			{
				return defValue;
			}
		}

		public DateTime Read(string key, DateTime defValue)
		{
			string result = Read(key, defValue.Ticks.ToString());
			try
			{
				return new DateTime(Convert.ToInt64(result));
			}
			catch
			{
				return defValue;
			}
		}

        #endregion
    }
}
