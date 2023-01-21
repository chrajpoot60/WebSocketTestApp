using WebSocketTestApp;
using WebSocketTestApp.Extensions;
using WebSocketTestApp.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<WebSocketObjectCollection>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(1)
};

app.UseWebSockets(webSocketOptions);

//app.UseMiddleware<HttpContextMiddleware>();

app.AddWebSocketMiddleware();

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAuthorization();

app.MapControllers();

app.Run();

