using System.Text.Json.Serialization;
using BlackjackApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers use a separate JSON pipeline from minimal API's
// ConfigureHttpJsonOptions, so the string-enum converter (needed so the
// React frontend can send/receive "Hit" instead of a raw number) has to be
// registered here too.
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// GameSessionService holds ALL session state now that GameEngine is
// stateless (Option B) - see its own comments for why Singleton is fine
// here and where a real multi-user app would need to change this.
builder.Services.AddSingleton<GameSessionService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactDev", policy =>
    {
        // Vite's default dev server port. Adjust if your frontend runs elsewhere.
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowReactDev");
app.MapControllers();

app.Run();