using MCP.Clints;
using MCP.DTOs;
using Microsoft.Data.SqlClient;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace MCP.Tools
{
    [McpServerToolType]
    public class DbTools
    {
        private readonly DbClient _dbClient;

        public DbTools(DbClient dbClient)
        {
            _dbClient = dbClient;
        }

        [McpServerTool, Description("Executa uma consulta SQL customizada no banco. Use apenas comandos SELECT. Parâmetros: table (nome da tabela), columns (colunas separadas por vírgula, ou * para todas), where (condição opcional), limit (número máximo de registros, padrão 100)")]
        public async Task<ToolDto<string>> ExecuteCustomQuery(
            [Description("Nome da tabela para consultar")] string table,
            [Description("Colunas a serem retornadas (ex: 'ID,NOME,EMAIL' ou '*' para todas)")] string columns = "*",
            [Description("Condição WHERE opcional (ex: 'NOME LIKE %João%' ou 'ID = 123')")] string? where = null,
            [Description("Número máximo de registros a retornar (1-1000)")] int limit = 100)
        {
            try
            {
                var tableValid = _dbClient.IsValidInput(table);
                var columnsValid = _dbClient.IsValidInput(columns);
                
                if (!tableValid || !columnsValid)
                {
                    return new ToolDto<string>(true, "Erro: Entrada inválida detectada. Use apenas caracteres alfanuméricos, underscore e vírgulas.", null);
                }

                if (!string.IsNullOrEmpty(where))
                {
                    var whereValid = _dbClient.IsValidWhereClause(where);
                    
                    if (!whereValid)
                    {
                        return new ToolDto<string>(true, "Erro: Cláusula WHERE contém caracteres ou comandos não permitidos.", null);
                    }
                }

                if (limit < 1 || limit > 1000)
                {
                    limit = 100;
                }

                var query = $"SELECT TOP {limit} {columns} FROM {table}";
                if (!string.IsNullOrEmpty(where))
                {
                    query += $" WHERE {where}";
                }

                var result = await _dbClient.ExecuteSelectQueryAsync(query);
                
                return new ToolDto<string>(false, "Resultado da consulta.", result);
            }
            catch (Exception ex)
            {
                return new ToolDto<string>(true, $"Erro ao executar consulta: {ex.Message}", null);
            }
        }

        [McpServerTool, Description("Lista todas as tabelas disponíveis no banco de dados ERP")]
        public async Task<ToolDto<string>> GetAvailableTables()
        {
            try
            {
                var query = @"
                    SELECT 
                        TABLE_NAME as Tabela,
                        (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = t.TABLE_NAME) as TotalColunas
                    FROM INFORMATION_SCHEMA.TABLES t
                    WHERE TABLE_TYPE = 'BASE TABLE'
                    ORDER BY TABLE_NAME";

                var result = await _dbClient.ExecuteSelectQueryAsync(query);
                return new ToolDto<string>(false, "Resultado da consulta.", result);
            }
            catch (Exception ex)
            {
                return new ToolDto<string>(true, $"Erro ao listar tabelas: {ex.Message}", null);
            }
        }

        [McpServerTool, Description("Mostra a estrutura de uma tabela específica (colunas, tipos, etc.)")]
        public async Task<ToolDto<string>> GetTableStructure([Description("Nome da tabela")] string tableName)
        {
            try
            {
                if (!_dbClient.IsValidInput(tableName))
                {
                    return new ToolDto<string>(true, "Erro: Nome de tabela inválido.", null);
                }

                var query = @"
                    SELECT 
                        COLUMN_NAME as Coluna,
                        DATA_TYPE as Tipo,
                        IS_NULLABLE as PermiteNull,
                        COLUMN_DEFAULT as ValorPadrao,
                        CHARACTER_MAXIMUM_LENGTH as TamanhoMaximo
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = @tableName
                    ORDER BY ORDINAL_POSITION";

                var result = await _dbClient.ExecuteSelectQueryAsync(query);

                return new ToolDto<string>(false, "Resultado da consulta.", result);
            }
            catch (Exception ex)
            {
                return new ToolDto<string>(true, $"Erro ao obter estrutura da tabela: {ex.Message}", null);
            }
        }

        [McpServerTool, Description("Busca inteligente por texto em múltiplas tabelas e colunas do ERP")]
        public async Task<ToolDto<string>> SmartSearch(
            [Description("Texto a ser procurado")] string searchText,
            [Description("Tabelas específicas para buscar (opcional, separadas por vírgula)")] string? tables = null,
            [Description("Limite de resultados por tabela")] int limitPerTable = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    return new ToolDto<string>(true, "Erro: Texto de busca não pode estar vazio.", null);
                }

                searchText = searchText.Replace("'", "''").Trim();
                limitPerTable = Math.Max(1, Math.Min(limitPerTable, 50));

                var resultados = new List<string>();

                var tablesToSearch = new List<string>();

                if (!string.IsNullOrEmpty(tables))
                {
                    tablesToSearch = tables.Split(',').Select(t => t.Trim()).Where(_dbClient.IsValidInput).ToList();
                }
                else
                {
                    tablesToSearch = new List<string> { "T_CLIENTES", "T_PRODUTOS", "T_VENDAS", "T_FUNCIONARIOS" };
                }

                foreach (var table in tablesToSearch)
                {
                    try
                    {
                        var tableResults = _dbClient.SearchInTable(table, searchText, limitPerTable);
                        if (!string.IsNullOrEmpty(tableResults.ToString()))
                        {
                            resultados.Add($"\n=== Resultados em {table} ===\n{tableResults}");
                        }
                    }
                    catch (Exception ex)
                    {
                        resultados.Add($"\n=== Erro em {table} ===\n{ex.Message}");
                    }
                }

                return new ToolDto<string>(false, "Resultado da busca", resultados.ToString());
            }
            catch (Exception ex)
            {
                return new ToolDto<string>(true, "$\"Erro na busca inteligente: {ex.Message}\"", null);
            }
        }
    }
}
