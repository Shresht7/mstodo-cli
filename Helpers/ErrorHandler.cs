using System;
using System.IO;

namespace mstodo_cli
{
    public static class ErrorHandler
    {
        public static void HandleException(Exception ex, string appDir)
        {
            Console.WriteLine($"Error: {ex.Message}");

            // Log the full exception details to a file
            string logFilePath = Path.Combine(appDir, "error.log");
            File.AppendAllText(logFilePath, $"[{DateTime.Now}] {ex.ToString()}\n");
            Console.WriteLine($"For more details, please check the log file: {logFilePath}");
        }
    }
}