using CAD_Agent.Models;

namespace CAD_Agent.Services
{
    public class SupabaseService
    {
        private readonly string _url;
        private readonly string _apiKey;

        public SupabaseService()
        {
            if (!File.Exists("secrets.txt"))
            {
                throw new FileNotFoundException("Brak pliku secrets.txt z kluczami do bazy danych!");
            }

            string[] secrets = File.ReadAllLines("secrets.txt");
            _url = secrets[0].Trim();
            _apiKey = secrets[1].Trim();
        }

        public async Task UploadBOMDataAsync(List<BOMItem> bomData)
        {
            string jsonBody = JsonConvert.SerializeObject(bomData);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            HttpClient client = new();
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
    }
}
