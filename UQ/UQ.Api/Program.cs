using Microsoft.EntityFrameworkCore;
using UQ.Api.Application;
using UQ.Api.Infrastructure;
using UQ.Api.Infrastructure.Quartz;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
var services = builder.Services;
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddControllers();
services.AddHealthChecks();

services.AddDbContextPool<AppDbContext>(opt =>
{
    opt.UseMySQL(builder.Configuration.GetConnectionString("uqDb"));
});
services.AddTransient<DbContext, AppDbContext>();
services.AddTransient<IAppDbContext, AppDbContext>();

builder.Services.AddHttpClient();
services.AddTransient<IProducer, Producer>();
services.AddTransient<IProducer, Producer>();
services.AddTransient<IConsumer, Consumer>();

services.AddQuartzJobs(builder.Configuration);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHealthChecks("/health");


app.Run();

namespace UQ.Api
{
    record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
    {
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}