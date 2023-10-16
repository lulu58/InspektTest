// Konvertierung Object <---> JSON-String mit Newtonsoft.Json
// Lulu 02.02.2023
// Beispiele:
// - einfache Klasse mit Properties
// var book1 = new Book() { Title = "So ein Mist", Author = "Monty", ISBN = 111222111 };
// string s = Visutronik.Commons.JsonTools.ObjectToJson(book1);
// var book2 = Visutronik.Commons.JsonTools.ObjectFromJson<Book>(s) as Book;
//
// - Containerklasse mit mehreren Objekten
// string sbl = Visutronik.Commons.JsonTools.ObjectToJson(bookList);
// List<Book> list2 = Visutronik.Commons.JsonTools.ObjectFromJson<List<Book>>(sbl) as List<Book>;

using Newtonsoft.Json;
using System;
using System.Diagnostics;

namespace Visutronik.Commons
{
    public class JsonTools
    {
        #region --- static converters ---

        /// <summary>
        /// convert an object to JSON string
        /// </summary>
        /// <param name="object">the source object</param>
        /// <returns>JSON string</returns>
        public static string ObjectToJson(object obj)
        {
            string s = "";
            try
            {
                s = JsonConvert.SerializeObject(obj);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return s;
        }

        /// <summary>
        /// convert a JSON string to obj object
        /// </summary>
        /// <param name="json">JSON string</param>
        /// <returns>object or null</returns>
        public static object ObjectFromJson<T>(string json)
        {
            Debug.WriteLine("Deserialize " + json);
            object obj = default(T);
            try
            {
                obj = JsonConvert.DeserializeObject<T>(json);
            }
            catch (Newtonsoft.Json.JsonReaderException jrex)
            {
                Debug.WriteLine(jrex.Message);
            }
            return obj;
        }

        #endregion

    }
}
