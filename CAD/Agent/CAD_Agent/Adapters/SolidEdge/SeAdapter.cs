using CAD_Agent.Interfaces;
using CAD_Agent.Models;

namespace CAD_Agent.Adapters.SolidEdge
{
    internal class SeAdapter : ICADAdapter
    {
        public List<BOMItem> GetBOMData(string filePath, Dictionary<string, Queue<BOMItem>> cloudProjectData)
        {
            string projectDirectory = Path.GetDirectoryName(filePath);
            string thumbnailsDirectory = Path.Combine(projectDirectory, "Miniatury");
            Directory.CreateDirectory(thumbnailsDirectory);

            DirectoryInfo dirInfo = new (projectDirectory);

            Dictionary<string, (string Extension, DateTime ModifiedDate)> projectFiles = dirInfo
                .EnumerateFiles("*.*", SearchOption.AllDirectories)
                .Where(f => f.Extension.Equals(".asm", StringComparison.OrdinalIgnoreCase) ||
                            f.Extension.Equals(".par", StringComparison.OrdinalIgnoreCase) ||
                            f.Extension.Equals(".psm", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(
                    f => Path.GetFileNameWithoutExtension(f.Name),
                    f => (Extension: f.Extension.ToLower(), ModifiedDate: f.LastWriteTimeUtc),
                    StringComparer.OrdinalIgnoreCase
                );

            Dictionary<string, string> thumbnails = Directory
                .GetFiles(thumbnailsDirectory, "*.jpg", SearchOption.TopDirectoryOnly)
                .ToDictionary(f => Path.GetFileNameWithoutExtension(f),
                              f => f,
                              StringComparer.OrdinalIgnoreCase);

            List<BOMItem> bomData = new ();
     
            SeApp application = null;
            SeDocument document = null;
            SeAssembly assembly = null;

            bool wasOpenByAgent = false;
            try
            {
                Console.WriteLine();
                Console.WriteLine("2. Rozpoczynamy połączenie z aplikacją Solid Edge:");
                application = GetApplication(out wasOpenByAgent);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Udało się połączyć z Solid Edge.");
                Console.ResetColor();

                Console.WriteLine();
                Console.WriteLine("3. Rozpoczynamy otwieranie głównego złożenia:");
                document = GetOpenDocument(application, filePath);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Udało się otworzyć główne złożenie.");
                Console.ResetColor();

                Console.WriteLine();
                Console.WriteLine("4. Rozpoczynamy skanowanie drzewa głównego złożenia:");
                if (document is SeAssembly assemblyDocument)
                {
                    assembly = assemblyDocument;
                }
                SeOccurrences occurrences = null;
                try
                {
                    occurrences = assembly.Occurrences;
                    SeDataScanner.Scan(occurrences, 
                                       bomData, 
                                       string.Empty, 
                                       projectFiles, 
                                       thumbnails, 
                                       thumbnailsDirectory,
                                       cloudProjectData);
                }
                finally
                {
                    SeHelper.ReleaseCom(ref occurrences);
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Udało się przeskanować drzewo.");
                Console.ResetColor();
            }
            finally
            {
                try
                {
                    assembly?.Close(false);
                    document?.Close(false);
                }
                catch
                {

                }
                SeHelper.ReleaseCom(ref assembly);
                SeHelper.ReleaseCom(ref document);
                Cleanup(application, wasOpenByAgent);
            }

            return bomData;
        }

        private SeApp GetApplication(out bool wasOpenByAgent)
        {
            wasOpenByAgent = false;
            try
            {
                SeApp app = (SeApp)Marshal.GetActiveObject("SolidEdge.Application");
                Console.WriteLine("Podpięto pod aktywną sesję Solid Edge.");
                return app;
            }
            catch 
            {
                Console.WriteLine("Brak aktywnej sesji Solid Edge. Tworzenie nowej instancji programu...");
                try
                {
                    Type type = Type.GetTypeFromProgID("SolidEdge.Application");
                    SeApp newApp = (SeApp)Activator.CreateInstance(type);
                    Console.WriteLine("Utworzono instancję Solid Edge.");

                    newApp.Visible = false;
                    newApp.DisplayAlerts = false;
                    wasOpenByAgent = true;
                    return newApp;
                }
                catch (Exception ex)
                {
                    throw new Exception("Nie udało się połączyć z Solid Edge.", ex);
                }
            }
        }

        private static SeDocument GetOpenDocument(SeApp application, string filePath)
        {
            SeDocument document = null;
            SeDocuments documents = null;

            try
            {
                application.DisplayAlerts = false;
                int seOpenNoAssemblyContext = 32;
                int seOpenNoVisible = 128;
                int openFlags = seOpenNoAssemblyContext | seOpenNoVisible;

                documents = application.Documents;
                document = (SeDocument)documents.Open(filePath, openFlags);
            }
            catch (Exception ex)
            {
                throw new Exception("Nie udało się otworzyć głównego złożenia.", ex);
            }
            finally
            {
                application.DisplayAlerts = true;
                SeHelper.ReleaseCom(ref documents);
            }

            return document;
        }

        private void Cleanup(SeApp application, bool wasStartedByAgent)
        {
            if (application != null)
            {
                if (wasStartedByAgent)
                {
                    try
                    {
                        application.Quit();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Ostrzeżenie przy zakończeniu procesu Solid Edge]: {ex.Message}");
                    }
                }
                Marshal.ReleaseComObject(application);
            }
        }
    }
}