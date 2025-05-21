using System;
using System.Windows.Forms;
using System.IO;
using Proyecto_Final.Data.Database;
using Proyecto_Final.UI.Forms;

namespace Proyecto_Final
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Configurar la ruta de la base de datos
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(baseDirectory, "Data"));
            
            // Inicializar la base de datos
            DatabaseInitializer.Initialize();
            
            // Iniciar la aplicación con el formulario principal
            Application.Run(new MainForm());
        }
    }
}