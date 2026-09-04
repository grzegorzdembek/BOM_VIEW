using CAD_Agent.Services;
using CAD_Agent.Factories;

namespace CAD_Agent
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("=== Uruchomiono Agenta CAD! ===");
            Console.ResetColor();

            if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
            {
                Console.WriteLine();
                Console.WriteLine("Nie wybrano głównego złożenia.");
                Console.WriteLine("Użyj agenta poprzez upuszczenie na niego pliku głównego złożenia.");
                Console.WriteLine();

                Console.WriteLine("Naciśnij dowolny klawisz, aby zamknąć...");
                Console.ReadKey();
                return;
            }

            if (args.Length != 1)
            {
                Console.WriteLine();
                Console.WriteLine("Wybrano zbyt wiele plików.");
                Console.WriteLine();

                Console.WriteLine("Naciśnij dowolny klawisz, aby zamknąć...");
                Console.ReadKey();
                return;
            }

            string topLevelAssemblyPath = args[0];
            if (!File.Exists(topLevelAssemblyPath))
            {
                Console.WriteLine();
                Console.WriteLine($"Plik pod ściężką {topLevelAssemblyPath} nie istnieje.");
                Console.WriteLine();

                Console.WriteLine("Naciśnij dowolny klawisz, aby zamknąć...");
                Console.ReadKey();
                return;
            }

            string topLevelAssemblyName = Path.GetFileNameWithoutExtension(topLevelAssemblyPath);
            string projectDirectory = Path.GetDirectoryName(topLevelAssemblyPath);
            Console.WriteLine();
            Console.WriteLine("1. Wybór pliku głównego złożenia pod dane dla BOM VIEW:");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"Nazwa pliku: {topLevelAssemblyName}");
            Console.WriteLine($"Ścieżka do pliku: {topLevelAssemblyPath}");
            Console.WriteLine($"Folder projektu: {projectDirectory}");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine($"Czy na pewno chcesz kontynuować?");
            Console.Write("Wciśnij [Y/y] aby kontynuować (Tak) lub [N/n] aby anulować (Nie)... ");
            Console.WriteLine();

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
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Potwierdzono. Trwa uruchamianie procesu...");
                    Console.ResetColor();
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
                var supaBaseService = new SupabaseService();
                foreach (var item in bomData)
                {
                    item.ProjectName = topLevelAssemblyName;

                    if (!string.IsNullOrEmpty(item.Thumbnail))
                    {
                        item.Thumbnail = supaBaseService.GetPublicThumbnailUrl(topLevelAssemblyName, item.Thumbnail);
                    }
                }

                Console.WriteLine();
                Console.WriteLine("============================================================");
                Console.WriteLine("Rozpoczynamy synchronizację z bazą danych...");
                Console.WriteLine("Trwa weryfikacja i wysyłka miniatur na serwer plików...");
                await supaBaseService.UploadThumbnailsAsync(projectDirectory, topLevelAssemblyName);
                Console.WriteLine($"Czyszczenie starych danych dla projektu: {topLevelAssemblyName}...");
                await supaBaseService.DeleteProjectDataAsync(topLevelAssemblyName);
                Console.WriteLine("Wysyłanie zaktualizowanego zestawienia BOM...");
                await supaBaseService.UploadBOMDataAsync(bomData);
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Dane pomyślnie zapisano w chmurze.");
                Console.ResetColor();
                Console.WriteLine("============================================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"BŁĄD: {ex.Message}");
                Console.ResetColor();
            }

            Console.WriteLine();
            Console.ResetColor();
            Console.WriteLine("Naciśnij dowolny klawisz, aby zamknąć...");
            Console.ReadKey();
        }
    }
}
