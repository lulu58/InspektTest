// Testumgebung für Checker und Prüflisten werden (statt ElHaInspect)
// viel Code von 2022_Würth übernehmbar!!!
//
// 08.01.2022   1.0.0.1 
// 09.01.2022   1.0.0.2
// 19.01.2023   1.0.0.3 add OverlayDrawCheckerCircle, OverlayDrawCheckerRect
// 21.01.2023   1.0.0.4 add class InspectionHelper, create new checkerList from GUI ok
// 23.01.2023   1.0.0.5 chg/add in namespace Visutronik.Imaging
// 24.01.2023   1.0.0.6 content of class Instuctions moved to class InspectModel
// 27.01.2023   1.0.0.7 Settings.Save() -> C:\ProgramData\Visutronik GmbH\InspektTest\ProgSettings.xml
// 31.01.2023   1.0.0.8 chg DlgInstruction, add InstructionEnums.cs
// 03.02.2023   1.0.0.9 viel geändert!!!
// 16.10.2023   1.0.1.0 GIT-Version, chg mouse operations
// 17.10.2023   1.0.1.0 viel geändert!!!

//TODO checker property dialog -> DlgInstruction, change checker size & position

using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Visutronik.Imaging;
using Visutronik.Inspektion;
using Visutronik.Commons;
using System.Drawing.Text;

namespace Visutronik.InspektTest
{
    /// <summary>
    /// Application Main Form
    /// </summary>
    public partial class MainForm : Form
    {
        #region ===== const and vars ====================================================================

        const bool DEBUG_GUI_INIT = false;    // 
        const bool DEBUG_OVL_DRAW = true;
        const bool DEBUG_PICTBOX1 = true;

        const string PROG_NAME = "InspektTest";
        const string PROG_VERSION = "1.0.1.1";
        const string PROG_VENDOR = "Visutronik GmbH";

        // class instances
        readonly ProgSettings settings = ProgSettings.Instance;
        readonly InspectModel model = new InspectModel();
        readonly CommonApplicationData appData = new CommonApplicationData(PROG_VENDOR, PROG_NAME);

        readonly FormState formState = new FormState();
        readonly Logger logger = new Logger();

        System.Drawing.Bitmap imgSource;
        System.Drawing.Size imgSize = new System.Drawing.Size(0, 0);
        readonly Overlay ovl = new Overlay();
        readonly Overlay ovl2 = new Overlay();   // for drawing in SetupMode
        private bool imgLoaded = false;
        private double dPictZoom = 1.0;             // PictureBox-Zoomfaktor, abhängig von Bild- und PictureBox-Größe
        private System.Drawing.Point ptMousePos = new System.Drawing.Point(0, 0);   // aktuelle Mausposition bei Click
        private System.Drawing.Point ptZero = new System.Drawing.Point(0, 0);       // Nullpunkt des Bildes in PictureBox


        // TODO 3 Modi: Inspect, ModifyChecker, AddChecker
        private int ProgMode = 0;
        private bool SetupMode = false;
        private bool ModifyMode = false;

        private InstructionParams currentInstruction = null;

        private delegate void SetStringCallback(string str);
        private delegate void SetImageCallback(Bitmap img);

        readonly Dictionary<int, string> ProgModeDict = new Dictionary<int, string>()
        {
            {0, "Inspektion" },
            {1, "Bearbeitung" },
            {2, "Setup" }
        };

        private Color[] checkercolor = { Color.Yellow, Color.Pink, Color.Green };

        // Verzeichnisse
        const string STR_PROJECTPATH = @"D:\temp\JuS";
        const string STR_PARAMPATH = "Parameter";
        const string STR_PRODUKTPATH = "Produkte";
        const string STR_LOGPATH = "Log";
        const string STR_INITPATH = "Init";
        const string STR_IMAGEPATH = "Bilder";

        string strProjektPfad   = "";
        string strParamPfad     = "";
        string strProduktPfad   = "";
        string strLogPfad       = "";
        string strInitPfad      = "";
        string strBilderPfad    = "";

        #endregion

        #region ===== form and form ctrl handlers =======================================================

        #region --- mainform load & close ----------------------------------

        public MainForm()
        {
            InitializeComponent();
            this.Text = PROG_NAME + " V" + PROG_VERSION;

            model.ImageLoaded += OnModelImageLoaded;
            model.ShowCheckerRect += OverlayDrawCheckerRect;
            model.ShowCheckerCircle += OverlayDrawCheckerCircle;
            model.OnOperationReady += OperationReady;

        }

        private void OperationReady(InstructionParams instruction, bool result)
        {
            Debug.WriteLine("OperationReady: " + result);
        }

        /// <summary>
        /// Event: FormLoad
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFormLoad(object sender, EventArgs e)
        {
            bool result = true;

            settings.SetSettingsPath(appData.ApplicationFolderPath);
            if (!settings.LoadSettings())
            {
                MessageBox.Show("Fehler beim Laden der Programmeinstellungen!", this.Text);
            }
            else
            {
                result &= FileTools.EnsureFolderExistAsync(settings.LogFolder);
                result &= FileTools.EnsureFolderExistAsync(settings.ImageFolder);
                result &= FileTools.EnsureFolderExistAsync(settings.InstructionFolder);
            }

            if (result)
            {
                logger.SetFolder(settings.LogFolder);
                logger.Enabled = true;
            }
            else
            {
                logger.Enabled = false;
                // alternative Pfade erzeugen:
                if (CheckVerzeichnisse(settings.ProjFolder))
                {
                    settings.LogFolder = strLogPfad;
                    settings.ImageFolder = strBilderPfad;
                    settings.InstructionFolder = strParamPfad;
                    settings.SaveSettings();
                    logger.SetFolder(settings.LogFolder);
                    logger.Enabled = true;
                }
            }


            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;

            foreach (var pm in ProgModeDict)
            {
                cbxMode.Items.Add($"{pm.Key} : {pm.Value}");
            }
            cbxMode.SelectedIndex = 0;

            if (settings.Fullscreen) SetFullscreen(true);
        }

        /// <summary>
        /// Event: FormClosing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (!settings.SaveSettings())
            {
                MessageBox.Show("Fehler beim Speichern der Programmeinstellungen!");
            }
        }

        /// <summary>
        /// Event: FormResize
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFormResize(object sender, EventArgs e)
        {
            Debug.WriteLine("OnFormResize() ...");
        }

        // callback method
        private void OnModelImageLoaded(Bitmap img)
        {
            ovl.SetOverlay(img);
            ovl2.SetOverlay(img);
            this.imgSource = img;
            this.imgSize = img.Size;    // wichtig für PictureBox1 <---> imgSource
            this.imgLoaded = true;
            ComputeZoomAndZeroPoint(pictureBox1.ClientSize, imgSource.Size);

            Debug.WriteLine("Model_ImageLoaded");
            //ShowImage();
        }

        #endregion

        #region --- menu handlers ------------------------------------------

        /// <summary>
        /// Lade Bild aus Datei
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMenuBildLaden_Click(object sender, EventArgs e)
        {
            // @"d:\Temp\JUS\Kamerabilder\3a.bmp"
            // @"d:\Temp\JUS\Kamerabilder\Lage_3+4\3a.bmp"
            LoadImage();
        }

        /// <summary>
        /// Load instruction list from file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMenuListeLaden_Click(object sender, EventArgs e)
        {
            ListeLaden();
        }

        private void OnMenuListeSpeichern_Click(object sender, EventArgs e)
        {
            ListeSpeichern();
        }


        private void OnEinstellungen_Click(object sender, EventArgs e)
        {
            Einstellungen();
        }


        private void OnMenuExit_Click(object sender, EventArgs e)
        {
            Close();
        }


        private void OnVollbild_Click(object sender, EventArgs e)
        {
            UmschaltungVollbild();

            if (settings.Fullscreen)
                vollbildToolStripMenuItem.Text = "&Normales Fenster";
            else
                vollbildToolStripMenuItem.Text = "&Ganzer Bildschirm";
        }


        /// <summary>
        /// Instruktion-Liste anzeigen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnListeAnzeigen_Click(object sender, EventArgs e)
        {
            ShowInstructions();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHelp_Click(object sender, EventArgs e)
        {
            ShowHelp();
        }

        /// <summary>
        /// Toolstrip: Listbox "Modus" Selektion geändert
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Umschaltung Inspektion - Einrichtung
            if (sender is ToolStripComboBox cbx)
            {
                ProgMode = cbx.SelectedIndex;
                ModifyMode = (ProgMode == 1);
                SetupMode = (ProgMode == 2);
                Debug.WriteLine($"Changed: ProgMode = {ProgMode}, ModifyMode = {ModifyMode}, SetupMode = {SetupMode}");
            }
        }

        #endregion

        #region --- GUI button handlers ------------------------------------

        private void BtnListeLaden_Click(object sender, EventArgs e)
        {
            ListeLaden();
        }
        private void BtnListeSpeichern_Click(object sender, EventArgs e)
        {
            ListeSpeichern();
        }

        // Neue (leere) Anweisungsliste erstellen
        private void BtnListeNeu_Click(object sender, EventArgs e)
        {
            if (SetupMode)
            {
                DiagBox("Neue Anweisungsliste");
                model.CreateNewInstructions();
            }
            else
            {
                DiagBox("neue Test-Anweisungsliste");
                model.CreateTestInstructions();

                foreach (var o in model)
                {
                    Debug.WriteLine(o.ToString()); // "Visutronik.Inspektion.Instruction"
                    Debug.WriteLine(o.OperationParams);
                }
            }
        }

        /// <summary>
        /// App beenden und Einstellungen speichern
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnEnde_Click(object sender, EventArgs e)
        {
            model.StopInspect();
            model.CloseInspect();

            Close();
        }

        /// <summary>
        /// hier kommt alles zusammengeschmissen:
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnTest_Click(object sender, EventArgs e)
        {
            DoTheTest();

            //ShowImage();    // after DoTheTest

            //model.ShowCheckerRect += OverlayDrawCheckerRect;
            //model.Inspect();
            //DiagBox(model.LastError);
        }

        #endregion

        #region --- overlay drawing methods --------------------------------

        private void OverlayDrawCheckerRect(RectangleF rect, int colorindex)
        {
            Debug.WriteLineIf(DEBUG_OVL_DRAW, $"OverlayDrawCheckerRect(rect={rect}, col={colorindex})");
            System.Drawing.Color color = checkercolor[colorindex];
            ovl?.DrawRectangleF(rect, color);
            PointF ptAnfasser = new PointF(rect.X, rect.Y);
            ovl?.DrawCircleMarker(ptAnfasser, color);
            ptAnfasser.X += rect.Width;
            ptAnfasser.Y += rect.Height;
            ovl?.DrawCircleMarker(ptAnfasser, color);
        }

        private void OverlayDrawCheckerCircle(CircleF circle, int colorindex)
        {
            Debug.WriteLineIf(DEBUG_OVL_DRAW, $"OverlayDrawCheckerCircle: center={circle.center}, r={circle.radius}, col={colorindex}");
            System.Drawing.Color color = checkercolor[colorindex];

            ovl?.DrawCircle(circle.center, circle.radius, color);

            PointF ptAnfasser = circle.center;
            ovl?.DrawCircleMarker(ptAnfasser, color);

            ptAnfasser.X += circle.radius;
            ovl?.DrawCircleMarker(ptAnfasser, color);
        }

        private void OverlayDrawCheckerLine(LineF line, int colorindex)
        {
            Debug.WriteLineIf(DEBUG_OVL_DRAW, $"OverlayDrawCheckerLine: {line.P1}, {line.P2}, col={colorindex}");
            System.Drawing.Color color = checkercolor[colorindex];

            ovl?.DrawLine(line.P1, line.P2, color);

            PointF ptAnfasser = line.P1;
            ovl?.DrawCircleMarker(ptAnfasser, color);

            ptAnfasser = line.P2;
            ovl?.DrawCircleMarker(ptAnfasser, color);
        }

        #endregion

        #endregion === form handlers ===

        #region ===== picturebox methods ================================================================

        /// <summary>
        /// PictureBox1 SizeChanged event handler
        /// Neuberechnung des Picturebox-Zooms und Image-Nullpunktes
        /// </summary>
        private void PictureBox1_SizeChanged(object sender, EventArgs e)
        {
            Debug.WriteLineIf(DEBUG_PICTBOX1, "PictureBox1 SizeChanged to " + this.pictureBox1.Size.ToString());
            Debug.WriteLineIf(DEBUG_PICTBOX1, "PictureBox1 ClientSize = " + this.pictureBox1.ClientSize.ToString());
            if (imgSource != null)
            {
                this.ComputeZoomAndZeroPoint(pictureBox1.ClientSize, imgSource.Size);
            }
        }

        /// <summary>
        /// Bild in Picturebox anzeigen
        /// Wenn Checkerliste geladen ist, Checker anzeigen
        /// Im Setup-Mode neu erzeugtes Checker-Rechteck anzeigen
        /// </summary>
        private void ShowImage()
        {
            if (imgSource == null)
            {
                Debug.WriteLine("ShowImage(): no src image loaded!");
                return;
            }

            Bitmap imageToShow = imgSource;
            if (model.DrawCheckersToOverlay())
            {
                // show the composed image
                imageToShow = ovl.GetImageWithOverlay(imageToShow);
                //Bitmap imgSrcWithOvl = ovl.GetImageWithOverlay(imgSource);
                //pictureBox1.Image = imgSrcWithOvl;
            }

            if (SetupMode)
            {
                // zusätzlich in ovl2 erzeugte neue Objekte anzeigen
                imageToShow = ovl2.GetImageWithOverlay(imageToShow);
            }

            pictureBox1.Image = imageToShow;
        }

        #region --- picturebox1 mouse work ------------------------------

        System.Drawing.Point ptMouseDown;
        System.Drawing.Point ptMouseUp;
        bool IsCreatingChecker = false;
        bool mouseMoved = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (!imgLoaded) return;

            PictureBox picbox = (PictureBox)sender;
            System.Drawing.Point posMouse = e.Location;

            // Maus- in Bildkoordinaten wandeln:
            if (picbox.SizeMode == PictureBoxSizeMode.Zoom)
            {
                ptMouseDown = this.PictureboxZuBild(posMouse);
            }
            else
            {
                ptMouseDown = posMouse;
            }
            Debug.WriteLineIf(DEBUG_PICTBOX1, $"ptMouseDown = {ptMouseDown}");

            mouseMoved = false;

            if (SetupMode || ModifyMode)
            {
                // TODO explore current checker
            }

            if (SetupMode)
            {
                IsCreatingChecker = true;
                ovl2.ClearOverlay();
            }
            else if (ModifyMode)
            {
                // Position des Checkers 
            }
            else
            {
                // Inspect-Mode: Maus ignorieren
            }
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (!imgLoaded) return;

            PictureBox picbox = (PictureBox)sender;
            System.Drawing.Point posMouse = e.Location;

            // Maus- in Bildkoordinaten wandeln:
            if (picbox.SizeMode == PictureBoxSizeMode.Zoom)
            {
                ptMouseUp = this.PictureboxZuBild(posMouse);
            }
            else
            {
                ptMouseUp = posMouse;
            }
            Debug.WriteLineIf(DEBUG_PICTBOX1, $"{posMouse} -> ptMouseUp = {ptMouseUp} ");

            if (SetupMode && mouseMoved)    // Setup mode
            {
                IsCreatingChecker = false;
                Rectangle r = InstructionHelper.GetNormalizedRectangleFromPoints(ptMouseDown, ptMouseUp);

                //ovl2.ClearOverlay();
                //ovl2.DrawRectangle(r, Color.Green);
                //ShowImage();    // in setup mode

                // NewChecker-Dialog - Setzen der weiteren Parameter
                if (JaNeinAntwortBox("Neuen Checker anlegen"))
                {
                    int count = model.instructionList.Count;
                    // TODO camidx abhängig von tabIndex
                    int camidx = 0;
                    DlgInstruction dlg = new DlgInstruction(r, camidx, count + 1);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        if (!model.AddInstruction(dlg.GetInstruction()))
                        {
                            MsgBox("AddInstruction: Fehler!");
                        }
                        ovl.ClearOverlay();
                    }
                }
                ovl2.ClearOverlay();
                ShowImage();  // mouse up in setup mode when mouse was moved
            }
            else if (ModifyMode)
            {
                // Bearbeitungsmodus
                // TODO prüfen, ob Maus in vorhandenem Checker ist...
                // wenn ja, Checker selektiern und evtl. Parameter bearbeiten
                // TODO prüfen, ob Maus in vorhandenem Checker ist...
                if (model.FindObject(ptMouseDown, out currentInstruction))
                {
                    Rectangle r = InstructionHelper.GetNormalizedRectangleFromPoints(ptMouseDown, ptMouseUp);

                    DiagBox(currentInstruction.Name);
                    DlgInstruction dlg = new DlgInstruction(currentInstruction, r);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {

                    }
                }
            }
            else
            {
                // Inspekt-Modus, Maus ignorieren
            }
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!imgLoaded) return;

            PictureBox picbox = (PictureBox)sender;
            System.Drawing.Point posMouse = e.Location;

            if (SetupMode && IsCreatingChecker)
            {
                // Maus- in Bildkoordinaten wandeln:
                if (picbox.SizeMode == PictureBoxSizeMode.Zoom)
                {
                    ptMousePos = this.PictureboxZuBild(posMouse);
                }
                else
                {
                    ptMousePos = posMouse;
                }
                //Debug.WriteLineIf(DEBUG_PICTBOX1, $"ptMousePos = {ptMousePos}");
                mouseMoved = true;

                Rectangle r = InstructionHelper.GetNormalizedRectangleFromPoints(ptMouseDown, ptMousePos);
                ovl2.ClearOverlay();
                ovl2.DrawRectangle(r, Color.Yellow);
                ShowImage();    // mouse move in setup mode
            }
        }

        /// <summary>
        /// Mausclick mit rechter Maustaste im Bild soll Checker markieren...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (!imgLoaded) return;

            PictureBox picbox = (PictureBox)sender;
            System.Drawing.Point posMouse = e.Location;
            System.Drawing.Point ptImage;

            // Maus- in Bildkoordinaten wandeln:
            if (picbox.SizeMode == PictureBoxSizeMode.Zoom)
            {
                ptImage = this.PictureboxZuBild(posMouse);
            }
            else
            {
                ptImage = posMouse;
            }
            Debug.WriteLine($"Clicked ptImage = {ptImage}");

            if (!SetupMode)
            {
                if (e.Button == MouseButtons.Right)
                {
                    model.FindObject(ptImage, out currentInstruction);
                    if (currentInstruction != null)
                    {
                        Debug.WriteLine("Mouseclick is in " + currentInstruction.Name);
                        DiagBox($"{currentInstruction.Number}: {currentInstruction.Name}");

                        model.UnselectAllObjects();
                        model.SelectObject(currentInstruction.Number);

                        // PictureBox neu zeichnen:

                        ShowImage();    // in setup mode
                    }
                }
            }
        }

        #endregion --- picturebox1 mouse work ---

        #endregion

        #region ===== Helper methods ====================================================================


        private void LoadImage()
        {
            string imgpath = settings.LastImage;
            Debug.WriteLine("From ProgSettings: " + imgpath);

            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "Image files (*.bmp)|*.bmp" + "|all files (*.*)|*.*",
                InitialDirectory = settings.ImageFolder,
                DefaultExt = "bmp",
                FileName = imgpath,
                Multiselect = false,
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                imgpath = ofd.FileName;

                if (model.LoadImageFromFile(imgpath))
                {
                    settings.LastImage = imgpath;
                    // srcImage und Overlays werden in callback Model_ImageLoaded() erzeugt!
                    ShowImage();    // after load image
                }
                else
                {
                    DiagBox("Fehler: kein Bild geladen!");
                }
            }
        }


        private bool LoadSourceImage(string path)
        {
            try
            {
                imgSource = (Bitmap)Image.FromFile(path);
                this.imgSize = imgSource.Size;
                Debug.WriteLine($"LoadSourceImage: {path}, image size = {imgSize}");

                this.ComputeZoomAndZeroPoint(pictureBox1.ClientSize, imgSize);

                // create empty overlay bitmaps with size of imgSource:
                ovl.SetOverlay(imgSource);
                ovl2.SetOverlay(imgSource);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("LoadSourceImage: " + ex.Message);
                return false;
            }
            return true;
        }


        /// <summary>
        /// Auswahl und Laden der Anweisungsliste aus Datei
        /// </summary>
        private void ListeLaden()
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "Json files (*.json)|*.json" + "|all files (*.*)|*.*",
                InitialDirectory = settings.InstructionFolder,
                DefaultExt = "json",
                FileName = model.InstructionFile,
                Multiselect = false,
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                model.InstructionFile = ofd.FileName;
                if (model.LoadInstructionsFromFile())
                {
                    DiagBox("Anweisungen geladen:");
                    DiagBox(model.InstructionFile);

                    settings.LastInstruction = model.InstructionFile;

                    if (imgSource != null)
                    {
                        ovl.ClearOverlay();
                        ovl2.ClearOverlay();

                        model.DrawCheckersToOverlay();  // Checkers in Overlay anzeigen:
                        ShowImage();          // Bild mit Overlay anzeigen (load instructions)
                    }
                    else DiagBox("Warnung: noch kein Kamerabild!");
                }
                else
                {
                    DiagBox("Fehler beim Laden der Liste:");
                    DiagBox(model.LastError);
                }
            }
        }

        /// <summary>
        /// Speichern der Anweisungsliste in Datei
        /// </summary>
        private void ListeSpeichern()
        {
            SaveFileDialog sfd = new SaveFileDialog()
            {
                Filter = "Json file (*.json)|*.json" + "|all files (*.*)|*.*",
                InitialDirectory = settings.InstructionFolder,
                DefaultExt = "json",
                FileName = model.InstructionFile,
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                model.InstructionFile = sfd.FileName;
                if (model.SaveInstructionsToFile())
                {
                    DiagBox("Anweisungen gespeichert:");
                    DiagBox(model.InstructionFile);
                    settings.LastInstruction = model.InstructionFile;
                }
                else
                {
                    DiagBox("Fehler beim Speichern:");
                    DiagBox(model.LastError);
                }
            }
        }

        /// <summary>
        /// Programmeinstellungen anpassen
        /// </summary>
        private void Einstellungen()
        {
            //throw new NotImplementedException();
            DiagBox("Einstellungen nicht implementiert");
            //TODO Einstellungen
        }


        private void DoTheTest()
        {

            DiagBox("Do the test...");
            bool result;
            DiagBox("1. Bild laden");
            DiagBox($"    {settings.LastImage}");
            result = model.LoadImageFromFile(settings.LastImage);
            if (!result) { DiagBox("Fehler!"); return; }

            DiagBox("2. Anweisungsliste laden");
            DiagBox($"    {settings.LastInstruction}");
            //model.CreateTestInstructions();
            result = model.LoadInstructionsFromFile(settings.LastInstruction);
            if (!result) { DiagBox("Fehler!"); return; }

            // test some drawing to overlay bitmap:
            if (ovl != null)
            {
                Color drawColor = Color.Blue;
                //ovl.SetOverlay(imgSource);

                //ovl.DrawRectangle(new Rectangle(20, 20, 200, 130), drawColor);       // internal drawing methods
                //ovl.DrawArrow(new Point(100, 400), new Point(600, 400), drawColor, false, true);
                //ovl.DrawStringCentric("Do the test", new Point(imgSource.Width / 2, 30), drawColor, 
                //    new Font(FontFamily.GenericSansSerif, 36.0f));
            }

            DiagBox("3. Test ausführen");
            result = model.Inspect();
            if (!result) { DiagBox("Fehler!"); return; }

            DiagBox("4. Testergebnis anzeigen");

            string strErgebnis = result ? "Test erfolgreich" : "Fehler: " + model.LastError;
            DiagBox(strErgebnis);
            DiagBox("- - - - - - - - - -");

            //Rectangle testchecker = new Rectangle(350, 200, 200, 150);
            //ovl.DrawRectangle(new Rectangle(20, 20, 200, 130), Color.LimeGreen);
            //ShowImage();

            //DlgInstruction dlg = new DlgInstruction(testchecker);
            //if (dlg.ShowDialog() == DialogResult.OK)
            //{
            //    if (!model.AddInstruction(dlg.GetInstruction()))
            //    {
            //        MsgBox("AddInstruction: Fehler!");
            //    }
            //    ovl.ClearOverlay();
            //}


            if (ovl != null)
            {
                ovl.DrawStringCentric(
                    strErgebnis,
                    new Point(imgSource.Width / 2, imgSource.Height - 50),
                    Color.Red, new Font(FontFamily.GenericSansSerif, 36.0f));
            }
            ShowImage();    // after DoTheTest
        }

        /// <summary>
        /// zeigt Anweisungsliste in DiagBox
        /// </summary>
        private void ShowInstructions()
        {
            if (model.HasInstructionList())
            {
                // TODO Dialog DlgInstructionList anzeigen
                DiagBoxClear();
                foreach (var i in model)
                {
                    string s = string.Format($"{i.Number} : {i.Name} - {i.Description}");
                    DiagBox(s);
                }
            }
            else
            {
                DiagBox("Keine Instruktionsliste!");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ShowHelp()
        {
            DiagBoxClear();
            DiagBox("Noch keine Hilfe!");
        }

        #endregion

        #region ===== GUI helper methods ================================================================

        /// <summary>
        /// Buttons usw. je nach Programmstatus freigeben
        /// </summary>
        private void UpdateControls()
        {
            //ENUM_STATUS current = programmstatus.Status;
            //switch (current)
            //{
            //    case ENUM_STATUS.ST_BEREIT:
            //        this.btnMessStart.Enabled = true;
            //        this.btnMessSpeichern.Enabled = false;
            //        this.btnMessAbbruch.Enabled = false;
            //        this.cbMessZoom.Enabled = true;
            //        this.btnBelichtungPlus.Enabled = true;
            //        this.btnBelichtungMinus.Enabled = true;
            //        this.btnKalibStart.Enabled = true;
            //        this.btnKalibSpeichern.Enabled = false;
            //        this.btnKalibAbbruch.Enabled = false;

            //        break;
            //    case ENUM_STATUS.ST_MESSUNG_START:
            //        this.btnMessStart.Enabled = false;
            //        this.btnMessSpeichern.Enabled = false;
            //        this.btnMessAbbruch.Enabled = true;
            //        this.btnBelichtungPlus.Enabled = false;
            //        this.btnBelichtungMinus.Enabled = false;
            //        this.cbMessZoom.Enabled = false;
            //        break;
            //    case ENUM_STATUS.ST_MESSUNG:
            //        this.btnMessStart.Enabled = false;
            //        this.btnMessSpeichern.Enabled = false;
            //        this.btnMessAbbruch.Enabled = true;
            //        break;
            //    case ENUM_STATUS.ST_MESSUNG_SPEICHERN:
            //        this.btnMessStart.Enabled = false;
            //        this.btnMessSpeichern.Enabled = true;
            //        this.btnMessAbbruch.Enabled = true;
            //        break;

            //    case ENUM_STATUS.ST_KALIB_START:
            //        this.btnKalibStart.Enabled = false;
            //        this.btnKalibSpeichern.Enabled = true;      // auch manuell eingetragene Werte können gespeichert werden!
            //        this.btnKalibAbbruch.Enabled = true;
            //        break;
            //    case ENUM_STATUS.ST_KALIB:
            //        this.btnKalibStart.Enabled = false;
            //        this.btnKalibSpeichern.Enabled = false;
            //        this.btnKalibAbbruch.Enabled = true;
            //        break;
            //    case ENUM_STATUS.ST_KALIB_SPEICHERN:
            //        this.btnKalibStart.Enabled = false;
            //        this.btnKalibSpeichern.Enabled = true;
            //        this.btnKalibAbbruch.Enabled = true;
            //        break;

            //    case ENUM_STATUS.ST_ABBRUCH:
            //        this.btnMessStart.Enabled = true;
            //        this.btnMessSpeichern.Enabled = false;
            //        this.btnMessAbbruch.Enabled = false;
            //        this.cbMessZoom.Enabled = true;
            //        this.btnBelichtungPlus.Enabled = true;
            //        this.btnBelichtungMinus.Enabled = true;

            //        this.btnKalibStart.Enabled = true;
            //        this.btnKalibSpeichern.Enabled = false;
            //        this.btnKalibAbbruch.Enabled = false;
            //        break;
            //}

            // Button für Speicherung aktivieren
#if !DEBUG
            //this.btnMessSpeichern.Enabled = (nBenutzerStufe > -1);
#endif

            // Messmodus:
            //int mm = this.settings.MessModus;
            //this.leiterbreiteToolStripMenuItem.Checked = (mm == MESSMODUS_STRIP);
            //this.linie1ToolStripMenuItem.Checked = (mm == MESSMODUS_LINIE1);
            //this.linie2ToolStripMenuItem.Checked = (mm == MESSMODUS_LINIE2);
            //this.radiusToolStripMenuItem.Checked = (mm == MESSMODUS_RADIUS);
            //this.winkelToolStripMenuItem.Checked = (mm == MESSMODUS_WINKEL);
            //this.SetStatusLabelMessModus(strMessmodus[mm]);

            //this.SetStatusLabelBenutzer("angemeldet: " + strBenutzer);

            // Benutzerabhängige Rechte:
            //Debug.WriteLine(" - {0} - Stufe {1}", strBenutzer, nBenutzerStufe);
            //switch (nBenutzerStufe)
            //{
            //    case -1: // abgemeldet
            //        this.tabPageEinstellungen.Enabled = false;
            //        this.tabPageKalibrierung.Enabled = false;
            //        break;
            //    case 0: // normal
            //        this.tabPageEinstellungen.Enabled = false;
            //        this.tabPageKalibrierung.Enabled = false;
            //        break;
            //    case 1: // eingeweiht - darf kalibrieren
            //        this.tabPageEinstellungen.Enabled = false;
            //        this.tabPageKalibrierung.Enabled = true;
            //        break;
            //    case 2: // chef - darf verwalten
            //        this.tabPageEinstellungen.Enabled = true;
            //        this.tabPageKalibrierung.Enabled = true;
            //        break;
            //    case 3: // superguru mit hintertürchen
            //        this.tabPageEinstellungen.Enabled = true;
            //        this.tabPageKalibrierung.Enabled = true;
            //        break;
            //}
        }

        /// <summary>
        /// Vollbilddarstellung aktivieren / deaktivieren
        /// </summary>
        /// <param name="full">true to show in fullscreen mode</param>
        private void SetFullscreen(bool full)
        {
            if (full)
            {
                Debug.WriteLineIf(DEBUG_GUI_INIT, "FULLSCREEN-Mode");
                formState.Maximize(this);
            }
            else
            {
                Debug.WriteLineIf(DEBUG_GUI_INIT, "WINDOW-Mode");
                formState.Restore(this);
            }
        }

        void MsgBox(string msg)
        {
            Debug.WriteLine("MsgBox: " + msg);
            MessageBox.Show(msg, this.Text);
        }

        /// <summary>
        /// Dialogbox mit Ja/Nein-Auswahlmöglichkeit
        /// </summary>
        /// <param name="msg">anzuzeigende Frage</param>
        /// <returns><c>bool</c>true wenn "Ja" selektiert wurde</returns>
        bool JaNeinAntwortBox(string msg)
        {
            Debug.WriteLine("JaNeinAntwortBox(): " + msg);
            DialogResult result = MessageBox.Show(msg, this.Text,
                                                  MessageBoxButtons.YesNo,
                                                  MessageBoxIcon.Question,
                                                  MessageBoxDefaultButton.Button1);
            Debug.WriteLine("JaNeinAntwortBox() Antwort = " + result.ToString());
            return (result == DialogResult.Yes);
        }

        /// <summary>
        /// Stringausgabe in Listbox listBox1, threadsafe
        /// </summary>
        /// <param name="msg">string to append to listbox</param>
        void DiagBox(string msg)
        {
            int MAX_LBX_ITEMS = 250;

            // am Ende einfügen und den letzten Eintrag sichtbar machen:
            //listBox1.Items.Add(msg);
            //listBox1.SelectedIndex = listBox1.Items.Count - 1;

            // immer ersten Eintrag sichtbar machen, z.B. für Fehlerlog:
            // lbDiagnose.Items.Insert(0, msg);
            // lbDiagnose.SelectedIndex = -1;

            listBox1.BeginInvoke(new Action(() =>
            {
                if (listBox1.Items.Count > MAX_LBX_ITEMS) listBox1.Items.RemoveAt(0);
                listBox1.Items.Add(msg);

                //scroll to last item and highlight it
                //listBox1.SelectedIndex = listBox1.Items.Count - 1;

                // If you only want to scroll to the bottom and not change user's selection:
                listBox1.TopIndex = listBox1.Items.Count - 1;
            }));

            Debug.WriteLine(msg);
        }

        /// <summary>
        /// Ausgabe-Listbox löschen
        /// </summary>
        void DiagBoxClear()
        {
            listBox1.Items.Clear();
        }

        /// <summary>
        /// Statuslabel setzen
        /// </summary>
        /// <param name="s"></param>
        void SetStatusLabelProgrammStatus(string s)
        {
            this.StatusLabel1.Text = s;
        }

        /// <summary>
        /// Bildzustand anzeigen (threadsafe)
        /// </summary>
        /// <param name="s">Status label text</param>
        void SetStatusLabelBildzustand(string s)
        {
            if (this.statusStrip1.InvokeRequired)
            {
                this.statusStrip1.Invoke(new SetStringCallback(SetStatusLabelBildzustand), new object[] { s });
            }
            else
            {
                StatusLabel2.Text = s;
            }
        }

        void SetStatusLabelMessModus(string s)
        {
            StatusLabel3.Text = s;
        }

        /// <summary>
        /// (threadsafe)
        /// </summary>
        /// <param name="img">Bitmap to show in pictureBox1</param>
        private void ZeigeInPicturebox(Bitmap img)
        {
            // TODO Crash beim Beenden ZeigeInPicturebox: selten
            //			if (this.pictureBox1 == null) return;
            //			if (img == null) return;

            if (this.pictureBox1.InvokeRequired)                // Invoke nötig?
            {
                // Debug.WriteLine("ZeigeInPicturebox: InvokeRequired");
                if (img == null)
                {
                    Debug.WriteLine("ZeigeInPicturebox: img = null!"); // tritt beim crash nicht auf!
                    return;
                }

                this.pictureBox1.Invoke(new SetImageCallback(ZeigeInPicturebox), new object[] { img });
                // Der Zugriff wird als Message in die Warteschlange des GUI-Thread gestellt...
            }
            else
            {
                // Kein Invoke nötig - Vorgang sicher durchführbar
                this.pictureBox1.Image = img;

                // TODO beobachten, ob Speicher volläuft, ggf. altes image disposen....
            }
        }

        #region --- Bild / Picturebox Koordinaten ---

        /// <summary>
        /// Wandelt picturebox-Koordinaten in Bildkoordinaten um
        /// Achtung: funktioniert nur bei Zoom-Modus, wenn Bild die PictureBox ausfüllt.
        /// Bleiben leere Ränder, ist eine weitere Untersuchung notwendig!!!
        /// </summary>
        /// <param name="ptPicture">Koordinaten in PictureBox</param>
        /// <returns><c>Point</c>Koordinaten im Kamerabild</returns>
        private System.Drawing.Point PictureboxZuBild(System.Drawing.Point ptPicture)
        {
            // TODO PictureboxZuBild: unterschiedliche PictureBoxen können unterschiedl. ptZero / PictZoom aufweisen!

            double dPictX = (double)ptPicture.X;
            double dPictY = (double)ptPicture.Y;

            System.Drawing.Point ptImage = new System.Drawing.Point();
            ptImage.X = Convert.ToInt32(0.5 + (dPictX - this.ptZero.X) / this.dPictZoom);
            ptImage.Y = Convert.ToInt32(0.5 + (dPictY - this.ptZero.Y) / this.dPictZoom);

            Debug.WriteLineIf(DEBUG_PICTBOX1, $" PictureBoxzuBild: Zoom={dPictZoom}, Zero={ptZero}, X={dPictX}, Y={dPictY}, {ptImage}");

            if (ptImage.X < 0) ptImage.X = 0;
            if (ptImage.Y < 0) ptImage.Y = 0;
            //// TODO PictureboxZuBild() 2023: ist das korrekt???
            if (imgSize.Width > 0)
            {
                if (ptImage.X > imgSize.Width) ptImage.X = imgSize.Width;
                if (ptImage.Y > imgSize.Height) ptImage.Y = imgSize.Height;
            }
            else
            {
                Debug.WriteLine("Form1.imgSize nicht initialisiert!!!");
            }
            //Debug.WriteLineIf(DEBUG_PICTBOX1, $"  GetPointImage(): Maus {ptPicture} -> Image {ptImage}");
            return ptImage;
        }

        /// <summary>
        /// Wandelt Bildkoordinaten in picturebox-Koordinaten um
        /// </summary>
        /// <param name="ptImage">Bildkoordinaten</param>
        /// <returns><c>Point</c>Picturebox-Koordinaten</returns>
        private System.Drawing.Point BildZuPicturebox(System.Drawing.Point ptImage)
        {
            double dImgX = (double)ptImage.X;
            double dImgY = (double)ptImage.Y;

            System.Drawing.Point ptPicture = new System.Drawing.Point();

            // ptImage = (ptPicture - ptZero) / dZoom
            // ptPicture = ptImage * dZoom + ptZero
            ptPicture.X = Convert.ToInt32(0.5 + dImgX * this.dPictZoom + this.ptZero.X);
            ptPicture.Y = Convert.ToInt32(0.5 + dImgY * this.dPictZoom + this.ptZero.Y);
            return ptPicture;
        }


        /// <summary>
        /// Für PictureBox Zoomfaktor und Bildnullpunkt berechnen
        /// Wrapper (bei InitCamera, BildLaden, PictureBox.SizeChanged)
        /// </summary>
        /// <returns><c>bool</c>true bei Erfolg</returns>
        private bool ComputeZoomAndZeroPoint(Size clientsize, Size imagesize)
        {
            Debug.WriteLineIf(DEBUG_GUI_INIT, "ComputeZoomAndZeroPoint()");
            bool result = true;
            // Problem mit cam.image: selten Ausnahme wenn Bildgröße verändert wird
            try
            {
                result = GuiTools.ComputeZoom(clientsize,
                                               imagesize, //cam.imgCamera.Size,
                                               out this.dPictZoom,
                                               out this.ptZero);
                if (result)
                {
                    Debug.WriteLine($" ComputeZoomAndZeroPoint: Zoom = {dPictZoom}, Zero = {ptZero}");
                }
                else
                {
                    MsgBox("Fehler bei GuiTools.ComputeZoom!");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ComputeZoomAndZeroPoint() Exception: " + ex.Message);
                result = false;
            }
            return result;
        }

        #endregion

        #region --- Fullscreen ---------------------------------------------------------------------

        /// <summary>
        /// Fullscreen ein / aus, ...
        /// </summary>
        void UmschaltungVollbild()
        {
            bool bFull = !settings.Fullscreen;
            SetFullscreen(bFull);
            settings.Fullscreen = bFull;
        }

        // Funktioniert nur, wenn untergeordnete Controls das Event weiterreichen!
        // TODO als Accelerator implementieren
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F12)
            {
                UmschaltungVollbild();
            }
            if (e.KeyCode == Keys.F11)
            {
                //Konfiguration();
            }
            if (e.KeyCode == Keys.F1)
            {
                //Hilfe();
            }
        }

        #endregion

        #endregion GUI helper methods

        #region ===== App helper methods ====================================================================


        /// <summary>
        /// Überprüft Vorhandensein der erforderlichen Verzeichnisstruktur
        /// </summary>
        /// <returns>true wenn alle Verzeichnisse existieren</returns>
        private bool CheckVerzeichnisse(string projPath = STR_PROJECTPATH)
        {
            Debug.WriteLine($"MainForm.CheckVerzeichnisse({projPath})");

            bool result = true;
            string msg = "";
            try
            {
                this.strProjektPfad = projPath;
                this.strParamPfad = Path.Combine(projPath, STR_PARAMPATH);
                this.strProduktPfad = Path.Combine(projPath, STR_PRODUKTPATH);
                this.strLogPfad = Path.Combine(projPath, STR_LOGPATH);
                this.strInitPfad = Path.Combine(projPath, STR_INITPATH);
                this.strBilderPfad = Path.Combine(projPath, STR_IMAGEPATH);

                result &= FileTools.EnsureFolderExistAsync(strProjektPfad, 100);
                result &= FileTools.EnsureFolderExistAsync(strParamPfad, 100);
                result &= FileTools.EnsureFolderExistAsync(strLogPfad, 100);
                //result &= FileTools.EnsureFolderExistAsync(strInitPfad, 100);
                //result &= FileTools.EnsureFolderExistAsync(strProduktPfad, 100);
                result &= FileTools.EnsureFolderExistAsync(strBilderPfad, 100);
            }
            catch (IOException ex)
            {
                msg = "CheckVerzeichnisse(): " + ex.Message;
                result = false;
            }
            catch (Exception ex)
            {
                msg = "CheckVerzeichnisse(): " + ex.Message;
                result = false;
            }
            if (!result) { Debug.WriteLine(msg); }
            return result;
        }



        #endregion

    }
}
