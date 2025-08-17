namespace API.Requests
{
    public record UserRequest(
        string Name,
        string Email,
        string Password
    );
}
