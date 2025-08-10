using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;

namespace MCP.Clints
{
    public class DbClient
    {
        private static IConfiguration? _configuration;

        public DbClient(IConfiguration config)
        {
            _configuration = config;
        }

        private static string ConnectionString =>
           _configuration?.GetConnectionString("DefaultConnection")
           ?? _configuration?["DefaultConnection"]
           ?? throw new InvalidOperationException("Configuration not initialized or connection string not found.");

        public async Task<string> ExecuteSelectQueryAsync(string query)
        {
            try
            {
                using var connection = new SqlConnection(ConnectionString);

                await connection.OpenAsync();


                using var command = new SqlCommand(query, connection);

                using var reader = await command.ExecuteReaderAsync();
                var resultados = new List<string>();

                int count = 0;
                while (await reader.ReadAsync())
                {
                    count++;
                    Console.WriteLine($"Lendo registro {count}");

                    var campos = new List<string>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        campos.Add($"{reader.GetName(i)}: {reader[i]?.ToString() ?? "N/A"}");
                    }
                    resultados.Add($"[{string.Join(", ", campos)}]");

                    if (count >= 1000) break;
                }

                return resultados.Count > 0
                    ? $"Encontrados {resultados.Count} registros:\n{string.Join("\n", resultados)}"
                    : "Nenhum registro encontrado.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na query: {ex.Message}");
                throw;
            }
        }

        public bool IsValidInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            var pattern = @"^[a-zA-Z0-9_,\s\*]+$";
            return Regex.IsMatch(input, pattern);
        }

        public bool IsValidWhereClause(string whereClause)
        {
            if (string.IsNullOrWhiteSpace(whereClause))
                return true;

            var dangerousKeywords = new[]
            {
                "DROP", "DELETE", "INSERT", "UPDATE", "ALTER", "CREATE", "EXEC", "EXECUTE",
                "SP_", "XP_", "OPENROWSET", "OPENQUERY", "OPENDATASOURCE", "--", "/*", "*/"
            };

            var upperWhere = whereClause.ToUpper();
            return !dangerousKeywords.Any(keyword => upperWhere.Contains(keyword));
        }

        public async Task<string> SearchInTable(string tableName, string searchText, int limit)
        {
            var textColumns = GetTextColumns(tableName);

            if (!textColumns.Any())
            {
                return "";
            }

            var whereConditions = textColumns.Select(col => $"{col} LIKE '%{searchText}%'");
            var whereClause = string.Join(" OR ", whereConditions);

            var query = $"SELECT TOP {limit} * FROM {tableName} WHERE {whereClause}";

            return await ExecuteSelectQueryAsync(query);
        }

        private static List<string> GetTextColumns(string tableName)
        {
            var textColumns = new List<string>();

            try
            {
                var query = @"
                    SELECT COLUMN_NAME 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = @tableName 
                    AND DATA_TYPE IN ('varchar', 'nvarchar', 'char', 'nchar', 'text', 'ntext')";

                using var connection = new SqlConnection(ConnectionString);
                connection.Open();

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@tableName", tableName);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    textColumns.Add(reader["COLUMN_NAME"].ToString());
                }
            }
            catch
            {
                throw;
            }

            return textColumns;
        }
    }
}
