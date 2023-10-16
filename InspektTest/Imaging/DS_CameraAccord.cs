//
// DirectShow camera class based on Accord.NET library:
// http://accord-framework.net
// Accord.NET Framework is published under LGPL v3 license
// Lulu
// 29.03.2022

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using Accord.Video;
using Accord.Video.DirectShow;

namespace Visutronik.Imaging
{
    class DS_Camera
    {
		public delegate void ImageAvailable(System.Drawing.Bitmap bm);
		public delegate void MessageAvailable(string message);

		/// <summary>
		/// subscribers event for images
		/// </summary>
		public event ImageAvailable ImageAvailableEvent;

		/// <summary>
		/// subscribers event for messages
		/// </summary>
		public event MessageAvailable MessageAvailableEvent;

		/// <summary>
		/// Flag: camera device is open
		/// </summary>
		public bool IsOpen { get; internal set; } = false;

		/// <summary>
		/// returns frames per second
		/// </summary>
		public int FramesReceived { get { return videoSource.FramesReceived; } }

		/// <summary>
		/// Get last image
		/// </summary>
		/// <returns>last camera or loaded image</returns>
		public System.Drawing.Image GetImage()  { return _image; }

		/// <summary>
		/// Get a list of all found DS cameras
		/// </summary>
		/// <returns>string list with camera names</returns>
		public List<string> GetCameraNamesList() { return CameraNamesList; }

        private FilterInfoCollection videoDevices;
		private VideoCaptureDevice videoSource = null;
		private Bitmap _image = null;
		private bool bDeviceExist = false;
        private List<string> CameraNamesList = new List<string>();
		private string msg = "?";

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
            bool result = Open();
			return result;
        }

		/// <summary>
		/// Find all cameras and store the devices names in a list
		/// </summary>
		/// <returns>true if cameras found</returns>
		private bool FindCameras()
        {
            CameraNamesList.Clear();
            try
            {
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (videoDevices.Count == 0)
                    throw new ApplicationException();

                bDeviceExist = true;
                foreach (FilterInfo device in videoDevices)
                {
                    CameraNamesList.Add(device.Name);
                }
            }
            catch (ApplicationException)
            {
				SetMessage("No video devices found!");
                bDeviceExist = false;
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
			else
            {
				SetMessage("No video source!");
			}
		}


		/// <summary>
		/// Open the video source
		/// </summary>
		/// <param name="cameraIndex">camera index in names list</param>
		/// <returns>true is success</returns>
		public bool Open(int cameraIndex = 0)
		{
			Debug.WriteLine("OpenVideoSource()");

			bool bResult = false;
			if (bDeviceExist)
			{
				Close();
				videoSource = new VideoCaptureDevice(videoDevices[cameraIndex].MonikerString);
				videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);
				videoSource.VideoSourceError += new VideoSourceErrorEventHandler(video_Error);
				Debug.WriteLine("  Video source: " + videoSource.Source);

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

				#region --- experimental: change cam properties ---

				int exposure = 500;
				CameraControlFlags flags = CameraControlFlags.Manual;

				//new 2022 - returns false:
				if (videoSource.GetCameraProperty(CameraControlProperty.Exposure, out exposure, out flags))
				{
					Debug.WriteLine($"  Get Exposure: value={exposure}, flags={flags}");
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

                #endregion

                //videoSource.VideoResolution = new Size(640, 480);

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

                videoSource.Start();
				bResult = videoSource.IsRunning;
				msg = bResult ? "Open camera ok" :	"Open camera fail!";
			}
			else
            {
				msg = "No camera found";
			}
			IsOpen = bResult;
			SetMessage(msg);
			return bResult;
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
					Debug.WriteLine("stopping video device ...");
					videoSource.SignalToStop();
					videoSource.WaitForStop();
				}
				Debug.WriteLine("closing video device ...");
				videoSource = null;
			}
			msg = "Camera closed";
			SetMessage(msg);
			IsOpen = false;
		}

		#region --- DS event handler ---

		/// <summary>
		/// eventhandler if new frame is ready
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
		{
			Trace.Write("*");

			// Note:Since video source may have multiple clients, each client is responsible for
			// making a copy (cloning) of the passed video frame, because the video source disposes
			// its own original copy after notifying of clients.
			// http://www.aforgenet.com/framework/docs/html/3c0dd0f0-680a-373a-6518-626f4f29bc7b.htm

			_image = (Bitmap)eventArgs.Frame.Clone();

			if (ImageAvailableEvent != null)
			{
				ImageAvailableEvent(_image);
			}
			//aviWriter.AddFrame(image);
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
			IsOpen = false;
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



    }
}
