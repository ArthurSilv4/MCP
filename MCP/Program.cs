using MCP.Clints;
using ModelContextProtocol.Protocol;

var builder = WebApplication.CreateBuilder(args);

var serverInfo = new Implementation { Name = "DotnetMCPServerSSE", Version = "1.0.0" };

builder.Services
    .AddMcpServer(mcp =>
    {
        mcp.ServerInfo = serverInfo;
    })
    .WithHttpTransport()
    .WithToolsFromAssembly();

builder.Services.AddHttpClient<ApiClient>(client =>
{
    var baseAddress = builder.Configuration["API_BASE_ADDRESS"];
    if (string.IsNullOrEmpty(baseAddress))
    {
        throw new InvalidOperationException("API_BASE_ADDRESS configuration is not set.");
    }
    client.BaseAddress = new Uri(baseAddress);
});

var app = builder.Build();

app.MapMcp();

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

await app.RunAsync();
