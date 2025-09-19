// ISapService.cs

using System.Threading.Tasks;
using System.Collections.Generic;

public interface ISapService
{
    /// <summary>
    /// Melakukan login ke SAP Service Layer dan mengembalikan session info.
    /// </summary>
    Task<SapSessionResponse> LoginAsync();

    /// <summary>
    /// Mengambil daftar Business Partners menggunakan session SAP yang aktif.
    /// </summary>
    Task<List<BusinessPartnerDto>> GetBusinessPartnersAsync(string sessionId, string routeId);

    Task CreateQueryAsync();

    Task<string> ExecuteQueryAsync(string sqlCode, string sessionId, string routeId);

    Task<string> LogoutAsync(string sessionId, string routeId);
}