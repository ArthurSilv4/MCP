using System.ComponentModel.DataAnnotations;

namespace API.Requests
{
    public record UserRequest(
        string Name,
        string Email,
        string Password
    );
}
