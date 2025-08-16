using Microsoft.SemanticKernel.ChatCompletion;

namespace CHAT.Models
{
    public class ChatMessage
    {
        public AuthorRole Role { get; set; }
        public string Text { get; set; }
        public List<Microsoft.SemanticKernel.KernelContent> Contents { get; set; }

        public ChatMessage(AuthorRole role, string text)
        {
            Role = role;
            Text = text;
            Contents = new List<Microsoft.SemanticKernel.KernelContent>
            {
                new Microsoft.SemanticKernel.TextContent(text)
            };
        }

        public ChatMessage(AuthorRole role, Microsoft.SemanticKernel.KernelContent content)
        {
            Role = role;
            Text = content.ToString() ?? "";
            Contents = new List<Microsoft.SemanticKernel.KernelContent> { content };
        }
    }

    public static class ChatRole
    {
        public static AuthorRole System => AuthorRole.System;
        public static AuthorRole User => AuthorRole.User;
        public static AuthorRole Assistant => AuthorRole.Assistant;
    }
}