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

            public const string Type = "Typ"; // 1 

            public const string TitleEng = "Title"; public const string TitlePl = "Tytuł"; // 2

            public const string Provider = "Dostawca"; // 3

            public const string MaterialName = "material_nazwa"; // 4

            public const string Thickness = "Grubość materiału"; // 5

            public const string SizeX = "Model_Rozwinięcia_RozmiarArkuszaX"; // 6

            public const string SizeY = "Model_Rozwinięcia_RozmiarArkuszaY"; // 7

            public const string Material = "Material"; // 8 

            public const string Finish = "Finish"; // 9

            public const string Color = "Color"; // 10

            public const string Mass = "Masa"; // 11

            public const string Quantity = "Ilość"; // 12

            public const string DxfDate = "DXF"; // 13      
        }

        public static class SePartTypes
        {
            public const string Assembly = "A"; public const string AssemblyFull = "Złożenie";
            public const string SheetMetal = "B"; public const string SheetMetalFulFull = "Blacha";
            public const string Part = "C"; public const string PartFull = "Część";
            public const string Commercial = "Z"; public const string CommercialFull = "Handlowe";
            public const string Steelmaking = "K"; public const string SteelmakingFull = "Hutnicze";
            public const string Standard = "N"; public const string StandardFull = "Normalia";
        }
    }
}
