using API.Context;
using API.DTOs;
using API.Models;
using API.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllUsers(CancellationToken ct)
        {
            var users = await _context.Users.ToListAsync(ct);

            var userDto = users.Select(u => new UserDto(u.Id, u.Name, u.Email)).ToList();

            return Ok(userDto);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateUser([FromBody] UserRequest user, CancellationToken ct)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Name) || string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Password))
            {
                return BadRequest("Invalid user data.");
            }

            var newUser = new User(user.Name, user.Email, user.Password);

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync(ct);

            var userDto = new UserDto(newUser.Id, newUser.Name, newUser.Email);
            return CreatedAtAction(nameof(GetAllUsers), new { id = newUser.Id }, userDto);
        }
    }
}
