using System.Reflection;

namespace TheGateKeeper.Server
{
    public static class Helper
    {
        public static string GetCurrentExecutedDirectory()
        {
            // Get the full path of the executable
            string executablePath = Assembly.GetExecutingAssembly().Location;

            // Extract just the directory path
            string directoryPath = Path.GetDirectoryName(executablePath);

            return directoryPath;
        }
    }
}
