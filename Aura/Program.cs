namespace Aura.GUI
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {

            ApplicationConfiguration.Initialize();
            GUI gui = new()
            {
                Text = $"{Application.ProductName} - v{Application.ProductVersion}",
                Icon = Properties.Resources.aura
            };
            Application.Run(gui);
        }
    }
}