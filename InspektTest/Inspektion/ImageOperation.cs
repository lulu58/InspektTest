using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Visutronik.Inspektion
{
    abstract class ImageOperation
    {
        public string Name { get; set; }

        public string Parameters { get; set; }

        public string Result { get; set; }


        /// <summary>
        /// Execute the image operation using the parameters
        /// </summary>
        /// <returns>true if no operational errors</returns>
        public bool Execute()
        {
            return true;
        }
    }
}
