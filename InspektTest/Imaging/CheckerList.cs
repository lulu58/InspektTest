/// <summary>
/// CheckerList class is a list wrapper with drawing method for checker objects
/// Checker class represents simple geometric objects (Line, Circle, Cross, Rectangle, Wire)
/// </summary>
/// Last modifications: 
/// 2015/07/06	add setting of checker params with int values
/// 2015/07/07	add generic setting of checker params with type and int values
/// 2015/07/13	add property "selected"
/// 2015/07/20	add IEnumerable interface to CheckerList
/// 2015/08/20	add DoCheck() methods
/// 2023/02/03  separate files for 	CheckerList class / Checker class


using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace Visutronik.Imaging
{
    ///===================================================================================================================
    /// <summary>
    /// Klasse CheckerList 
    /// </summary>
    class CheckerList : IEnumerable<Checker>
    {
        List<Checker> checkerList; // = new List<Checker>();

        public CheckerList()
        {
            checkerList = new List<Checker>();
        }

        // wichtig!
        public IEnumerator<Checker> GetEnumerator()
        {
            return checkerList.GetEnumerator();
        }

        // wichtig!
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Clear()
        {
            this.checkerList.Clear();
        }

        public void Add(Checker c)
        {
            this.checkerList.Add(c);
        }

        public void Remove(Checker c)
        {
            this.checkerList.Remove(c);
        }

        // draw all checkerList to overlay bitmap		
        public void Draw(Bitmap bmOverlay)
        {
            //Debug.WriteLine("CheckerList.Draw()");
            if (bmOverlay != null)
            {
                foreach (Checker c in checkerList)
                {
                    c.Draw(bmOverlay);
                }
            }
        }

        // Get count of checkerList in checkerList list
        public int Count
        {
            get { return checkerList.Count; }
        }


        // find the first selected checker
        public int Select(System.Drawing.Point pt)
        {
            int idx = -1;
            foreach (Checker c in checkerList)
            {
                c.selected = false;             // unselect all
                if (c.IsSelected(pt))
                {
                    idx = checkerList.IndexOf(c);
                }
            }
            return idx;
        }


        /// <summary>
        /// DoChecks calls check methods from all checkerList
        /// </summary>
        /// <param name="bBreakOnError">bool bBreakOnError</param>
        /// <returns></returns>
        public bool DoChecks(bool bBreakOnError)
        {
            bool result = true;

            Debug.WriteLine("Checkerlist.DoChecks()");
            foreach (Checker c in checkerList)
            {
                Checker.Checkertype ct = c.type;
                int idx = checkerList.IndexOf(c);
                Debug.WriteLine(" - teste Checker " + idx.ToString() + " vom Typ " + ct.ToString());
                result = c.DoCheck();
                if (bBreakOnError && !result) break;
            }
            return result;
        }

    }
    ///===================================================================================================================

}
