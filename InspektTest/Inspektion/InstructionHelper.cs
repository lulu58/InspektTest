// Helper methods for image operation instructions
//
// - parameter converters (rectangle, circle, line)
// - mouse helpers
// - rectangle tools
// 23.01.2023
// 17.10.2023 

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Visutronik.Imaging;
//using static Visutronik.Imaging.Checker;

namespace Visutronik.Inspektion
{
    /// <summary>
    /// Helper methods for image operation instructions
    /// </summary>
    public static class InstructionHelper
    {
        #region --- string arrays ---

        public static string[] Operators  = new string[] { "Kamerabild", "Bild laden", "Filter", "Checker", "Math.Op" };

        public static string[] FilterTypes = new string[] { "Smooth", "Binarization", "Edges", "Invert" };

        public static string[] CheckerTypes = new string[] { "Mean", "Size", "Radius", "Length" };

        public static string[] CameraNames = new string[] { "---", "Links", "Mitte", "Rechts" };

        public static string[] AreaNames = new string[] { 
            "Vollbild", "Rechteck", "Kreis", "Kreisring", "Kreissegment", "Linie" };

        public static string[] EvalTypes = new string[] {
            "Compare" };


        public static int GetAreaIndex(string area)
        {
            int idx = -1;
            for (int i = 0; i < AreaNames.Length; i++)
            {
                if (AreaNames[i].Equals(area))
                {
                    idx = i; break;
                }
            }
            return idx;
        }

        #endregion

        #region --- parameter converters ---

        /// <summary>
        /// get rectangle from parameter string
        /// inst4.ImageArea = "Rect"; inst4.ImageAreaParams = "{200, 200, 50, 50}";
        /// </summary>
        /// <param name="s">image area param string</param>
        /// <returns>RectangleF, if fail size is zero</returns>
        public static RectangleF GetRectangleFromString(string s)
        {
            RectangleF r = new RectangleF(0, 0, 0, 0);
            string h = s;
            if (s.Length > 8)
            {
                if (s.StartsWith("{"))
                {
                    h = s.Substring(1, s.Length - 2);
                    //Debug.WriteLine($" {s} ->   substring = {h}");
                }
                string[] hi = h.Split(',');
                if (hi.Length == 4)
                {
                    if (h.Contains("X="))   // Format Rectangle.ToString() -> {X=565,Y=509,Width=50,Height=44} 
                    {
                        hi[0] = hi[0].Remove(0,2);
                        hi[1] = hi[1].Remove(0, 2);
                        hi[2] = hi[2].Remove(0, 6);
                        hi[3] = hi[3].Remove(0, 7);
                    }

                    r.X = float.Parse(hi[0]);
                    r.Y = float.Parse(hi[1]);
                    r.Width = float.Parse(hi[2]);
                    r.Height = float.Parse(hi[3]);
                }
            }
            else
            {
                throw new InvalidOperationException("Invalid string" + s);
            }
            return r;
        }

        /// <summary>
        /// Get parameter string from rectangle
        /// </summary>
        /// <param name="r"></param>
        /// <returns>param string</returns>
        public static string GetStringFromRectangleF(RectangleF r)
        {
            //string s = r.ToString(); // {X=565,Y=509,Width=50,Height=44}
            string s = string.Format($"{{{r.X}, {r.Y}, {r.Width}, {r.Height}}}");
            //Debug.WriteLine("  GetStringFromRectangleF: " + s);
            return s;
        }

        /// <summary>
        /// Get parameter string from rectangle
        /// </summary>
        /// <param name="r"></param>
        /// <returns>param string</returns>
        public static string GetStringFromRectangle(Rectangle r)
        {
            //Debug.WriteLine("r = " + r.ToString());
            //string s = string.Format($"{{ {r.X}, {r.Y}, {r.Width}, {r.Height} }}");
            // System.FormatException
            // Der Index, basierend auf 0 (null), muss größer als oder gleich Null sein, und kleiner als die Größe der Argumentenliste.
            string s = "{" + string.Format($"{r.X}, {r.Y}, {r.Width}, {r.Height}") + "}";
            //Debug.WriteLine("GetStringFromRectangle: " + s);
            return s;
        }

        /// <summary>
        /// get circle from parameter string
        ///  inst5.ImageArea = "CircleF"; inst5.ImageAreaParams = "{660, 400, 150}";
        /// </summary>
        /// <param name="s">image area param string</param>
        /// <returns></returns>
        public static CircleF GetCircleFromString(string s)
        {
            System.Drawing.PointF centerPoint = new System.Drawing.PointF(0, 0);
            float radius = 0;
            CircleF circle = new CircleF(centerPoint, radius);
            string h = s;
            if (s.Length > 6)
            {
                if (s.StartsWith("{"))
                {
                    h = s.Substring(1, s.Length - 2);
                    //Debug.WriteLine($" {s} ->   substring = {h}");
                }
                string[] hi = h.Split(',');
                if (hi.Length == 3)
                {
                    centerPoint.X = float.Parse(hi[0]);
                    centerPoint.Y = float.Parse(hi[1]);
                    radius = float.Parse(hi[2]);
                }
            }
            else
            {
                throw new InvalidOperationException("Invalid string:" + s);
            }
            circle.center = centerPoint; circle.radius = radius;
            return circle;
        }

        public static CircleF GetRingFromString(string s)
        {
            System.Drawing.PointF centerPoint = new System.Drawing.PointF(0, 0);
            float radius1 = 0;
            float radius2 = 0;
            CircleF circle = new CircleF(centerPoint, radius1);
            string h = s;
            if (s.Length > 6)
            {
                if (s.StartsWith("{"))
                {
                    h = s.Substring(1, s.Length - 2);
                    //Debug.WriteLine($" {s} ->   substring = {h}");
                }
                string[] hi = h.Split(',');
                if (hi.Length == 4)
                {
                    centerPoint.X = float.Parse(hi[0]);
                    centerPoint.Y = float.Parse(hi[1]);
                    radius1 = float.Parse(hi[2]);
                    radius2 = float.Parse(hi[3]);
                }
            }
            else
            {
                throw new InvalidOperationException("Invalid string:" + s);
            }
            circle.center = centerPoint; circle.radius = radius1;
            return circle;
        }

        /// <summary>
        /// get circle from parameter string
        ///  inst5.ImageArea = "CircleF"; inst5.ImageAreaParams = "{660, 400, 150}";
        /// </summary>
        /// <param name="s">image area param string</param>
        /// <returns></returns>
        public static LineF GetLineFromString(string s)
        {
            System.Drawing.PointF pt1 = new System.Drawing.PointF(0, 0);
            System.Drawing.PointF pt2 = new System.Drawing.PointF(0, 0);

            LineF line = new LineF(pt1, pt2);
            string h = s;
            if (s.Length > 6)
            {
                if (s.StartsWith("{"))
                {
                    h = s.Substring(1, s.Length - 2);
                    //Debug.WriteLine($" {s} ->   substring = {h}");
                }
                string[] hi = h.Split(',');
                if (hi.Length == 4)
                {
                    pt1.X = float.Parse(hi[0]);
                    pt1.Y = float.Parse(hi[1]);
                    pt2.X = float.Parse(hi[2]);
                    pt2.Y = float.Parse(hi[2]);
                    line.P1 = pt1;
                    line.P2 = pt2;
                }
            }
            else
            {
                throw new InvalidOperationException("Invalid string:" + s);
            }
            return line;
        }


        #endregion

        #region --- mouse helpers ---

        public static Rectangle GetNormalizedRectangleFromPoints(Point pt1, Point pt2)
        {
            Rectangle rect = new Rectangle(pt1.X, pt1.Y, pt2.X - pt1.X, pt2.Y - pt1.Y);
            NormalizeRectangle(ref rect);
            return rect;
        }

        #endregion

        #region --- rectangle tools ---
        //TODO move rectangle tools to Visutronik.Imaging
        /// <summary>
        /// Normalize rectangle so that upper left point is [X, Y]
        /// </summary>
        /// <param name="rect">in / out Rectangle</param>
        public static void NormalizeRectangle(ref Rectangle rect)
        {
            if (rect != null)
            {
                if (rect.Width < 0) { rect.X += rect.Width; rect.Width = -rect.Width; }
                if (rect.Height < 0) { rect.Y += rect.Height; rect.Height = -rect.Height; }
            }
        }

        /// <summary>
        /// Normalize rectangle so that upper left point is [X, Y]
        /// </summary>
        /// <param name="rect">in / out RectangleF</param>
        public static void NormalizeRectangleF(ref RectangleF rect)
        {
            if (rect != null)
            {
                if (rect.Width < 0) { rect.Width = - rect.Width;  rect.X += rect.Width; }
                if (rect.Height < 0) { rect.Height = -rect.Height; rect.Y += rect.Height; }
            }
        }

        #endregion

    }
}
