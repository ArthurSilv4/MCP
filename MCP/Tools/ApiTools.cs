using MCP.Clints;
using MCP.DTOs;
using MCP.Requests;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace MCP.Tools
{
    [McpServerToolType]
    public class ApiTools
    {
        private readonly ApiClient _apiClient;

        public ApiTools(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [McpServerTool, Description("Retorna todos os usuario do sistema")]
        public async Task<ToolDto<List<UserDto>>> GetUsers(CancellationToken ct)
        {
            try
            {
                var users = await _apiClient.GetUsers(ct);
                return new ToolDto<List<UserDto>>(false, "Usuários encontrados.", users);
            }
            catch (Exception ex)
            {
                return new ToolDto<List<UserDto>>(true, $"Erro ao buscar usuários: {ex.Message}", null);
            }
        }

        [McpServerTool, Description("Cria um novo usuário no sistema. Parâmetros: name (nome do usuário), email (email do usuário), senha (senha do usuário)")]
        public async Task<ToolDto<UserDto>> CreateUser(
            [Description("Nome do usuário")] string name,
            [Description("Email do usuário")] string email,
            [Description("Senha do usuário")] string password)
        {
            try
            {
                var userRequest = new UserRequest(name, email, password);
                var user = await _apiClient.CreateUser(userRequest);
                return new ToolDto<UserDto>(false, "Usuário criado com sucesso.", user);
            }
            catch (Exception ex)
            {
                return new ToolDto<UserDto>(true, $"Erro ao criar usuário: {ex.Message}", null);
            }
        }
    }
}
