using System.Text.Json.Serialization;
using BlackjackApi.Services;
using BlackjackApi.Engine;


var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<GameEngine>();

builder.Services.AddSingleton<GameSessionService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactDev", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Membangun aplikasi Web API berdasarkan konfigurasi builder di atas
var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();

// Menerapkan aturan izin CORS yang sudah dikonfigurasi sebelumnya
app.UseCors("AllowReactDev");

// Memetakan HTTP request yang masuk ke masing-masing Controller yang sesuai
app.MapControllers();

// Menjalankan HTTP web server ASP.NET Core
app.Run();