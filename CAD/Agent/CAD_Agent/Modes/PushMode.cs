using CAD_Agent.Services;
using CAD_Agent.Factories;

namespace CAD_Agent.Modes
{
    internal static class PushMode
    {
        public static async Task ExecuteAsync(string topLevelAssemblyPath, string topLevelAssemblyName, string projectDirectory)
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

            /* to narazie nie jest istotne wiec pomijamy
            Console.WriteLine("Trwa weryfikacja i wysyłka miniatur na serwer plików...");
            await supaBaseService.UploadThumbnailsAsync(projectDirectory, topLevelAssemblyName);
            */

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
    }
}
