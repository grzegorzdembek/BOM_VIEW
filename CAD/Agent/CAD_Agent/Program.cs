using CAD_Agent.Services;
using CAD_Agent.Factories;

namespace CAD_Agent
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
            {
                Console.WriteLine("BŁĄD: Nie wybrano głównego złożenia.");
                Console.WriteLine("Użyj programu poprzez menu 'Wyślij do' na pliku głównego złożenia.");
                Console.ReadKey();
                return;
            }

            if (args.Length != 1)
            {
                Console.WriteLine("Wybrano zbyt wiele plików.");
                Console.ReadKey();
                return;
            }

            string topLevelAssemblyPath = args[0];

            if (!File.Exists(topLevelAssemblyPath))
            {
                Console.WriteLine($"BŁĄD: Plik nie istnieje pod sciężką {topLevelAssemblyPath}");
                Console.ReadKey();
                return;
            }

            string topLevelAssemblyName = Path.GetFileNameWithoutExtension(topLevelAssemblyPath);

            string projectDirectory = Path.GetDirectoryName(topLevelAssemblyPath);

            Console.WriteLine("============================================================");
            Console.WriteLine("=== Wybór pliku głównego złożenia pod dane dla BOM VIEW. ===");
            Console.WriteLine("============================================================");          
            Console.WriteLine($"Nazwa pliku: {topLevelAssemblyName}");
            Console.WriteLine($"Ścieżka do pliku: {topLevelAssemblyPath}");
            Console.WriteLine($"Folder projektu: {projectDirectory}");
            Console.WriteLine($"Czy na pewno chcesz kontynuować?");
            Console.WriteLine("============================================================");
            Console.Write("Wciśnij [Y/y] aby kontynuować (Tak) lub [N/n] aby anulować (Nie)... ");

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);

                if (keyInfo.Key != ConsoleKey.Y && keyInfo.Key != ConsoleKey.N)
                {
                    Console.WriteLine();
                    Console.WriteLine("Wymagane potwierdzenie. Wciśnij klawisz Y(Tak) lub N(Nie).");
                }

                if (keyInfo.Key == ConsoleKey.Y)
                {
                    Console.WriteLine();
                    Console.WriteLine("Potwierdzono. Trwa uruchamianie Agenta...");
                    break;
                }

                if (keyInfo.Key == ConsoleKey.N)
                {
                    Console.WriteLine();
                    Console.WriteLine("Operacja anulowana.");
                    Console.WriteLine("Wciśnij dowolny klawisz, aby zamknąć...");
                    Console.ReadKey();
                    return;
                }
            }

            try
            {
                var adapter = CADAdapterFactory.GetAdapter(topLevelAssemblyPath);
                var bomData = adapter.GetBOMData(topLevelAssemblyPath);

                // PROJECT NAME
                foreach (var item in bomData)
                {
                    item.ProjectName = topLevelAssemblyName;
                }

                Console.WriteLine();
                Console.WriteLine("============================================================");
                Console.WriteLine("Rozpoczynamy synchronizację z bazą danych Supabase...");

                var supaBaseService = new SupabaseService();

                Console.WriteLine($"Czyszczenie starych danych dla projektu: {topLevelAssemblyName}...");
                await supaBaseService.DeleteProjectDataAsync(topLevelAssemblyName);

                Console.WriteLine("Wysyłanie zaktualizowanego zestawienia BOM...");
                await supaBaseService.UploadBOMDataAsync(bomData);

                Console.WriteLine();
                Console.WriteLine("============================================================");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Dane pomyślnie zapisane w chmurze.");
                Console.ResetColor();
                Console.WriteLine("============================================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine($"BŁĄD: {ex.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("Naciśnij dowolny klawisz, aby zamknąć...");
            Console.ReadKey();
        }
    }
}
