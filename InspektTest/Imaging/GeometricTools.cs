/// <summary>
/// Klassen für geometrische Berechnungen 
/// (aus Projekt ggp-Leiterplattenmessplatz 2015)
/// Geradenschnittpunkt2D
/// </summary>
/// Hamann / Visutronik GmbH
/// 2015-11-20 V1.0.27
/// 2023 chg namespace

// enthält:
// - 2D EuclideanDistance
// - Geradenschnittpunkt2D, Geradenschnittwinkel2D
// - BerechneAusgleichsgeradeOrthogonal
// - NormalizeRectPoints
// - NormalizeRect
// - PointInPolygon
// - 2D EinheitsNormalenVector
// - 3D CrossProduct
// - Lotfußpunkt eines Lots eines Punktes P3 auf Gerade durch (P1, P2) berechnen
// - Startpunkte für Kantenantastung berechnen
// - Endpunkte für Kantenantastung berechnen
// - ComputeCircle
// Next: Ausreisser nach Bestfit-Geradenberechnung eliminieren oder aus Messwertetabelle entfernen


using System;
//using System.Drawing;
using System.Diagnostics;

namespace Visutronik.Imaging
{
	/// <summary>
	/// GeometricTools.
	/// </summary>
	public class GeometricTools
	{

		const double TINY = 1.0E-20;			// sehr kleine Zahl, aus "Numerical Receipes"

		public GeometricTools()
		{
		}

		#region === Common 2D Geometric Computation TOOLS ===

		// Euclidean distance between 2 points (double)
		public static double EuclideanDistance(DPoint p1, DPoint p2)
		{
			double dx = p2.X - p1.X;
			double dy = p2.Y - p1.Y;
			return Math.Sqrt(dx * dx + dy * dy);
		}

		public static float EuclideanDistanceF(System.Drawing.PointF p1, System.Drawing.PointF p2)
		{
			double dx = p2.X - p1.X;
			double dy = p2.Y - p1.Y;
			return (float) Math.Sqrt(dx * dx + dy * dy);
		}

		
		// Euclidean distance between 2 points (int)
		public static int EuclideanDistanceInt(System.Drawing.Point p1, System.Drawing.Point p2)
		{
			double dx = p2.X - p1.X;
			double dy = p2.Y - p1.Y;
			return Convert.ToInt32(Math.Sqrt(dx * dx + dy * dy));
		}


        /// <summary>
        /// Berechnet den Schnittpunkt zweier Geraden
        /// </summary>
        /// <param name="P11">Erster Punkt auf erster Gerade</param>
        /// <param name="P12">Zweiter Punkt auf erster Gerade</param>
        /// <param name="P21">Erster Punkt auf zweiter Gerade</param>
        /// <param name="P22">Zweiter Punkt auf zweiter Gerade</param>
        /// <param name="SP">Schnittpunkt der Geraden</param>
        /// <returns>true wenn Schnittpunkt existiert</returns>
        public static bool Geradenschnittpunkt2D(
					System.Drawing.PointF P11, 
					System.Drawing.PointF P12, 
					System.Drawing.PointF P21, 
					System.Drawing.PointF P22, 
					out System.Drawing.PointF SP)
        {
            SP = new System.Drawing.PointF();
            float a, b, c;

            // Richtungsvektoren (deltaX, deltaY)
            System.Drawing.PointF V1 = new System.Drawing.PointF(P12.X - P11.X, P12.Y - P11.Y);
            System.Drawing.PointF V2 = new System.Drawing.PointF(P22.X - P21.X, P22.Y - P21.Y);

            if (V2.X == 0.0F)
            {
                Debug.WriteLine("Gerade 2 ist senkrecht / vertikal!");
                if (V1.X == 0.0F)
                {
                    Debug.WriteLine("Gerade 1 leider auch ...!");
                    return false;
                }

                a = (P21.X - P11.X) / V1.X;
            }
            else if (V2.Y == 0.0F)
            {
                Debug.WriteLine("Gerade 2 ist waagerecht / horizontal!");
                if (V1.Y == 0.0F)
                {
                    Debug.WriteLine("Gerade 1 leider auch ...!");
                    return false;
                }
                a = (P21.Y - P11.Y) / V1.Y;
            }
            else
            {
                // Kollinearität testen:
                float r1 = V1.X / V2.X;
                float r2 = V1.Y / V2.Y;
                if (r1 == r2)
                {
                    Debug.WriteLine("Die Geraden sind kollinear und schneiden sich nicht!!!");
                    return false;
                }
                c = V1.X / V1.Y;
                b = (P11.X - P21.X - c * (P11.Y - P21.Y)) / (V2.X - c * V2.Y);
                a = (P21.Y - P11.Y + b * V2.Y) / V1.Y;
            }

            SP = new System.Drawing.PointF(P11.X + a * V1.X, P11.Y + a * V1.Y);
            Debug.WriteLine("Schnittpunkt " + SP.ToString());
            return true;
        }

        public static bool Geradenschnittwinkel2D(
				System.Drawing.PointF P11, 
				System.Drawing.PointF P12, 
				System.Drawing.PointF P21, 
				System.Drawing.PointF P22, 
				out double alpha)
        {
            alpha = 0.0;

            // TODO GeometricTools.Geradenschnittwinkel2D(): Implementierung 
            Debug.WriteLine("Implementierung GeometricTools.Geradenschnittwinkel2D() fehlt ...");
			throw new NotImplementedException();
            //return false;
        }
        
        
		/// <summary>
		/// Orthogonale Regression: Bestfit-Gerade in der Form a*x + b*y = c
		/// Wrapper
		/// </summary>
		/// <param name="pt">Liste mit Wertepaaren</param>
		/// <param name="a">out Geradenparameter a</param>
		/// <param name="b">out Geradenparameter b</param>
		/// <param name="c">out Geradenparameter c</param>
		/// <param name="d">out mittl. Abweichung d</param>
		/// 
		/// http://www.codeproject.com/Articles/576228/Line-Fitting-in-Images-Using-Orthogonal-Linear-Reg
        /// (falsche Berechnung des Korrelationskoeff.)
        /// https://de.wikipedia.org/wiki/Orthogonale_Regression
        /// http://www.mathematik.ch/anwendungenmath/Korrelation_Regression/Regression_Korrelation.pdf
        ///  
        public static bool BerechneAusgleichsgeradeOrthogonal(DPoint[] pt,
                                                 out double a, out double b, out double c, out double d)
        {
            int anz = pt.Length;
            double[] X = new double[anz];
            double[] Y = new double[anz];
            for (int i = 0; i < anz; i++)
            {
                X[i] = pt[i].X;
                Y[i] = pt[i].Y;
            }
            return BerechneAusgleichsgeradeOrthogonal(X, Y, out a, out b, out c, out d);
        }


        /// <summary>
        /// Orthogonale Regression: Bestfit-Gerade in der Form a*x + b*y = c
        /// </summary>
        /// <param name="x">Array mit Ordinatenwerten</param>
        /// <param name="y">Array mit Abszissenwerten</param>
        /// <param name="a">out Geradenparameter a</param>
        /// <param name="b">out Geradenparameter b</param>
        /// <param name="c">out Geradenparameter c</param>
        /// <param name="d">out Mittlere Abweichung (Streuung) d</param>
        /// 
        public static bool BerechneAusgleichsgeradeOrthogonal(
			double[] x, double[] y,
            out double a, out double b, out double c, out double d)
        {
            Debug.WriteLine("CalculateLineEquation()");
            bool result = true;

            a = 0.0; b = 0.0; c = 0.0; d = 0.0;
            double r;
            int n = x.Length;
            double anz = (double)n;
            double sX = 0.0, sY = 0.0;
            double mX, mY, sXX = 0.0, sXY = 0.0, sYY = 0.0;

            if (n > 2)
            {
                //Calculate sums of X and Y
                for (int idx = 0; idx < n; idx++)
                {
                    sX += x[idx]; 
                    sY += y[idx];
                }
                //Calculate X and Y means (sample means)
                mX = sX / anz;        // arithmetisches Mittel der xi
                mY = sY / anz;        // arithmetisches Mittel der yi

                //Calculate sum of X squared, sum of Y squared and sum of X * Y
                for (int idx = 0; idx < n; idx++)
                {
                    sXX += (x[idx] - mX) * (x[idx] - mX);       // Summierte Varianz der X-Werte
                    sYY += (y[idx] - mY) * (y[idx] - mY);       // Summierte Varianz der Y-Werte
                    sXY += (x[idx] - mX) * (y[idx] - mY);
                }

                double decanz = anz - 1.0;						// dekrementierte Anzahl
                double mX2 = sXX / decanz;						// Mittlere Varianz der X-Werte
                double mY2 = sYY / decanz;						// Mittlere Varianz der Y-Werte
                double covar = Math.Abs(sXY / decanz);          // Kovarianz

                // Korrelationskoeffizient (nicht aussagekräftig, wenn Gerade parallel zu den Koordinatenachsen ist!)
                r = covar / Math.Sqrt(mX2 * mY2);

                //Debug.WriteLine("  mX   = " + mX.ToString("F3"));
                //Debug.WriteLine("  mY   = " + mY.ToString("F3"));
                //Debug.WriteLine("  decanz = " + decanz.ToString("F3"));
                //Debug.WriteLine("  mX2 = " + mX2.ToString("F3"));
                //Debug.WriteLine("  mY2 = " + mY2.ToString("F3"));
                //Debug.WriteLine("  CoV = " + covar.ToString("F3"));
                //Debug.WriteLine("  r = " + r.ToString("F3"));

                bool isVertical = sXY == 0.0 && sXX < sYY;
                bool isHorizontal = sXY == 0.0 && sXX > sYY;
                bool isIndeterminate = sXY == 0.0 && sXX == sYY;
                //double slope = double.NaN;						// Geradenanstieg
                //double intercept = double.NaN;					// Schnittpunkt mit y-Achse

                if (isVertical)
                {
                    a = 1.0;
                    b = 0.0;
                    c = mX;
                }
                else if (isHorizontal)
                {
                    a = 0.0;
                    b = 1.0;
                    c = mY;
                }
                else if (isIndeterminate)
                {
                    a = double.NaN;
                    b = double.NaN;
                    c = double.NaN;

                    Debug.WriteLine("BerechneAusgleichsgeradeOrthogonal() FEHLER: unbestimmt");
                    result = false;
                }
                else
                {
                    double slope = (sYY - sXX + Math.Sqrt((sYY - sXX) * (sYY - sXX) + 4.0 * sXY * sXY)) / (2.0 * sXY);
                    double intercept = mY - slope * mX;
                    //double normFactor = (intercept >= 0.0 ? 1.0 : -1.0) * Math.Sqrt(slope * slope + 1.0);	// warum Vorzeichenumkehr???
                    double normFactor = Math.Sqrt(slope * slope + 1.0);
                    a = -slope / normFactor;
                    b = 1.0 / normFactor;
                    c = intercept / normFactor;
                }
                
                // Mittlere Abweichung der Messpunkte von der Ausgleichsgeraden berechnen:
                // LH 2015-12-10
                d = 0;
                for (int idx = 0; idx < n; idx++)
                {
                	d += Math.Abs((a * x[idx] + b * y[idx] - c) / Math.Sqrt(a * a + b * b));
                }
                d = d / anz;
                Debug.WriteLine(" - meandist = " + d.ToString("F2"));
            }
            else
            {
                Debug.WriteLine("BerechneAusgleichsgeradeOrthogonal() FEHLER: zu wenig Punkte!");
                result = false;
            }
            return result;
        }

		
		// change coordinates to ensure that p1 is top-left and p2 is bottom-right corner of a rectangle
		public static void NormalizeRectPoints(ref System.Drawing.Point p1, ref System.Drawing.Point p2)
		{
			int tmp;
			if (p1.X > p2.X)
			{
				tmp = p1.X; p1.X = p2.X; p2.X = tmp;	// swap X values
			}
			if (p1.Y > p2.Y)
			{
				tmp = p1.Y; p1.Y = p2.Y; p2.Y = tmp;	// swap Y values
			}
		}


		//
		// from TIS icImagingControl sample
		public struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}
		
		
		// change RECT to ensure that Top, Left is top-left and Bottom, Right is bottom-right corner of a rectangle
		// (from TIS icImagingControl sample)
		public static RECT NormalizeRect(RECT val)
		{
			int Tmp;
			RECT r = val;
			if( r.Top > r.Bottom )
			{
				Tmp = r.Top; r.Top = r.Bottom;	r.Bottom = Tmp;
			}
			if( r.Top < 0 )		{ r.Top = 0; }
			//int h = icImagingControl1.ImageHeight;
			//if (r.Bottom >= h) r.Bottom = h - 1;
			if( r.Left > r.Right )
			{
				Tmp = r.Left; r.Left = r.Right; r.Right = Tmp;
			}
			if( r.Left < 0 )	r.Left = 0;
			//int w = icImagingControl1.ImageWidth;
			//if (r.Right >= w)	r.Right = w - 1;
			return r;
		}

		
		// ungetestet!!!
		public static System.Drawing.RectangleF NormalizeRectF(System.Drawing.RectangleF val)
		{
			float Tmp1, Tmp2;
			System.Drawing.RectangleF r = val;

			if (r.Top > r.Bottom)
			{
				Tmp1 = r.Bottom; Tmp2 = r.Top - r.Bottom;
				r.X = Tmp1;	r.Width = Tmp2;
			}
			if (r.X < 0) r.X = 0;

			if (r.Left > r.Right)
			{
				Tmp1 = r.Right;	Tmp2 = r.Left - r.Right;
				r.Y = Tmp1;	r.Height = Tmp2;
			}
			if (r.Y < 0) r.Y = 0;
			return r;
		}


		
		// http://www.ecse.rpi.edu/Homepages/wrf/Research/Short_Notes/pnpoly.html
		/// <summary>
		/// Liegt Punkt in Polygon???
		/// </summary>
		/// <param name="pt">Point </param>
		/// <param name="poly">Polygon point array</param>
		/// <returns></returns>
		public static bool PointInPolygon(System.Drawing.PointF pt, System.Drawing.PointF[] poly)
		{
			int i, j, c = 0;
			int polylength = poly.GetLength(0);
			for (i = 0, j = polylength - 1; i < polylength; j = i++)
			{
				if (((poly[i].Y > pt.Y) != (poly[j].Y > pt.Y)) &&
				    (pt.X < (poly[j].X - poly[i].X) * (pt.Y - poly[i].Y) / (poly[j].Y - poly[i].Y) + poly[i].X))
				{
					if (c == 0) c = 1; else c = 0; //c = !c;
				}
			}
			return (c != 0);
		}
		
		
		/// <summary>
		/// Berechnet Einheits-Normalenvektor einer durch 2 Punkte gehenden Geraden
		/// </summary>
		/// <param name="pt1">Geradenpunkt 1</param>
		/// <param name="pt2">Geradenpunkt 2</param>
		/// <returns>Einheitsvektor (Länge = 1) der Normalen</returns>
		public static double[] EinheitsNormalenVector(DPoint pt1, DPoint pt2)
		{
			//const double TINY = 1.0E-20;			// sehr kleine Zahl, aus "Numerical Receipes"
			
			double len = EuclideanDistance(pt1, pt2);
			// Debug.WriteLine("GeometricTools.EinheitsNormalenVector(): pt1={0} pt2={1} len={2}", pt1.ToString(), pt2.ToString(), len.ToString());
			if (len < TINY)
			{
				// Debug.WriteLine("  Exception! len < eps");
				throw new InvalidOperationException("Punkte dürfen nicht identisch sein");
			}
			double dx_norm = (pt2.X - pt1.X) / len;
			double dy_norm = (pt2.Y - pt1.Y) / len;
			double[] envec = new double[]{-dy_norm, dx_norm};
			return envec;
		}
		
		#endregion

		#region === Common 3D Geometric Computation TOOLS ===

		/// Berechnet Kreuzprodukt von 2 Vektoren
		///
		public double[] CrossProduct(double[] V1, double[] V2)
		{
			double[] V3 = new double[3];
			
			if ((V1.Length != 3) || (V2.Length != 3))
			{
				throw new ArgumentOutOfRangeException();
			}
			
			// https://de.wikipedia.org/wiki/Normalenvektor
			
			V3[0] = V1[1] * V2[2] - V1[2] * V2[1];
			V3[1] = V1[2] * V2[0] - V1[0] * V2[2];
			V3[2] = V1[0] * V2[1] - V1[1] * V2[0];
			
			return V3;
		}
		
		#endregion
		
		#region === Sophisticated Geometric Computation TOOLS ===
		
		/// <summary>
		/// Berechnet Lotfußpunkt eines Lots eines Punktes P3 auf Gerade durch (P1, P2)
		/// </summary>
		/// <param name="P1">Geradenpunkt 1</param>
		/// <param name="P2">Geradenpunkt 2</param>
		/// <param name="P3">Lotpunkt</param>
		/// <returns>Lotfußpunkt</returns>
		/// 
		public static System.Drawing.PointF Lotfusspunkt(
				System.Drawing.PointF P1, System.Drawing.PointF P2, System.Drawing.PointF P3)
		{
			DebugList("Lotfußpunkt im zweidimensionalen Raum");
			DebugList("  P1 = " + P1.ToString());
			DebugList("  P2 = " + P2.ToString());
			DebugList("  P3 = " + P3.ToString());

			System.Drawing.PointF LFP = new System.Drawing.PointF();
			float a, b, c, d, e, f, g;
			float r = 0, s = 0;
			
			//	 a		   c         e			a = r * c + s * e
			//	( )	= r * ( ) + s * ( )   ===>
			//	 b         d         f			b = r * d + s * f


			// Substitutionen:
			a = P1.X - P3.X;		// Geradenpunkt 1 zu Lotpunkt
			b = P1.Y - P3.Y;
			c = P1.X - P2.X;		// Geradenpunkt 1 zu 2
			d = P1.Y - P2.Y;
			e = -d;					// Geradenpunkt 1 zu 2 um 90° gedreht
			f = c;

			DebugList("  a  = " + a.ToString() + ",  b  = " + b.ToString());
			DebugList("  c  = " + c.ToString() + ",  d  = " + d.ToString());
			DebugList("  e  = " + e.ToString() + ",  f  = " + f.ToString());
			
			// Spezial = Trivialfälle
			// (führen sonst zu Division durch Null)
			if (c == 0.0f)
			{
				DebugList("  Gerade ist vertikal");
				LFP.X = P1.X;
				LFP.Y = P3.Y;
			}
			else if (d == 0.0f)
			{
				DebugList("  Gerade ist horizontal");
				LFP.X = P3.X;
				LFP.Y = P1.Y;
			}
			else if (Math.Abs(c) > Math.Abs(d))
			{
				DebugList("  Weg 1:");
				g = c / d;		// weitere Subst. für Berechnung von s vorteilhaft...

				s = (a - b * g) / (e - f * g);
				r = ( a - s * e) / c;

				DebugList("  r  = " + r.ToString());
				DebugList("  s  = " + s.ToString());
				LFP.X = P1.X - r * c;
				LFP.Y = P1.Y - r * d;
			}
			else
			{
				DebugList("   Weg 2:");
				g = d / c;
				
				s = (b - a * g) / (f - e * g);
				r = ( a - s * e) / c;
				
				DebugList("  r = " + r.ToString());
				DebugList("  s = " + s.ToString());
				LFP.X = P1.X - r * c;
				LFP.Y = P1.Y - r * d;
			}
			
			DebugList("  LFP = " + LFP.ToString());
			DebugList("-------------------------------");
			
			return LFP;
		}
		
		
		/// <summary>
		/// Berechnet für Suchlinien Start- und Endpunktlisten um Lot, um eine Kante zu suchen:
		/// im zweidimensionalen Raum
		/// </summary>
		/// <param name="LFP">Lotfußpunkt auf Startseite</param>
		/// <param name="LP">Lotpunkt aug gegenüberliegender Seite</param>
		/// <param name="dist">Abstand der Startpunkte</param>
		/// <param name="count">Anzahl der Suchstrahlen links / rechts vonm Lot</param>
		/// <param name="startpunktliste">Ergebnis: Startpunkte</param>
		/// <param name="endpunktliste">Ergebnis: Endpunkte</param>
		/// <returns></returns>
		public static bool Startpunkte(System.Drawing.PointF LFP,		// vorher berechneter Lotfußpunkt
		                               System.Drawing.PointF LP,		// Lotpunkt auf anderer Streifenseite
		                               float sldist,					// Abstand der Suchstrahlen
		                               int count,						// halbe Anzahl der Suchstrahlen
		                               out System.Collections.Generic.List<System.Drawing.PointF> startpunktliste,
		                               out System.Collections.Generic.List<System.Drawing.PointF> endpunktliste)
		{
			DebugList("  Startpunkte für Kantenantastung berechnen:");
			DebugList("   - Lotfusspunkt LFP : " + LFP.ToString());
			DebugList("   - Lotpunkt     LP  : " + LP.ToString());
			
			System.Collections.Generic.List<System.Drawing.PointF> spl = new System.Collections.Generic.List<System.Drawing.PointF>();
			System.Collections.Generic.List<System.Drawing.PointF> epl = new System.Collections.Generic.List<System.Drawing.PointF>();
			float deltaX, deltaY;
			double beta;												// Anstiegswinkel
			
			float teiler = 1.0F;										// 2 für halbe Lotlinie
			deltaX = (LP.X - LFP.X) / teiler;							// Suchlinienvektor
			deltaY = (LP.Y - LFP.Y) / teiler;
			beta = Math.Atan2(deltaY, deltaX);							// Anstieg der Lotgeraden
			
			System.Drawing.PointF LEP = new System.Drawing.PointF(LFP.X + deltaX, LFP.Y + deltaY);	// Endpunkt auf Lotlinie

			float deltaX0 = (float) (sldist * Math.Sin(-beta));			// 90° zum Lot
			float deltaY0 = (float) (sldist * Math.Cos(beta));

			/*
			// Debug:
			double betaGrad = 180.0 * beta / Math.PI;
			Debug.WriteLine("  Lotlinie Anstieg : " + betaGrad.ToString("F1"));
			Debug.WriteLine("  deltaX  = " + deltaX.ToString("F1") + "  deltaY  = " + deltaY.ToString("F1"));
			

			betaGrad = Math.Atan2(deltaY0, deltaX0) * 180.0 / Math.PI;
			Debug.WriteLine("  Senkrechte zum Lot Anstieg : " + betaGrad.ToString("F1"));
			Debug.WriteLine("  deltaX0 = " + deltaX0.ToString("F1") + "  deltaY0 = " + deltaY0.ToString("F1"));
			 */
			for (int i = -count; i <= count; i++)
			{
//				deltaX = (float) (sldist * i * Math.Sin(beta));			// Start- und Endpunkte parallel
//				deltaY = (float) (sldist * i * Math.Cos(beta));			//

				deltaX = deltaX0 * i;			// Start- und Endpunkte parallel
				deltaY = deltaY0 * i;			//
				
				System.Drawing.PointF sp = new System.Drawing.PointF(LFP.X + deltaX, LFP.Y + deltaY);
				spl.Add(sp);
				
				System.Drawing.PointF ep = new System.Drawing.PointF(LEP.X + deltaX, LEP.Y + deltaY);
				epl.Add(ep);
			}
			
			startpunktliste = spl;
			endpunktliste = epl;
			return true;
		}


        /// <summary>
        /// Endpunkte für Kantenantastung aus Startpunkten berechnen
        /// </summary>
        /// <param name="LFP"></param>
        /// <param name="LP"></param>
        /// <param name="startkantenpunkte"></param>
        /// <param name="startpunktliste"></param>
        /// <param name="endpunktliste"></param>
        /// <returns></returns>
        public static bool Endpunkte(System.Drawing.PointF LFP,	// vorher berechneter Lotfußpunkt
		                             System.Drawing.PointF LP,	// Lotpunkt auf anderer Streifenseite
		                             DPoint[] startkantenpunkte,
		                             out System.Collections.Generic.List<System.Drawing.PointF> startpunktliste,
		                             out System.Collections.Generic.List<System.Drawing.PointF> endpunktliste)
		{
			DebugList("Endpunkte für Kantenantastung im zweidimensionalen Raum");
			
			System.Collections.Generic.List<System.Drawing.PointF> spl = new System.Collections.Generic.List<System.Drawing.PointF>();
			System.Collections.Generic.List<System.Drawing.PointF> epl = new System.Collections.Generic.List<System.Drawing.PointF>();
			float deltaX, deltaY;

			deltaX = (LP.X - LFP.X);								// Länge der Suchlinie
			deltaY = (LP.Y - LFP.Y);
			//double beta = Math.Atan2(deltaY, deltaX);					// Anstieg der Lotgeraden

			int n = startkantenpunkte.Length;
			
			for (int i = 0; i < n; i++)
			{
				if (startkantenpunkte[i].X != 0.0)
				{
					System.Drawing.PointF skp = new System.Drawing.PointF((float) startkantenpunkte[i].X, (float) startkantenpunkte[i].Y);
					// Startpunkte ergeben sich aus bereits gefundenen Kantenpunkten und Lotlänge:
					// Problem: Wenn Lotfußpunkt weit von Kante entfernt ist, liegt Suchstrahl auch weit weg...
					// Lösung : vorher neuen Lotfußpunkt berechnen, der auf Kante liegt
					System.Drawing.PointF sp = new System.Drawing.PointF(skp.X + deltaX, skp.Y + deltaY);
					spl.Add(sp);
					
					// Endpunkte ergeben sich aus gefundenen Kantenpunkten und halber Lotlänge:
					System.Drawing.PointF ep = new System.Drawing.PointF(skp.X + deltaX / 2.0f, skp.Y + deltaY / 2.0F);
					epl.Add(ep);
				}
				else
				{
				}
			}
			startpunktliste = spl;
			endpunktliste = epl;
			return (spl.Count > 0);
		}

        /// <summary>
        /// Endpunkte für Kantenantastung aus Startpunkten berechnen
        /// </summary>
        /// <param name="LFP"></param>
        /// <param name="LP"></param>
        /// <param name="startkantenpunkte"></param>
        /// <param name="startpunktliste"></param>
        /// <param name="endpunktliste"></param>
        /// <returns></returns>
        /// 
        public static bool Endpunkte2( double[] lotvektor,		// Lotvektor
		                              double lotlaenge,		// Abstand Lotpunkt von approx. Kante auf anderer Seite
		                              DPoint[] startkantenpunkte,
		                              ref System.Collections.Generic.List<System.Drawing.PointF> spl,
		                              ref System.Collections.Generic.List<System.Drawing.PointF> epl)
		{
			DebugList("Endpunkte für Kantenantastung im zweidimensionalen Raum");
			
			// Grundidee:
			// Wir haben bereits die erste Kante ermittelt (startkantenpunkte).
			// Daraus wird eine approximierte Gerade berechnet. Die Normale dieser Geraden
			// wird hier als umgekehrter Lotvektor angesehen.
			// Damit sollen zu jedem Startkantenpunktpunkt
			
//			if (spl == null)
//				System.Collections.Generic.List<PointF> spl = new System.Collections.Generic.List<PointF>();
//			if (epl == null)
//				System.Collections.Generic.List<PointF> epl = new System.Collections.Generic.List<PointF>();
			
			float deltaXs, deltaYs, deltaXe, deltaYe;

			deltaXs = (float) (lotvektor[0] * lotlaenge);		// Länge der Suchlinie Startpunkt
			deltaYs = (float) (lotvektor[1] * lotlaenge);

			deltaXe = 0.25F * deltaXs;							// Länge der Suchlinie Endpunkt
			deltaYe = 0.25F * deltaYs;							// Länge der Suchlinie

			int n = startkantenpunkte.Length;

//			PointF EP = new PointF((float) (ptNeuerLFP.X - 500F * env[0]),
//			                       (float) (ptNeuerLFP.Y - 500F * env[1]) );

			
			for (int i = 0; i < n; i++)
			{
				if (startkantenpunkte[i].X != 0.0)
				{
					System.Drawing.PointF skp = new System.Drawing.PointF((float) startkantenpunkte[i].X, (float) startkantenpunkte[i].Y);
					// Startpunkte ergeben sich aus bereits gefundenen Kantenpunkten und Lotlänge:
					// Problem: Wenn Lotfußpunkt weit von Kante entfernt ist, liegt Suchstrahl auch weit weg...
					// Lösung : vorher neuen Lotfußpunkt berechnen, der auf Kante liegt
					System.Drawing.PointF sp = new System.Drawing.PointF(skp.X - deltaXs, skp.Y - deltaYs);
					spl.Add(sp);
					
					// Endpunkte ergeben sich aus gefundenen Kantenpunkten und halber Lotlänge:
					System.Drawing.PointF ep = new System.Drawing.PointF(skp.X - deltaXe, skp.Y - deltaYe);
					//PointF ep = new PointF(sp.X + deltaXe, sp.Y + deltaYe);
					//PointF ep = new PointF(skp.X, skp.Y);
					epl.Add(ep);
				}
				else
				{
				}
			}
			return (spl.Count > 0);
		}

		
		public static bool ComputeCircle(DPoint[] points,		// Berechne BestFit-Kreis aus Punktmenge
		                          out double center_x,	        // Resultate
		                          out double center_y,
		                          out double radius,
		                          out double sigma)
		{
			double		k1, k2, k3, k4, k5, k6, k7, k8, k9, k10;
			double		hx, hy;
			double		hx2, hy2, hr2;				// square of hx, hy, r
			double		h1, h2;						// help variables
			double		x0, y0, r0, s0;
			int			k;							// number of used valid points
			bool		bRes;

			DebugList("Visutronik.GeometricTools.ComputeCircle()");

			// compute co-efficients
			k1 = k2 = k3 = k4 = k5 = k6 = k7 = k8 = k9 = k10 = 0;
			k = 0;

			// preset result
			x0 = 0.0; y0 = 0.0; r0 = 0.0; s0 = 1.0;

			for (int i = 0; i < points.Length; i++)
			{
				hx = points[i].X;
				hy = points[i].Y;

				k++;							// inc number of valid points
				hx2  = hx * hx;					// compute co-efficients
				hy2  = hy * hy;
				k1  += hx;
				k2  += hy;
				k4  += hx * hy;
				k5  += hx2 * hx + hx * hy2;
				k7  += hx2;
				k8  += hy2;
				k9  += hy2 * hy + hy * hx2;
				k10 += (hx2 + hy2) * (hx2 + hy2);
			}

			if (k > 2) 							// we need so much valid points
			{
				k3 = 3.0 * k7 + k8;
				k6 = k7 + k8;
				
				// compute results
				//	h1 = k * (k4*k5 - k7*k9) + pow(k1, 2.0)*k9 - k1*(k2*k5 + k4*k6) + k2*k7*k6;
				//	h2 = 2.0 * ( k* (pow(k4, 2.0) - k7*k8) + pow(k1, 2.0)*k8 - 2.0*k1*k2*k4 + pow(k2, 2.0)*k7);

				// pow(x) ersetzt durch x* x (27.10.2006)
				h1 = k * (k4*k5 - k7*k9) + k1*k1*k9 -  k1 * (k2*k5 + k4*k6) + k2*k7*k6;
				h2 = 2.0 * (k* (k4*k4 - k7*k8) + k1*k1*k8 - 2.0*k1*k2*k4 + k2*k2*k7);
				
				// Mittelpunkt
				y0 = h1 / h2;
				//x0 = -(k*(2.0*k4*y0 - k5) + k1*(k6 - 2.0*k2*y0)) / (2.0*(k*k7 - pow(k1, 2.0)));
				x0 = -(k*(2.0*k4*y0 - k5) + k1*(k6 - 2.0*k2*y0)) / (2.0*(k*k7 - k1 * k1));
				
				// Radius
				hx2 = x0 * x0;
				hy2 = y0 * y0;
				r0 = Math.Sqrt(hx2 + hy2 + (k6 - 2.0*x0*k1 - 2.0*y0*k2) / k);

				// statistics
				hr2 = r0 * r0;

				// Int-Multiplikatoren durch double ersetzt (27.10.2006), keine Veränd. des Ergebnisses
				s0 = k*hr2*hr2 - 2.0*k*hr2*hx2 + 4.0*hr2*x0*k1 - 2.0*k*hr2*hy2 + 4.0*hr2*y0*k2
					- 2.0*hr2*k6 + k*hx2*hx2 - 4.0*hx2*x0*k1 + 2.0*k*hx2*hy2 - 4.0*hx2*y0*k2
					+ hx2*(6.0*k7 + 2.0*k8) - 4.0*x0*hy2*k1 + 8.0*x0*y0*k4 - 4.0*x0*k5 + k*hy2*hy2
					- 4.0*hy2*y0*k2 + hy2*(2.0*k7 + 6.0*k8) - 4.0*y0*k9 + k10;

				// sigma = Varianz / Anzahl = mittlere Abweichung
				s0 = Math.Sqrt(s0) / k;

//		TRACE(_T("CIprLib3::Compute circle, obj=%d "), obj);
//		TRACE(_T(" X0=%3.2f  Y0=%3.2f  R=%3.2f  S=%3.2f Obj:%d Pkt:%d\n"), x0, y0, r0, sigma, obj, k);

//		TRACE(_T("circle;%3.3f;%3.3f;%3.3f;%3.2f\n"), x0, y0, r0, sigma, obj);
				bRes = true;
			}
			else
			{
				DebugList(String.Format("ComputeCircle() Fehler: - nur {0} Punkte gefunden!", k.ToString()));
				//m_nLastError = E_NOEDGEPOINTS;
				bRes = false;
			}
			
			center_x = x0;	center_y = y0;
			radius = r0;	sigma = s0;
			return bRes;
		} // end ComputeCircle()

		
		
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="s"></param>
		public static void DebugList(string s)
		{
			System.Diagnostics.Debug.WriteLine(s);
		}
		
		#endregion
	}
}

