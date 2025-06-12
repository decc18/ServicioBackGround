using ServicioBackground.Logging;
using ServicioBackground.Data;
using NLog.Web;
using ServicioBackground.BackgroundWorker;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseWindowsService();

// Crear carpeta de logs si no existe
var logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
if (!Directory.Exists(logDirectory))
{
    Directory.CreateDirectory(logDirectory);
}

// Configura puerto
builder.WebHost.UseUrls("http://localhost:8019");

// Add NLog
builder.Logging.ClearProviders(); // Elimina otros providers de logging
builder.Logging.SetMinimumLevel(LogLevel.Information); // Ajusta el nivel mínimo si es necesario
builder.Host.UseNLog();

// Register NLogLogger as singleton
builder.Services.AddSingleton<INLogLogger, NLogLogger>();

// Register repository
builder.Services.AddSingleton<ITransaccionRepository, TransaccionRepository>();

// Registrar el servicio en background
builder.Services.AddHostedService<PosBackgroundService>();
builder.Services.AddHostedService<PosBackgroundServiceReintentos>(); 

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo {  Title = "Servicio V1.1", Version = "1.0" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger(options => options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0);
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.Run();
