///
///
///

namespace Visutronik.Inspektion
{
    /// <summary>
    /// 
    /// </summary>
    public enum OperatorType
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

}
