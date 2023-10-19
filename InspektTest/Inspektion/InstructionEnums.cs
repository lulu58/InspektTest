///
/// enums gehören keiner Klasse an.
///

namespace Visutronik.Inspektion
{
    public enum CameraType
    {
        None = -1, Left = 0, Middle = 1, Right = 2
    }

    /// <summary>
    /// 
    /// </summary>
    public enum OperationType
    {
        None = -1,
        SnapImage,      // Bild von Kamera holen
        LoadImage,      // Bild aus Datei oder Resource laden
        Filter,         // globale Filteroperation (Smooth, Crop, Stretch, Binarization, HistogramEqualization, 
        Checker,        // Bildausschnitt auswerten Grauwerte, Kanten, Blobs, ...
        MathOp          // Mathematische Verknüpfung von Ergebnissen
    };

    public enum FilterType
    {
        None = -1,
        Smooth = 0,
        Binary,
        Edge,
        Invert
    };


    public enum CheckerType
    {
        None = -1,
        Mean,       // "Mean"
        Object,     // "Object"
        BWRatio     // "BW-Ratio"
    };


    /// <summary>
    /// Type of image areas
    /// </summary>
    public enum ImageAreaType
    {
        None = -1,
        Full,
        Rect,
        Circle,
        Ring,
        CircleSegment,
        Line
    };

    public enum EvalType
    {
        None = -1,
        Contrast,
        Blob,
        Size
        // TODO more EvalTypes
    };

}
