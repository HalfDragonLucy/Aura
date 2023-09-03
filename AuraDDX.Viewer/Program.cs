using AuraDDX.Viewer;

namespace AuraViewer
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            string[] commandLineArgs = Environment.GetCommandLineArgs();
            Application.Run(new Viewer(commandLineArgs));
        }
    }
}
