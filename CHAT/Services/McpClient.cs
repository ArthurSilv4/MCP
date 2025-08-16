using ModelContextProtocol.Client;

namespace CHAT.Services
{
    public class McpClient
    {
        private readonly SseClientTransport _transport;
        private IMcpClient? _mcpClient;
        private IList<McpClientTool>? _tools;
        private readonly string _endpoint;
        private int _executionCount = 0;

        public McpClient(IConfiguration configuration)
        {
            _endpoint = configuration["McpServer:Endpoint"] ?? "https://mcp-kdq3.onrender.com";
            Console.WriteLine($"[MCP] Initializing client with endpoint: {_endpoint}");

            _transport = new SseClientTransport(new SseClientTransportOptions
            {
                Name = "MCP Database Server",
                Endpoint = new Uri(_endpoint)
            });
        }

        public async Task InitializeAsync()
        {
            try
            {
                Console.WriteLine($"[MCP] Connecting to server at {_endpoint}...");
                _mcpClient = await McpClientFactory.CreateAsync(_transport);
                Console.WriteLine("[MCP] Connection established, listing tools...");
                
                _tools = await _mcpClient.ListToolsAsync();
                Console.WriteLine($"[MCP] Client initialized with {_tools.Count} tools");
                
                foreach (var tool in _tools)
                {
                    Console.WriteLine($"[MCP] Tool available: {tool.Name} - {tool.Description}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MCP] Error initializing client: {ex.GetType().Name} - {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[MCP] Inner exception: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}");
                }
                Console.WriteLine($"[MCP] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<IList<McpClientTool>> GetToolsAsync()
        {
            if (_tools == null) 
            {
                Console.WriteLine("[MCP] Tools not initialized, initializing now...");
                await InitializeAsync();
            }
            return _tools!;
        }

        public async Task<string> ExecuteToolAsync(string toolName, Dictionary<string, string> args)
        {
            try
            {
                _executionCount++;
                Console.WriteLine($"[MCP] 🚀 INICIANDO Execução #{_executionCount} - Ferramenta: {toolName}");
                
                if (_mcpClient == null) 
                {
                    Console.WriteLine("[MCP] Client not initialized, initializing now...");
                    await InitializeAsync();
                }

                var tool = _tools!.FirstOrDefault(t => t.Name == toolName);
                if (tool == null)
                {
                    var availableTools = string.Join(", ", _tools!.Select(t => t.Name));
                    var errorMsg = $"❌ Ferramenta '{toolName}' não encontrada.\n\n🔧 Ferramentas disponíveis: {availableTools}";
                    Console.WriteLine($"[MCP] {errorMsg}");
                    return errorMsg;
                }

                Console.WriteLine($"[MCP] 📝 Argumentos: {string.Join(", ", args.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");

                var startTime = DateTime.Now;
                var result = await _mcpClient!.CallToolAsync(tool.Name, args.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value));
                var executionTime = DateTime.Now - startTime;
                
                var resultText = result?.ToString() ?? "Nenhum resultado retornado";
                
                Console.WriteLine($"[MCP] ✅ CONCLUÍDA Execução #{_executionCount} em {executionTime.TotalMilliseconds}ms");
                Console.WriteLine($"[MCP] 📊 Tamanho: {resultText.Length} chars - Preview: {resultText.Substring(0, Math.Min(100, resultText.Length))}...");
                
                // Formatear resultado com marcadores claros
                var formattedResult = FormatToolResult(toolName, resultText, _executionCount);
                
                return formattedResult;
            }
            catch (Exception ex)
            {
                var errorMessage = $"❌ **ERRO na Execução #{_executionCount}**\n\nFerramenta: {toolName}\nErro: {ex.Message}";
                Console.WriteLine($"[MCP] {errorMessage}");
                return errorMessage;
            }
        }

        private string FormatToolResult(string toolName, string result, int executionNumber)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            
            var icon = toolName switch
            {
                "get_available_tables" => "📋",
                "get_table_structure" => "🏗️",
                "execute_custom_query" => "💾",
                "smart_search" => "🔍",
                _ => "🔧"
            };

            var toolDisplayName = toolName switch
            {
                "get_available_tables" => "Listagem de Tabelas",
                "get_table_structure" => "Estrutura da Tabela", 
                "execute_custom_query" => "Consulta SQL",
                "smart_search" => "Busca Inteligente",
                _ => toolName
            };

            // Resultado com marcadores específicos para detecção
            return $@"🚀 **{icon} {toolDisplayName} (Execução #{executionNumber})** [{timestamp}]

{result}

---
";
        }

        public async Task<string> TestConnectionAsync()
        {
            try
            {
                Console.WriteLine($"[MCP] Testing connection to {_endpoint}...");
                
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10);
                
                var response = await httpClient.GetAsync(_endpoint);
                Console.WriteLine($"[MCP] HTTP status: {response.StatusCode}");
                
                return $"✅ Servidor acessível. Status: {response.StatusCode}";
            }
            catch (Exception ex)
            {
                var errorMessage = $"❌ Erro de conectividade: {ex.GetType().Name} - {ex.Message}";
                Console.WriteLine($"[MCP] {errorMessage}");
                return errorMessage;
            }
        }

        public void ResetExecutionCount()
        {
            _executionCount = 0;
            Console.WriteLine("[MCP] 🔄 Contador de execuções resetado");
        }

        public int GetExecutionCount() => _executionCount;
    }
}
