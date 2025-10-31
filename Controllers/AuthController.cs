using Microsoft.AspNetCore.Mvc;
using AssociationPortal.Data;
using AssociationPortal.Models;
using AssociationPortal.Helpers;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace AssociationPortal.Controllers
{
    [Route("auth")]
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Member model)
        {
            if (_context.Members.Any(x => x.Email == model.Email))
                return BadRequest(new { message = "Email already exists" });

            var newMember = new Member
            {
                FullName = model.FullName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                PasswordHash = PasswordHelper.HashPassword(model.PasswordHash),
                CreatedAt = DateTime.Now
            };

            _context.Members.Add(newMember);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registration successful" });
        }
        [HttpGet("register")]
        public IActionResult Register()
        {
        return View();
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Member member)
            {
                var existingMember = await _context.Members
                .FirstOrDefaultAsync(m => m.Email == member.Email);

                if (existingMember == null)
                {
                return BadRequest(new { message = "Email không tồn tại." });
                }

                if (PasswordHelper.VerifyPassword(member.PasswordHash, existingMember.PasswordHash)) 
                {
                    return BadRequest(new { message = "Sai mật khẩu." });
                }

                return Ok(new { message = "Đăng nhập thành công!" });
                }
        [HttpGet("login")]
            public IActionResult Login()
            {
                return View();
            }

    }
}
