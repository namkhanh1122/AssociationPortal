using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AssociationPortal.Data;
using AssociationPortal.Helpers;
using AssociationPortal.Models;
using AssociationPortal.Models.Auth;
using System.IdentityModel.Tokens.Jwt;


namespace AssociationPortal.Controllers
{
    [Route("auth")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ApplicationDbContext context, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        // === REGISTER ===
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest(new { message = "Thiếu thông tin đăng ký." });

            if (await _context.Members.AnyAsync(x => x.Email == req.Email))
                return BadRequest(new { message = "Email đã tồn tại." });

            var member = new Member
            {
                FullName = req.FullName,
                Email = req.Email.ToLower().Trim(),
                PhoneNumber = req.PhoneNumber,
                PasswordHash = PasswordHelper.HashPassword(req.Password),
                CreatedAt = DateTime.Now
            };

            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng ký thành công!" });
        }

        // === LOGIN ===
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            _logger.LogInformation("=== LOGIN REQUEST === Email={Email}", req.Email);

            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest(new { message = "Thiếu email hoặc mật khẩu." });

            var member = await _context.Members.FirstOrDefaultAsync(m => m.Email == req.Email.ToLower());

            if (member == null)
                return BadRequest(new { message = "Email không tồn tại." });

            if (!PasswordHelper.VerifyPassword(req.Password, member.PasswordHash))
                return BadRequest(new { message = "Sai mật khẩu." });

            // Lấy quyền
            var permissions = await (from mp in _context.MemberPermisions
                                     join pd in _context.PermisionDetails on mp.PermisionId equals pd.PermisionId
                                     where mp.MemberId == member.MemberId && mp.Licensed == 1
                                     select pd.ActionCode.ToString()).ToListAsync();

            // JWT claims
            var claims = new List<Claim>
            {
                new Claim("memberId", member.MemberId.ToString()),
                new Claim(ClaimTypes.Email, member.Email),
                new Claim("permissions", string.Join(",", permissions))
            };

            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(int.Parse(jwtSettings["ExpiryMinutes"])),
                signingCredentials: creds
            );

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                message = "Đăng nhập thành công!",
                token = jwtToken,
                memberId = member.MemberId,
                fullName = member.FullName,
                permissions
            });
        }

        [HttpGet("register")]
        public IActionResult RegisterView() => View("Register");

        [HttpGet("login")]
        public IActionResult LoginView() => View("Login");
    }
}
