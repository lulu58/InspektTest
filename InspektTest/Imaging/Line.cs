using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Visutronik.Imaging
{
    public struct LineF
    {
        public System.Drawing.PointF P1;
        public System.Drawing.PointF P2;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="center">center point as PointF</param>
        /// <param name="radius">radius</param>
        public LineF(System.Drawing.PointF p1, System.Drawing.PointF p2)
        {
            P1 = p1; P2 = p2;
        }

        // ctor
        public LineF(System.Drawing.RectangleF rect)
        {
            this.P1 = new System.Drawing.PointF(rect.X, rect.Y);
            this.P2 = new System.Drawing.PointF(rect.X + rect.Width, rect.Y + rect.Height);
        }
    }

    public struct Line
    {
        public System.Drawing.Point P1;
        public System.Drawing.Point P2;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="center">center point as PointF</param>
        /// <param name="radius">radius</param>
        public Line(System.Drawing.Point p1, System.Drawing.Point p2)
        {
            P1 = p1; P2 = p2;
        }

        // ctor
        public Line(System.Drawing.Rectangle rect)
        {
            this.P1 = new System.Drawing.Point(rect.X, rect.Y);
            this.P2 = new System.Drawing.Point(rect.X + rect.Width, rect.Y + rect.Height);
        }
    }

}
