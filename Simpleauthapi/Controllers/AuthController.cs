using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SimpleAuthApi.Data;
using SimpleAuthApi.Models;
using SimpleAuthApi.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace SimpleAuthApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;

        public AuthController(AppDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        

        // SignUp Endpoint
        [HttpPost("signup")]
        public IActionResult SignUp(User user)
        {
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                return BadRequest("Email already exists");
            }

            _context.Users.Add(user);
            _context.SaveChanges();
            return Ok("User registered successfully");
        }

        // SignIn Endpoint with JWT Token generation
        [HttpPost("signin")]
        public IActionResult SignIn([FromBody] LoginRequest request)
        {
            var user = _context.Users.FirstOrDefault(u =>
                u.Email == request.Email && u.Password == request.Password);

            if (user == null)
            {
                return Unauthorized("Invalid credentials");
            }

            // Generate JWT Token
            var token = _jwtService.GenerateToken(user);

            return Ok(new { message = "Login successful", token });
        }
        [Authorize(Roles = "admin")]
        [HttpGet("getAll")]
        public IActionResult GetAllUsers()
        {
            var users = _context.Users.ToList();
            return Ok(users);
        }

        // GetOneUser - Accessible by Admin and Standard
        [Authorize(Roles = "admin,standard")]
        [HttpGet("getOne/{id}")]
        public IActionResult GetOneUser(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound("User not found");
            }

            return Ok(user);
        }

        // UpdateUser - Admin can update any user, Standard can only update their own data
        [Authorize(Roles = "admin,standard")]
        [HttpPut("update/{id}")]
        public IActionResult UpdateUser(int id, [FromBody] User updatedUser)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.Id == id);
            if (existingUser == null)
            {
                return NotFound("User not found");
            }

            // Check if the logged-in user is allowed to update this data
            if (User.IsInRole("admin") || existingUser.Email == User.Identity.Name)
            {
                // Update city and hobbies only
                existingUser.City = updatedUser.City ?? existingUser.City;
                existingUser.Hobbies = updatedUser.Hobbies ?? existingUser.Hobbies;
                
                _context.SaveChanges();
                return Ok("User data updated successfully");
            }
            else
            {
                return Unauthorized("You are not allowed to update this data.");
            }
        }
    }

    // LoginRequest model
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
