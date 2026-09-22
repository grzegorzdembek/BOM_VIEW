using CAD_Agent.Services;
using CAD_Agent.Factories;
using CAD_Agent.Models;

namespace CAD_Agent.Modes
{
    internal static class PushMode
    {
        public static async Task ExecuteAsync(string topLevelAssemblyPath, string topLevelAssemblyName, string projectDirectory)
        {
            SupabaseService sbService = new ();

            Console.WriteLine(); Console.WriteLine("Wywiad agenta z chmurą: Sprawdzanie poprzedniego stanu projektu...");
            Dictionary<string, Queue<BOMItem>> cloudProjectData = await sbService.GetProjectDataAsync(topLevelAssemblyName);

            var adapter = CADAdapterFactory.GetAdapter(topLevelAssemblyPath);
            var bomData = adapter.GetBOMData(topLevelAssemblyPath, cloudProjectData);
            

            foreach (var item in bomData)
            {
                item.ProjectName = topLevelAssemblyName;

                if (!string.IsNullOrEmpty(item.Thumbnail))
                {
                    item.Thumbnail = sbService.GetPublicThumbnailUrl(topLevelAssemblyName, item.Thumbnail);
                }
            }

            /* to narazie nie jest istotne wiec pomijamy
            Console.WriteLine("Trwa weryfikacja i wysyłka miniatur na serwer plików...");
            await supaBaseService.UploadThumbnailsAsync(projectDirectory, topLevelAssemblyName);
            */

            Console.WriteLine("Wysyłanie zaktualizowanego zestawienia BOM...");
            await sbService.UploadBOMDataAsync(bomData);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Dane pomyślnie zapisano w chmurze.");
            Console.ResetColor();
        }
    }
}
