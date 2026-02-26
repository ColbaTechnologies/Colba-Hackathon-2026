using WebApplication1.Persistence;
using WebApplication1.Processing;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
builder.Services.AddRavenDb(configuration);

builder.Services.AddSingleton<IMessageStore, MessageStore>();
builder.Services.AddSingleton<IMessageQueue, MessageQueue>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.Run();