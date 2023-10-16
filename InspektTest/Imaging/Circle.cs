/// 23.01.2023  add structs CicleD, RingF
/// 

using System;

/// Geometric type: Circle
/// struct CircleF
/// struct CircleD
namespace Visutronik.Imaging
{
    /// <summary>
    /// Struct for circle params center, radius as float
    /// </summary>
    public struct CircleF
    {
        public System.Drawing.PointF center;
        public float radius;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="center">center point as PointF</param>
        /// <param name="radius">radius</param>
        public CircleF(System.Drawing.PointF center, float radius)
        {
            this.center = center;
            this.radius = radius;
        }

        // ctor
        public CircleF(System.Drawing.RectangleF rect, float radius)
        {
            this.center = new System.Drawing.PointF(rect.X, rect.Y);
            this.radius = radius;
        }
    }

    /// <summary>
    /// Circle with params as double
    /// </summary>
    public struct CircleD
    {
        public DPoint center;
        public double radius;

        // ctor
        public CircleD(DPoint center, double radius)
        {
            this.center = center;
            this.radius = radius;
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="rect">constructing rectangle, edge point -> center, diagonale -> rect</param>
        public CircleD(System.Drawing.RectangleF rect)
        {
            this.center = new DPoint(rect.X, rect.Y);
            double w = rect.Width;
            double h = rect.Height;
            this.radius = Math.Sqrt(w * w + h * h);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public struct RingF
    {
        public System.Drawing.PointF center;
        public float radius1;     // inner circle
        public float radius2;     // outer circle

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="center">center point as PointF</param>
        /// <param name="radius"></param>
        public RingF(System.Drawing.PointF center, float radius1, float radius2)
        {
            this.center = center;
            this.radius1 = radius1;
            this.radius2 = radius2;
        }

        // ctor
        //public RingF(System.Drawing.RectangleF rect, float radius)
        //{
        //    this.center = new System.Drawing.PointF(rect.X, rect.Y);
        //    this.radius = radius;
        //}
    }
}
