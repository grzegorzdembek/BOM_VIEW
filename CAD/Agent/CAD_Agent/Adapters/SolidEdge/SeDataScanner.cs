using CAD_Agent.Models;

namespace CAD_Agent.Adapters.SolidEdge
{
    internal class SeDataScanner
    {
        private static readonly Dictionary<string, BOMItem> globalCache = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, int> typeCounters = new(StringComparer.OrdinalIgnoreCase);

        public static void Scan(SeOccurrences occurrences,
                                List<BOMItem> bomData, 
                                string prefix, 
                                Dictionary<string, string> projectFiles, 
                                Dictionary<string, string> thumbnails,
                                string thumbnailsDirectory)
        {
            Dictionary<string, BOMItem> internalCache = new(StringComparer.OrdinalIgnoreCase);
            int levelCounter = 0;

            int depth = string.IsNullOrEmpty(prefix) ? 0 : prefix.Split('.').Length;
            string indent = new (' ', depth * 4);

            int count = occurrences.Count;
            for (int i = 1; i <= count; i++)
            { 
                SeOccurrence occurrence = null;
                SeDocument document = null;
                try
                {
                    try
                    {
                        occurrence = (SeOccurrence)occurrences.Item(i);
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"{indent}!!! BŁĄD: Odrzucono wystąpienie z powodu: {ex.Message}");
                        Console.ResetColor();
                        continue;
                    }
                    string OccurrencePath = occurrence.OccurrenceFileName;         
                    string OccurrenceName = Path.GetFileNameWithoutExtension(OccurrencePath);

                    if (occurrence.IncludeInBom == false)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine($"{indent}[-] Pominięto: {OccurrenceName} (Wykluczono z BOM)");
                        Console.ResetColor();
                        continue;
                    }              

                    if (internalCache.TryGetValue(OccurrenceName, out BOMItem existingItem))
                    {
                        int currentStructureQuantity = existingItem.Structure_Quantity;
                        existingItem.Structure_Quantity = currentStructureQuantity + 1;

                        continue;
                    }

                    levelCounter++;
                    string currentStructureID = string.IsNullOrEmpty(prefix) ? levelCounter.ToString() : $"{prefix}.{levelCounter}";
                    Console.WriteLine($"{indent}[{currentStructureID}] {OccurrenceName}");

                    BOMItem newItem = new()
                    {
                        Structure_ID = currentStructureID,
                        Structure_Quantity = 1,
                        PartNumber = OccurrenceName
                    };

                    bool isAsmExtension = false;
                    if (projectFiles.TryGetValue(OccurrenceName, out string extension))
                    {
                        isAsmExtension = extension.Equals(".asm", StringComparison.OrdinalIgnoreCase);
                    }

                    if (!globalCache.TryGetValue(OccurrenceName, out BOMItem cachedItem))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine($"{indent} └─> Odczyt właściwości...");
                        Console.ResetColor();

                        document = (SeDocument)occurrence.OccurrenceDocument;

                        using SePropertiesReader reader = new(document);

                        newItem.PARTS_Quantity = reader.Quantity;

                        newItem.Type = reader.Type;
                        newItem.Title = reader.TitleEng ?? reader.TitlePl;
                        newItem.Provider = reader.Provider;

                        newItem.MaterialName = reader.MaterialName;
                        newItem.MechanicalMaterial = reader.MechanicalMaterial;

                        newItem.Thickness = reader.Thickness;
                        newItem.SizeX = reader.SizeX;
                        newItem.SizeY = reader.SizeY;

                        newItem.Finish = reader.Finish;
                        newItem.Color = reader.Color;

                        //newItem.Mass = reader.Mass;
                        //newItem.Class = reader.Class; 

                        newItem.DxfDate = reader.DxfDate;
                        
                        string typ = string.IsNullOrEmpty(newItem.Type) ? "Brak" : newItem.Type;
                        if (!typeCounters.ContainsKey(typ))
                        {
                            typeCounters[typ] = 0;
                        }
                        typeCounters[typ]++;

                        newItem.Parts_ID = typeCounters[typ];

                        if (!thumbnails.ContainsKey(OccurrenceName))
                        {
                            string thumbnailPath = Path.Combine(thumbnailsDirectory, $"{OccurrenceName}.jpg");
                            try
                            {
                                SeHelper.ExtractAndSaveThumbnail(OccurrencePath, thumbnailPath, 256);
                                thumbnails[OccurrenceName] = thumbnailPath;
                            }
                            catch (Exception ex)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"{indent} [!] Nie udało się wygenerować miniatury dla {OccurrenceName}: {ex.Message}");
                                Console.ResetColor();
                            }
                        }

                        newItem.Thumbnail = $"{OccurrenceName}.jpg";

                        globalCache[OccurrenceName] = newItem;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine($"{indent} └─> Sklonowano dane z pamięci podręcznej");
                        Console.ResetColor();

                        newItem.Parts_ID = cachedItem.Parts_ID;
                        newItem.PARTS_Quantity = cachedItem.PARTS_Quantity;

                        newItem.Thumbnail = cachedItem.Thumbnail;

                        newItem.Type = cachedItem.Type;
                        newItem.Title = cachedItem.Title;
                        newItem.Provider = cachedItem.Provider;

                        newItem.MaterialName = cachedItem.MaterialName;
                        newItem.MechanicalMaterial = cachedItem.MechanicalMaterial;

                        newItem.Thickness = cachedItem.Thickness;
                        newItem.SizeX = cachedItem.SizeX;
                        newItem.SizeY = cachedItem.SizeY;  
                        
                        newItem.Finish = cachedItem.Finish;
                        newItem.Color = cachedItem.Color;

                        newItem.Mass = cachedItem.Mass;
                        newItem.Class = cachedItem.Class;

                        newItem.DxfDate = cachedItem.DxfDate;
                    }

                    internalCache.Add(OccurrenceName, newItem);
                    bomData.Add(newItem);

                    if (isAsmExtension && newItem.Type == "A")
                    {
                        document ??= (SeDocument)occurrence.OccurrenceDocument;

                        if (document is SeAssembly subAssemblyDoc)
                        {
                            SeOccurrences subOccurrences = null;
                            try
                            {
                                subOccurrences = subAssemblyDoc.Occurrences;
                                Scan(subOccurrences, bomData, currentStructureID, projectFiles, thumbnails, thumbnailsDirectory);
                            }
                            finally
                            {
                                SeHelper.ReleaseCom(ref subOccurrences);
                            }
                        }
                    }

                }
                finally
                {
                    SeHelper.ReleaseCom(ref document);
                    SeHelper.ReleaseCom(ref occurrence);
                }
            }
        }
    }
}