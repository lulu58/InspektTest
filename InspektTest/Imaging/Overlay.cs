/// Graphics Overlay Klasse
/// div. Projekte
/// Visutronik / Lutz Hamann
/// Datum: 30.06.2015
/// 2015-07-10	chg	Namespace
/// 2015-07-13	add some internal drawing methods
/// 2015-10-15	add param: _linewidth
/// 2015-10-19	Umstellung Point -> PointF
/// 2015-10-21	add text drawing methods (from checker class)
/// 2015-10-26	add arrows and labeltext drawing methods
/// 2023-01-31	less debug output
/// 
/* 
 usage:
 	Bitmap imgSource;
 	imgSource.Load(....);

 	// create an empty overlay bitmap with size of imgSource:
 	Overlay ovl = new Overlay();
 	ovl.SetOverlay(imgSource);

 	// draw to overlay bitmap:
  	ovl.DrawRect(new Rectangle(...));		// internal drawing methods
  	ovl.DrawArrow(...);
  	checkerList.Draw(ovl.ovlbitmap);			// external drawing to overlay bitmap

	// show the composed image
	Bitmap imgSrcWithOvl = ovl.GetImageWithOverlay(imgSource);
	picturebox1.Image = imgSrcWithOvl; 
 */
 
using System;
using System.Drawing;
using System.Drawing.Imaging;		// ColorMatrix, ImageAttributes,...
using System.Diagnostics;
using Visutronik.Commons;

namespace Visutronik.Imaging
{
	/// <summary>
	/// Description of Overlay.
	/// </summary>
	public class Overlay
	{
		#region === Properties ===

		public bool UseDebug { get; set; } = false;
		public System.Drawing.Font TextFont	{ get; set; }
		public System.Drawing.Font LabelFont	{ get; set; }

		private int _linewidth = 1;
		public int LineWidth
		{ 
			get { return _linewidth; }
			set 
			{ 
				if ((value > 0) && (value < 21)) _linewidth = value; 
				else _linewidth = 3; 
			}
		}
		
		private float _accessorsize = 20.0F;
		public float AccessorSize { 
			get { return _accessorsize; }
			set { _accessorsize = value; }
		}
		

		public System.Drawing.Bitmap ovlbitmap = null;


		private float _textfontsize = 12.0f;	// Fontgröße für DrawText()
		private float _labelfontsize = 36.0F;	// Fontgröße für DrawLabel()
		
		#endregion === Properties ===
		
		#region === overlay creation and access methods ===

		/// <summary>
		/// ctor
		/// </summary>
		public Overlay()
		{
			UseDebug = false;
		}
		
		/// <summary>
		/// ctor with initialization
		/// </summary>
		/// <param name="bm"></param>
		public Overlay(System.Drawing.Bitmap bm)
		{
			Debug.WriteLine("Overlay.Overlay()");
			this.SetOverlay(bm);
		}

		/// <summary>
		/// Erzeugt Overlay-Bitmap und macht einige Initialisierungen ...
		/// </summary>
		/// <param name="bm"></param>
		public void SetOverlay(System.Drawing.Bitmap bm)
		{
			Debug.WriteLineIf(UseDebug, "Overlay.SetOverlay(bitmap)");
			if ((ovlbitmap == null) || (ovlbitmap.Size != bm.Size))
			{
				System.Drawing.Rectangle r = new System.Drawing.Rectangle(0, 0, bm.Width, bm.Height);
				// Overlay hat immer Farben, unabhängig von Kamerabild:
				ovlbitmap = bm.Clone(r, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
				Debug.WriteLineIf(UseDebug, "Overlay.SetOverlay(): new overlay created");
			}
			else
			{
				Debug.WriteLineIf(UseDebug, "Overlay.SetOverlay(): no need to create new overlay");
			}
			ClearOverlay();

			// create text font			
			this.TextFont = new System.Drawing.Font(FontFamily.GenericSansSerif, 
			                                         _textfontsize,	// 15.75F, 
			                                         System.Drawing.FontStyle.Bold, 
			                                         System.Drawing.GraphicsUnit.Point, 
			                                         ((byte)(0)));	// gdi character set
			
			// compute default fontsize for labelling:
			_labelfontsize = GuiTools.ComputeFontsize(
											bm.Width,
											640, 	// int min_imgsize
		                                    3200, 	// int max_imgsize
		                                    12.0F,	// float min_fontsize 
		                                    72.0F); // float max_fontsize
            //Debug.WriteLineIf(UseDebug, "_labelfontsize = " + _labelfontsize.ToString());

            // create label font			
            this.LabelFont = new System.Drawing.Font("Microsoft Sans Serif", 
			                                         _labelfontsize,	// 15.75F, 
			                                         System.Drawing.FontStyle.Bold, 
			                                         System.Drawing.GraphicsUnit.Point, 
			                                         ((byte)(0)));	// gdi character set
			
			
			// compute default accessor size (circles, arrowheads...)
			float alength = bm.Width / 100.0F;
			if (alength < 10) alength = 10;
			if (alength > 50) alength = 50;
			this._accessorsize = alength;
			
			if (bm.Width < 1600) _linewidth = 2; else _linewidth = 3;
				
		}
		
		/// <summary>
		/// Overlay-Bitmap löschen
		/// </summary>
		public void ClearOverlay()
		{
			if (ovlbitmap == null)
			{
				throw new NullReferenceException("Bitmap not initialized");
			}
			else
			{
				Graphics graphicsObj = Graphics.FromImage(ovlbitmap);
				graphicsObj.Clear(Color.Black); //.White); // Set Bitmap background
     			graphicsObj.Dispose();
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="destBmp"></param>
		/// <returns></returns>
		public System.Drawing.Bitmap GetImageWithOverlay(System.Drawing.Bitmap destBmp)
		{
			//return OverlayBitmap(destBmp, ovlbitmap, new Point(0,0));
			return OverlayBitmap(destBmp, ovlbitmap);	// Lulu-Verson
		}

		#endregion  === overlay creation and access methods ===

		
		// change fontsize for checker texts with size check (returns new fontsize):
		public float SetTextFontsize(float fs)
		{
			_textfontsize = Math.Abs(fs);
			if (_textfontsize < 6.0f) _textfontsize = 6.0f; 
			if (_textfontsize > 72.0f) _textfontsize = 72.0f;
			return _textfontsize;			
		}

		// change fontsize for checker labels with size check (returns new fontsize):
		public float SetLabelFontsize(float fs)
		{
			_textfontsize = Math.Abs(fs);
			if (_labelfontsize < 6.0f) _labelfontsize = 6.0f; 
			if (_labelfontsize > 72.0f) _labelfontsize = 72.0f;
			return _labelfontsize;			
		}
		
		#region  === overlay drawing methods ===

		// Draws a text horizontal aligned beginning from the point:
		public void DrawText(string s,
		                     System.Drawing.PointF pt, 
		                     System.Drawing.Color color)
		{
			//Debug.WriteLineIf(UseDebug, "Overlay.DrawText()");
			this.DrawString(s, pt, color, this.TextFont);
		}

		// Draws a label horizontal aligned beginning from the point:
		public void DrawLabel(string s,
		                     System.Drawing.PointF pt, 
		                     System.Drawing.Color color)
		{
			//Debug.WriteLineIf(UseDebug, "Overlay.DrawLabel()");
			this.DrawString(s, pt, color, this.LabelFont);
		}
		
		
		// Draws a string horizontal aligned beginning from the point:
		public void DrawString(string s,
		                     System.Drawing.PointF pt, 
		                     System.Drawing.Color color,
							 System.Drawing.Font font)
		{
			//Debug.WriteLineIf(UseDebug, "Overlay.DrawString()");
			if (ovlbitmap == null)
			{
				throw new NullReferenceException("Bitmap not initialized");
			}
			else
			{
				using (System.Drawing.Graphics graphicsObj = Graphics.FromImage(ovlbitmap))
				{
					System.Drawing.Brush brush = new SolidBrush(color);
					graphicsObj.DrawString(s, font, brush, pt);
					brush.Dispose();
				}
			}
		}
		


		// Draws a text horizontal aligned and centered to the point:
		public void DrawTextCentric(string s, 
		                     System.Drawing.PointF pt, 
		                     System.Drawing.Color color)
		{
			this.DrawStringCentric(s, pt, color, this.TextFont);
		}
		
		// Draws a text horizontal aligned and centered:
		public void DrawLabelCentric(string s,
		                     System.Drawing.PointF pt, 
		                     System.Drawing.Color color)
		{
			this.DrawStringCentric(s, pt, color, this.LabelFont);
		}

		/// <summary>
		/// Draws a text horizontal aligned and centered:
		/// </summary>
		/// <param name="s"></param>
		/// <param name="pt"></param>
		/// <param name="color"></param>
		public void DrawStringCentric(string s,
		                     System.Drawing.PointF pt, 
		                     System.Drawing.Color color,
		                     System.Drawing.Font font)
		{
			Debug.WriteLineIf(UseDebug, "Overlay.DrawStringCentric()");
			if (ovlbitmap == null)
			{
				throw new NullReferenceException("Bitmap not initialized");
			}
			else
			{
				using (System.Drawing.Graphics graphicsObj = Graphics.FromImage(ovlbitmap))
				{
					// dazu Textgröße mit Font ermitteln und Offset zu pt addieren
					System.Drawing.Brush brush = new SolidBrush(color);
					
					StringFormat stringFormat = new StringFormat() 
					{ 
    					Alignment = StringAlignment.Center, 		//Horizontale Orientieren
    					LineAlignment = StringAlignment.Center,  	//Vertikale Orientierung
					};
					graphicsObj.DrawString(s, font, brush, pt, stringFormat);
					stringFormat.Dispose();
					brush.Dispose();
				}
			}
		}
		
		/// <summary>
		/// Zeichnet Gerade mit Pfeilspitzen / Draws a line with arrowheads 
		/// </summary>
		/// <param name="pt1">Anfangspunkt der Geraden</param>
		/// <param name="pt2">Endpunkt der Geraden</param>
		/// <param name="col">Zeichenfarbe</param>
		/// <param name="at_pt1">Pfeilspitze an Anfangspunkt</param>
		/// <param name="at_pt2">Pfeilspitze an Endpunkt</param>
		/// 
		public void DrawArrow(System.Drawing.PointF pt1, 
		                     System.Drawing.PointF pt2, 
		                     System.Drawing.Color col,
		                     bool at_pt1,
		                     bool at_pt2)
		{
			// http://csharphelper.com/blog/2014/12/draw-lines-with-arrowheads-in-c/

			Debug.WriteLineIf(UseDebug, "Overlay.DrawArrow()");
			if (ovlbitmap == null)
			{
				throw new NullReferenceException("Bitmap not initialized");
			}
			else
			{
				Graphics graphicsObj = Graphics.FromImage(ovlbitmap);
				Pen myPen = new Pen(col, _linewidth);
				// draw the shaft
     			graphicsObj.DrawLine(myPen, pt1, pt2);
     			// Find the arrow shaft unit vector.
    			float vx = pt2.X - pt1.X;
    			float vy = pt2.Y - pt1.Y;
    			float dist = (float)Math.Sqrt(vx * vx + vy * vy);
    			vx /= dist;
    			vy /= dist;
     			// now draw arrowheads
     			if (at_pt1)	DrawArrowhead(graphicsObj, myPen, pt1, -vx, -vy);
     			if (at_pt2)	DrawArrowhead(graphicsObj, myPen, pt2, vx, vy);

     			myPen.Dispose();
     			graphicsObj.Dispose();
			}
		}

		// Draw an arrowhead at the given point in the normalizede direction <nx, ny>.
		private void DrawArrowhead(Graphics gr, Pen pen, PointF p, float nx, float ny)
		{
			float length = this._accessorsize;
    		float ax = length * (-ny - nx);
    		float ay = length * (nx - ny);
    		PointF[] points =
    		{
        		new PointF(p.X + ax, p.Y + ay),  p,  new PointF(p.X - ay, p.Y + ax)
    		};
    		gr.DrawLines(pen, points);
		}
		
		
		/// <summary>
		/// Draws a line 
		/// </summary>
		/// <param name="pt1"></param>
		/// <param name="pt2"></param>
		/// <param name="col"></param>
		public void DrawLine(System.Drawing.PointF pt1, 
		                     System.Drawing.PointF pt2, 
		                     System.Drawing.Color col)
		{
			Debug.WriteLineIf(UseDebug, "Overlay.DrawLine()");
			if (ovlbitmap == null)
			{
				throw new NullReferenceException("Bitmap not initialized");
			}
			else
			{
				using (Graphics graphicsObj = Graphics.FromImage(ovlbitmap))
				{
					Pen myPen = new Pen(col, _linewidth);
	     			graphicsObj.DrawLine(myPen, pt1, pt2);
	     			myPen.Dispose();
				}
			}
		}

		/// <summary>
		/// Draws a cross with accessorsize 
		/// </summary>
		/// <param name="pt1"></param>
		/// <param name="pt2"></param>
		/// <param name="col"></param>
		public void DrawCross(System.Drawing.PointF pt, 
		                     System.Drawing.Color col)
		{
			Debug.WriteLineIf(UseDebug, "Overlay.DrawCross()");
			if (ovlbitmap == null)
			{
				throw new NullReferenceException("Bitmap not initialized");
			}
			else
			{
				float len = _accessorsize / 2.0F;
				using (Graphics graphicsObj = Graphics.FromImage(ovlbitmap))
				{
					Pen myPen = new Pen(col, _linewidth);
					System.Drawing.PointF pt1 = pt; // new PointF(pt.X, pt.Y);
					System.Drawing.PointF pt2 = pt; // new PointF(pt.X, pt.Y);
					pt1.X -= len; pt2.X += len;
	     			graphicsObj.DrawLine(myPen, pt1, pt2);
					pt1.X += len; pt2.X -= len;
					pt1.Y -= len; pt2.Y += len;
	     			graphicsObj.DrawLine(myPen, pt1, pt2);
	     			myPen.Dispose();
				}
			}
		}

        /// <summary>
        /// draw a circle to the overlay
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="radius"></param>
        /// <param name="col"></param>
        /// <exception cref="NullReferenceException"></exception>
        public void DrawCircle(System.Drawing.PointF pt,
		                       float radius, 
		                       System.Drawing.Color col)
		{
			Debug.WriteLineIf(UseDebug, "Overlay.DrawCircle()");
			if (ovlbitmap == null)
			{
				throw new NullReferenceException("Bitmap not initialized");
			}
			else
			{
				RectangleF rect = new RectangleF(pt.X, pt.Y, 2 * radius, 2 * radius);
				rect.Offset(-radius, -radius);
				using (Graphics graphicsObj = Graphics.FromImage(ovlbitmap))
				{
					Pen myPen = new Pen(col, _linewidth);
    	 			graphicsObj.DrawEllipse(myPen, rect);
    	 			myPen.Dispose();
				}
			}
		}

		public void DrawCircleMarker(System.Drawing.PointF pt, 
				                     System.Drawing.Color col)
		{
			Debug.WriteLineIf(UseDebug, "Overlay.DrawCircle()");
			if (ovlbitmap == null)
			{
				throw new NullReferenceException("Bitmap not initialized");
			}
			else
			{
				float radius = _accessorsize; 
				float len = _accessorsize / 2.0F;
				RectangleF rect = new RectangleF(pt.X, pt.Y, 2.0F * radius, 2.0F * radius);
				rect.Offset(-radius, -radius);
				using (Graphics graphicsObj = Graphics.FromImage(ovlbitmap))
				{
					Pen myPen = new Pen(col, _linewidth);
     				graphicsObj.DrawEllipse(myPen, rect);
     				
					System.Drawing.PointF pt1 = pt;
					System.Drawing.PointF pt2 = pt;
					pt1.X -= len; pt2.X += len;
	     			graphicsObj.DrawLine(myPen, pt1, pt2);
					pt1.X += len; pt2.X -= len;
					pt1.Y -= len; pt2.Y += len;
	     			graphicsObj.DrawLine(myPen, pt1, pt2);
	     				
     				myPen.Dispose();
				}
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="pt1"></param>
		/// <param name="pt2"></param>
		/// <param name="col"></param>
		public void DrawRectangle(System.Drawing.PointF pt1, 
		                          System.Drawing.PointF pt2, 
		                          System.Drawing.Color col)
		{
			Debug.WriteLineIf(UseDebug, "Overlay.DrawRectangle()");
			if (ovlbitmap == null)
			{
				throw new NullReferenceException("Bitmap not initialized");
			}
			else
			{
				using (Graphics graphicsObj = Graphics.FromImage(ovlbitmap))
				{
					Pen myPen = new Pen(col, _linewidth);
	     			graphicsObj.DrawRectangle(myPen, pt1.X, pt1.Y, pt2.X-pt1.X, pt2.Y-pt1.Y);
    	 			myPen.Dispose();
				}
			}
		}

        public void DrawRectangleF(System.Drawing.RectangleF rect,
                          System.Drawing.Color col)
        {
            Debug.WriteLineIf(UseDebug, "Overlay.DrawRectangleF()");
            if (ovlbitmap == null)
            {
                throw new NullReferenceException("Bitmap not initialized");
            }
            else
            {
                using (Graphics graphicsObj = Graphics.FromImage(ovlbitmap))
                {
                    Pen myPen = new Pen(col, _linewidth);
                    graphicsObj.DrawRectangle(myPen, rect.X, rect.Y, rect.Width, rect.Height);
                    myPen.Dispose();
                }
            }
        }
        /// <summary>
        /// Draw a rectangle with offset
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="pt"></param>
        /// <param name="col"></param>
        public void DrawRectangle(System.Drawing.Rectangle rect, 
		                          System.Drawing.Point pt, 
		                          System.Drawing.Color col)
		{
			Debug.WriteLineIf(UseDebug, "Overlay.DrawRectangle() with offset");
			if (ovlbitmap == null)
			{
				throw new NullReferenceException("Bitmap not initialized");
			}
			else
			{
				rect.Offset(pt.X - rect.Width/2, pt.Y - rect.Height/2);
				using (Graphics graphicsObj = Graphics.FromImage(ovlbitmap))
				{
					Pen myPen = new Pen(col, _linewidth);
	     			graphicsObj.DrawRectangle(myPen, rect);
	     			myPen.Dispose();
				}
			}
		}

		/// <summary>
		/// Draw a rectangle without offset
		/// </summary>
		/// <param name="rect"></param>
		/// <param name="pt"></param>
		/// <param name="col"></param>
		public void DrawRectangle(System.Drawing.Rectangle rect, 
		                          System.Drawing.Color col)
		{
			Debug.WriteLineIf(UseDebug, "Overlay.DrawRectangle() with offset");
			if (ovlbitmap == null)
			{
				throw new NullReferenceException("Bitmap not initialized");
			}
			else
			{
				using (Graphics graphicsObj = Graphics.FromImage(ovlbitmap))
				{
					Pen myPen = new Pen(col, _linewidth);
    	 			graphicsObj.DrawRectangle(myPen, rect);
    	 			myPen.Dispose();
				}
			}
		}
		
		public void FillRectangle(System.Drawing.Rectangle rect, 
		                          System.Drawing.Point pt, 
		                          System.Drawing.Color col)
		{
			Debug.WriteLine("Overlay.DrawRectangle()");
			if (ovlbitmap == null)
			{
				throw new NullReferenceException("Bitmap not initialized");
			}
			else
			{
				using (Graphics graphicsObj = Graphics.FromImage(ovlbitmap))
				{
					Brush brush = new SolidBrush(col);
					rect.Offset(pt.X - rect.Width/2, pt.Y - rect.Height/2);
    	 			graphicsObj.FillRectangle(brush, rect);
    	 			brush.Dispose();
				}
			}
		}
		
		
		public void DrawEllipse(System.Drawing.Rectangle rect, 
		                        System.Drawing.Point pt, 
		                        System.Drawing.Color col)
		{
			Debug.WriteLine("Overlay.DrawRectangle()");
			if (ovlbitmap == null)
			{
				throw new NullReferenceException("Bitmap not initialized");
			}
			else
			{
				using (Graphics graphicsObj = Graphics.FromImage(ovlbitmap))
				{
					Pen myPen = new Pen(col, _linewidth);
					rect.Offset(pt.X - rect.Width/2, pt.Y - rect.Height/2);
    	 			graphicsObj.DrawEllipse(myPen, rect);
    	 			myPen.Dispose();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rect"></param>
		/// <param name="pt"></param>
		/// <param name="col"></param>
		public void FillEllipse(System.Drawing.Rectangle rect, System.Drawing.Point pt, System.Drawing.Color col)
		{
			Debug.WriteLine("Overlay.DrawRectangle()");
			if (ovlbitmap == null)
			{
				throw new NullReferenceException("Bitmap not initialized");
			}
			else
			{
				using (Graphics graphicsObj = Graphics.FromImage(ovlbitmap))
				{
					Brush brush = new SolidBrush(col);
					rect.Offset(pt.X - rect.Width/2, pt.Y - rect.Height/2);
    	 			graphicsObj.FillEllipse(brush, rect);
    	 			brush.Dispose();
				}
			}
		}

		#endregion === overlay drawing methods ===
	
		
		#region === overlay superpositioning methods ===
		
	
		/// from: http://www.codeproject.com/Articles/4892/A-Bitmap-Manipulation-Class-With-Support-For-Forma
		/// <summary>
		/// das geht nicht so schön. wir wollen keine alphakanäle nutzen ....
		/// </summary>
		/// <param name="destBmp"></param>
		/// <param name="bmpToOverlay"></param>
		/// <param name="overlayPoint"></param>
		/// <returns></returns>
		public static System.Drawing.Bitmap OverlayBitmap(System.Drawing.Bitmap destBmp, 
		                                                  System.Drawing.Bitmap bmpToOverlay, 
		                                                  System.Drawing.Point overlayPoint)
		{
			Debug.WriteLine("OverlayBitmap - codeproject-Version");

			//Copy the destination bitmap
			//NOTE: Can't clone here, because if destBmp is indexed instead of just RGB, Graphics.FromImage will fail
			Bitmap newBmp = new Bitmap(destBmp.Size.Width, destBmp.Size.Height);

			//Create a graphics object attached to the bitmap
			Graphics newBmpGraphics = Graphics.FromImage(newBmp);
			//newBmpGraphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy; //.SourceOver;
			
			//Draw the input bitmap into this new graphics object
			Rectangle destRect = new Rectangle(0, 0, destBmp.Size.Width, destBmp.Size.Height);
			newBmpGraphics.DrawImage(destBmp,
			                         destRect,
			                         0, 0, destBmp.Size.Width, destBmp.Size.Height,
			                         GraphicsUnit.Pixel);

			//Create a new bitmap object the same size as the overlay bitmap
			Bitmap overlayBmp = new Bitmap(bmpToOverlay.Size.Width,
			                               bmpToOverlay.Size.Height);

			//Make overlayBmp transparent
			//overlayBmp.MakeTransparent(overlayBmp.GetPixel(0,0));

			//Create a graphics object attached to the overlay bitmap
			Graphics overlayBmpGraphics = Graphics.FromImage(overlayBmp);
			
			//Create a color matrix which will be applied to the overlay bitmap
			//to modify the alpha of the entire image
			float overlayAlphaFloat = 0.5f;
			float[][] colorMatrixItems = {
				new float[] {1, 0, 0, 0, 0},
				new float[] {0, 1, 0, 0, 0},
				new float[] {0, 0, 1, 0, 0},
				new float[] {0, 0, 0, overlayAlphaFloat, 0},
				new float[] {0, 0, 0, 0, 1}
			};
			ColorMatrix colorMatrix = new ColorMatrix(colorMatrixItems);

			//Create an ImageAttributes class to contain a color matrix attribute
			ImageAttributes imageAttrs = new ImageAttributes();
			imageAttrs.SetColorMatrix(colorMatrix,
			                          ColorMatrixFlag.Default, 
			                          ColorAdjustType.Bitmap);

			//Draw the overlay bitmap into the graphics object,
			//applying the image attributes
			//which includes the reduced alpha
			System.Drawing.Rectangle drawRect = new System.Drawing.Rectangle(0, 0, bmpToOverlay.Size.Width, bmpToOverlay.Size.Height);
//			bmpToOverlay.MakeTransparent(bmpToOverlay.GetPixel(0,0));
			overlayBmpGraphics.DrawImage(bmpToOverlay,
			                             drawRect,
			                             0, 0, bmpToOverlay.Size.Width, bmpToOverlay.Size.Height,
			                             System.Drawing.GraphicsUnit.Pixel);
//			                             GraphicsUnit.Pixel,
//			                             imageAttrs);
			overlayBmpGraphics.Dispose();
			
			//overlayBmp now contains bmpToOverlay w/ the alpha applied.
			//Draw it onto the target graphics object
			//Note that pixel units must be specified 
			//to ensure the framework doesn't attempt
			//to compensate for varying horizontal resolutions 
			//in images by resizing; in this case,
			//that's the opposite of what we want.
			newBmpGraphics.DrawImage(overlayBmp,
    								new Rectangle(overlayPoint.X, overlayPoint.Y, 
    												bmpToOverlay.Width, bmpToOverlay.Height),
								    drawRect,
    								GraphicsUnit.Pixel);
			newBmpGraphics.Dispose();
			return newBmp;
		}


		/// <summary>
		/// Draw an overlay bitmap to another bitmap - test o.k.
		/// </summary>
		/// <param name="destBmp"></param>
		/// <param name="bmpToOverlay"></param>
		/// <returns></returns>
		/// destBmp and bmpToOverlay must have the same size!!!
		public static System.Drawing.Bitmap OverlayBitmap(System.Drawing.Bitmap destBmp, 
		                                   				  System.Drawing.Bitmap bmpToOverlay)
		{
			// workaround:
			if (destBmp == null)
			{
				Debug.WriteLine("Overlay.OverlayBitmap() ERROR: destBmp not initialized!");
				return bmpToOverlay;
			}
			if (bmpToOverlay == null)
			{
				Debug.WriteLine("Overlay.OverlayBitmap() ERROR: bmpToOverlay not initialized!");
				return destBmp;
			}
			
			if (destBmp.Size != bmpToOverlay.Size)
			{
				Debug.WriteLine("Overlay.OverlayBitmap() ERROR: unequal size of images!");
				return destBmp;
			}

			//Debug.WriteLine("OverlayBitmap - Lulu-Version");
			Rectangle destRect = new Rectangle(0, 0, destBmp.Size.Width, destBmp.Size.Height);
			//create the result bitmap and draw destBmp:
			//NOTE: Can't clone here, because if destBmp is indexed instead of just RGB, Graphics.FromImage will fail
			Bitmap resultBmp = new Bitmap(destBmp.Size.Width, destBmp.Size.Height, PixelFormat.Format24bppRgb);
			Graphics resultGraphics = Graphics.FromImage(resultBmp);
			resultGraphics.DrawImage(destBmp, destRect);
			// now draw the overlay image (transparent color fom top left corner):
			resultGraphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;	// important!!!
			bmpToOverlay.MakeTransparent(bmpToOverlay.GetPixel(0,0));								// important!!!
			resultGraphics.DrawImage(bmpToOverlay, destRect);
			// cleanup:
			resultGraphics.Dispose();
			return resultBmp;
		}

		#endregion === overlay superpositioning methods ===
		
	}
}
