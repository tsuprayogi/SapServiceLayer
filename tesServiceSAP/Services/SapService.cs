// SapService.cs
using Newtonsoft.Json;
using System.Text;

public class SapService : ISapService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SapService> _logger;

    public SapService(HttpClient httpClient, ILogger<SapService> logger)
    {
        _httpClient = httpClient;        
        _logger = logger;
    }

    public async Task<SapSessionResponse> LoginAsync()
    {
        var payload = new
        {
            UserName = "manager",
            Password = "Admin123#",
            CompanyDB = "USER_TRAINING"
        };

        var url = "http://DESKTOP-NPFMUL9:50001/b1s/v1/Login";
        _logger.LogInformation("🔐 Kirim login ke: {Url}", url);

        var jsonPayload = JsonConvert.SerializeObject(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("❌ Gagal login ({StatusCode}): {Response}", response.StatusCode, json);
            throw new HttpRequestException("Login SAP gagal.");
        }

        var loginResult = JsonConvert.DeserializeObject<LoginResponse>(json);

if (loginResult == null || string.IsNullOrWhiteSpace(loginResult.SessionId))
{
    _logger.LogError("❌ SessionId kosong. Respons: {Json}", json);
    throw new Exception("Gagal mendapatkan SessionId dari respons SAP.");
}

// ➕ Tambahkan fallback nilai RouteId jika kosong
var routeId = string.IsNullOrWhiteSpace(loginResult.RouteId) ? "none" : loginResult.RouteId;

_logger.LogInformation("✅ Session ID: {SessionId}, Route ID: {RouteId}", loginResult.SessionId, routeId);

return new SapSessionResponse
{
    SessionId = loginResult.SessionId,
    RouteId = routeId
};
    }


    public async Task<List<BusinessPartnerDto>> GetBusinessPartnersAsync(string sessionId, string routeId)
    {
        if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(routeId))
            throw new ArgumentException("SessionId atau RouteId tidak valid.");

        var url = "http://DESKTOP-NPFMUL9:50001/b1s/v1/BusinessPartners?$select=CardCode,CardName,CardType";

        _httpClient.DefaultRequestHeaders.Remove("Cookie");
        _httpClient.DefaultRequestHeaders.Add("Cookie", $"B1SESSION={sessionId}; RouteId={routeId}");

        var response = await _httpClient.GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"❌ Gagal ambil Business Partners: {response.StatusCode}");
            throw new HttpRequestException("Gagal mengambil data Business Partners dari SAP.");
        }

        var result = JsonConvert.DeserializeObject<ODataResponse<BusinessPartnerDto>>(json);
        Console.WriteLine($"📦 Diterima {result?.Value.Count ?? 0} Business Partners dari SAP.");
        return result?.Value ?? new List<BusinessPartnerDto>();
    }



    public async Task CreateQueryAsync()
{
    var queryPayload = new
    {
        SqlCode = "MyNewSQLQuery",
        SqlName = "GetItems",
        SqlText = "SELECT ItemCode, ItemName, ItmsGrpCod FROM OITM"
    };

    var content = new StringContent(
    JsonConvert.SerializeObject(queryPayload),
    Encoding.UTF8,
    "application/json"
    );


    var url = "http://DESKTOP-NPFMUL9:50001/b1s/v1/SQLQueries";
    var response = await _httpClient.PostAsync(url, content);
    response.EnsureSuccessStatusCode();
}

public async Task<string> ExecuteQueryAsync(string sqlCode, string sessionId, string routeId)
{
    if (string.IsNullOrWhiteSpace(sqlCode))
        throw new ArgumentException("SqlCode tidak boleh kosong.");

    var url = $"http://DESKTOP-NPFMUL9:50001/b1s/v1/SQLQueries('{sqlCode}')/List";

    _httpClient.DefaultRequestHeaders.Remove("Cookie");
    _httpClient.DefaultRequestHeaders.Add("Cookie", $"B1SESSION={sessionId}; RouteId={routeId}");

    var response = await _httpClient.GetAsync(url);
    var content = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine($"❌ Gagal eksekusi query ({response.StatusCode}): {content}");
        throw new HttpRequestException("Gagal menjalankan query SQL di SAP.");
    }

    //Console.WriteLine($"📊 Hasil query: {content}");
    return content;
}

    public async Task<string> LogoutAsync(string sessionId, string routeId)
    {
        var url = "http://DESKTOP-NPFMUL9:50001/b1s/v1/Logout";

        _httpClient.DefaultRequestHeaders.Remove("Cookie");
        _httpClient.DefaultRequestHeaders.Add("Cookie", $"B1SESSION={sessionId}; RouteId={routeId}");

        var response = await _httpClient.PostAsync(url, null);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"❌ Logout gagal ({response.StatusCode}): {content}");
            throw new HttpRequestException("Gagal logout dari SAP.");
        }

        Console.WriteLine($"✅ Logout sukses: {content}");
        return content;
    }
}