using Hitch;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddHitch();

builder.Services.AddControllers();

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
