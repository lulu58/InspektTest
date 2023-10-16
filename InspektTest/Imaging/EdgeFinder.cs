//=====================================================================================
// Kantenfindung in Grauwertbildern
//=====================================================================================
// Hamann / Visutronik GmbH
// 2012...2013	C++-Version, Projekt FP3
// 2015-07-06	improved error handling
//
//=====================================================================================


using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using AForge;
using AForge.Imaging;
using System.Diagnostics;


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
		
		public DPoint()				// add 2015-10-27
		{
			this.X = 0.0; this.Y = 0.0;
		}
		
		public DPoint(PointF p)		// add 2015-10-27
		{
			this.X = Convert.ToDouble(p.X);
			this.Y = Convert.ToDouble(p.Y);
		}

		public DPoint(double _x, double _y)
		{
			this.X = _x; this.Y = _y;
		}
		
		public DPoint(DPoint p)		// add 2015-10-27
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
		public static DPoint operator +(DPoint pt1, DPoint pt2 )
		{
			return new DPoint( pt1.X + pt2.X, pt1.Y + pt2.Y );
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
			return new System.Drawing.PointF((float) this.X, (float) this.Y);
		}
		
		
		
		// 
		public override string ToString()
		{
			return String.Format("Pt({0}, {1})", X.ToString("F1"), Y.ToString("F1"));
		}
		
		#endregion
	}


	/// <summary>
	/// Klasse: Suchlinienpunkt
	/// </summary>
	public class SearchLinePoint
	{
		public double X;			// Punktkoordinaten
		public double Y;
		public double gval;			// Grauwert des Punktes
		public double derive1;		// erste Ableitung des Grauwertes
		public double derive2;		// zweite Ableitung des Grauwertes

		// ctor
		public SearchLinePoint()
		{
			this.X = this.Y = this.gval = this.derive1 = this.derive2 = 0.0;
		}
		
		public override string ToString()
		{
			return "SLPt(" + X.ToString("F1") + ", " + Y.ToString("F1")
				+ ") , g = " + gval.ToString()
				+ ", g' = "  + derive1.ToString()
				+ ", g'' = " + derive2.ToString()
				+ ")";
		}
	}

	/// <summary>
	/// Klasse: Kantenpunkt
	/// </summary>
	public class EdgePoint
	{
		public double X;
		public double Y;
		public double slope;
		public int type;

		// ctor
		public EdgePoint(DPoint dpt)
		{
			this.X = dpt.X; this.Y = dpt.Y; this.slope = 0.0; this.type = 0;
		}

		// ctor
		public EdgePoint(double _x, double _y)
		{
			X = _x; Y = _y; slope = 0.0; type = 0;
		}

		//
		public DPoint ToDPoint()
		{
			return new DPoint(X, Y);
		}

		//
		public override string ToString()
		{
			return String.Format("EPt({0}, {1}), slope={2}, type={3}", X.ToString("F1"), Y.ToString("F1"), slope, type);
		}
	}

	
	/// <summary>
	/// 
	/// </summary>
	public enum EdgeTyp
	{
		TYPE_NOEDGE,
		TYPE_EDGE_UP,
		TYPE_EDGE_DOWN
	};

	
	/// <summary>
	/// Klasse zur automatischen Kantenpunkterkennung im Subpixelbereich
	/// </summary>
	class EdgeTool
	{
		#region ----- Properties -----
		public string strName { get; set; }					// Name des Checkers ( zur Ausgabe)
		public string strError { get { return sDiag; } }	// Letzte Fehlerbeschreibung
		public int ErrorCode { get; set; }					// letzter Fehlercode
		public bool bNoParamCheck { get; set; }				// evtl. Parameterüberprüfung ausschalten

		public double MinSlope { get; set; }				// minimaler Grauwertanstieg an Kantenposition
		public int	FindEdgeMode { get; set; }				// Art der Kantenfindung

		
		#endregion

		#region ----- Constants -----
		//
		public const int MAX_LINEPOINTS = 3000;		// max. Array size for interpolated line points
		private const double dStepR     = 1.0;		// (interpolierter) Pixelabstand auf Suchstrahl
		// Kantentypen:
		public const int TYPE_NOEDGE    = 0;		// keine Kante gefunden
		public const int TYPE_EDGE_UP   = 1;		// Dunkel->Hell Kante gefunden
		public const int TYPE_EDGE_DOWN = 2;		// Hell->Dunkel Kante gefunden
		// Kantensuchmodus:
		public const int MODE_EDGE_ALL  = 0;		// suche alle Kanten
		public const int MODE_EDGE_UP   = 1;		// suche Dunkel->Hell Kante
		public const int MODE_EDGE_DOWN = 2;		// suche Hell->Dunkel Kante

		// Error codes:
		public const int ERROR_NO    = 0;
		public const int ERROR_PARAM = 1;
		public const int ERROR_MATH  = 2;
		public const int ERROR_IMAGE = 3;
		public const int ERROR_SLINE = 4;			// length of search line
		public const int ERROR_EDGE  = 5;			// no valid edge found

		//		public const double EF_MATH_PI;		// Math.PI
		//		public const double EF_RAD2DEGREE;	// alpha(°) = alpha(rad) * IPR_RAD2DEGREE
		//		public const double EF_RAD2GVAL;	// für Gradientenrichtung
		#endregion

		#region ----- private properties -----

		SearchLinePoint[] linepoints;				// Suchlinienpunkt-Array
		DPoint PointStart, PointEnd;				// Anfangs-und Endpunkte des Suchstrahles
		DPoint ptEdge = new DPoint();				// Kantenpunkt
		double dSlope = 0.0;						// Anstieg der Grauwertapproximation
		double dStepPix = 1.0;						// Abstand zwischen 2 Punkten auf Suchstrahl in Pixel

		int nNumLP = 0;								// number of line points
		int nNumEP = 0;								// number of edge points

		int nEdgeType = 0;
		int nPlateauLen = 3;						// Länge eines Bereiches am Anfang und Ende der Suchlinie, der als Plateau betrachtet wird
		double dPlateauStartGval = 0.0;				// mittlerer Grauwert am Anfang der Suchlinie
		double dPlateauEndGval = 0.0;				// mittlerer Grauwert am Ende der Suchlinie
		double dMinPlateauDiff = 20.0;				// minimale Grauwertdifferenz an Kante
		double dAlpha = 0.0;						// Richtung der Suchgeraden
		double dCosAlpha = 0.0;						// = cos(dAlpha) der Suchlinie
		double dSinAlpha = 0.0;						// = sin(dAlpha) der Suchlinie

		string sDiag = "";							// Fehler-Klartext
		bool bDiag = true;							// Flag: Diagnoseausgaben

		AForge.Imaging.UnmanagedImage image = null;

		#endregion

		/// <summary>
		/// Konstruktor
		/// </summary>
		public EdgeTool()
		{
			bNoParamCheck = true;
			// Array initialisieren:
			linepoints = new SearchLinePoint[MAX_LINEPOINTS];
			
			// Elemente initialisieren
			for (int i = 0; i < MAX_LINEPOINTS; i++)
			{
				linepoints[i] = new SearchLinePoint();
			}
			strName = "";
			MinSlope = 20.0;						// 
			FindEdgeMode = MODE_EDGE_ALL;			// Art der Kantenfindung
		}

		/// <summary>
		/// Debug mode on / off
		/// </summary>
		/// <param name="b">true is debug output is wanted</param>
		public void SetDebugMode(bool b)
		{
			this.bDiag = b;
		}
		
		/// <summary>
		/// Set source image
		/// </summary>
		/// <param name="img">source image (must be Format8bppIndexed)</param>
		/// <returns>true if success</returns>
		public bool SetImage(AForge.Imaging.UnmanagedImage img)
		{
			if (bDiag) Debug.WriteLine(" SetImage()");

			if (img != null)
			{
				if (img.PixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
				{
					image = img;
				}
				else
				{
					throw (new FormatException("Invalid image format, must be Format8bppIndexed"));
				}
			}
			return (image != null);
		}

		/// <summary>
		/// Mehrere Suchlinien-Parameter zugleich setzen
		/// </summary>
		/// <param name="ptStart">Suchlinien-Anfangspunkt</param>
		/// <param name="ptEnd">Suchlinien-Endpunkt</param>
		/// <param name="mode">Kantensuchmodus</param>
		/// <param name="minSlope">min. Anstieg</param>
		/// <param name="name">Name der Suchlinie</param>
		/// <returns></returns>
		public bool SetSearchLineParams(DPoint ptStart, DPoint ptEnd, int mode, double minSlope, string name)
		{
			bool result = SetSearchLine(ptStart, ptEnd);
			if (result)	result = SetSearchMode(mode);
			if (result)
			{
				this.MinSlope = minSlope;
				this.strName = name;
			}
			return result;
		}
		
		
		/// <summary>
		/// erzeugt Suchlinienpuffer mit bilinearer Geradeninterpolation
		/// </summary>
		/// <param name="ptStart">Suchlinien-Anfangspunkt</param>
		/// <param name="ptEnd">Suchlinien-Endpunkt</param>
		/// <returns>true wenn erfolgreich</returns>
		public bool SetSearchLine(DPoint ptStart, DPoint ptEnd)
		{
			if (bDiag) Debug.WriteLine(" EdgeFinder.SetSearchLine(): line from {0} to {1}",
			                           ptStart.ToString(),
			                           ptEnd.ToString());

			ErrorCode = ERROR_NO;

			double 	dDiffX = 0.0, dDiffY = 0.0,
			dLength = 0.0,
			dX, dY,
			dGval;
			int nLength;

			// parameter check:
			if (image == null)
			{
				sDiag = "  EdgeFinder.SetSearchLine(): ERROR no image assigned!";
				ErrorCode = ERROR_IMAGE;
			}

			// check that searchline is inside the image ...
			if (ErrorCode == ERROR_NO)
			{
				double dImgSizeX = (double)image.Width - 1.0;
				double dImgSizeY = (double)image.Height - 1.0;

				if ((ptStart.X < 1.0) || (ptStart.Y < 1.0) || (ptEnd.X < 1.0) || (ptEnd.Y < 1.0))
					ErrorCode = ERROR_PARAM;

				if ((ptStart.X > dImgSizeX) || (ptStart.Y > dImgSizeY) || (ptEnd.X > dImgSizeX) || (ptEnd.Y > dImgSizeY))
					ErrorCode = ERROR_PARAM;

				if (ErrorCode != ERROR_NO)
				{
					sDiag = "  EdgeFinder.SetSearchLine(): ERROR searchline out of image!";
				}
			}

			if (ErrorCode == ERROR_NO)
			{
				PointStart = ptStart;
				PointEnd = ptEnd;
				nNumLP = 0;												// reset count of linepoints in buffer
				dStepPix = 0;

				// Analyse des Suchstrahls:
				dDiffX = PointEnd.X - PointStart.X;						// horizontale Komponente der Suchlinie
				dDiffY = PointEnd.Y - PointStart.Y;						// vertikale Komponente der Suchlinie
				dLength = Math.Sqrt(dDiffX * dDiffX + dDiffY * dDiffY);	// Länge der Suchlinie
				dAlpha = Math.Atan2(dDiffY, dDiffX);					// Richtung der Suchlinie

				nLength = (int)dLength;
				if (nLength > MAX_LINEPOINTS - 1)	// Suchlinie zu lang
				{
					sDiag = string.Format("  EdgeFinder.SetSearchLine(): ERROR searchline too long - length = {0}!", nLength.ToString());
					ErrorCode = ERROR_SLINE;
				}
				if (nLength < 10)					// Suchlinie zu kurz
				{
					sDiag = string.Format("  EdgeFinder.SetSearchLine(): ERROR searchline too short - length = {0}!", nLength.ToString());
					ErrorCode = ERROR_SLINE;
				}
			}

			if (ErrorCode == ERROR_NO)
			{
				// Linienpunkte durch bilineare Interpolation berechnen
				dCosAlpha = Math.Cos(dAlpha);
				dSinAlpha = Math.Sin(dAlpha);
				for (double r = 0.0; r < dLength; r += dStepR)
				{
					dX = PointStart.X + dCosAlpha * r;
					dY = PointStart.Y + dSinAlpha * r;
					if (!InterpolateBilinear(dX, dY, out dGval))
					{
						ErrorCode = ERROR_MATH;
						break;
					}

					linepoints[nNumLP].X = dX;
					linepoints[nNumLP].Y = dY;
					linepoints[nNumLP].gval = dGval;
					linepoints[nNumLP].derive1 = 0;
					linepoints[nNumLP].derive2 = 0;
					nNumLP++;
				}
				dStepPix = dStepR;
			}

			// compute first and second derivate of grey values
			if (ErrorCode == ERROR_NO)
			{
				if (!ComputeLineDerivates())
				{
					sDiag = string.Format("  EdgeFinder.SetSearchLine(): ERROR ComputeLineDerivates!");
					ErrorCode = ERROR_SLINE;
				}
			}
			return (ErrorCode == 0);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="mode"></param>
		/// <returns></returns>
		public bool SetSearchMode(int mode)
		{
			if ((mode < MODE_EDGE_ALL) || (mode > MODE_EDGE_UP))
			{
				ErrorCode = ERROR_PARAM; FindEdgeMode = MODE_EDGE_ALL;
			}
			else FindEdgeMode = mode;
			return (ErrorCode == 0);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public DPoint GetEdge()
		{
			return new DPoint(ptEdge.X, ptEdge.Y);
			//return ptEdge;
		}

		/// <summary>
		/// Kantenalgorithmus auf Basis von erster und zweiter Ableitung
		/// Lulu 02.02.2012
		/// </summary>
		/// <param name="ep"></param>
		/// <returns></returns>
		//public bool FindEdge(out EdgePoint ep)
		public bool FindEdge()
		{
			if (bDiag) Debug.WriteLine("EdgeFinder.FindEdge() - " + strName);

			ErrorCode = 0;
			int idx = 0;
			double	dAlpha = 0.0;										// Winkel der Geraden zwischen 2 Linienpunkten

			// parameter check:
			if (this.nNumLP < 10)
			{
				sDiag =" EdgeFinder.FindEdge(): ERROR no searchline set!";
				ErrorCode = ERROR_SLINE;
			}

			if (ErrorCode == 0)
			{
				// Test, ob im Bereich zwischen Start und Ende eine ausreichende Grauwertdifferenz erreicht wird:
				// da evtl. mehrere Kanten im Bereich liegen können, keinen Fehler generieren!
				// gemittelte Plateaugrauwerte berechnen
				double dPlateauGval	= 0.0;
				double dPlateauGvalDiff  = 0.0;

				// Anfangsplateau
				for (idx = 0; idx < nPlateauLen; idx++)
				{
					dPlateauGval += linepoints[idx].gval;
				}
				dPlateauStartGval = dPlateauGval / (double) nPlateauLen;

				// Endplateau
				dPlateauGval = 0.0;
				for (idx = nNumLP - nPlateauLen - 1; idx < nNumLP; idx++)
				{
					dPlateauGval += linepoints[idx].gval;
				}
				dPlateauEndGval = dPlateauGval / (double) nPlateauLen;
				
				if (bDiag) Debug.WriteLine("  Plateaugrauwerte: " + dPlateauStartGval.ToString("F1") + " " +
				                           dPlateauEndGval.ToString("F1") +
				                           ", Plateaulänge = " + nPlateauLen.ToString());

				dPlateauGvalDiff = Math.Abs(dPlateauEndGval - dPlateauStartGval);
				if (dPlateauGvalDiff < dMinPlateauDiff)		// 20 ist brauchbarer Wert...
				{
					if (bDiag) Debug.WriteLine("  WARNUNG Plateaugrauwertedifferenz nur " + dPlateauGvalDiff.ToString("F1"));
				}
			}

			if (ErrorCode == 0)
			{
				// Kanten extrahieren mit Subpixelgenauigkeit:
				nNumEP = 0;		// reset number of found edges
				for (idx = 2; idx < nNumLP - 3; idx++)
				{
					// Kanten durch zweite Ableitung (Nulldurchgang) finden:
					if (linepoints[idx].derive2 * linepoints[idx + 1].derive2 < 0)
					{
						// gemittelter Anstieg im Intervall
						dSlope = (linepoints[idx].derive1 + linepoints[idx+1].derive1) / 2.0;
						
						// Nulldurchgang im Intervall gefunden
						if (Math.Abs(dSlope) < MinSlope)									// zu schwache Kante
						{
							if (bDiag) Debug.WriteLine("   edge at pos. {0} not strong enough: {1}", idx.ToString(), dSlope);
							idx++;
							continue;
						}

						if ((dSlope > 0.0) && (FindEdgeMode == TYPE_EDGE_DOWN))		// falsche Kantenrichtung
						{
							if (bDiag) Debug.WriteLine("   edge not TYPE_EDGE_DOWN  at pos. " + idx.ToString() + ", slope = " + dSlope.ToString());
							idx++;
							continue;
						}

						if ((dSlope < 0.0) && (FindEdgeMode == TYPE_EDGE_UP))			// falsche Kantenrichtung
						{
							if (bDiag) Debug.WriteLine("   edge not TYPE_EDGE_UP  at pos. " + idx.ToString() + ", slope = " + dSlope.ToString());
							idx++;
							continue;
						}

						// Kante mit ausreichender Steilheit gefunden, Nulldurchgang im Intervall aus linearer Interpolation bestimmen:
						dSlope = linepoints[idx].derive1;
						nNumEP++;

						double dSecDerive1 = linepoints[idx].derive2;
						double dSecDerive2 = linepoints[idx + 1].derive2;
						double dDeltaR = dSecDerive1 * dStepPix / (dSecDerive1 - dSecDerive2);	// Position des Nulldurchganges ab Intervallstart

						// Richtung der Kantennormalen:
						dAlpha = Math.Atan2(linepoints[idx + 1].Y - linepoints[idx].Y, linepoints[idx + 1].X - linepoints[idx].X);
						dSinAlpha = Math.Sin(dAlpha);
						dCosAlpha = Math.Cos(dAlpha);

						// gefundener Kantenpunkt:
						ptEdge.X = linepoints[idx].X + dCosAlpha * dDeltaR;
						ptEdge.Y = linepoints[idx].Y + dSinAlpha * dDeltaR;

						if (linepoints[idx].derive1 > 0.0)	nEdgeType = TYPE_EDGE_UP;		// Dunkel->Hell Kante
						else								nEdgeType = TYPE_EDGE_DOWN;		// Hell->Dunkel Kante

						if (bDiag)
						{
							Debug.WriteLine("  Edge found!");
							Debug.WriteLine("   Edge at idx = " + idx.ToString() + ": EdgeType = " + nEdgeType.ToString() + ", slope = " + dSlope.ToString("F1"));
//					Debug.WriteLine(_T("  Linepoint1 = (%.1f, %.1f) y' = %.1f\n"), linepoints[idx].x, linepoints[idx].y, linepoints[idx].derive1);
							Debug.WriteLine("   Edgepoint  = (" + ptEdge.X.ToString("F1") + ", " + ptEdge.Y.ToString("F1") + ")");
//					Debug.WriteLine(_T("  Linepoint2 = (%.1f, %.1f)\n"), linepoints[idx+1].x, linepoints[idx+1].y);
							Debug.WriteLine("  EdgeType = " + this.nEdgeType.ToString());
							Debug.WriteLine("  DeltaR   = " + dDeltaR.ToString("F1"));
						}
						break;
					}
				}

				if (bDiag) Debug.WriteLine("  m_nNumEP = " + nNumEP.ToString());

				if (nNumEP == 0)
				{
					sDiag = "  ERROR no valid edge found!";
					ErrorCode = ERROR_EDGE;
				}
			}

			if (ErrorCode != 0)
			{
				Debug.WriteLine("EdgeFinder.FindEdge(): ERROR " + sDiag);
			}
			return (ErrorCode == 0);
		} // end of FindEdge()



		/// <summary>
		/// Bilineare Grauwert-Interpolation über 4 Pixel:
		/// 		g0	g1
		/// 		g2	g3
		/// </summary>
		/// <param name="dX">x position of g0 in image</param>
		/// <param name="dY">y position of g0 in image</param>
		/// <param name="dGval">OUT interpolated grey value at position</param>
		/// <returns>true if o.k.</returns>
		bool InterpolateBilinear(double dX, double dY, out double dGval)
		{
			bool bResult = true;
			dGval = 0.0;
			// Position des ersten Umgebungspixel:
			int nX = (int)Math.Floor(dX);
			int nY = (int)Math.Floor(dY);

			if (!bNoParamCheck)
			{
				// check if points are inside the image
				// no param check if time is critical
				if ((nX < 1) || (nY < 1) || (nX > image.Width - 1) || (nY > image.Height - 1))
				{
					sDiag = "  EdgeFinder.InterpolateBilinear(): ERROR points out of image!";
					bResult = false;
				}
			}

			if (bResult)
			{
				// Grauwerte der Umgebungspixel:
				//float g0 = (float)*(m_pSrc + nY * nStride + nX);
				//float g1 = (float)*(m_pSrc + nY * nStride + nX + 1);
				//float g2 = (float)*(m_pSrc + (nY + 1) * nStride + nX);
				//float g3 = (float)*(m_pSrc + (nY + 1) * nStride + nX + 1);

				//Color color = image.GetPixel(nX, nY);
				//Debug.WriteLine("  RGB = " + color.R.ToString() + ", " + color.R.ToString() + ", " + color.R.ToString());
				//Debug.WriteLine("  HSB = " + color.GetHue().ToString() + ", " + color.GetSaturation().ToString() + ", " + color.GetBrightness().ToString());
				//  RGB = 133, 133, 133
				//  HSB = 0, 2,980232E-08, 0,5215687

				double g0 = (double)image.GetPixel(nX, nY).R;
				double g1 = (double)image.GetPixel(nX + 1, nY).R;
				double g2 = (double)image.GetPixel(nX, nY + 1).R;
				double g3 = (double)image.GetPixel(nX + 1, nY + 1).R;

				double h = dX - Math.Floor(dX);				// gebrochener Anteil horizontal
				double v = dY - Math.Floor(dY);				// gebrochener Anteil vertikal
				double p = g0 * (1.0 - h) * (1.0 - v)		// interpolierter Grauwert
					+ g1 * h * (1.0 - v)
					+ g2 * (1.0 - h) * v
					+ g3 * h * v;

				dGval = p;		// set resulting gval
			}
			// Debug.WriteLine("  InterpolateBilinear(" + dX.ToString("F1") + ", " + dY.ToString("F1") + ") --> " + dGval.ToString("F1"));
			return bResult;
		}


		/// <summary>
		/// Erste und zweite Ableitung des Linienpuffers bilden:
		/// </summary>
		/// <returns>true id success</returns>
		bool ComputeLineDerivates()
		{
			if (nNumLP < 10)
			{
				sDiag = string.Format(" EdgeFinder.ComputeLineDerivates(): ERROR searchline too short - length = {0}!", nNumLP.ToString());
				return false;
			}

			double dDerive1;
			double dDerive2;
			int n = nNumLP;
			int idx = 0;

			// erste Ableitung bilden
			for (idx = 2; idx < n - 2; idx++)
			{
				/*
  				// Faltung mit -1 -1 0 1 1
				dDerive1 = 	  linepoints[idx + 2].gval
							+ linepoints[idx + 1].gval
							- linepoints[idx - 1].gval
							- linepoints[idx - 2].gval;
				 */
				// Faltung mit -2 -1 0 1 2
				dDerive1 = 	  linepoints[idx + 2].gval
					+ linepoints[idx + 2].gval
					+ linepoints[idx + 1].gval
					- linepoints[idx - 1].gval
					- linepoints[idx - 2].gval
					- linepoints[idx - 2].gval;
				linepoints[idx].derive1 = dDerive1;
			}

			// zweite Ableitung bilden
			for (idx = 2; idx < n - 2; idx++)
			{
				/*
				// Faltung 1. Ableitung mit -1 -1 0 1 1
				dDerive2 = 	  linepoints[idx + 2].derive1
							+ linepoints[idx + 1].derive1
							- linepoints[idx - 1].derive1
							- linepoints[idx - 2].derive1;
				 */
				// Faltung 1. Ableitung mit -2 -1 0 1 2
				dDerive2 =    linepoints[idx + 2].derive1
					+ linepoints[idx + 2].derive1
					+ linepoints[idx + 1].derive1
					- linepoints[idx - 1].derive1
					- linepoints[idx - 2].derive1
					- linepoints[idx - 2].derive1;

				linepoints[idx].derive2 = dDerive2;
			}
			return true;
		}

		// Peak-Detektor - geschätzter Nulldurchgang (Subpixel) durch Parabel-Funktion y = x²
		// aus: Diplomarbeit Johann Sladczyk (Techn. Universität Dortmund)
		//		Kap. 5.4. Laserliniensegmentierung
		// funktioniert bei Minima und auch Maxima
		// Offset = 0.5 * (f(x-1) - f(x+1)) / (f(x+1) - 2.0 * f(x) + f(x-1));
		// Parameter:	Funktionswerte val_m1=f(x-1), Maximum / Minimum bei val = f(x), val_p1=f(x+1)
		// return:		Offset zu x (bei sehr schwachen Maxima wird 0.0 zurückgegeben)
		double ParabolicEstimation(double val_m1, double val, double val_p1)
		{
			double dEstimatedPositionOffset = 0.0;
			double divider = val_p1 - 2.0 * val + val_m1;
			if (Math.Abs(divider) > 0.001)							// avoid division by ZERO !!!
			{
				dEstimatedPositionOffset = 0.5 * (val_m1 - val_p1) / divider;
			}
			if (bDiag)
			{
				Debug.WriteLine(" ParabolicEstimation:");
				Debug.WriteLine("  f(x-1) = " + val_m1.ToString("F2"));
				Debug.WriteLine("  f(x)   = " + val.ToString("F2"));
				Debug.WriteLine("  f(x+1) = " + val_p1.ToString("F2"));
				Debug.WriteLine("  dEstimatedPositionOffset = " + dEstimatedPositionOffset.ToString("F2"));
			}
			return dEstimatedPositionOffset;
		}


		/// <summary>
		/// Ausgabe aller Parameters (Koordinaten, Grauwert oder Ableitungen) einer Suchlinie
		/// </summary>
		/// <returns>string mit allen Daten</returns>
		public string DebugSearchLine()
		{
			string sOut = "";
			StringBuilder sb = new StringBuilder();

			// Header
			sb.AppendLine("--- Suchlinie ---");
			sb.AppendLine(string.Format("SL-Name        : {0}", this.strName));
			sb.AppendLine(string.Format("Mindestanstieg : {0}", this.MinSlope.ToString("F1")) );
			
			// Tabelle
			sb.AppendLine(string.Format("{0}", "i; X;     Y;    gval; gval'; gval''"));
			for (int i = 0; i < nNumLP; i++)
			{
				sb.AppendLine(string.Format("{0}; {1}; {2}; {3}; {4}",
				                            i.ToString("D3"),
				                            linepoints[i].X.ToString("F1"),
				                            linepoints[i].Y.ToString("F1"),
				                            linepoints[i].gval.ToString("F1"),
				                            linepoints[i].derive1.ToString("F2"),
				                            linepoints[i].derive2.ToString("F2") ));
			}
			sOut = sb.ToString().Replace('.', ',');		// Germanize!!!
			return sOut;
		}

	}
}
