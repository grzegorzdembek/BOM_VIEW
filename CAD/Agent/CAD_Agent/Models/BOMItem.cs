namespace CAD_Agent.Models
{
    public class BOMItem
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("structure_id")] 
        public string Structure_ID { get; set; }   
        
        [JsonProperty("structure_quantity")]
        public int Structure_Quantity { get; set; }  

        [JsonProperty("parts_id")]  
        public int Parts_ID { get; set; }  
        
        [JsonProperty("parts_quantity")]
        public int PARTS_Quantity { get; set; }

        [JsonProperty("part_number")] 
        public string PartNumber { get; set; }

        [JsonProperty("thumbnail")] 
        public string Thumbnail { get; set; }

        [JsonProperty("drawing")]
        public string Drawing { get; set; }

        [JsonProperty("type")] 
        public string Type { get; set; }               
 
        [JsonProperty("title")]
        public string Title { get; set; }
 
        [JsonProperty("provider")]
        public string Provider { get; set; }
 
        [JsonProperty("material_name")]
        public string MaterialName { get; set; }

        [JsonProperty("mechanical_material")]
        public string MechanicalMaterial { get; set; }

        [JsonProperty("thickness")]
        public double Thickness { get; set; }

        [JsonProperty("size_x")]
        public double SizeX { get; set; }

        [JsonProperty("size_y")]
        public double SizeY { get; set; }

        [JsonProperty("finish")]
        public string Finish { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("mass")]
        public double Mass { get; set; }

        [JsonProperty("class")]
        public string Class { get; set; }

        [JsonProperty("dxf_date")]
        public string DxfDate { get; set; }

        [JsonProperty("type_name")] 
        public string TypeName { get; set; }

        [JsonProperty("project_name")] 
        public string ProjectName { get; set; }

        [JsonProperty("sync_pending")]
        public bool SyncPending { get; set; }

        [JsonProperty("dynamic_properties")]
        public Dictionary<string, string> DynamicProperties { get; set; } = new Dictionary<string, string>();

        [JsonProperty("file_modified_at")]
        public DateTime? FileModifiedAt { get; set; }
    }
}
