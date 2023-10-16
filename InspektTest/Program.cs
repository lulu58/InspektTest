using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Visutronik.InspektTest
{
    internal static class Program
    {
        //mit Mutex Mehrfachstart verhindern
        static bool createdNew;     // Flag ob Programm gerade neu gestartet wird

        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Mutex erzeugen
            System.Threading.Mutex mutex = new System.Threading.Mutex(true, Application.ProductName, out createdNew);

            if (createdNew)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());

                //try
                //{
                //    Application.Run(new MainForm());
                //}
                //catch (Exception ex) 
                //{
                //    MessageBox.Show("InspektTest: " + ex.Message);
                //}

                // Mutex auch wieder Freigeben besser ist besser
                mutex.ReleaseMutex();
            }
            else
            {
                MessageBox.Show("Programm wurde bereits gestartet!",
                                Application.ProductName,
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
