namespace MCP.DTOs
{
    public record ToolDto<T>(bool Error, string? Message, T? Data);
}
