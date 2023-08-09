namespace SinovadMediaServer
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            ApplicationConfiguration.Initialize();
            System.Windows.Forms.Application.Run(new Form1(args));
        }

    }
}