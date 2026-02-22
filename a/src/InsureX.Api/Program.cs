using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------
// Add services to the container
// ----------------------------

// Add controllers (if you later want API controllers)
builder.Services.AddControllers();

// Add OpenAPI / Swagger support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Replaces builder.Services.AddOpenApi();

// (Optional) Add EF DbContext here if needed
// builder.Services.AddDbContext<InsureXDbContext>(options =>
//     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// ----------------------------
// Configure the HTTP request pipeline
// ----------------------------

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(); // Swagger UI at /swagger
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Map minimal API endpoints
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", 
    "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        )).ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// Map controllers (optional for future API endpoints)
app.MapControllers();

app.Run();

// ----------------------------
// Records / DTOs
// ----------------------------
record WeatherForecast(DateOnly Date, int TemperatureC, string Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}