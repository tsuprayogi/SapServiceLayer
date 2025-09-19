using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Newtonsoft.Json;


[ApiController]
[Route("api/[controller]")]
public class SapController : ControllerBase
{
    private readonly ISapService _sapService;

    public SapController(ISapService sapService)
    {
        _sapService = sapService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginToSap([FromBody] LoginRequestDto loginRequest)
    {
        try
        {
            // 👉 Bisa kamu ubah kalau login pakai parameter (username, password, db)
            var sapSession = await _sapService.LoginAsync();

            var token = JwtHelper.GenerateToken(
                userName: loginRequest.Username ?? "manager",
                sessionId: sapSession.SessionId,
                routeId: sapSession.RouteId
            );

            return Ok(new
            {
                message = "✅ Login berhasil ke SAP.",
                token = token,
                sessionId = sapSession.SessionId,
                routeId = sapSession.RouteId
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = "❌ Login gagal.",
                error = ex.Message
            });
        }
    }

    [Authorize]
    [HttpGet("business-partners")]
    public async Task<IActionResult> GetBusinessPartners()
    {
        var sessionId = User.FindFirst("SapSessionId")?.Value;
        var routeId = User.FindFirst("RouteId")?.Value;

        if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(routeId))
        {
            return Unauthorized(new
            {
                message = "❌ Token tidak mengandung session SAP yang valid."
            });
        }

        try
        {
            var partners = await _sapService.GetBusinessPartnersAsync(sessionId, routeId);
            return Ok(new
            {
                message = $"✅ Ditemukan {partners.Count} partner bisnis.",
                data = partners
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "❌ Gagal mengambil data dari SAP.",
                error = ex.Message
            });
        }
    }

[Authorize]
[HttpPost("create-query")]
public async Task<IActionResult> CreateQuery()
{
    var sessionId = User.FindFirst("SapSessionId")?.Value;
    var routeId = User.FindFirst("RouteId")?.Value;

    if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(routeId))
    {
        return Unauthorized(new
        {
            message = "❌ Token tidak memuat session SAP yang valid untuk membuat query."
        });
    }

    try
    {
        // Sisipkan session ke HttpClient via middleware
        await _sapService.CreateQueryAsync();

        return Ok(new
        {
            message = "✅ Query SQL berhasil dibuat di SAP Service Layer."
        });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new
        {
            message = "❌ Gagal membuat query di SAP.",
            error = ex.Message
        });
    }
}


[Authorize]
[HttpGet("run-query/{sqlCode}")]
public async Task<IActionResult> RunQuery(string sqlCode)
{
    var sessionId = User.FindFirst("SapSessionId")?.Value;
    var routeId = User.FindFirst("RouteId")?.Value;

    if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(routeId))
    {
        return Unauthorized(new { message = "❌ Token tidak memuat session SAP yang valid." });
    }

        try
        {
            var result = await _sapService.ExecuteQueryAsync(sqlCode, sessionId, routeId);        
            var parsed = JsonConvert.DeserializeObject<QueryResultDto>(result);
            return Ok(new {
            message = "✅ Query berhasil dijalankan.",
            data = parsed.Value
        });
       // return Ok(new { message = "✅ Query berhasil dijalankan.", data = JsonConvert.DeserializeObject<object>(result) });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "❌ Gagal menjalankan query.", error = ex.Message });
        }
}

[Authorize]
[HttpPost("logout")]
public async Task<IActionResult> LogoutFromSap()
{
    var sessionId = User.FindFirst("SapSessionId")?.Value;
    var routeId = User.FindFirst("RouteId")?.Value;

    if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(routeId))
    {
        return Unauthorized(new
        {
            message = "❌ Token tidak memuat session SAP yang valid untuk logout."
        });
    }

    try
    {
        var result = await _sapService.LogoutAsync(sessionId, routeId);
        return Ok(new
        {
            message = "✅ Logout dari SAP Service Layer berhasil.",
            result
        });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new
        {
            message = "❌ Gagal logout dari SAP.",
            error = ex.Message
        });
    }
}
}