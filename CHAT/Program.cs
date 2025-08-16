using CHAT.Components;
using CHAT.Services;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// MCP Client
var mcpClient = new McpClient(builder.Configuration);
await mcpClient.InitializeAsync();
builder.Services.AddSingleton(mcpClient);

// Configurar Semantic Kernel
var kernelBuilder = Kernel.CreateBuilder();

// Adicionar OpenAI Chat Completion
kernelBuilder.AddOpenAIChatCompletion(
    modelId: "gpt-4o-mini",
    apiKey: builder.Configuration["OpenAI:Key"]!
);

// Obter tools do MCP e adicionar como funções no kernel
var tools = await mcpClient.GetToolsAsync();
var kernelFunctions = new List<KernelFunction>();

foreach (var tool in tools)
{
    var kernelFunction = KernelFunctionFactory.CreateFromMethod(
        method: async (Dictionary<string, object> parameters) =>
        {
            try
            {
                var stringArgs = parameters.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString() ?? "");
                var result = await mcpClient.ExecuteToolAsync(tool.Name, stringArgs);
                return result;
            }
            catch (Exception ex)
            {
                return $"Erro ao executar ferramenta {tool.Name}: {ex.Message}";
            }
        },
        functionName: tool.Name,
        description: tool.Description ?? "Ferramenta MCP sem descrição"
        // Remover InputSchema pois não existe no McpClientTool
    );
    
    kernelFunctions.Add(kernelFunction);
}

// Adicionar todas as funções MCP como um plugin
if (kernelFunctions.Any())
{
    kernelBuilder.Plugins.AddFromFunctions("McpTools", kernelFunctions);
}

var kernel = kernelBuilder.Build();
builder.Services.AddSingleton(kernel);

// Registrar IChatCompletionService
builder.Services.AddSingleton<IChatCompletionService>(serviceProvider =>
    serviceProvider.GetRequiredService<Kernel>().GetRequiredService<IChatCompletionService>());

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.UseStaticFiles();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
