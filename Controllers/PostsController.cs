using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using AssociationPortal.Data;
using AssociationPortal.Models;

namespace AssociationPortal.Controllers
{
    [Route("post")]
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostController(ApplicationDbContext context)
        {
            _context = context;
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
            if (string.IsNullOrWhiteSpace(model.PostTitle) || string.IsNullOrWhiteSpace(model.PostContent))
                return BadRequest(new { message = "Vui lòng nhập đầy đủ tiêu đề và nội dung!" });

            var parameters = new[]
            {
                new SqlParameter("@MemberId", model.MemberId),
                new SqlParameter("@CategoryId", model.CategoryId ?? (object)DBNull.Value),
                new SqlParameter("@Title", model.PostTitle),
                new SqlParameter("@Content", model.PostContent),
                new SqlParameter("@ThumbnailUrl", model.PostThumbnailUrl ?? (object)DBNull.Value)
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_add_post @MemberId, @CategoryId, @Title, @Content, @ThumbnailUrl",
                parameters);

            return Ok(new { message = "Bài viết của bạn đã được gửi và đang chờ duyệt!" });
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
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
                return NotFound();

            if (approve)
            {
                post.ApprovedStatus = 1; // Approved
                post.ApprovedDate = DateTime.Now;
            }
            else
            {
                post.ApprovedStatus = 2; // Rejected
                post.RejectedReason = reason ?? "Không có lý do";
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = approve ? "Bài viết đã được duyệt!" : "Bài viết đã bị từ chối!" });
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
