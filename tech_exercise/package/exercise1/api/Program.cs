using Microsoft.EntityFrameworkCore;
using StargateAPI.Business;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options => options.InvalidModelStateResponseFactory = ApiExceptionHandler.InvalidModelState);
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
// Required by UseExceptionHandler() in .NET 8; never used, since ApiExceptionHandler handles every exception.
builder.Services.AddProblemDetails();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml")));
builder.Services.AddDbContext<StargateContext>(options => 
    options.UseSqlite(builder.Configuration.GetConnectionString("StarbaseApiDatabase")));

builder.Services.AddStargateMediatR();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Create/upgrade and seed the SQLite database on startup so a fresh clone runs with just `dotnet run`.
    // Development only: in production, migrations should be applied as a deployment step,
    // since multiple app instances migrating at startup can race.
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<StargateContext>();
    context.Database.Migrate();
    DevelopmentSeeder.Seed(context);
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Exposes the generated Program class to the tests' WebApplicationFactory.
public partial class Program { }
