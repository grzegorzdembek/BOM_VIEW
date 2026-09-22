using CAD_Agent.Models;

namespace CAD_Agent.Services
{
    internal class SupabaseService
    {
        private readonly string _url;
        private readonly string _apiKey;

        public SupabaseService()
        {
            string secretsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "secrets.txt");
            if (!File.Exists(secretsPath))
            {
                throw new FileNotFoundException("Brak pliku secrets.txt z danymi do Supabase.");
            }

            string[] secrets = File.ReadAllLines(secretsPath);
            _url = secrets[0].Trim();
            _apiKey = secrets[1].Trim();
        }

        public async Task<Dictionary<string, Queue<BOMItem>>> GetProjectDataAsync(string projectName)
        {
            HttpClient client = new();
            client.DefaultRequestHeaders.Add("apikey", _apiKey);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            string queryUrl = $"{_url}?project_name=eq.{Uri.EscapeDataString(projectName)}";

            var response = await client.GetAsync(queryUrl);

            if (!response.IsSuccessStatusCode)
            {
                string errorResponse = await response.Content.ReadAsStringAsync();
                throw new Exception($"Nie udało się pobrać danych początkowych: {response.StatusCode}. Szczegóły: {errorResponse}");
            }

            string jsonResponse = await response.Content.ReadAsStringAsync();
            var items = JsonConvert.DeserializeObject<List<BOMItem>>(jsonResponse) ?? new List<BOMItem>();

            var cloudData = new Dictionary<string, Queue<BOMItem>>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in items)
            {
                if (!cloudData.ContainsKey(item.PartNumber))
                {
                    cloudData[item.PartNumber] = new Queue<BOMItem>();
                }

                cloudData[item.PartNumber].Enqueue(item);
            }

            return cloudData;
        }

        public async Task UploadBOMDataAsync(List<BOMItem> bomData)
        {
            string jsonBody = JsonConvert.SerializeObject(bomData);

            StringContent content = new (jsonBody, Encoding.UTF8, "application/json");
            HttpClient client = new ();

            client.DefaultRequestHeaders.Add("apikey", _apiKey);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            client.DefaultRequestHeaders.Add("Prefer", "return=minimal");

            var response = await client.PostAsync(_url, content);

            if (!response.IsSuccessStatusCode)
            {
                string errorResponse = await response.Content.ReadAsStringAsync();
                throw new Exception($"Serwer odrzucił dane: {response.StatusCode}. Szczegóły: {errorResponse}");
            }
        }

        public async Task DeleteProjectDataAsync(string projectName)
        {
            HttpClient client = new ();

            client.DefaultRequestHeaders.Add("apikey", _apiKey);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            string deleteUrl = $"{_url}?project_name=eq.{projectName}";

            var response = await client.DeleteAsync(deleteUrl);

            if (!response.IsSuccessStatusCode)
            {
                string errorResponse = await response.Content.ReadAsStringAsync();
                throw new Exception($"Nie udało się wyczyścić starych danych: {response.StatusCode}. Szczegóły: {errorResponse}");
            }
        }

        public string GetPublicThumbnailUrl(string projectName, string fileName)
        {
            Uri uri = new (_url);
            string baseUrl = $"{uri.Scheme}://{uri.Host}";

            string safeProject = Uri.EscapeDataString(projectName);
            string safeFile = Uri.EscapeDataString(fileName);

            return $"{baseUrl}/storage/v1/object/public/thumbnails/{safeProject}/{safeFile}";
        }

        public async Task UploadThumbnailsAsync(string projectDirectory, string projectName)
        {
            string thumbnailsDir = Path.Combine(projectDirectory, "Miniatury");
            if (!Directory.Exists(thumbnailsDir))
            {
                return;
            }

            string[] files = Directory.GetFiles(thumbnailsDir, "*.jpg");
            if (files.Length == 0)
            {
                return;
            }

            Uri uri = new (_url);
            string baseUrl = $"{uri.Scheme}://{uri.Host}";
            string safeProject = Uri.EscapeDataString(projectName);

            using HttpClient client = new();
            client.DefaultRequestHeaders.Add("apikey", _apiKey);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            int counter = 0;
            foreach (string filePath in files)
            {
                string fileName = Path.GetFileName(filePath);
                string safeFile = Uri.EscapeDataString(fileName);
                string uploadUrl = $"{baseUrl}/storage/v1/object/thumbnails/{safeProject}/{safeFile}";

                byte[] fileBytes = File.ReadAllBytes(filePath);
                using ByteArrayContent content = new(fileBytes);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

                var response = await client.PostAsync(uploadUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    if (!error.Contains("already exists"))
                    {
                        Console.WriteLine($"\n[Ostrzeżenie] Nie udało się wysłać {fileName}: {error}");
                    }
                }

                counter++;
                Console.Write($"\rWysyłanie miniatur: {counter}/{files.Length}");
            }
            Console.WriteLine();
        }
    }
}
