using CAD_Agent.Factories;
using CAD_Agent.Modes;
using CAD_Agent.Services;

namespace CAD_Agent
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Magenta; Console.WriteLine("Uruchomiono Agenta CAD!"); Console.ResetColor();

            if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
            {
                Console.WriteLine(); Console.WriteLine("Nie wybrano głównego złożenia."); Console.WriteLine("Użyj agenta poprzez upuszczenie na niego pliku głównego złożenia.");
                Console.WriteLine(); Console.WriteLine("Naciśnij dowolny klawisz, aby zamknąć..."); Console.ReadKey();
                return;

            }
            if (args.Length != 1)
            {
                Console.WriteLine(); Console.WriteLine("Wybrano zbyt wiele plików.");
                Console.WriteLine(); Console.WriteLine("Naciśnij dowolny klawisz, aby zamknąć..."); Console.ReadKey();
                return;
            }

            string topLvlAsmPath = args[0];
            if (!File.Exists(topLvlAsmPath))
            {
                Console.WriteLine(); Console.WriteLine($"Nie odnaleziono pliku: {topLvlAsmPath}.");
                Console.WriteLine(); Console.WriteLine("Naciśnij dowolny klawisz, aby zamknąć..."); Console.ReadKey();
                return;
            }

            string topLvlAsmName = Path.GetFileNameWithoutExtension(topLvlAsmPath);
            string topLvlAsmExtension = Path.GetExtension(topLvlAsmPath);
            string projectDirectory = Path.GetDirectoryName(topLvlAsmPath);

            Console.WriteLine(); Console.WriteLine("Wybór pliku głównego złożenia pod dane dla BOM VIEW:");
            Console.WriteLine($"{"Nazwa pliku",-40}| {"Folder projektu",-60}|");
            string fullFileName = $"{topLvlAsmName}{topLvlAsmExtension}";
            Console.ForegroundColor = ConsoleColor.Magenta; Console.WriteLine($"{fullFileName,-40}| {projectDirectory,-60}|"); Console.ResetColor();
             
            Console.WriteLine(); Console.WriteLine("Wybierz tryb pracy Agenta:"); Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("[ 1 ] PUSH MODE - Przygotowuje dane z otwartego złożenia i wysyła je do bazy.");
            Console.WriteLine("[ 2 ] PULL MODE - Pobiera dane z bazy i modyfikuje właściwości plików. ");
            Console.ForegroundColor = ConsoleColor.DarkYellow; Console.WriteLine("[ 0 ] Anuluj i wyjdź"); Console.ResetColor(); Console.Write("Wybierz opcję: ");

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);

                if (keyInfo.Key == ConsoleKey.D1 || keyInfo.Key == ConsoleKey.NumPad1)
                {
                    Console.WriteLine("1"); Console.WriteLine(); 
                    Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine("Potwierdzono. Trwa uruchamianie PUSH MODE..."); Console.ResetColor();

                    try { await PushMode.ExecuteAsync(topLvlAsmPath, topLvlAsmName, projectDirectory); }
                    catch (Exception ex) { Console.WriteLine(); Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine($"BŁĄD W TRYBIE PUSH: {ex.Message}"); Console.ResetColor(); }
                    break;
                }
                else if (keyInfo.Key == ConsoleKey.D2 || keyInfo.Key == ConsoleKey.NumPad2)
                {
                    Console.WriteLine("2"); Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine("Potwierdzono. Trwa uruchamianie procesu PULL..."); Console.ResetColor();

                    try { await PullMode.ExecuteAsync(topLvlAsmPath, topLvlAsmName, projectDirectory); }
                    catch (Exception ex) { Console.WriteLine(); Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine($"BŁĄD W TRYBIE PULL: {ex.Message}"); Console.ResetColor(); }
                    break;
                }
                else if (keyInfo.Key == ConsoleKey.D0 || keyInfo.Key == ConsoleKey.NumPad0 || keyInfo.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine("0"); Console.WriteLine(); Console.WriteLine("Operacja anulowana.");
                    break;
                }
            }

            Console.WriteLine(); Console.ResetColor(); Console.WriteLine("Naciśnij dowolny klawisz, aby zamknąć..."); Console.ReadKey();
        }
    }
}