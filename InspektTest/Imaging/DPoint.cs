///
///
///

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Visutronik.Imaging
{
    /// <summary>
    /// struct: point of double values
    /// Hinweis: evtl. struct AForge.DoublePoint benutzen...
    /// </summary>
    public class DPoint
    {
        #region Eigenschaften

        public double X;
        public double Y;

        #endregion

        #region Konstruktoren

        public DPoint()             // add 2015-10-27
        {
            this.X = 0.0; this.Y = 0.0;
        }

        public DPoint(PointF p)     // add 2015-10-27
        {
            this.X = Convert.ToDouble(p.X);
            this.Y = Convert.ToDouble(p.Y);
        }

        public DPoint(double _x, double _y)
        {
            this.X = _x; this.Y = _y;
        }

        public DPoint(DPoint p)     // add 2015-10-27
        {
            this.X = p.X;
            this.Y = p.Y;
        }

        public DPoint(System.Drawing.Point p)
        {
            this.X = Convert.ToDouble(p.X);
            this.Y = Convert.ToDouble(p.Y);
        }

        #endregion

        #region Operatoren

        // Achtung: einige Operatoren (z.B. == ) können nicht überladen werden!!!
        // Bei Zuweisung stets Konstruktoren verwenden!!!

        /// <summary>
        /// Addition operator - adds values of two points.
        /// </summary>
        /// <returns>Returns new point which coordinates equal to sum of corresponding
        /// coordinates of specified points.</returns>
        /// (aus AForge)
        public static DPoint operator +(DPoint pt1, DPoint pt2)
        {
            return new DPoint(pt1.X + pt2.X, pt1.Y + pt2.Y);
        }


        #endregion

        #region Methoden

        // convert to integer point object
        public System.Drawing.Point ToPoint()
        {
            return new System.Drawing.Point(Convert.ToInt32(X), Convert.ToInt32(Y));
        }

        // convert to integer point object with rounding x and y to nearest integral number
        public System.Drawing.Point Round()
        {
            return new System.Drawing.Point(Convert.ToInt32(Math.Round(X)), Convert.ToInt32(Math.Round(Y)));
        }

        // convert to float point
        public System.Drawing.PointF ToPointF()
        {
            return new System.Drawing.PointF((float)this.X, (float)this.Y);
        }



        // 
        public override string ToString()
        {
            return String.Format("Pt({0}, {1})", X.ToString("F1"), Y.ToString("F1"));
        }

        #endregion
    }


}
