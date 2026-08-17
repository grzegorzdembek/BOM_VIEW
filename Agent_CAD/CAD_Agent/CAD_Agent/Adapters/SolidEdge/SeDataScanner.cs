using CAD_Agent.Models;

namespace CAD_Agent.Adapters.SolidEdge
{
    internal class SeDataScanner
    {
        public static void Scan(SeOccurrences assemblyOccurrences, Dictionary<string, BOMItem> data, HashSet<string> processed)
        {
            ProcessScanning(assemblyOccurrences, data, processed, new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase));
        }

        private static void ProcessScanning(SeOccurrences occurrences, Dictionary<string, BOMItem> data, HashSet<string> processed, Dictionary<string, bool> assemblyCache)
        {
            int count = occurrences.Count;

            for (int i = 1; i <= count; i++)
            {
                SeOccurrence occurrence = null;
                SeDocument document = null;

                try
                {
                    occurrence = (SeOccurrence)occurrences.Item(i);

                    if (occurrence.IncludeInBom == false)
                    {
                        continue;
                    }

                    document = (SeDocument)occurrence.OccurrenceDocument;

                    string filePath = null;

                    try
                    {
                        filePath = document.FullName;
                    }
                    catch
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(filePath))
                    {
                        continue;
                    }

                    bool isAssembly = document is SeAssembly;

                    if (processed.Add(filePath))
                    {
                        using var properties = new SePropertiesReader(document);

                        data[filePath] = new BOMItem
                        {
                            FileName = Path.GetFileNameWithoutExtension(filePath),
                            Title = properties.TitleEng ?? properties.TitlePl,
                            Quantity = properties.Count
                        };

                        if (isAssembly)
                        {
                            assemblyCache[filePath] = properties.IsTypeA;
                        }
                    }

                    if (isAssembly && assemblyCache.TryGetValue(filePath, out bool isTypeA) && isTypeA)
                    {
                        SeOccurrences subOccurrences = null;

                        try
                        {
                            subOccurrences = ((SeAssembly)document).Occurrences;
                            ProcessScanning(subOccurrences, data, processed, assemblyCache);
                        }
                        finally
                        {
                            SeHelper.ReleaseCom(ref subOccurrences);
                        }
                    }
                }
                catch
                {
                    continue;
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