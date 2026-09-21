namespace CAD_Agent.Modes
{
    internal static class PullMode
    {
        public static async Task ExecuteAsync(string topLevelAssemblyPath, string topLevelAssemblyName, string projectDirectory)
        {
            Console.WriteLine();
            Console.WriteLine("============================================================");
            Console.WriteLine("Rozpoczynamy tryb PULL (Pobieranie zmian)...");


            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[ INFO ] Tryb PULL jest w trakcie implementacji.");
            Console.ResetColor();
            Console.WriteLine("============================================================");

            await Task.CompletedTask;
        }
    }
}
