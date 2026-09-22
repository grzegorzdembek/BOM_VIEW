using CAD_Agent.Models;

namespace CAD_Agent.Adapters.SolidEdge
{
    internal class SeDataScanner
    {
        private static readonly Dictionary<string, BOMItem> globalCache = new (StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, int> typeCounters = new (StringComparer.OrdinalIgnoreCase);
        public static readonly List<string> FilesToUpload = new ();

        public static void Scan(SeOccurrences occurrences, List<BOMItem> bomData, string prefix, Dictionary<string, (string Extension, DateTime ModifiedDate)> projectFiles, Dictionary<string, string> thumbnails, string thumbnailsDirectory, Dictionary<string, Queue<BOMItem>> cloudProjectData)
        {
            int depth = string.IsNullOrEmpty(prefix) ? 0 : prefix.Split('.').Length;
            string indent = new(' ', depth * 4);

            Dictionary<string, BOMItem> internalCache = new (StringComparer.OrdinalIgnoreCase);
            int levelCounter = 0;

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
                        Console.WriteLine($"{indent}! Unknown occurrence was rejected: {ex.Message}");
                        Console.ResetColor();
                        continue;
                    }

                    if (occurrence.IncludeInBom == false) 
                    { 
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine($"{indent}[-] Missed occurrence because it is excluded from BOM)");
                        Console.ResetColor(); 
                        continue; 
                    }

                    string OccurrencePath = occurrence.OccurrenceFileName; 
                    string OccurrenceName = Path.GetFileNameWithoutExtension(OccurrencePath);

                    if (internalCache.TryGetValue(OccurrenceName, out BOMItem existingItem)) 
                    { 
                        int currentStructureQuantity = existingItem.Structure_Quantity;
                        existingItem.Structure_Quantity = currentStructureQuantity + 1; 
                        continue; 
                    }

                    levelCounter++; 
                    string currentStructureID = string.IsNullOrEmpty(prefix) ? levelCounter.ToString() : $"{prefix}.{levelCounter}";
                    Console.WriteLine($"{indent}[{currentStructureID}] {OccurrenceName}");

                    BOMItem newItem = new ()
                    {
                        Structure_ID = currentStructureID,
                        Structure_Quantity = 1,
                        PartNumber = OccurrenceName
                    };

                    bool isAsmExtension = false;
                    DateTime currentFileDate = DateTime.MinValue;
                    if (projectFiles.TryGetValue(OccurrenceName, out var fileData)) 
                    {
                        isAsmExtension = fileData.Extension.Equals(".asm", StringComparison.OrdinalIgnoreCase);
                        currentFileDate = fileData.ModifiedDate;
                    }
                   
                    BOMItem cloudItem = null; 
                    if (cloudProjectData.TryGetValue(OccurrenceName, out Queue<BOMItem> queue) && queue.Count > 0)
                    { 
                        cloudItem = queue.Dequeue(); 
                    }

                    if (!globalCache.TryGetValue(OccurrenceName, out BOMItem cachedItem))
                    {
                        if (cloudItem != null && cloudItem.FileModifiedAt.HasValue && cloudItem.FileModifiedAt.Value >= currentFileDate)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkBlue; 
                            Console.WriteLine($"{indent} └─> Occurrence has no modification. Using cloud project data..."); 
                            Console.ResetColor();

                            newItem.Id = cloudItem.Id; 
                            newItem.FileModifiedAt = cloudItem.FileModifiedAt;
                            newItem.Thumbnail = cloudItem.Thumbnail; 
                            newItem.Parts_ID = cloudItem.Parts_ID;
                            newItem.PARTS_Quantity = cloudItem.PARTS_Quantity;
                            newItem.Type = cloudItem.Type;
                            newItem.Title = cloudItem.Title;
                            newItem.Provider = cloudItem.Provider;
                            newItem.MaterialName = cloudItem.MaterialName;
                            newItem.MechanicalMaterial = cloudItem.MechanicalMaterial;
                            newItem.Thickness = cloudItem.Thickness;
                            newItem.SizeX = cloudItem.SizeX;
                            newItem.SizeY = cloudItem.SizeY;
                            newItem.Finish = cloudItem.Finish;
                            newItem.Color = cloudItem.Color;
                            newItem.Mass = cloudItem.Mass;
                            newItem.Class = cloudItem.Class;
                            newItem.DxfDate = cloudItem.DxfDate;
                            newItem.TypeName = cloudItem.TypeName;

                            string typ = string.IsNullOrEmpty(newItem.Type) ? "Brak" : newItem.Type;
                            if (!typeCounters.ContainsKey(typ))
                            { 
                                typeCounters[typ] = 0;
                            }
                            globalCache[OccurrenceName] = newItem;
                        }
                        else
                        {
                            if (cloudItem != null) 
                            { 
                                Console.ForegroundColor = ConsoleColor.DarkYellow; 
                                Console.WriteLine($"{indent} └─> Occurrence has any modifictions. Using document properties.");
                                Console.ResetColor(); newItem.Id = cloudItem.Id; 
                            } 
                            else 
                            { 
                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                                Console.WriteLine($"{indent} └─> New occurrence. Using document properties."); 
                                Console.ResetColor(); newItem.Id = Guid.NewGuid().ToString();
                            } 

                            document = (SeDocument)occurrence.OccurrenceDocument;
                            using SePropertiesReader reader = new(document);

                            string type = reader.Type;
                            newItem.FileModifiedAt = currentFileDate;
                            newItem.PARTS_Quantity = reader.Quantity;
                            newItem.Type = type;
                            newItem.Title = reader.TitleEng ?? reader.TitlePl;
                            newItem.Provider = reader.Provider;
                            newItem.MaterialName = reader.MaterialName;
                            newItem.MechanicalMaterial = reader.MechanicalMaterial;
                            newItem.Thickness = reader.Thickness;
                            newItem.SizeX = reader.SizeX;
                            newItem.SizeY = reader.SizeY;
                            newItem.Finish = reader.Finish;
                            newItem.Color = reader.Color;
                            newItem.DxfDate = reader.DxfDate;
                            newItem.TypeName = SeHelper.GetExtendedType(type);

                            string typ = string.IsNullOrEmpty(newItem.Type) ? "Brak" : newItem.Type;
                            if (!typeCounters.ContainsKey(typ))
                            { 
                                typeCounters[typ] = 0; 
                            }
                            typeCounters[typ]++;
                            newItem.Parts_ID = typeCounters[typ];

                            string thumbnailPath = Path.Combine(thumbnailsDirectory, $"{OccurrenceName}.jpg");
                            try
                            {
                                SeHelper.ExtractAndSaveThumbnail(OccurrencePath, thumbnailPath, 256);
                                thumbnails[OccurrenceName] = thumbnailPath;
                                if (!FilesToUpload.Contains(thumbnailPath))
                                { 
                                    FilesToUpload.Add(thumbnailPath); 
                                }
                            }
                            catch (Exception ex) 
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"{indent} [!] Error generating thumbnail {OccurrenceName}: {ex.Message}");
                                Console.ResetColor(); }

                            newItem.Thumbnail = $"{OccurrenceName}.jpg";
                            globalCache[OccurrenceName] = newItem;
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green; 
                        Console.WriteLine($"{indent} └─> Using global cache:");
                        Console.ResetColor(); 

                        newItem.Id = cloudItem != null ? cloudItem.Id : Guid.NewGuid().ToString();

                        newItem.FileModifiedAt = cachedItem.FileModifiedAt;
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
                        newItem.TypeName = cachedItem.TypeName;
                    }

                    internalCache.Add(OccurrenceName, newItem);
                    bomData.Add(newItem);

                    if (isAsmExtension && newItem.Type == "A") 
                    { 
                        document = (SeDocument)occurrence.OccurrenceDocument; 
                        if (document is SeAssembly subAssemblyDoc)
                        { SeOccurrences subOccurrences = null; 
                            try 
                            { 
                                subOccurrences = subAssemblyDoc.Occurrences; 
                                Scan(subOccurrences, bomData, currentStructureID, projectFiles, thumbnails, thumbnailsDirectory, cloudProjectData); 
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