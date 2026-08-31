using CAD_Agent.Interfaces;
using CAD_Agent.Models;

namespace CAD_Agent.Adapters.SolidEdge
{
    public class SeAdapter : ICADAdapter
    {
        public List<(string FileName, string Path)> _projectFiles;
        public List<BOMItem> GetBOMData(string filePath)
        {
            string projectDirectory = Path.GetDirectoryName(filePath);

            Dictionary<string, string> projectFiles = Directory
                .GetFiles(projectDirectory, "*.*", SearchOption.AllDirectories)
                .Where(f => f.EndsWith(".asm", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".par", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".psm", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(
                    f => Path.GetFileNameWithoutExtension(f), 
                    f => Path.GetExtension(f).ToLower()       
                );

            List<BOMItem> bomData = new ();
     
            SeApp application = null;
            SeDocument document = null;
            SeAssembly assembly = null;

            bool wasOpenByAgent = false;
            try
            {
                // 1.
                Console.WriteLine("============================================================");
                Console.WriteLine("Rozpoczynamy połączenie z aplikacją Solid Edge.");
                Console.WriteLine("============================================================");
                application = GetApplication(out wasOpenByAgent);
                Console.WriteLine("Udało się połączyć z Solid Edge.");

                // 2.
                Console.WriteLine("============================================================");
                Console.WriteLine("Rozpoczynamy otwieranie głównego złożenia.");
                Console.WriteLine("============================================================");
                document = GetOpenDocument(application, filePath);
                Console.WriteLine("Udało się otworzyć główne złożenie.");

                // 3.
                Console.WriteLine("============================================================");
                Console.WriteLine("Rozpoczynamy skanowanie drzewa głównego złożenia.");
                Console.WriteLine("============================================================");
                if (document is SeAssembly assemblyDocument)
                {
                    assembly = assemblyDocument;
                }
                SeOccurrences occurrences = null;
                try
                {
                    occurrences = assembly.Occurrences;
                    SeDataScanner.Scan(occurrences, bomData, string.Empty, projectFiles);
                }
                finally
                {
                    SeHelper.ReleaseCom(ref occurrences);
                }
                Console.WriteLine("Udało się przeskanować drzewo.");
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