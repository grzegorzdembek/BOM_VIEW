using CAD_Agent.Interfaces;
using CAD_Agent.Adapters.SolidEdge;

namespace CAD_Agent.Factories
{
    public static class CADAdapterFactory
    {
        public static ICADAdapter GetAdapter(string filePath)
        {
            string fileExtension = Path.GetExtension(filePath).ToLower();

            if (fileExtension == ".asm")
            {
                return new SeAdapter();
            }

            throw new NotSupportedException($"Brak obsługi dla pliku o rozszerzeniu '{fileExtension}'");
        }
    }
}
