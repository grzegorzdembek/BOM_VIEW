using CAD_Agent.Models;

namespace CAD_Agent.Interfaces
{
    public interface ICADAdapter
    {
        List<BOMItem> GetBOM(string filePath);
    }
}
