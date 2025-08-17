
using CONSOLECHAT.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.Client;

var standardForegroundColor = ConsoleColor.White;
Console.ForegroundColor = standardForegroundColor;

var aiSolution = InputHelper.GetAISolution();

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive", true);

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

// config modelos de ai
var kernelBuilder = Kernel.CreateBuilder();
PromptExecutionSettings settings;

if (aiSolution == InputHelper.OLLAMA)
{
    kernelBuilder.AddOllamaChatCompletion(
        modelId: configuration["Ollama:Model"]!,
        endpoint: new Uri(configuration["Ollama:Endpoint"]!),
        serviceId: "chat");
    settings = new OllamaPromptExecutionSettings
    {
        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new() { RetainArgumentTypes = true })
    };
}
else if (aiSolution == InputHelper.OPENAI)
{
    kernelBuilder.AddOpenAIChatCompletion(
        modelId: configuration["OpenAI:Model"]!,
        apiKey: configuration["OpenAI:ApiKey"]!,
        serviceId: "chat");
    settings = new OpenAIPromptExecutionSettings
    {
        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new() { RetainArgumentTypes = true })
    };
}
else
    throw new Exception($"Solucao de AI invalida: {aiSolution}");

// config mcp client
var mcpName = configuration["MCP:Name"]!;
await using var mcpClient = await McpClientFactory.CreateAsync(new SseClientTransport(new()
{
    Name = mcpName,
    Endpoint = new Uri(configuration["MCP:Endpoint"]!)
}));

var tools = await mcpClient.ListToolsAsync().ConfigureAwait(false);

Kernel kernel = kernelBuilder.Build();
kernel.Plugins.AddFromFunctions(mcpName,
    tools.Select(aiFunction => aiFunction.AsKernelFunction()));

// config chat

var aiChatService = kernel.GetRequiredService<IChatCompletionService>();
var chatHistory = new ChatHistory();
chatHistory.Add(new ChatMessageContent(AuthorRole.System,
    "Você é um assistente de IA que ajuda a consultar minha api de usuarios " +
    "Você tem acesso a ferramentas da api" +
    "Ao gerar uma resposta coloca sempre no testo oque você fez" +
    "Ações que foram executadas"));
while (true)
{
    Console.WriteLine("Sua pergunta:");
    Console.ForegroundColor = ConsoleColor.Cyan;
    var userPrompt = Console.ReadLine();
    Console.ForegroundColor = standardForegroundColor;


    chatHistory.Add(new ChatMessageContent(AuthorRole.User, userPrompt));

    Console.WriteLine();
    Console.WriteLine("Resposta da IA:");
    Console.WriteLine();

    ChatMessageContent chatResult = await aiChatService
        .GetChatMessageContentAsync(chatHistory, settings, kernel);

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine(chatResult.Content);
    Console.ForegroundColor = standardForegroundColor;
    chatHistory.Add(new ChatMessageContent(AuthorRole.Assistant, chatResult.Content));

    Console.WriteLine();
    Console.WriteLine();

}
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
