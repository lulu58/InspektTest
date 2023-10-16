/*
 * Benutzer: Lulu
 * Datum: 21.10.2015
 * 04.02.2023   add SetCueText() from CommonTools.cs
 * 16.10.2023   chg ComputeZoom() syntax and comments
 */

/// <summary>
/// SOUND, PICTUREBOX TOOLS, WinForm, WinAPI, Vollbild mit FormState
/// classes: GuiTools, WinAPI, FormState
/// </summary>

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;


namespace Visutronik.Commons
{
    /// <summary>
    /// singleton class GuiTools
    /// </summary>
    public sealed class GuiTools
    {
        private readonly static GuiTools instance = new GuiTools();
        public static GuiTools Instance { get { return instance; } }

        private GuiTools()
        {
        }

        #region ===== SOUND =====

        [DllImport("kernel32.dll")]
        public static extern bool Beep(int Frequenz, int Dauer);

        // gibt einen Alarmton aus
        public static void AlarmBeep()
        {
            for (int i = 0; i < 3; i++)
            {
                Beep(440, 250);
                Beep(880, 250);
            }
        }

        // gibt einen Warnton aus
        public static void WarnBeep()
        {
            Beep(1760, 250);
            Beep(880, 250);
        }

        // gibt einen einfachen beep aus
        public static void Beep()
        {
            Beep(880, 500);
        }

        #endregion ===== SOUND =====

        #region ===== PICTUREBOX TOOLS =====

        /// <summary>
        /// Berechnet den Zoomfaktor, wenn ein Bild in einem Control dargestellt wird
        /// </summary>
        /// <param name="controlSize">Größe des Controls</param>
        /// <param name="imageSize">Größe der Bitmap</param>
        /// <param name="dZoom">Rückgabewert: optimaler Zoomfaktor</param>
        /// <param name="ptZero">Rückgabewert: Position von Bitmap-Ursprung im Control</param>
        /// <returns>true wenn Berechnung o.k.</returns>
        public static bool ComputeZoom(System.Drawing.Size controlSize,
                                       System.Drawing.Size imageSize,
                                       out double dZoom,
                                       out System.Drawing.Point ptZero)
        {
            dZoom = 1.0;
            ptZero = new System.Drawing.Point(0, 0);

            if (controlSize.Width == 0) return false;
            if (imageSize.Width == 0) return false;

            double dRatioC = (double)controlSize.Height / (double)controlSize.Width;
            double dRatioI = (double)imageSize.Height / (double)imageSize.Width;
            double dZoomH = (double)controlSize.Width / (double)imageSize.Width;    // horizontal
            double dZoomV = (double)controlSize.Height / (double)imageSize.Height;  // vertikal
            int ZeroX, ZeroY;

            //Debug.WriteLine("ComputeZoom(): ControlSize = " + controlSize.ToString());
            //Debug.WriteLine("ComputeZoom(): ImageSize = "   + imageSize.ToString());
            //Debug.WriteLine("ComputeZoom(): RatioC = {0}, RatioI = {1}", dRatioC.ToString(), dRatioI.ToString());
            //Debug.WriteLine("ComputeZoom(): ZoomH = {0},  ZoomV = {1}", dZoomH.ToString(), dZoomV.ToString());

            // mit kleinstem Zoomfaktor weitermachen
            dZoom = dZoomH <= dZoomV ? dZoomH : dZoomV;

            if (dRatioC <= dRatioI)
            {
                // links und rechts freier Rand
                ZeroX = (int)(((double)controlSize.Width - dZoom * imageSize.Width) / 2.0);
                ZeroY = 0;
            }
            else
            {
                // oben und unten freier Rand
                ZeroX = 0;
                ZeroY = (int)(((double)controlSize.Height - dZoom * imageSize.Height) / 2.0);
            }

            // Image-Point[0,0] -> Control-Point[?, ?]
            ptZero.X = ZeroX; ptZero.Y = ZeroY;
            //Debug.WriteLine("  ptZero = " + ptZero.ToString());
            return true;
        }

        /// <summary>
        /// Berechnet optimale Fontgröße in Abhängigkeit von Bildgröße
        /// </summary>
        public static float ComputeFontsize(int imgsize,
                                            int min_imgsize,
                                            int max_imgsize,
                                            float min_fontsize,
                                            float max_fontsize)
        {
            //Debug.WriteLine("GuiTools.ComputeFontsize(): imgsize      = " + imgsize.ToString());
            //Debug.WriteLine("GuiTools.ComputeFontsize(): min_fontsize = " + min_fontsize.ToString());
            //Debug.WriteLine("GuiTools.ComputeFontsize(): max_fontsize = " + max_fontsize.ToString());

            float fontsize = min_fontsize;
            if (imgsize < min_imgsize)
            {
            }
            else if (imgsize > max_imgsize)
            {
                fontsize = max_fontsize;
            }
            else
            {
                float m = (max_fontsize - min_fontsize) / (max_imgsize - min_imgsize);
                float n = min_fontsize - m * min_imgsize;
                //Debug.WriteLine("GuiTools.ComputeFontsize(): m = " + m.ToString());
                //Debug.WriteLine("GuiTools.ComputeFontsize(): n = " + n.ToString());
                fontsize = m * imgsize + n;
            }
            return fontsize;
        }

        #endregion

        #region ===== WINFORM TOOLS =====

        //Imports the user32.dll
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam);


        /// <summary>
        /// Setzt Cue-Text für Textboxen - ab Windows Vista
        /// </summary>
        /// <param name="box"></param>
        /// <param name="cuetxt"></param>
        public static void SetCueText(TextBox box, string cuetxt)
        {
            SendMessage(box.Handle, 0x1500 + 1, IntPtr.Zero, cuetxt);
        }

        #endregion ===== WINFORM TOOLS =====

    }


    /// <summary>
    /// Selected Win API Function Calls
    /// http://www.vesic.org/english/blog/winforms/full-screen-maximize/
    /// </summary>

    public class WinApi
    {
        [DllImport("user32.dll", EntryPoint = "GetSystemMetrics")]
        public static extern int GetSystemMetrics(int which);

        [DllImport("user32.dll")]
        public static extern void
            SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter,
                         int X, int Y, int width, int height, uint flags);

        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;
        private readonly static IntPtr HWND_TOP = IntPtr.Zero;
        private const int SWP_SHOWWINDOW = 64; // 0x0040

        public static int ScreenX
        {
            get { return GetSystemMetrics(SM_CXSCREEN); }
        }

        public static int ScreenY
        {
            get { return GetSystemMetrics(SM_CYSCREEN); }
        }

        public static void SetWinFullScreen(IntPtr hwnd)
        {
            SetWindowPos(hwnd, HWND_TOP, 0, 0, ScreenX, ScreenY, SWP_SHOWWINDOW);
        }
    }


    /// <summary>
    /// Class used to preserve / restore state of the form
    /// http://www.vesic.org/english/blog/winforms/full-screen-maximize/
    /// Hinweis von Lulu:
    /// mit "targetForm.TopMost = true"
    /// werden Dialoge im Hintergrund geöffnet und die GUI wird unbedienbar!!!
    /// Damit zu Programmstart im Window-Modus alles normal angezeigt wird,
    /// wurden winState und brdStyle entsprechend initialisiert.
    /// </summary>
    public class FormState
    {
        private FormWindowState winState = FormWindowState.Normal;
        private FormBorderStyle brdStyle = FormBorderStyle.Sizable;
        //private bool topMost = false;
        private System.Drawing.Rectangle bounds;

        private bool IsMaximized = false;

        public void Maximize(Form targetForm)
        {
            if (!IsMaximized)
            {
                IsMaximized = true;
                Save(targetForm);
                targetForm.WindowState = FormWindowState.Maximized;
                targetForm.FormBorderStyle = FormBorderStyle.None;
                //targetForm.TopMost = true;
                WinApi.SetWinFullScreen(targetForm.Handle);
            }
        }

        public void Save(Form targetForm)
        {
            winState = targetForm.WindowState;
            brdStyle = targetForm.FormBorderStyle;
            //topMost = targetForm.TopMost;
            bounds = targetForm.Bounds;
        }

        public void Restore(Form targetForm)
        {
            targetForm.WindowState = winState;
            targetForm.FormBorderStyle = brdStyle;
            //targetForm.TopMost = topMost;
            targetForm.Bounds = bounds;
            IsMaximized = false;
        }


    }
}
