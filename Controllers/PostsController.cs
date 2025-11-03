using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using AssociationPortal.Data;
using AssociationPortal.Models;
using System.IdentityModel.Tokens.Jwt;


namespace AssociationPortal.Controllers
{
    [Route("post")]
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly PermissionService _permissionService;

        public PostController(ApplicationDbContext context,PermissionService permissionService)
        {
            _context = context;
            _permissionService = permissionService;

        }

        //  Trang đăng bài (hiển thị form)
        [HttpGet("createpost")]
        public IActionResult createPost()
        {
            return View();
        }

        //  Lấy danh sách danh mục (hiển thị trong dropdown)
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories
            .Where(c => c.CategoryStatus == 1)
            .Select(c => new CategoryOptionDto
        {
            CategoryId = c.CategoryId,
            CategoryName = c.CategoryName
        })
        .ToListAsync();

            categories.Insert(0, new CategoryOptionDto
        {
            CategoryId = null,
            CategoryName = "Chưa có danh mục"
        });

        return Ok(categories);

        }

        //  Xử lý đăng bài (qua AJAX)
     [HttpPost("createpost")]
public async Task<IActionResult> CreatePost([FromBody] Post model)
{
    try
    {
        // --- Lấy JWT từ Header Authorization ---
        var authHeader = Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            return Unauthorized(new { message = "Thiếu token xác thực!" });

        var token = authHeader.Substring("Bearer ".Length).Trim();

        // --- Giải mã token để lấy memberId ---
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var memberIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == "memberId");

        if (memberIdClaim == null)
            return Unauthorized(new { message = "Token không hợp lệ!" });

        var memberId = long.Parse(memberIdClaim.Value);

        // --- Kiểm tra dữ liệu ---
        if (string.IsNullOrWhiteSpace(model.PostTitle) || string.IsNullOrWhiteSpace(model.PostContent))
            return BadRequest(new { message = "Vui lòng nhập đầy đủ tiêu đề và nội dung!" });

        // --- Gọi stored procedure ---
        var parameters = new[]
        {
            new SqlParameter("@MemberId", memberId), // ✅ gán tự động từ token
            new SqlParameter("@CategoryId", model.CategoryId ?? (object)DBNull.Value),
            new SqlParameter("@Title", model.PostTitle),
            new SqlParameter("@Content", model.PostContent),
            new SqlParameter("@ThumbnailUrl", model.PostThumbnailUrl ?? (object)DBNull.Value)
        };

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_add_post @MemberId, @CategoryId, @Title, @Content, @ThumbnailUrl",
            parameters
        );

        return Ok(new { message = "Bài viết của bạn đã được gửi và đang chờ duyệt!" });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
    }
}

        // API: Lấy danh sách bài viết chờ duyệt
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingPosts()
        {
            var posts = await _context.Posts
                .Include(p => p.Member)
                .Include(p => p.Category)
                .Where(p => p.ApprovedStatus == 0) // Pending
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new
                {
                    p.PostId,
                    p.PostTitle,
                    AuthorName = p.Member.FullName,
                    CategoryName = p.Category.CategoryName ?? "Chưa có danh mục",
                    p.CreatedAt
                })
                .ToListAsync();

            return Ok(posts);
        }

        //  API: Duyệt hoặc từ chối bài viết
        [HttpPost("approve")]
public async Task<IActionResult> ApprovePost(long postId, bool approve, string? reason = null)
{
    try
    {
        // --- Lấy JWT từ Header ---
        var authHeader = Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            return Unauthorized(new { message = "Thiếu token xác thực!" });

        var token = authHeader.Substring("Bearer ".Length).Trim();

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var memberIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == "memberId");

        if (memberIdClaim == null)
            return Unauthorized(new { message = "Token không hợp lệ!" });

        var memberId = long.Parse(memberIdClaim.Value);

        // --- Check quyền Duyệt bài viết ---
        var permissions = await _permissionService.GetPermissionsByMemberIdAsync(memberId);
        if (!permissions.Contains(200) && !permissions.Contains(301)) 
        {
            return StatusCode(403, new { message = "Bạn không có quyền duyệt bài viết!" });
        }

        // --- Tìm bài viết ---
        var post = await _context.Posts.FindAsync(postId);
        if (post == null)
            return NotFound();

        if (approve)
        {
            post.ApprovedStatus = 1;
            post.ApprovedDate = DateTime.Now;
        }
        else
        {
            post.ApprovedStatus = 2;
            post.RejectedReason = reason ?? "Không có lý do";
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = approve ? "Bài viết đã được duyệt!" : "Bài viết đã bị từ chối!" });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
    }
}

            // Lấy danh sách bài viết đã duyệt (hiển thị ở trang Tin Tức)
        [HttpGet("approved")]
        public async Task<IActionResult> GetApprovedPosts()
        {
            var posts = await _context.Posts
                .Include(p => p.Category)
            .Where(p => p.ApprovedStatus == 1)
            .OrderByDescending(p => p.ApprovedDate)
            .Select(p => new
            {
            p.PostId,
            p.PostTitle,
            p.PostContent,
            p.PostThumbnailUrl,
            CategoryName = p.Category.CategoryName ?? "Chưa có danh mục",
            p.ApprovedDate
            })
            .ToListAsync();

            return Ok(posts);
        }
        [HttpGet("pendingposts")]
        public IActionResult PendingPosts()
        {
            return View();
        }

        [HttpGet("news")]
        public IActionResult News()
        {
            return View();
        }



        [HttpGet("get-news")]
        public async Task<IActionResult> GetNews()
        {
            var posts = await _context.Database.SqlQueryRaw<NewsDto>(
            "EXEC sp_get_news_homepage"
            ).ToListAsync();

            return Json(posts);
        }


        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(long id)
        {
            var postList1 = await _context.Database.SqlQueryRaw<PostDetailDto>(
                "EXEC sp_get_post_detail @PostId",new SqlParameter("@PostId", id)
            ).ToListAsync();
            var post = postList1.FirstOrDefault();

            if (post == null)
            return NotFound();

            return View(post);
            }


        public class NewsDto
        {
            public long PostId { get; set; }
            public string PostTitle { get; set; }
            public string? ThumbnailUrl { get; set; }
            public string? CategoryName { get; set; }
            public DateTime? ApprovedDate { get; set; }
            public int ViewCount { get; set; }
            public int IsFeatured { get; set; }
        }

        public class PostDetailDto
        {
            public long PostId { get; set; }
            public string PostTitle { get; set; }
            public string PostContent { get; set; }
            public string? ThumbnailUrl { get; set; }
            public string? CategoryName { get; set; }
            public string? AuthorName { get; set; }
            public DateTime? ApprovedDate { get; set; }
            public int ViewCount { get; set; }
        }
        [HttpGet("detail/{id}")]
        public async Task<IActionResult> Detail(long id)
        {
            var postList = await _context.Database.SqlQueryRaw<PostDetailDto>(
                "EXEC sp_get_post_detail @PostId", 
                new SqlParameter("@PostId", id)
            ).ToListAsync();

            var post = postList.FirstOrDefault();

                if (post == null)
                return NotFound();

            return View("Detail", post);
        }

        [HttpPost("increaseview")]
        public async Task<IActionResult> IncreaseView([FromBody] PostIdRequest request)
        {
            if (request == null || request.PostId <= 0)
            return BadRequest(new { message = "Dữ liệu không hợp lệ" });

            var param = new SqlParameter("@PostId", request.PostId);
            await _context.Database.ExecuteSqlRawAsync("EXEC sp_increase_post_view @PostId", param);
            return Ok(new { message = "Lượt xem đã được cập nhật" });
        }

        public class PostIdRequest
        {
            public long PostId { get; set; }
        }



    }
}
