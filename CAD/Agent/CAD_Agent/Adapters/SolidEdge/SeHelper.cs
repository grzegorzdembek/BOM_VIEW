namespace CAD_Agent.Adapters.SolidEdge
{
    internal class SeHelper
    {
        public static string GetExtendedType(string type)
        {
            if ( type == Constants.SePartTypes.Assembly)
            {
                return Constants.SePartTypes.AssemblyExtended;
            }

            if (type == Constants.SePartTypes.Part)
            {
                return Constants.SePartTypes.PartExtended;
            }

            if (type == Constants.SePartTypes.SheetMetal)
            {
                return Constants.SePartTypes.SheetMetalExtended;
            }

            if (type == Constants.SePartTypes.Commercial)
            {
                return Constants.SePartTypes.CommercialExtended;
            }

            if (type == Constants.SePartTypes.Steelmaking)
            {
                return Constants.SePartTypes.SteelmakingExtended;
            }

            if (type == Constants.SePartTypes.Standard)
            {
                return Constants.SePartTypes.StandardExtended;
            }

            return null;
        }

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

        [StructLayout(LayoutKind.Sequential)]
        public struct SIZE
        {
            public int cx;
            public int cy;
        }

        [Flags]
        public enum SIIGBF
        {
            SIIGBF_RESIZETOFIT = 0x00,
            SIIGBF_BIGGERSIZEOK = 0x01,
            SIIGBF_MEMORYONLY = 0x02,
            SIIGBF_ICONONLY = 0x04,
            SIIGBF_THUMBNAILONLY = 0x08,
            SIIGBF_INCACHEONLY = 0x10,
        }

        [ComImport]
        [Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IShellItemImageFactory
        {
            void GetImage(
                [In, MarshalAs(UnmanagedType.Struct)] SIZE size,
                [In] SIIGBF flags,
                [Out] out IntPtr phbm);
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        public static extern void SHCreateItemFromParsingName(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszPath,
            [In] IntPtr pbc,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [Out, MarshalAs(UnmanagedType.Interface, IidParameterIndex = 2)] out IShellItemImageFactory ppv);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject(IntPtr hObject);

        public static void ExtractAndSaveThumbnail(string sourceFilePath, string targetImagePath, int size = 256)
        {
            IShellItemImageFactory factory = null;
            IntPtr hBitmap = IntPtr.Zero;

            try
            {
                Guid iid = typeof(IShellItemImageFactory).GUID;
                SHCreateItemFromParsingName(sourceFilePath, IntPtr.Zero, iid, out factory);

                SIZE imgSize = new() { cx = size, cy = size };
                factory.GetImage(imgSize, SIIGBF.SIIGBF_RESIZETOFIT, out hBitmap);

                if (hBitmap != IntPtr.Zero)
                {
                    using Image img = Image.FromHbitmap(hBitmap);
                    img.Save(targetImagePath, ImageFormat.Jpeg);
                }
            }
            finally
            {
                if (hBitmap != IntPtr.Zero)
                {
                    DeleteObject(hBitmap);
                }

                if (factory != null)
                {
                    Marshal.ReleaseComObject(factory);
                }
            }
        }

    }
}
