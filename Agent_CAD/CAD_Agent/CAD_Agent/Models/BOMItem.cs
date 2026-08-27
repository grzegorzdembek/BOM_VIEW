/*
 * LISTA strukturalna:
 * 
 * Structure_ID -> "Nr"  
 * Structure_Quantity -> "Ilość"
 * 
 * FILENAME -> "Numer części"
 * THUMBNAIL -> "Miniatura"
 * 
 * 1. TYPE - > "Typ" 
 * 2. TITLE -> "Nazwa" 
 * 3. PROVIDER -> "Dostawca"
 * 4. MATERIAL NAME -> "Rodzaj materiału"
 * 5. THICKNESS -> "Grubość"
 * 6. WIDTH -> "Szerokość"
 * 7. LENGTH -> "Długość"
 * 8. MECHANICAL MATERIAL -> Materiał
 * 9. FINISH -> "Wykończenie"
 * 10. COLOR -> "Kolor"
 * 11. MASS -> "Masa"
 * 12. DXF -> "Data wygenerowania Dxf"
 */

/*
 * LISTA Części: 
 * 
 * Parts_ID -> "Nr"  
 * Parts_Quantity -> "Ilość"
 * 
 * FILENAME -> "Numer części"
 * THUMBNAIL -> "Miniatura"
 * 
 * 1. TYPE - > "Typ" 
 * 2. TITLE -> "Nazwa" 
 * 3. PROVIDER -> "Dostawca"
 * 4. MATERIAL NAME -> "Rodzaj materiału"
 * 5. THICKNESS -> "Grubość"
 * 6. WIDTH -> "Szerokość"
 * 7. LENGTH -> "Długość"
 * 8. MECHANICAL MATERIAL -> Materiał
 * 9. FINISH -> "Wykończenie"
 * 10. COLOR -> "Kolor"
 * 11. MASS -> "Masa"
 * 12. DXF -> "Data wygenerowania Dxf"
 */

namespace CAD_Agent.Models
{
    public class BOMItem
    {
        [JsonProperty("project_name")]
        public string ProjectName { get; set; }

        [JsonProperty("structure_id")]
        public string Structure_ID { get; set; }        // Nr w strukturze


        [JsonProperty("structure_quantity")]
        public int Structure_Quantity { get; set; }  // Ilość na poziomie danego złożenia. Zliczana podczas skanowania.


        [JsonProperty("parts_id")]
        public int Parts_ID { get; set; }            // Nr w liście części


        [JsonProperty("parts_quantity")]
        public int PARTS_Quantity { get; set; }         // Ilość z właściwości. Całkowita ilość do Listy Części.


        [JsonProperty("type")]
        public string Type { get; set; }                // Typ


        [JsonProperty("part_number")]
        public string PartNumber { get; set; }          // Numer części (FileName)


        [JsonProperty("title")]
        public string Title { get; set; }               // Nazwa


        [JsonProperty("provider")]
        public string Provider { get; set; }            // Dostawca


        [JsonProperty("material_name")]
        public string MaterialName { get; set; }        // Rodzaj materiału (MaterialName)


        [JsonProperty("thickness")]
        public double Thickness { get; set; }           // Grubość [mm]


        [JsonProperty("size_x")]
        public double SizeX { get; set; }               // Szerokość [mm]


        [JsonProperty("size_y")]
        public double SizeY { get; set; }               // Długość [mm]


        [JsonProperty("mechanical_material")]
        public string MechanicalMaterial { get; set; }            // Materiał (MechanicalMaterial)


        [JsonProperty("class")]
        public string Class { get; set; }               // Klasa


        [JsonProperty("finish")]
        public string Finish { get; set; }              // Wykończenie


        [JsonProperty("color")]
        public string Color { get; set; }               // Kolor


        [JsonProperty("mass")]
        public double Mass { get; set; }                // Masa (Jedn.)


        [JsonProperty("dxf_date")]
        public string DxfDate { get; set; }             // Data Dxf


        [JsonProperty("thumbnail")]
        public string Thumbnail { get; set; }           // Miniatura
    }
}
