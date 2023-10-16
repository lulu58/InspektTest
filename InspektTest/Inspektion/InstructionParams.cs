/*
 * Projekt  : Inspekt
 * Datei    : Inspektionsanweisung
 * 25.10.2018 initial
 * 06.01.2023
 * 25.01.2023   add prop CameraIndex
 * 31.01.2023   chg props, add dictionaries
 * 
 * Idee: Eine Bildverarbeitungsanweisung besteht aus folgenden Teile:
 * - Bild und Bildparameter
 * - auszuführende Operationen mit Parametern
 * - Bewertung der Operationsergebnisse
 * - Gesamtergebnis
 * Parameter werden als String-Properties übergeben und müssen durch Parser aufbereitet werden
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Visutronik.Imaging;

namespace Visutronik.Inspektion
{
    /// <summary>
    /// Description of Instruction.
    /// </summary>
    public class InstructionParams
    {
        const bool DIAG_PARAMS = false;

        #region --- properties ---

        // instruction
        public int Number { get; set; } = 0;
        public string Name { get; set; } = "None";
        public string Description { get; set; } = string.Empty;
        public bool IsSelected { get; set; } = false;

        // area
        public int CameraIndex { get; set; } = 0;
        public int ImageIndex { get; set; } = 0;
        public int ImageAreaIndex { get; set; } = 0;
        public string ImageAreaParams { get; set; } = string.Empty;

        // operation
        public string Operation { get; set; } = "None";
        public int OperatorIdx { get; set; } = 0;
        public string OperationParams { get; set; } = string.Empty;
        public int FilterIdx { get; set; } = 0;
        public int CheckerIdx { get; set; } = 0;

        // evaluation / Auswertung
        public string Evaluation { get; set; } = string.Empty;
        public string EvaluationParams { get; set; } = string.Empty;

        // result
        public bool ResultSuccess { get; internal set; } = false;
        public float ResultValue { get; internal set; } = 0.0f;
        public string ResultError { get; internal set; } = string.Empty;

        // last operation error
        public string LastError { get; internal set; } = string.Empty;

        #endregion

        #region --- private vars ---

        OperatorType opType = OperatorType.None;

        RectangleF rectHull = new RectangleF();
        CircleF circle = new CircleF();
        LineF line = new LineF();

        #endregion

        #region --- public methods ---

        /// <summary>
        /// ctor setting default properties
        /// </summary>
        public InstructionParams()
        {
            Number = -1;
            Name = "No name";
            Description = "No description available";
            IsSelected = false;

            CameraIndex = 0;
            ImageIndex = 0;
            ImageAreaIndex = 0;
            ImageAreaParams = "";   // a x b, MP, r, P1, P2, ...

            Operation = "";         // Load, Save, Filter, Checker, ...
            OperationParams = "";

            Evaluation = "";        // "Compare", "", ...
            EvaluationParams = "";

            ResultSuccess = false;
            ResultValue = 0.0f;
            ResultError = "";
        }


        /// <summary>
        /// Set area for image operations
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="area"></param>
        /// <param name="areaParams"></param>
        public bool SetImage(int cam, int idx, string area = "Full", string areaParams = "")
        {
            Debug.WriteLine($"  SetImage({cam}, {idx}, {area}, {areaParams}");

            CameraIndex = cam;
            ImageIndex = idx;
            ImageAreaIndex = InstructionHelper.GetAreaIndex(area);
            ImageAreaParams = areaParams;
            return CheckImageAreaParameters();
        }

        /// <summary>
        /// Set image area operation
        /// </summary>
        /// <param name="op">operation</param>
        /// <param name="opParams">operation parameters</param>
        public bool SetOperation(string op, string opParams = "")
        {
            Operation = op;
            OperationParams = opParams;
            return CheckOperationParameters();
        }

        /// <summary>
        /// Set area evaluation parameters
        /// </summary>
        /// <param name="eval"></param>
        /// <param name="evalParams"></param>
        /// <returns></returns>
        public bool SetEvaluation(string eval, string evalParams = "")
        {
            Evaluation = eval;
            EvaluationParams = evalParams;
            return CheckEvaluationParameters();
        }

        /// <summary>
        /// Find out if point (from mouse click) is inside the checker
        /// tested, ok
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public bool PointIsInside(System.Drawing.Point pt)
        {
            bool result = (OperatorIdx == (int)OperatorType.Checker);
            //Debug.WriteLine("PointIsInside: " + ImageArea);
            if (result)
            {
                // check if point is inside the instruction object's rectangular hull
                if ((pt.X < rectHull.X) || (pt.X > rectHull.X + rectHull.Width)) result = false;
                else if ((pt.Y < rectHull.Y) || (pt.Y > rectHull.Y + rectHull.Height)) result = false;

                if (result && (ImageAreaIndex == (int) ImageAreaType.Circle))
                {
                    // PointIsInside(): more checks for circles
                    float dx = circle.center.X - pt.X;
                    float dy = circle.center.Y - pt.Y;
                    //Debug.WriteLine($"  {dx} {dy}");
                    if (Math.Sqrt(dx * dx + dy * dy) > circle.radius) result = false;
                }
            }
            return result;
        }

        /// <summary>
        /// Parsen der geladenen Anweisungsparameter und Setzen der internen Variablen
        /// </summary>
        /// <returns></returns>
        public bool SetInternalParametersAfterLoading()
        {
            Debug.WriteLineIf(DIAG_PARAMS, "SetInternalParametersAfterLoading(): Instruction " + Name);
            bool result = true;

            if (!CheckImageAreaParameters()) result = false;
            if (!CheckOperationParameters()) result = false;
            if (!CheckEvaluationParameters()) result = false;
            return result;
        }

        #endregion

        #region --- internal methods ---

        private bool CheckImageAreaParameters()
        {
            Debug.WriteLineIf(DIAG_PARAMS, "Instruction.CheckImageParameters(): " + ImageAreaIndex + " - " + ImageAreaParams);
            if (ImageAreaIndex == (int)ImageAreaType.Rect)
            {
                rectHull = InstructionHelper.GetRectangleFromString(ImageAreaParams);
            }
            else if (ImageAreaIndex == (int) ImageAreaType.Circle)
            {
                circle = InstructionHelper.GetCircleFromString(ImageAreaParams);
                rectHull = new RectangleF(
                    circle.center.X - circle.radius,
                    circle.center.Y - circle.radius,
                    2.0f * circle.radius,
                    2.0f * circle.radius);
            }
            else if (ImageAreaIndex == (int)ImageAreaType.Ring)
            {
                // TODO CheckImageAreaParameters() - Ring
                circle = InstructionHelper.GetRingFromString(ImageAreaParams);
            }
            else if (ImageAreaIndex == (int) ImageAreaType.CircleSegment)
            {
                // TODO CheckImageAreaParameters() - circlesegment
            }
            else if (ImageAreaIndex == (int) ImageAreaType.Line)
            {
                // TODO CheckImageAreaParameters() - Line
                line = InstructionHelper.GetLineFromString(ImageAreaParams);
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool CheckOperationParameters()
        {
            Debug.WriteLineIf(DIAG_PARAMS, "Instruction.CheckOperationParameters(): ");
            opType = OperatorType.None;
            if (Operation != string.Empty)
            {
                Debug.WriteLine(" Operation = " + Operation);
                switch (Operation)
                {
                    case "ImageLoad":
                        opType = OperatorType.LoadImage; break;
                    case "ImageFilter":
                        opType = OperatorType.Filter; break;
                    case "Checker":
                        opType = OperatorType.Checker; break;
                    //TODO add weitere OperatorType ...
                    default:
                        throw new Exception("unknown operation");
                }
                OperatorIdx = (int) opType;
            }

            if (OperationParams != string.Empty)
            {
                Debug.WriteLineIf(DIAG_PARAMS, " OperationParams = " + OperationParams);
                switch (opType)
                {
                    case OperatorType.LoadImage:
                        break;
                    case OperatorType.Filter:
                        break;
                    case OperatorType.Checker:
                        break;
                }
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool CheckEvaluationParameters()
        {
            Debug.WriteLineIf(DIAG_PARAMS, "Instruction.CheckEvaluationParameters()");
            //TODO CheckEvaluationParameters
            return true;
        }

        #endregion

        #region --- real operation ---

        // Todo real operation

        #endregion

        #region --- parameter converters ---
        #endregion
    }
}
