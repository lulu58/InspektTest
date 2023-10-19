// Schnittstellendatei für Kamerainterface

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Visutronik.Imaging
{
    public interface ICamera
    {
        string GetCameraInfo();

        bool InitCamera(int camidx, string[] camparam);

        bool StartCamera(int camidx);

        bool StopCamera(int camidx);

        Bitmap AcquireImage(int camidx);

        string GetLastError();

    }
}
