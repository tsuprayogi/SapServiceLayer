//program.cs

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Registrasi controller dan akses context user
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// Middleware handler untuk inject session SAP dari token
builder.Services.AddTransient<SapSessionHandler>();

// Registrasi HttpClient dengan session handler
builder.Services.AddHttpClient<ISapService, SapService>()
    .AddHttpMessageHandler<SapSessionHandler>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "Sap7Layer",
        ValidAudience = "Miranesia",
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("AstungkaraBali-secret-key-1234567890")) // 🔑 pastikan sama dengan saat generate token
    };
});


var app = builder.Build();

// Middleware dasar
//app.UseHttpsRedirection();
app.UseExceptionHandler("/error");
app.UseAuthentication(); // ⬅️ Penting untuk proses token sebelum cek [Authorize]
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();