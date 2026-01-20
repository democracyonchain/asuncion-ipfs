using AsuncionIpfs.Services;
using AsuncionIpfs.Services.IPFS;
using AsuncionIpfs.Services.Cardano;
using Microsoft.AspNetCore.Http.Features;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 268435456;
});

// IPFS client
builder.Services.AddHttpClient<IAddService, AddService>((sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var ipfsKey = cfg["Blockfrost:Ipfs:ApiKey"];

    if (string.IsNullOrWhiteSpace(ipfsKey))
        throw new Exception("Falta Blockfrost:Ipfs:ApiKey en appsettings.json");

    client.BaseAddress = new Uri("https://ipfs.blockfrost.io/api/v0/");
    client.DefaultRequestHeaders.Add("project_id", ipfsKey);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

// Cardano client (tu servicio actual)
builder.Services.AddHttpClient<BlockfrostService>(client => { })
.ConfigureHttpClient((sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();

    var network = (cfg["Blockfrost:Network"] ?? "preview").ToLowerInvariant();
    var cardanoKey = cfg["Blockfrost:Cardano:ApiKey"];

    if (string.IsNullOrWhiteSpace(cardanoKey))
        throw new Exception("Falta Blockfrost:Cardano:ApiKey en appsettings.json");

    var baseUrl = network switch
    {
        "preview" => "https://cardano-preview.blockfrost.io/api/v0/",
        "preprod" => "https://cardano-preprod.blockfrost.io/api/v0/",
        _ => "https://cardano-mainnet.blockfrost.io/api/v0/"
    };

    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("project_id", cardanoKey);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

// CORS + ASP.NET
builder.Services.AddCors(o => o.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
