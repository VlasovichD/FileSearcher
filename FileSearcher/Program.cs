using System;
using System.Windows.Forms;

// Application to find the specified file on disk.
// Allows you to view the file in the text box.
// It is possible to compress the found file.

namespace FileSearcher
{
    class SearchAttributes
    {
        public string SearchPath { get; set; }
        public string SearchPattern { get; set; }
    }

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
