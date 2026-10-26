using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<StargateContext>(options => 
    options.UseSqlite(builder.Configuration.GetConnectionString("StarbaseApiDatabase")));

builder.Services.AddMediatR(cfg =>
{
    cfg.AddRequestPreProcessor<CreateAstronautDutyPreProcessor>();
    cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Create/upgrade the SQLite database on startup so a fresh clone runs with just `dotnet run`.
    // Development only: in production, migrations should be applied as a deployment step,
    // since multiple app instances migrating at startup can race.
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<StargateContext>().Database.Migrate();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


