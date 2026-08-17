namespace CAD_Agent.Adapters.SolidEdge
{
    public class SeHelper
    {
        public static void ReleaseCom<T>(ref T comObject) where T : class
        {
            if (comObject != null)
            {
                try
                {
                    Marshal.ReleaseComObject(comObject);
                }
                finally
                {
                    comObject = null;
                }
            }
        }
    }
}
