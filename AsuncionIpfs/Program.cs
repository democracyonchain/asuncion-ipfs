using AsuncionIpfs.Services;
using AsuncionIpfs.Services.IPFS;
using Blockfrost.Api.Services;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 268435456; // 256 MB, ajusta según sea necesario
});

builder.Services.AddSingleton<IHealthService, HealthService>(); // Asegúrate de tener una implementación de HealthService
builder.Services.AddSingleton<IMetricsService, MetricsService>(); // Asegúrate de tener una implementación de MetricsService

builder.Services.AddHttpClient<AsuncionIpfs.Services.IAddService, AsuncionIpfs.Services.IPFS.AddService>(client =>
{
    client.BaseAddress = new Uri("https://ipfs.blockfrost.io");
});




// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();


app.Run();
