/// <summary>
/// CheckerList class is a list wrapper with drawing method for checker objects
/// Checker class represents simple geometric objects (Line, Circle, Cross, Rectangle, Wire)
/// </summary>
/// Last modifications: 
/// 2015/07/06	add setting of checker params with int values
/// 2015/07/07	add generic setting of checker params with type and int values
/// 2015/07/13	add property "selected"
/// 2015/08/20	add DoCheck() methods
/// 2023/02/03	separate file	
/// 


using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;

namespace Visutronik.Imaging
{

    ///===================================================================================================================
	/// <summary>
    /// Klasse Checker
    /// </summary>
    class Checker
	{
		public enum Checkertype {Line, Circle, Cross, Rectangle, Wire};

		public string name { get; set; }
		public string shortname { get; set; }
		public Checkertype type { get; set; }

		public float fParam1 { get; set; }		// Punktkoordinate1 X
		public float fParam2 { get; set; }		// Punktkoordinate1 Y
		public float fParam3 { get; set; }		// Punktkoordinate2 X oder Radius oder Länge
		public float fParam4 { get; set; }		// Punktkoordinate2 Y oder unbenutzt

		public int iParam1 { get; set; }		// Punktkoordinate1 X
		public int iParam2 { get; set; }		// Punktkoordinate1 Y
		public int iParam3 { get; set; }		// Punktkoordinate2 X
		public int iParam4 { get; set; }		// Punktkoordinate2 Y

		public Color color { get; set; }
		public float fontsize { get; set; }
		public bool selected { get; set; }
		
		/// <summary>
		/// Konstruktor
		/// </summary>
		public Checker()
		{
			name = "Checker";
			shortname = "Chk";
			//iType = (int) Checkertype.Line;
			type = Checkertype.Line;
			iParam1 = iParam2 = iParam3 = iParam4 = 0;
			fParam1 = fParam2 = fParam3 = fParam4 = 0.0f;
			color = Color.White;
			fontsize = 12.0f;
			selected = false;
		}

		public Checker(Checkertype t)
		{
			name = "Checker";
			shortname = "Chk";
			//iType = (int) Checkertype.Line;
			type = t;
			iParam1 = iParam2 = iParam3 = iParam4 = 0;
			fParam1 = fParam2 = fParam3 = fParam4 = 0.0f;
			color = Color.White;
			fontsize = 10.0f;
			selected = false;
		}

		public override string ToString()
		{
			return string.Format("{0}, Type={1}]", shortname, type);
		}

        #region --- set checker param methods ---

        // change fontsize for checker labels with size check (returns new fontsize):
        public float SetFontsize(float fs)
		{
			fontsize = Math.Abs(fs);
			if (fontsize < 6.0f) fontsize = 6.0f; 
			if (fontsize > 36.0f) fontsize = 36.0f;
			return fontsize;			
		}

        // generic parameter setting method:
        public void SetIntParam(Checkertype t, int p1, int p2, int p3, int p4)
		{
			type = t;
			iParam1 = p1;
			iParam2 = p2;
			iParam3 = p3;
			iParam4 = p4;
			this.ConvertIntParams();
		}

		public void SetLineParam(DPoint dpt1, DPoint dpt2)
		{
			type = Checkertype.Line;
			fParam1 = (float) dpt1.X;
			fParam2 = (float) dpt1.Y;
			fParam3 = (float) dpt2.X;
			fParam4 = (float) dpt2.Y;
			this.ConvertFloatParams();
		}

		
		public void SetLineParam(Point pt1, Point pt2)
		{
			type = Checkertype.Line;
			iParam1 = pt1.X;
			iParam2 = pt1.Y;
			iParam3 = pt2.X;
			iParam4 = pt2.Y;
			this.ConvertIntParams();
		}

		
		public void SetCircleParam(DPoint dpt, double radius)
		{
			type = Checkertype.Circle;
			fParam1 = (float) dpt.X;
			fParam2 = (float) dpt.Y;
			fParam3 = (float) radius;
			fParam4 = 0.0f;
			this.ConvertFloatParams();
		}

		public void SetCircleParam(Point pt, int radius)
		{
			type = Checkertype.Circle;
			iParam1 = pt.X;
			iParam2 = pt.Y;
			iParam3 = radius;
			iParam4 = 0;
			this.ConvertIntParams();
		}

		
		public void SetCrossParam(DPoint dpt, int length)
		{
			type = Checkertype.Cross;
			fParam1 = (float) dpt.X;
			fParam2 = (float) dpt.Y;
			fParam3 = (float) length;
			fParam4 = 0.0f;
			this.ConvertFloatParams();
		}

		public void SetCrossParam(Point pt, int length)
		{
			type = Checkertype.Cross;
			iParam1 = pt.X;
			iParam2 = pt.Y;
			iParam3 = length;
			iParam4 = 0;
			this.ConvertIntParams();
		}

		public void SetRectParam(DPoint dpt1, DPoint dpt2)
		{
			type = Checkertype.Rectangle;
			fParam1 = (float) dpt1.X;
			fParam2 = (float) dpt1.Y;
			fParam3 = (float) dpt2.X;
			fParam4 = (float) dpt2.Y;
			this.ConvertFloatParams();
		}
		
		public void SetRectParam(Point pt1, Point pt2)
		{
			type = Checkertype.Rectangle;
			iParam1 = pt1.X;
			iParam2 = pt1.Y;
			iParam3 = pt2.X;
			iParam4 = pt2.Y;
			this.ConvertIntParams();
		}

		public void SetRectParam(Rectangle r)
		{
			type = Checkertype.Rectangle;
			iParam1 = r.X;
			iParam2 = r.Y;
			iParam3 = r.X + r.Width;
			iParam4 = r.Y + r.Height;
			this.ConvertIntParams();
		}

		
		public void SetWireParam(DPoint dpt, double radius)
		{
			type = Checkertype.Wire;
			fParam1 = (float) dpt.X;
			fParam2 = (float) dpt.Y;
			fParam3 = (float) radius;
			fParam4 = 0.0f;
			this.ConvertFloatParams();
		}

		public void SetWireParam(Point pt, int radius)
		{
			type = Checkertype.Wire;
			iParam1 = pt.X;
			iParam2 = pt.Y;
			iParam3 = radius;
			iParam4 = 0;
			this.ConvertIntParams();
		}
		
		public void ConvertFloatParams()
		{
			iParam1 = Convert.ToInt32(fParam1);
			iParam2 = Convert.ToInt32(fParam2);
			iParam3 = Convert.ToInt32(fParam3);
			iParam4 = Convert.ToInt32(fParam4);
			//Debug.WriteLine(String.Format("Rect({0}, {1} ... {2}, {3})", iParam1, iParam2, iParam3, iParam4));
		}

		public void ConvertIntParams()
		{
			fParam1 = Convert.ToSingle(iParam1);
			fParam2 = Convert.ToSingle(iParam2);
			fParam3 = Convert.ToSingle(iParam3);
			fParam4 = Convert.ToSingle(iParam4);
			//Debug.WriteLine(String.Format("Rect({0}, {1} ... {2}, {3})", iParam1, iParam2, iParam3, iParam4));
		}

        #endregion

        /// <summary>
        /// Draw the checker to a bitmap 
        /// </summary>
        /// <param name="img">destination bitmap</param>
        public void Draw(System.Drawing.Bitmap img)
		{
			Debug.WriteLine("Checker.Draw(" + this.shortname + ") Type:" + this.type.ToString());
			//Debug.WriteLine(" - iParam1 = " + this.iParam1.ToString());
			//Debug.WriteLine(" - iParam2 = " + this.iParam2.ToString());
			//Debug.WriteLine(" - iParam3 = " + this.iParam3.ToString());
			//Debug.WriteLine(" - iParam4 = " + this.iParam4.ToString());

			Pen pen = new Pen(color);
			SolidBrush brush = new SolidBrush(color);
			Font font = new Font(FontFamily.GenericSansSerif, fontsize, FontStyle.Bold);
			Point pt1 = new Point();
			Point pt2 = new Point();
			int len = 1;
			int radius = 1;
			Size size;
			Rectangle rect;
			
			pt1.X = iParam1; pt1.Y = iParam2;
			pt2.X = iParam3; pt2.Y = iParam4;

			Graphics g = Graphics.FromImage(img);
			switch (type)
			{
				case Checkertype.Line:
					// Line with edge finder
					pt1.X = iParam1; pt1.Y = iParam2;
					pt2.X = iParam3; pt2.Y = iParam4;
					g.DrawLine(pen, pt1, pt2);
					break;
				case Checkertype.Circle:
					// circle
					radius = iParam3;
					pt1.X = iParam1 - radius;
					pt1.Y = iParam2 - radius;
					size = new Size(radius * 2, radius * 2);
					rect = new Rectangle(pt1, size);
					g.DrawEllipse(pen, rect);
					// center point
					len = 3;
					pt1.X = pt2.X = iParam1;
					pt1.Y = iParam2 - len; pt2.Y = iParam2 + len;
					g.DrawLine(pen, pt1, pt2);
					pt1.X = iParam1 - len; pt2.X = iParam1 + len;
					pt1.Y = pt2.Y = iParam2;
					g.DrawLine(pen, pt1, pt2);
					break;
				case Checkertype.Cross:
					// cross
					len = iParam3;
					pt1.X = pt2.X = iParam1;
					pt1.Y = iParam2 - len; pt2.Y = iParam2 + len;
					g.DrawLine(pen, pt1, pt2);
					pt1.X = iParam1 - len; pt2.X = iParam1 + len;
					pt1.Y = pt2.Y = iParam2;
					g.DrawLine(pen, pt1, pt2);
					break;
				case Checkertype.Rectangle:
					// Line with edge finder
					int w = iParam3 - iParam1;
					int h = iParam4 - iParam2;
					Rectangle rectRect = new Rectangle(iParam1, iParam2, w, h);
					g.DrawRectangle(pen, rectRect);
					break;
				case Checkertype.Wire:
					// Circle with edge finder for parallel lines inside
					radius = iParam3;
					pt1.X = iParam1 - radius;
					pt1.Y = iParam2 - radius;
					size = new Size(radius * 2, radius * 2);
					rect = new Rectangle(pt1, size);
					g.DrawEllipse(pen, rect);
					// center point
					len = 3;
					pt1.X = pt2.X = iParam1;
					pt1.Y = iParam2 - len; pt2.Y = iParam2 + len;
					g.DrawLine(pen, pt1, pt2);
					pt1.X = iParam1 - len; pt2.X = iParam1 + len;
					pt1.Y = pt2.Y = iParam2;
					g.DrawLine(pen, pt1, pt2);
					break;
				default:

					break;
			}

			// draw checker label:
			pt1.X = iParam1 + 5;
			if (pt1.X > img.Width - 35) pt1.X = iParam1 - 35;
			pt1.Y = iParam2 - 15;
			if (pt1.Y < 20) pt1.Y = iParam2 + 5;
			g.DrawString(shortname, font, brush, pt1);
			
			brush.Dispose();
			pen.Dispose();
		}
		
		

		// check if point is inside the checkerList drawing handles
		public bool IsSelected(System.Drawing.Point pt2)
		{
			System.Drawing.Point pt1 = new Point(iParam1, iParam2);
			if (GeometricTools.EuclideanDistanceInt(pt1, pt2) < 5)
			{
				this.selected = true;
			}
			return this.selected;
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool DoCheck()
		{
			bool result = false;
			switch (type)
			{
				case Checkertype.Line: 		result = DoLineCheck(); break;
				case Checkertype.Circle: 	result = DoCircleCheck(); break;
				case Checkertype.Cross: 	result = DoCrossCheck(); break;
				case Checkertype.Rectangle: result = DoRectCheck(); break;
				case Checkertype.Wire: 		result = DoWireCheck(); break;
			}
			return result;
		}

		#region unfinished check methods
		private bool DoLineCheck()
		{
			Debug.WriteLine("DoLineCheck()");
			return false;
		}

		
		private bool DoCircleCheck()
		{
			Debug.WriteLine("DoCircleCheck()");
			return false;
		}


		private bool DoCrossCheck()
		{
			Debug.WriteLine("DoCrossCheck()");
			return false;
		}


		private bool DoRectCheck()
		{
			Debug.WriteLine("DoRectCheck()");
			return false;
		}
	
		
		private bool DoWireCheck()
		{
			Debug.WriteLine("DoWireCheck()");
			return false;
		}
		
		#endregion
		
	}
}
