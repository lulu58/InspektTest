/*
 * Projekt  : Inspect / InspectTest
 * Datei    : model - Organisation des Ablaufes einer Inspektion
 * Benutzer : Lulu
 * Erstellt mit SharpDevelop.
 * 10/25/2018 initial
 * 19.12.2022	add json serialization
 *				begin coding Operate()
 */

using System;
using System.Drawing;
using System.Diagnostics;
using Visutronik.Imaging;   // struct CircleF
using AForge;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

namespace Visutronik.Inspektion
{
    /// <summary>
    /// Description of InspectModel.
    /// </summary>
    public class InspectModel : IEnumerable<InstructionParams>
    {
        #region ----- events ----------------------------------------------

        // Signalisierung der Checker mit Event:
        public delegate void ShowCheckerRectEvent(RectangleF rect, int colorindex);
        public event ShowCheckerRectEvent ShowCheckerRect;

        public delegate void ShowCheckerCircleEvent(CircleF circle, int colorindex);
        public event ShowCheckerCircleEvent ShowCheckerCircle;

        public delegate void ShowCheckerLineEvent(LineF circle, int colorindex);
        public event ShowCheckerLineEvent ShowCheckerLine;

        public delegate void OnOperationReadyEvent(InstructionParams instruction, bool result);
        public event OnOperationReadyEvent OnOperationReady;

        public delegate void SetImageLoadedEvent(Bitmap img);
        public event SetImageLoadedEvent ImageLoaded;

        public delegate void SetMessageOutEvent(string msg);
        public event SetMessageOutEvent SetMessageOut;

        public delegate void SetWriteToLogEvent(string msg);
        public event SetWriteToLogEvent WriteToLog;

        #endregion

        #region ----- vars ------------------------------------------------		

        /// <summary>
        /// 
        /// </summary>
        public string InstructionFile { get; set; } = @"instructions.json";

        /// <summary>
        /// 
        /// </summary>
        public List<InstructionParams> instructionList = new List<InstructionParams>();

        /// <summary>
        /// 
        /// </summary>
        public string LastError { get; internal set; } = "";

        public bool ModifiedFlag { get; internal set; } = false;
        /// <summary>

        /// <summary>
        /// 
        /// </summary>
        //private Instructions instructions = new Instructions();

        public bool HasInstructionList()
        {
            return instructionList.Count > 0;
        }

        private bool InspectionStop { get; set; } = false;

        private List<Bitmap> imageList = new List<Bitmap>();

        private Bitmap imgSource;
        private Bitmap imgDest;


        #endregion ----- vars ------------------------------------------------		

        /// <summary>
        /// ctor
        /// </summary>
        public InspectModel()
        {
            ModifiedFlag = false;

            for (int i = 0; i < 5; i++)
            {
                imageList.Add(new Bitmap(1, 1));
            }
        }

        // wichtig!
        public IEnumerator<InstructionParams> GetEnumerator()
        {
            return instructionList.GetEnumerator();
        }

        // wichtig!
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public Bitmap GetImage(int idx)
        {
            if ((idx >= 0) && (idx < imageList.Count))
                return imageList[idx];
            else
                return null;
        }

        /// <summary>
        /// Erzeugt leere Anweisungsliste
        /// </summary>
        public void CreateNewInstructions()
        {
            Debug.WriteLine("Model.CreateNewInstructions()");
            instructionList.Clear();
            ModifiedFlag = true;
        }

        /// <summary>
        /// complete inspection by iterating the inpection list
        /// soll mal in eigenem Thread laufen
        /// </summary>
        /// <returns>true if no operating errors occurs</returns>
        public bool Inspect()
        {
            Debug.WriteLine("--- Model.Inspect() ---");
            bool result = true;
            InspectionStop = false;
            LastError = "Inspektion ok";

            if (HasInstructionList())
            {
                try
                {
                    foreach (var iparam in instructionList)
                    {
                        result = Operate(iparam);

                        // Callback -> Resultate an GUI
                        OnOperationReady?.Invoke(iparam, result);

                        if (InspectionStop)
                        {
                            LastError = "Inspektion gestoppt";
                            result = false;
                        }
                        if (!result) break;
                    }
                }
                catch (Exception ex)
                {
                    LastError = ex.Message;
                    result = false;
                }
            }
            else
            {
                LastError = "Leere Anweisungsliste!";
                result = false;
            }

            if (!result)
            {
                OutMsg(LastError);
            }
            return result;
        }

        /// <summary>
        /// Inspektion anhalten
        /// </summary>
        public void StopInspect()
        {
            Debug.WriteLine("Model.StopInspect()");
            InspectionStop = true;
        }

        /// <summary>
        /// Alle Ressourcen freigeben...
        /// </summary>
        /// <returns></returns>
        public bool CloseInspect()
        {
            Debug.WriteLine("Model.CloseInspect()");
            bool result = true;
            try
            {
                // TODO CloseInspect() am Programmende: must dispose anything?
                // throw new Exception("Model.CloseInspect() not implemented!");
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                result = false;
            }
            return result;
        }


        /// <summary>
        /// Get all checkerList from instruction list
        /// and invoke events to draw to overlay...
        /// </summary>
        /// <returns>true if success</returns>
        public bool GetCheckers()
        {
            Debug.WriteLine("InspectModel.GetCheckers()");

            bool result = true;
            InspectionStop = false;
            LastError = "GetCheckers ok";

            if (this.HasInstructionList())
            {
                try
                {
                    foreach (var i in instructionList)
                    {
                        Debug.WriteLine($" - {i.Name}: {i.Operation}, selected={i.IsSelected}");

                        if (i.Operation == "Checker")
                        {
                            int colorindex = i.IsSelected ? 1 : 0; // erweiterungsfähig auf mehr Eigenschaften...
                            Debug.WriteLine($"  colorindex={colorindex}");
                            //Debug.WriteLine($"  {i.ImageArea}: {i.ImageAreaParams}");
                            if (i.ImageAreaIndex == (int)ImageAreaType.Rect)            // 1
                            {
                                // CheckerRect ermitteln und per Callback in MainForm.PictureBox anzeigen
                                RectangleF rect = InstructionHelper.GetRectangleFromString(i.ImageAreaParams);
                                // callback!
                                ShowCheckerRect?.Invoke(rect, colorindex);
                                continue;
                            }

                            if (i.ImageAreaIndex == (int)ImageAreaType.Circle)          // 2
                            {
                                CircleF circle = InstructionHelper.GetCircleFromString(i.ImageAreaParams);
                                ShowCheckerCircle?.Invoke(circle, colorindex);
                                continue;
                            }

                            if (i.ImageAreaIndex == (int)ImageAreaType.Ring)            // 3
                            {
                                // TODO
                                continue;
                            }

                            if (i.ImageAreaIndex == (int)ImageAreaType.CircleSegment)   // 4
                            {
                                // TODO
                                continue;
                            }

                            if (i.ImageAreaIndex == (int)ImageAreaType.Line)            // 5
                            {
                                LineF line = InstructionHelper.GetLineFromString(i.ImageAreaParams);
                                ShowCheckerLine?.Invoke(line, colorindex);
                                continue;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LastError = ex.Message;
                    result = false;
                }
            }
            else
            {
                LastError = "Leere Anweisungsliste!";
                result = false;
            }

            if (!result)
            {
                Debug.WriteLine("GetCheckers()" + LastError);
                OutMsg(LastError);
            }
            return result;
        }

        /// <summary>
        /// Sucht zum Punkt zugehörige Instruktion / Checker
        /// </summary>
        /// <param name="pt">Punkt im aktuellen Bild</param>
        /// <param name="fi">gefundene Instruktion oder null</param>
        /// <returns>true wenn Punkt zu Instruktion gehört</returns>
        public bool FindObject(System.Drawing.Point pt, out InstructionParams fi)
        {
            fi = null;
            try
            {
                // try to find an inspection object in instructions
                foreach (var i in instructionList)
                {
                    if (i.Operation == "Checker")
                    {
                        string areaName = InstructionHelper.AreaNames[i.ImageAreaIndex];

                        Debug.WriteLine($" Is mouse point in Checker {i.Name} - {areaName}?");
                        if (i.PointIsInside(pt))
                        {
                            Debug.WriteLine("  YES!");
                            fi = i; break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                OutMsg(LastError);
            }
            return (fi != null);
        }


        /// <summary>
        /// Reset all IsSelected flags from checkers
        /// </summary>
        /// <returns></returns>
        public bool UnselectAllObjects()
        {
            try
            {
                // try to find an inspection object in instructions
                foreach (var i in instructionList)
                {
                    if (i.Operation == "Checker")
                    {
                        i.IsSelected = false;
                        Debug.WriteLine($"unselect checker {i.Number}");
                    }
                }
                Debug.WriteLine("UnselectAllObjects");
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                OutMsg(LastError);
                return false;
            }
            return true;
        }


        /// <summary>
        /// set IsSelected flag to one checker
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public bool SelectObject(int idx)
        {
            try
            {
                // try to find an inspection object in instructions
                foreach (var i in instructionList)
                {
                    if (i.Operation == "Checker" && i.Number == idx)
                    {
                        Debug.WriteLine($"select checker {idx}");
                        i.IsSelected = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                OutMsg(LastError);
            }
            return true; 
        }

        /// <summary>
        /// AddSimpleChecker not used ...
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public bool AddSimpleChecker(Rectangle r)
        {
            Debug.WriteLine("InspectModel.AddChecker(rect)");

            InstructionParams i = new InstructionParams();

            i.Number = instructionList.Count;
            i.Name = "SimpleRectChecker";
            i.CameraIndex = 0;

            i.ImageIndex = 0;
            i.ImageAreaIndex = 0;
            i.ImageAreaParams = InstructionHelper.GetStringFromRectangle(r);

            i.Operation = "Checker";
            i.OperationParams = "";

            i.ResultError = "";
            i.ResultSuccess = false;
            i.ResultValue = 0.0f;

            return AddInstruction(i);
        }

        #region --- instruction list methods -----------------

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instruction">single instruction</param>
        /// <returns>true if success</returns>
        public bool AddInstruction(InstructionParams instruction)
        {
            Debug.WriteLine($"InspectModel.AddInstruction({instruction.Name})");
            bool result = true;
            try
            {
                // AddInstruction new instruction to list
                instructionList.Add(instruction);
                ModifiedFlag = true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                result = false;
            }
            return result;
        }


        /// <summary>
        /// Load Instruction List From JSON File
        /// </summary>
        /// <returns>true if instruction file loaded</returns>
        public bool LoadInstructionsFromFile()
        {
            return LoadInstructionsFromFile(InstructionFile);
        }

        /// <summary>
        /// JSON file deserialization
        /// </summary>
        /// <param name="path">path to file</param>
        /// <returns>true if success</returns>
        public bool LoadInstructionsFromFile(string path)
        {
            Debug.WriteLine($"InspectModel.LoadFromFile({path})");

            bool result = true;
            try
            {
                // deserialize JSON directly from a file
                using (StreamReader file = File.OpenText(path))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    instructionList = (List<InstructionParams>)serializer.Deserialize(file, typeof(List<InstructionParams>));
                }
                ModifiedFlag = false;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                result = false;
            }

            // Überprüfung der geladenen Anweisungen
            if (instructionList.Count == 0)
            {
                LastError = "Anweisungsliste ist leer!";
                result = false;
            }
            else
            {
                foreach (var i in instructionList)
                {
                    if (!i.SetInternalParametersAfterLoading())
                    {
                        LastError = i.LastError;
                        result = false;
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// Wrapper to save instruction list to file
        /// </summary>
        /// <returns></returns>
        public bool SaveInstructionsToFile()
        {
            Debug.WriteLine("Model.SaveInstructions()");
            bool result = true;
            if (ModifiedFlag)
            {
                result = SaveInstructionsToFile(InstructionFile);
            }
            return result;
        }

        /// <summary>
        /// JSON serialization - save instruction list as json file
        /// </summary>
        /// <param name="path">path to file</param>
        /// <returns>true if success</returns>
        public bool SaveInstructionsToFile(string path)
        {
            Debug.WriteLine($"InspectModel.SaveToFile({path})");
            bool result = instructionList.Count > 0;
            if (result)
            {
                try
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.NullValueHandling = NullValueHandling.Ignore;
                    using (StreamWriter sw = new StreamWriter(path))
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        writer.Formatting = Formatting.Indented;
                        serializer.Serialize(writer, instructionList);
                    }
                }
                catch (Exception ex)
                {
                    LastError = ex.Message;
                    result = false;
                }
            }
            else
            {
                LastError = "Anweisungsliste ist leer!";
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>true if success</returns>
        public bool CreateTestInstructions()
        {
            Debug.WriteLine("InspectModel.CreateTestInstructions()");
            bool result = true;
            try
            {
                instructionList.Clear();

                InstructionParams inst1 = new InstructionParams();
                inst1.Number = instructionList.Count;
                inst1.Name = "LoadImage";
                inst1.Description = "Load image from file";
                inst1.Operation = "ImageLoad";
                inst1.OperationParams = @"file=d:\Temp\JUS\Kamerabilder\Lage_3+4\3a.bmp; dstimage=1";
                inst1.CameraIndex = 0;
                instructionList.Add(inst1);

                InstructionParams inst2 = new InstructionParams();
                inst2.Number = instructionList.Count;
                inst2.Name = "GlobalBinarization";
                inst2.Description = "Global binarization using Otzu's algorithm";
                inst2.Operation = "ImageFilter";
                inst2.OperationParams = "Algo=Otzu; srcimage=1; dstimage=2";
                inst1.CameraIndex = 0;
                instructionList.Add(inst2);

                InstructionParams inst3 = new InstructionParams();
                inst3.Number = instructionList.Count;
                inst3.Name = "Checker 1";
                inst3.Description = "Check if part is present";
                inst3.ImageIndex = 0;
                inst3.ImageAreaIndex = (int)ImageAreaType.Rect;   // "Rect"
                inst3.ImageAreaParams = "{100, 100, 50, 50}";
                inst3.Operation = "Checker";
                inst3.OperationParams = "BlackWhiteRatio; ";
                inst3.Evaluation = "Contrast";
                inst3.EvaluationParams = "Min=20";
                instructionList.Add(inst3);

                InstructionParams inst4 = new InstructionParams();
                inst4.Number = instructionList.Count;
                inst4.Name = "Checker 2";
                inst4.Description = "Check if part is present";
                inst4.ImageIndex = 0;
                inst4.ImageAreaIndex = (int)ImageAreaType.Rect;   // "Rect"
                inst4.ImageAreaParams = "{200, 200, 50, 50}";
                inst4.Operation = "Checker";
                inst4.OperationParams = "FindObject";
                inst4.Evaluation = "Color=Black; Min=30";
                inst4.EvaluationParams = "";
                instructionList.Add(inst4);

                InstructionParams inst5 = new InstructionParams();
                inst5.Number = instructionList.Count;
                inst5.Name = "Checker 3";
                inst5.Description = "Check if part is present";
                inst5.IsSelected = false;
                inst5.ImageIndex = 0;
                inst5.ImageAreaIndex = (int)ImageAreaType.Circle;   // "Rect"
                inst5.ImageAreaParams = "{660, 400, 150}";
                inst5.Operation = "Checker";
                inst5.OperationParams = "FindObject";
                inst5.Evaluation = "Color=Black; Min=30";
                inst5.EvaluationParams = "";
                instructionList.Add(inst5);

                foreach (var i in instructionList)
                {
                    if (!i.SetInternalParametersAfterLoading())
                    {
                        LastError = i.LastError;
                        result = false;
                    }
                }
                ModifiedFlag = true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                result = false;
            }
            return result;
        }


        #endregion

        #region --- Inspection helper methods ----------------

        private void OutMsg(string msg)
        {
            SetMessageOut?.Invoke(msg);
        }

        private void Log(string msg)
        {
            WriteToLog?.Invoke(msg);
        }

        #endregion

        #region --- single instruction operations ------------

        /// <summary>
        /// do the operation with instruction params
        /// </summary>
        /// <param name="iparam">instruction params</param>
        /// <returns>true if success</returns>
        /// <exception cref="Exception"></exception>
        private bool Operate(InstructionParams iparam)
        {
            bool result = (iparam != null);
            if (!result)
            {
                LastError = "missing params";
                return false;
            }

            Debug.WriteLine("--- Operate: " + iparam.Name + " : " + iparam.Operation);
            LastError = "";
            switch (iparam.Operation)
            {
                //todo set camera params
                //todo add snap image from camera

                case "ImageLoad":
                    result = LoadImageFromParams(iparam.OperationParams);// @"d:\Temp\JUS\Kamerabilder\3a.bmp"
                    break;
                case "ImageFilter":
                    Debug.WriteLine("Todo global ImageFilter");
                    result = OperateFilter(iparam.OperationParams);
                    break;
                case "Checker":
                    Debug.WriteLine("OperateChecker");
                    result = OperateChecker(ref iparam);
                    break;
                // TODO add more operations
                default:
                    throw new Exception($"Operation: {iparam.Operation} not supported");
            }
            //Debug.WriteLine($" operation result = {result}, LastError = {LastError}");
            return result;
        }

        /// <summary>
        /// Load image from file
        /// </summary>
        /// <param name="filename">path to file</param>
        /// <param name="imgNr">storage number in image list</param>
        /// <returns>success </returns>
        public bool LoadImageFromFile(string filename, string imgNr = "0")
        {
            bool result = true;
            Debug.WriteLine($"InspectModel.LoadImageFromFile({filename})");
            try
            {
                imgSource = (Bitmap)Image.FromFile(filename);

                // send image to main form
                ImageLoaded?.Invoke(imgSource);

                int imgIndex = 0;
                if (imgNr != "")
                {
                    if (Int32.TryParse(imgNr, out imgIndex))
                    {
                        if (imgIndex < imageList.Count)
                        {
                            imageList[imgIndex] = imgSource;
                            Debug.WriteLine($"  - imgSource copied to imageList[{imgIndex}]");
                        }
                    }
                }
            }
            catch (System.IO.FileNotFoundException ioex)
            {
                LastError = "Not found " + ioex.Message;
                result = false;
            }
            catch (Exception ex)
            {
                LastError = "LoadImage: " + ex.Message;
                result = false;
            }
            //if (!result) Debug.WriteLine(LastError);
            return result;
        }


        /// <summary>
        /// Load image from file to dstimage
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool LoadImageFromParams(string opParams)
        {
            Debug.WriteLine($"InspectModel.LoadImageFromParams({opParams})");

            string filename = "";
            string imgNr = "";
            string[] pms = opParams.Split(';');
            foreach (string s1 in pms)
            {
                var s2 = s1.Trim(); // remove whitespace
                if (s2.StartsWith("file=")) filename = s2.Remove(0, 5);
                if (s2.StartsWith("dstimage=")) imgNr = s2.Remove(0, 9);
            }
            return LoadImageFromFile(filename, imgNr);
        }

        #endregion

        public bool OperateFilter(string opParams)
        {
            // TODO OperateFilter(...)
            string[] pms = opParams.Split(';');
            foreach (string s1 in pms)
            {
                var s2 = s1.Trim(); // remove whitespace
            }
            return true;
        }


        /// <summary>
        /// Checker-Operation ausführen - aus Liste oder einzeln
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public bool OperateChecker(ref InstructionParams i)
        {
            bool result = true;
            try
            {
                switch (i.OperatorIdx)
                {
                    case 0:

                        break;
                    case 1:
                        break;
                }
                // TODO echte Checkeroperation
                // hier Methode mit Inhalt füllen
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                result = false;
            }
            if (!result) Debug.WriteLine("OperateChecker: " + LastError);

            return result;
        }

    }
}

/*
 * Vorlage für Methode 
 * 
		/// <summary>
		/// 
		/// </summary>
		/// <param name="instruction"></param>
		/// <returns>true if success</returns>
		public bool AddInstruction(Instruction instruction)
		{
			bool result = true;
			try
			{
				// TODO Vorlage: hier Methode mit Inhalt füllen
			}
			catch (Exception ex)
			{
				LastError = ex.Message;
				result = false;
			}
            if (!result) Debug.WriteLine("xxx: " + LastError);
			return result;
		}
*/