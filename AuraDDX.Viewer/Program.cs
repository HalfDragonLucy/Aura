using AuraDDX.Viewer;

namespace AuraViewer
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new Viewer());
        }
    }
}
