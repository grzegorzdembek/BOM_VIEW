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

            string topLevelAssembly_Path = args[0];
            if (!File.Exists(topLevelAssembly_Path))
            {
                Console.WriteLine();
                Console.WriteLine($"Plik pod ściężką {topLevelAssembly_Path} nie istnieje.");
                Console.WriteLine();

                Console.WriteLine("Naciśnij dowolny klawisz, aby zamknąć...");
                Console.ReadKey();
                return;
            }

            string topLevelAssembly_Name = Path.GetFileNameWithoutExtension(topLevelAssembly_Path);
            string topLevelAssembly_Extension = Path.GetExtension(topLevelAssembly_Path);
            string project_Directory = Path.GetDirectoryName(topLevelAssembly_Path);

            Console.WriteLine();
            Console.WriteLine("1. Wybór pliku głównego złożenia pod dane dla BOM VIEW:");
            Console.WriteLine();
            Console.WriteLine($"{"Nazwa pliku",-40}| {"Folder projektu",-60}|");
            Console.ForegroundColor = ConsoleColor.Magenta;
            string fullFileName = $"{topLevelAssembly_Name}{topLevelAssembly_Extension}";
            Console.WriteLine($"{fullFileName,-40}| {project_Directory,-60}|");

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
                var adapter = CADAdapterFactory.GetAdapter(topLevelAssembly_Path);
                var bomData = adapter.GetBOMData(topLevelAssembly_Path);
                var supaBaseService = new SupabaseService();
                foreach (var item in bomData)
                {
                    item.ProjectName = topLevelAssembly_Name;

                    if (!string.IsNullOrEmpty(item.Thumbnail))
                    {
                        item.Thumbnail = supaBaseService.GetPublicThumbnailUrl(topLevelAssembly_Name, item.Thumbnail);
                    }
                }

                Console.WriteLine();
                Console.WriteLine("============================================================");
                Console.WriteLine("Rozpoczynamy synchronizację z bazą danych...");
                Console.WriteLine("Trwa weryfikacja i wysyłka miniatur na serwer plików...");
                await supaBaseService.UploadThumbnailsAsync(project_Directory, topLevelAssembly_Name);
                Console.WriteLine($"Czyszczenie starych danych dla projektu: {topLevelAssembly_Name}...");
                await supaBaseService.DeleteProjectDataAsync(topLevelAssembly_Name);
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
