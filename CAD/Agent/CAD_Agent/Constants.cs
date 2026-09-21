namespace CAD_Agent
{
    public static class Constants
    {
        public static class SeProperties
        {
            public const string CustomSet = "Custom"; 
            public const string ExtendedSummarySet = "ExtendedSummaryInformation";
            public const string SummarySet = "SummaryInformation";
            public const string MechanicalModelingSet = "MechanicalModeling";
            public const string Type = "Typ"; 
            public const string TitleEng = "Title"; public const string TitlePl = "Tytuł";
            public const string Provider = "Dostawca"; 
            public const string MaterialName = "material_nazwa"; 
            public const string Thickness = "Grubość materiału"; 
            public const string SizeX = "Model_Rozwinięcia_RozmiarArkuszaX"; 
            public const string SizeY = "Model_Rozwinięcia_RozmiarArkuszaY"; 
            public const string Material = "Material"; 
            public const string Finish = "Finish"; 
            public const string Color = "Color"; 
            public const string Mass = "Masa"; 
            public const string Quantity = "Ilość"; 
            public const string DxfDate = "DXF";       
        }

        public static class SePartTypes
        {
            public const string Assembly = "A"; public const string AssemblyExtended = "Złożenie";
            public const string SheetMetal = "B"; public const string SheetMetalExtended = "Blacha";
            public const string Part = "C"; public const string PartExtended = "Część";
            public const string Commercial = "Z"; public const string CommercialExtended = "Handlowe";
            public const string Steelmaking = "K"; public const string SteelmakingExtended = "Hutnicze";
            public const string Standard = "N"; public const string StandardExtended = "Normalia";
        }
    }
}
