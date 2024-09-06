using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using webapi.Database;
using webapi.Models;

namespace webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<Register> _passwordHasher;

        public WorkingController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _passwordHasher = new PasswordHasher<Register>();
        }

        // POST: api/working/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] Register register)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if user already exists
            var existingUser = await _context.Registers
                .FirstOrDefaultAsync(r => r.Email == register.Email);

            if (existingUser != null)
                return Conflict("User with this email already exists.");

            // Hash the password before saving
            register.Password = _passwordHasher.HashPassword(register, register.Password);

            // Add the new user to the database
            _context.Registers.Add(register);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully.");
        }

        // POST: api/working/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] Login loginModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if user exists
            var user = await _context.Registers
                .FirstOrDefaultAsync(r => r.Email == loginModel.Email);

            if (user == null || _passwordHasher.VerifyHashedPassword(user, user.Password, loginModel.Password) != PasswordVerificationResult.Success)
                return Unauthorized("Invalid email or password.");

            // Generate JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { Token = tokenString });
        }


        //////////////// For Inserting Products in Table/////////////////////
        [HttpPost("product")]
        [Authorize]
        public IActionResult Product([FromForm] Product productDto)
        {
            // Get the currently logged-in user's ID (from claims)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized("User is not authenticated.");
            }

            // Create a new Product entity
            var product = new Product
            {
                Name = productDto.Name,
                Price = productDto.Price,
                Description = productDto.Description,
                ImageUrl = productDto.ImageUrl,
                RegisteredId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value)

            };

            // Add the product to the database
            _context.Products.Add(product);
            _context.SaveChanges();

            return Ok(new { message = "Product created successfully", product });
        }


        // PUT: api/working/product/{id}
        [HttpPut("product/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] Product productDto)
        {
            // Get the currently logged-in user's ID (from claims)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized("User is not authenticated.");
            }

            // Find the existing product
            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
            {
                return NotFound("Product not found.");
            }

            // Update the product details
            existingProduct.Name = productDto.Name;
            existingProduct.Price = productDto.Price;
            existingProduct.Description = productDto.Description;
            existingProduct.ImageUrl = productDto.ImageUrl;

            // Save changes to the database
            _context.Products.Update(existingProduct);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product updated successfully", product = existingProduct });
        }


        // DELETE: api/working/product/{id}
        [HttpDelete("product/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            // Get the currently logged-in user's ID (from claims)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized("User is not authenticated.");
            }

            // Find the product to delete
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound("Product not found.");
            }

            // Remove the product from the database
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product deleted successfully" });
        }




        // GET: api/working/get-token
        [HttpGet("get-token")]
        [Authorize]
        public IActionResult GetToken()
        {
            return Ok(new { Message = "Token is valid" });
        }

       
    }

};

