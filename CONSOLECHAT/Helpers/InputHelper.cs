using Sharprompt;

namespace CONSOLECHAT.Helpers
{
    internal class InputHelper
    {
        public const string OPENAI = "OpenAi";
        public const string OLLAMA = "Ollama";

        public static string GetAISolution()
        {
            var answer = Prompt.Select<string>(options =>
            {
                options.Message = "Selecione a quantidade de registros a ser gerada";
                options.Items = [OPENAI, OLLAMA];
            });
            Console.WriteLine();
            return answer;
        }
    }
}
