//
// DirectShow camera class based on AForge.NET library:
// http://www.aforgenet.com/framework/
// AForge.NET Framework is published under LGPL v3 license
// Lulu
// 17.03.2022
// 17.06.2022   add OpenCameraAsync(), CloseCameraAsync()
// 18.10.2023   chg Namespace, add ICamera interface 
//              chg Open(...) to real sync method with boolean result

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Threading.Tasks;
using System.Threading;

namespace Visutronik.Imaging
{
    class DS_Camera : ICamera
    {
        #region === interface methods ===============================================================

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetCameraInfo()
        {
            return info;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="camidx"></param>
        /// <param name="camparam"></param>
        /// <returns></returns>
        public bool InitCamera(int camidx, string[] camparam)
        {
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="camidx">is ignored, only one camera is active</param>
        /// <returns></returns>
        public bool StartCamera(int camidx = 0)
        {
            if (!IsRunning)
            {
                // erste DS-Kamera öffnen
                Open(0);
                Thread.Sleep(100);
            }
            return IsRunning;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="camidx">is ignored, only one camera is active</param>
        /// <returns></returns>
        public bool StopCamera(int camidx = 0)
        {
            if (IsRunning)
                Close();
            return IsRunning == false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="camidx">is ignored, only one camera is active</param>
        /// <returns></returns>
        public Bitmap AcquireImage(int camidx = 0)
        {
            if (IsRunning)
            {
                Debug.WriteLine("DS_Camera.AcquireImage - camera is running");
                return GetBitmap();
            }

            Debug.WriteLine("DS_Camera.AcquireImage - camera is not running!!!");
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetLastError()
        {
            return msg;
        }

        #endregion

        #region === non interface props, vars, methods =====================================================

        #region --- non interface events and vars ---

        public delegate void ImageAvailable(System.Drawing.Bitmap bm);
        public delegate void MessageAvailable(string message);

        /// <summary>
        /// subscribers event for images
        /// </summary>
        public event ImageAvailable ImageAvailableEvent;

        /// <summary>
        /// subscribers event for messages (errors, info, ...)
        /// </summary>
        public event MessageAvailable MessageAvailableEvent;

        /// <summary>
        /// Flag: camera device is running and delivers frames
        /// </summary>
        public bool IsRunning { get; internal set; } = false;

        /// <summary>
        /// returns frames per second
        /// </summary>
        public int FramesReceived { get { return videoSource.FramesReceived; } }

        /// <summary>
        /// Get last image
        /// </summary>
        /// <returns>last camera or loaded image</returns>
        public System.Drawing.Image GetImage() { return _image; }

        public System.Drawing.Bitmap GetBitmap() { return _image; }


        /// <summary>
        /// Get a list of all found DS cameras
        /// </summary>
        /// <returns>string list with camera names</returns>
        public List<string> GetCameraNamesList() { return CameraNamesList; }

        /// <summary>
        /// Flag for extended debugger outputs
        /// </summary>
        public bool ExtDiag { get; set; } = false;


        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource = null;
        private Bitmap _image = null;
        private bool bDeviceExist = false;
        private readonly List<string> CameraNamesList = new List<string>();
        private string msg = "?";
        private string info = "?";

        #endregion

        #region --- non interface device methods ---

        /// <summary>
        /// ctor
        /// </summary>
        public DS_Camera()
        {
            FindCameras();
        }

        /// <summary>
        /// open first found camera
        /// </summary>
        /// <returns></returns>
        public bool Init()
        {
            Open();
            return IsRunning;
        }

        /// <summary>
        /// Find all cameras and store the devices names in a list
        /// </summary>
        /// <returns>true if cameras found</returns>
        public bool FindCameras()
        {
            CameraNamesList.Clear();
            try
            {
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (videoDevices.Count == 0)
                    throw new ApplicationException("No video devices");

                bDeviceExist = true;
                foreach (FilterInfo device in videoDevices)
                {
                    CameraNamesList.Add(device.Name);
                }
            }
            catch (ApplicationException aex)
            {
                bDeviceExist = false;
                msg = aex.Message;
            }
            return bDeviceExist;
        }

        /// <summary>
        /// Show the camera property dialog from DS camera driver
        /// </summary>
        /// <param name="cameraIndex">camera index in names list</param>
        /// <param name="windowHandle">parent window handle oder IntPtr.Zero for nonmodal dialog</param>
        public void ShowPropertyDialog(int cameraIndex, IntPtr windowHandle)
        {
            if (windowHandle == null)
                windowHandle = IntPtr.Zero;

            if (videoSource == null)
            {
                videoSource = new VideoCaptureDevice(videoDevices[cameraIndex].MonikerString);
            }

            if (videoSource != null)
            {
                videoSource.DisplayPropertyPage(windowHandle);   // modaler Dialog
            }
        }

        #endregion

        #region --- open and close video device ---

        public async Task WaitForOpenAsync(int cameraIndex = 0)
        {
            Debug.WriteLine("DS_Camera.WaitForOpen({0})", cameraIndex);
            await Task.Run(() => Open(cameraIndex));
            Debug.WriteLine("IsRunning = " + IsRunning);
        }

        /// <summary>
        /// Open the camera (syncronous)
        /// </summary>
        /// <param name="cameraIndex">index of camera in video device list</param>
        public bool Open(int cameraIndex = 0)
        {
            Debug.WriteLine("DS_Camera.Open()");

            if (IsRunning)
            {
                //await CloseCameraAsync();
                Task task = Task.Run(async () => await CloseCameraAsync().ConfigureAwait(true));
            }

            if (bDeviceExist)
            {
                try
                {
                    //videoSource = new VideoCaptureDevice(videoDevices[cameraIndex].MonikerString);
                    videoSource = new VideoCaptureDevice();
                    videoSource.Source = videoDevices[cameraIndex].MonikerString;
                    videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);
                    videoSource.VideoSourceError += new VideoSourceErrorEventHandler(video_Error);
                    Debug.WriteLine("  Video source: " + videoSource);
                    if (videoSource.VideoCapabilities == null)
                    {
                        Debug.WriteLine("  No VideoCapabilities!!!");
                    }
                    else
                    {
                        // show all video caps:
                        foreach (VideoCapabilities vc in videoSource.VideoCapabilities)
                        {
                            Size size = vc.FrameSize;
                            int framerate = vc.AverageFrameRate;

                            Debug.WriteLine("  Image size " + size.Width.ToString()
                                    + " * " + size.Height.ToString()
                                    + " / ca. " + framerate.ToString() + " fps");
                        }
                    }

                    #region --- experimental: get / change cam properties ---
                    /*
                                    int exposure = 500;
                                    // AForge.Video.DirectShow.CameraControlProperty: Pan (0), Tilt (1), Roll (2), Zoom (3), Exposure (4), Iris (5), Focus (6)
                                    // AForge.Video.DirectShow.CameraControlFlags: None (0), Auto (1), Manual (2);

                                    CameraControlFlags flags;
                                    //new 2022 - returns false:
                                    if (videoSource.GetCameraProperty(CameraControlProperty.Exposure, out exposure, out flags))
                                    {
                                        Debug.WriteLine($"  Get Exposure: value={exposure}, flags={flags}");
                                    }
                                    else
                                    {
                                        Debug.WriteLine("  Exposure not get!");
                                    }

                                    // new 2021 - returns false:
                                    if (videoSource.SetCameraProperty(CameraControlProperty.Exposure, 500, CameraControlFlags.Manual))
                                    {
                                        Debug.WriteLine("  Exposure set...");
                                    }
                                    else
                                    {
                                        Debug.WriteLine("  Exposure not set!");
                                    }

                                    Debug.WriteLine("Crossbar available: " + videoSource.CheckIfCrossbarAvailable());
                    */
                    #endregion

                    #region --- specific camera ident ---
                    /*				
                                   videoSource.DesiredFrameSize = new Size(640, 480);
                                   videoSource.DesiredFrameRate = 25;

                                   // IDS UI-1545LE
                                   // @device:sw:{860BB310-5D01-11D0-BD3B-00A0C911CE86}\{92ac0998-2f09-4676-853177ad878a62}
                                   // Size 1280 * 1024 / max. 13 fps

                                   // IDS uEye-1120 HDR cam	
                                   // @device:sw:{860BB310-5D01-11D0-BD3B-00A0C911CE86}\{92ac0998-2f09-4676-853177ad878a62}
                                   // Size 768 * 576 , max. 75 fps

                                   // Logitech Webcam
                                   // Size 320 * 240 / max. 100 fps
                                   // Size 352 * 288 / max. 100 fps
                                   // Size 160 * 120 / max. 100 fps
                                   // Size 176 * 144 / max. 100 fps
                                   // Size 640 * 480 / max. 100 fps

                                   if (videoSource.Source.Contains("vid_18ec&pid_3399"))
                                   {
                                       Debug.WriteLine("PC Camera USB2");
                                       videoSource.DesiredFrameSize = new Size(640, 480);
                                       videoSource.DesiredFrameRate = 30;
                                   }
                   */
                    if (videoSource.Source.Contains("vid_045e&pid_074a"))
                    {
                        Debug.WriteLine("Microsoft WebCamera USB2");
                    }
                    #endregion

                    //Task<bool> task2 = OpenCameraAsync();
                    Task<bool> task = Task.Run<bool>(async () => await OpenCameraAsync());
                    //bool b2 = await task2;

                    var serviceResult = task.Result; // hier wird tatsächlich gewartet!!!

                    IsRunning = videoSource.IsRunning;
                    msg = IsRunning ? "Open camera ok" : msg = "Open camera fail!";
                }
                catch (Exception ex)
                {
                    msg = ex.Message;
                }
            }
            else
            {
                msg = "No camera found";
                IsRunning = false;
            }
            SetMessage(msg);
            return IsRunning;
        }


        /// <summary>
        /// Open the video source async
        /// </summary>
        /// <param name="cameraIndex">camera index in names list</param>
        /// <returns>true if camera is running</returns>
        private async Task<bool> OpenCameraAsync()
        {
            Debug.WriteLine($"DS_Camera.OpenCameraAsync(...)");
            await Task.Run(() =>
            {
                videoSource.Start();
            });
            //Debug.WriteLine($"DS_Camera IsRunning ={videoSource.IsRunning}");
            return videoSource.IsRunning; // true
        }

        // experimental
        // videoSource.DisplayCrossbarPropertyPage(IntPtr.Zero);
        // System.NotSupportedException: "Crossbar configuration is not supported by currently running video source."

        /// <summary>
        /// close the video device safely
        /// </summary>
        public void Close()
        {
            if (videoSource != null)
            {
                if (videoSource.IsRunning)
                {
                    IsRunning = false;
                    Debug.WriteLine("DS_Camera.Close(): stopping video device ...");
                    videoSource.SignalToStop();
                    //Thread.Sleep(100);
                    videoSource.WaitForStop();
                }
                videoSource = null;
                Debug.WriteLine("DS_Camera.Close(): video device closed.");
            }
            msg = "Camera closed";
            SetMessage(msg);
        }

        /// <summary>
        /// 
        /// </summary>
        public async Task WaitForCloseAsync()
        {
            await CloseCameraAsync();
            msg = "Camera closed"; 
            SetMessage(msg);
            IsRunning = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="videoSource"></param>
        /// <returns></returns>
        public async Task CloseCameraAsync()
        {
            if (videoSource != null)
            {
                await Task.Run(() =>
                {
                    if (videoSource.IsRunning)
                    {
                        Debug.WriteLine("CloseCameraAsync(): stopping video device ...");
                        videoSource.SignalToStop();
                        Task.Delay(500).Wait();
                        Debug.WriteLine("CloseCameraAsync(): wait for stop ...");
                        videoSource.WaitForStop();
                    }
                    //videoSource = null;
                });
                Debug.WriteLine("CloseCameraAsync(): video device closed.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void CloseCamera()
        {
            if (videoSource != null)
            {
                if (videoSource.IsRunning)
                {
                    Debug.WriteLine("CloseCamera(): stopping video device ...");
                    videoSource.SignalToStop();
                    Task.Delay(500).Wait();
                    videoSource.WaitForStop();
                }
                Debug.WriteLine("CloseCamera(): video device closed.");
                videoSource = null;
            }
        }

        #endregion

        #region --- DS event handler ---

        /// <summary>
        /// eventhandler if new frame is ready
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Trace.WriteIf(ExtDiag, "*");
            _image = (Bitmap)eventArgs.Frame.Clone();
            ImageAvailableEvent?.Invoke(_image);
            
            // wenn aktiv, funktionieren GetImage(), GetBitmap() nicht.
            // Nebeneffekte???
            //_image.Dispose();
            //_image = null;
        }

        /// <summary>
        /// DS Event: a video error occured
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void video_Error(Object sender, VideoSourceErrorEventArgs eventArgs)
        {
            Trace.WriteLine("ERROR: video_Error!!!");
            Trace.WriteLine(eventArgs.Description);
            SetMessage("ERROR: " + eventArgs.Description.Substring(0, 30) + "...");
            IsRunning = false;
        }

        /// <summary>
        /// Send messages to subscribers 
        /// </summary>
        /// <param name="msg"></param>
        private void SetMessage(string msg)
        {
            if (MessageAvailableEvent != null)
            {
                MessageAvailableEvent(msg);
            }
        }

        #endregion

        #endregion
    }
}
