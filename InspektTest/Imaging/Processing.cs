/// Some simple image processing with Accord.NET
///
/// 08.09.2022 add Harris corner detection


using System;
using System.Collections.Generic;
using System.Linq;
//using System.Drawing;
using System.Threading.Tasks;
using System.Diagnostics;
using Accord;                       // some data types like IntPtr
using Accord.Imaging;
using Accord.Imaging.Filters;
using System.Drawing;
using System.Windows.Forms;

namespace Visutronik.Imaging
{
    /// <summary>
    /// Image processing result state
    /// </summary>
    public enum IprResults { IO, NIO, UNDEF };

    class Processing
    {
        #region --- properties ---

        /// <summary>
        /// Flag: if false, no processing is done
        /// </summary>
        public bool EnableProcessing { get; set; } = true;

        /// <summary>
        /// Result of image processing
        /// </summary>
        public IprResults IprResult { get; set; } = IprResults.UNDEF;

        public int Param1 { get; set; } = 50;  // range 0 ... 100
        public int Param2 { get; set; } = 50;  // range 0 ... 100

        public int AlgoNr { get; set; } = 0;    // range 1 ... 4
        public double BlobArea { get; set; } = 0.0;
        public double Sollwert { get; set; } = 125.0;
        public double Istwert { get; set; } = 0.0;
        public double Tolerance { get; set; } = 0.05;

        public System.Drawing.Bitmap ResultImage { get { return resultImage; } }

        #endregion

        #region --- image processing ---

        Mean smoothFilter = new Mean();
        Threshold binFilter = new Threshold();
        Invert invertFilter = new Invert();
        HSLFiltering hslFilter = new HSLFiltering();
        BlobCounter blobCounter = new BlobCounter();

        private System.Drawing.Bitmap resultImage = null;

        private Accord.IntPoint ptCOG;     // blob center of gravity

        /// <summary>
        /// Process the image: binarization after smoothing 
        /// </summary>
        /// <returns>true if success</returns>
        public bool Binarization(System.Drawing.Bitmap image)
        {
            Debug.WriteLine(" ProcessTheImage()");

            bool resultOk = true;
            UnmanagedImage camImage;
            UnmanagedImage greyImage;
            UnmanagedImage binImage;

            IprResult = IprResults.UNDEF;

            if (!EnableProcessing)
            {
                Debug.WriteLine(" - no processing required!");
                resultOk = false;
            }

            if (image == null)
            {
                Debug.WriteLine(" - no image!");
                resultOk = false;
            }

            if (resultOk)
            {
                try
                {
                    // using unmanaged images ...
                    camImage = UnmanagedImage.FromManagedImage(image);
                    if (camImage == null) return false;

                    // Bildvorbehandlung bis Binarisierung:
                    if (camImage.PixelFormat != System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
                        greyImage = Grayscale.CommonAlgorithms.BT709.Apply(camImage);
                    else
                        greyImage = camImage;

                    // smooth a little bit (3x3 kernel)
                    smoothFilter.ApplyInPlace(greyImage);

                    int threshold = Param1 * 255 / 100;
                    //Debug.WriteLine("nTreshold = " + nTreshold.ToString());
                    binFilter.ThresholdValue = threshold;
                    binImage = binFilter.Apply(greyImage);
                    //Debug.WriteLine(" - binarization ok");

                    resultImage = binImage.ToManagedImage();
                    IprResult = IprResults.IO;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(" - EXC: " + ex.Message);
                    resultOk = false;
                    IprResult = IprResults.NIO;
                }
            }
            return resultOk;
        }

        ///
        public bool ProcessTheImage(System.Drawing.Bitmap image)
        {
            bool resultOk = true;

            UnmanagedImage camImage;
            UnmanagedImage greyImage;
            UnmanagedImage binImage = null;
            IprResult = IprResults.UNDEF;
            double blobArea = 0;

            if (image == null)
            {
                resultOk = false;
            }

            int nAlgorithm = AlgoNr;
            if (nAlgorithm <= 4)
            {
                // Bildvorbehandlung bis Binarisierung:
                camImage = UnmanagedImage.FromManagedImage(image);
                if (camImage == null) return false;

                greyImage = Grayscale.CommonAlgorithms.BT709.Apply(camImage);

                // smoothFilter.ApplyInPlace(greyImage);
                // AForge.Imaging.Filters.Blur blurFilter = new Blur();
                // binImage = blurFilter.Apply(image);

                int nTreshold = Param1 * 255 / 100;
                //Debug.WriteLine("nTreshold = " + nTreshold.ToString());
                binFilter.ThresholdValue = nTreshold;
                binImage = binFilter.Apply(greyImage);
            }

            if (nAlgorithm == 1)
            {
                Debug.WriteLine("--- ProcessImage - Algorithmus 1 --");

                blobCounter.MinWidth = blobCounter.MinHeight = 20;
                blobCounter.FilterBlobs = true;
                blobCounter.CoupledSizeFiltering = true;            // only blobs with MinWidth AND MinHeight pass...
                blobCounter.ObjectsOrder = ObjectsOrder.Area;
                blobCounter.ProcessImage(binImage);

                Debug.WriteLine("Blobs :" + blobCounter.ObjectsCount);

                if (blobCounter.ObjectsCount > 0)
                {
                    //					Blob[] blobs = blobCounter.GetObjects(binImage, true);
                    Blob[] blobs = blobCounter.GetObjectsInformation();
                    foreach (Blob blob in blobs)
                    {
                        Trace.WriteLine("Blob" + blob.ID + ": " + blob.Area.ToString());

                        // check blob's properties
                        if (blob.ID == 1)   // maximal blob area
                        {
                            // the blob looks interesting, let's extract it
                            // extract image of partially initialized blob, which was provided by GetObjectsInformation()()() method.
                            blobCounter.ExtractBlobsImage(image, blob, true);
                            //							blobImage = blob.Image;
                            blobArea = (double)blob.Area;
                            ptCOG = blob.CenterOfGravity.Round();
                        }
                    }
                    // pictureBox4.Image = blobImage;

                    // Auswertung Ronden
                    double radius = 0.0;
                    if (blobArea > 1000.0)
                    {
                        radius = Math.Sqrt(blobArea / Math.PI);
                        Debug.WriteLine("Radius = " + radius.ToString());
                        this.Istwert = radius;
                        resultOk = this.InTolerance();
                    }
                }
                else
                {
                    // no valid blob found:
                }
                resultImage = binImage.ToManagedImage();
            }
            else if (nAlgorithm == 2)
            {
                Debug.WriteLine("--- ProcessImage - Algorithmus 2 --");
                invertFilter.ApplyInPlace(binImage);                            // blobcounter needs black background ...
                                                                                //				pictureBox3.Image = binImage;

                blobCounter.MinWidth = blobCounter.MinHeight = 20;
                blobCounter.FilterBlobs = true;
                blobCounter.CoupledSizeFiltering = true;            // only blobs with MinWidth AND MinHeight pass...
                blobCounter.ObjectsOrder = ObjectsOrder.Area;
                blobCounter.ProcessImage(binImage);

                Debug.WriteLine("Blobs :" + blobCounter.ObjectsCount);

                if (blobCounter.ObjectsCount > 0)
                {
                    //	Blob[] blobs = blobCounter.GetObjects(binImage, true);
                    Blob[] blobs = blobCounter.GetObjectsInformation();
                    foreach (Blob blob in blobs)
                    {
                        Trace.WriteLine("Blob" + blob.ID + ": " + blob.Area.ToString());

                        // check blob's properties
                        if (blob.ID == 1)   // maximal blob area
                        {
                            // the blob looks interesting, let's extract it
                            // extract image of partially initialized blob, which was provided by GetObjectsInformation()()() method.
                            blobCounter.ExtractBlobsImage(image, blob, true);
                            //blobImage = blob.Image;
                            blobArea = (double)blob.Area;
                            ptCOG = blob.CenterOfGravity.Round();
                        }
                    }
                    //					pictureBox4.Image = blobImage;

                    // Auswertung Ronden
                    double radius = 0.0;
                    if (blobArea > 1000.0)
                    {
                        radius = Math.Sqrt(blobArea / Math.PI);
                        Debug.WriteLine("Radius = " + radius.ToString());
                        this.Istwert = radius;
                        //resultOk = this.InTolerance();
                    }
                }
                else
                {
                    // no valid blob found:
                }
                resultImage = binImage.ToManagedImage();
            }
            else if (nAlgorithm == 3)
            {
                Debug.WriteLine("--- ProcessImage - Algorithmus 3 --");
                // blobcounter needs black background ...
                blobCounter.MinWidth = blobCounter.MinHeight = 20;
                blobCounter.FilterBlobs = true;
                blobCounter.CoupledSizeFiltering = true;            // only blobs with MinWidth AND MinHeight pass...
                blobCounter.ObjectsOrder = ObjectsOrder.Area;
                blobCounter.ProcessImage(binImage);

                Debug.WriteLine("Blobs :" + blobCounter.ObjectsCount);
                if (blobCounter.ObjectsCount == 3)
                {
                    resultOk = true;
                }
                resultImage = binImage.ToManagedImage();
            }
            else if (nAlgorithm == 4)
            {
                Debug.WriteLine("--- ProcessImage - Algorithmus 4 --");
                invertFilter.ApplyInPlace(binImage);
                // blobcounter needs black background ...
                blobCounter.MinWidth = blobCounter.MinHeight = 20;
                blobCounter.FilterBlobs = true;
                blobCounter.CoupledSizeFiltering = true;            // only blobs with MinWidth AND MinHeight pass...
                blobCounter.ObjectsOrder = ObjectsOrder.Area;
                blobCounter.ProcessImage(binImage);
                Debug.WriteLine("Blobs :" + blobCounter.ObjectsCount);
                if (blobCounter.ObjectsCount == 3)
                {
                    resultOk = true;
                }
                resultImage = binImage.ToManagedImage();
            }
            else // AlgoNr < 1 || AlgoNr > 4
            {
                resultOk = MarkColorRangeByHSL(image, Param1, Param2);
            }
            return resultOk;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <param name="nParam1"></param>
        /// <param name="nParam2"></param>
        /// <returns></returns>
        private bool MarkColorRangeByHSL(System.Drawing.Bitmap image, int nParam1, int nParam2)
        {
            int huemin, huemax;
            int hue = nParam1 * 36 / 10;            // auf 360°-Bereich beziehen
            int range = nParam2 * 36 / 10;          // auf 360°-Bereich beziehen

            huemin = hue - range / 2;
            if (huemin < 0) huemin = huemin + 360;
            huemax = huemin + range;
            //if (huemax > 360) huemax = huemax - 360;

            string s = "HSL HUE range = " + huemin.ToString() + " ... " + huemax.ToString();
            Debug.WriteLine(s);

            Accord.Imaging.Filters.Blur blurFilter = new Blur();
            blurFilter.ApplyInPlace(image);

            hslFilter.Hue = new IntRange(huemin, huemax);
            //			filter.Luminance = new DoubleRange(0.10, 0.90);
            //			filter.Saturation = new DoubleRange(0.25, 0.75);
            hslFilter.Luminance = new Range(0.10f, 0.90f);
            hslFilter.Saturation = new Range(0.10f, 1.0f);

            //filter.UpdateHue = false;
            //filter.UpdateLuminance = false;
            //filter.UpdateSaturation = false;

            hslFilter.FillColor = new HSL(120, 0.85f, 0.5f); // bright green
            hslFilter.FillOutsideRange = false;

            // apply the filter
            //filter.ApplyInPlace(image);
            resultImage = hslFilter.Apply(image);
            return true;
        }

        private bool InTolerance()
        {
            return ((Istwert >= Sollwert * (1.0 - Tolerance)) &&
                (Istwert <= Sollwert * (1.0 + Tolerance)));
        }

        #endregion

        #region === Corner detection ===

        /// <summary>
        ///        
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public bool ProcessCorners(System.Drawing.Bitmap image)
        {
            Debug.WriteLine("ProcessCorners()");

            // HarrisCornersDetector detector = new HarrisCornersDetector();
            // CornersMarker cornersMarker = new CornersMarker(detector, Color.White);
            // cornersMarker.ProcessImage(image)


            bool resultOk = true;
            IprResult = IprResults.UNDEF;

            if (image == null)
            {
                resultOk = false;
            }
            else
            {
                double sigma = 2.2;         // default = 1.2, größer -> weniger Punkte
                float k = .04f;             // Harris parameter k. Default value is 0.04. 
                float threshold = 5000f;   // default = 20000

                // Create a new Harris Corners Detector using the given parameters
                HarrisCornersDetector harris = new HarrisCornersDetector(k)
                {
                    Measure = HarrisCornerMeasure.Harris, // HarrisCornerMeasure.Noble,
                    Threshold = threshold,
                    Sigma = sigma
                };

                List<Accord.IntPoint> cornerList = harris.ProcessImage(image);
                foreach (IntPoint corner in cornerList)
                {
                    // ... 
                    Debug.WriteLine($"corner at {corner.X},{corner.Y}");
                }
                Debug.WriteLine($"corners found: {cornerList.Count}");

                // Create a new AForge's Corner Marker Filter
                CornersMarker corners = new CornersMarker(harris, Color.White);

                // Apply the filter and display it on a picturebox
                resultImage = corners.Apply(image);
            }
            return resultOk;
        }

        #endregion

    }
}
