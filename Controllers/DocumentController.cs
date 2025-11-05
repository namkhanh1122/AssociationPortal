using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using AssociationPortal.Data;
using AssociationPortal.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Data;
using Microsoft.Data.SqlClient;


namespace AssociationPortal.Controllers
{
    [Route("document")]
    public class DocumentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly PermissionService _permissionService;

        public DocumentController(ApplicationDbContext context, IWebHostEnvironment env, PermissionService permissionService)
        {
            _context = context;
            _env = env;
            _permissionService = permissionService;
        }

        private string UploadPath => Path.Combine(_env.WebRootPath, "uploads");

        private long GetCurrentMemberId()
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return 0;

            var token = authHeader.Substring("Bearer ".Length).Trim();
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var memberIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == "memberId");
            return memberIdClaim != null ? long.Parse(memberIdClaim.Value) : 0;
        }

        #region Views
        [HttpGet("createdocument")] 
        public IActionResult CreateDocument() => View();

        [HttpGet("list")] 
        public IActionResult ListDocuments() => View();

        [HttpGet("details/{id}")]
        public IActionResult DocumentDetails(long id)
        {
            ViewData["DocumentId"] = id;
            return View();
        }
        [HttpGet("pendingdocument")]
        public IActionResult PendingDocument()
        {
            return View(); // Razor View sẽ nằm ở Views/Document/PendingDocuments.cshtml
        }
        [HttpGet("listpublic")]
        public IActionResult ListPublicDocument()
        {
            return View(); // Views/Document/ListPublicDocument.cshtml
        }

        [HttpGet("detail/{id}")]
        public IActionResult PublicDocumentDetail(long id)
        {
            ViewData["DocumentId"] = id;
            return View(); // Views/Document/DocumentDetail.cshtml
        }



        #endregion

        #region API

        // Lấy danh sách Category
       // DTO cho frontend
        public class CategoryOptionDto
        {
            public long? CategoryId { get; set; }   
            public string CategoryName { get; set; } = string.Empty;
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            // Lấy category đang active
            var categories = await _context.Categories
                .Where(c => c.CategoryStatus == 1)
                .Select(c => new CategoryOptionDto
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName
                })
                .ToListAsync();

            // Thêm tùy chọn mặc định "Chưa có danh mục"
            categories.Insert(0, new CategoryOptionDto
            {
                CategoryId = null,
                CategoryName = "Chưa có danh mục"
            });

            return Ok(categories); // trả về JSON
        }


        // Upload document (có categoryId)
        [HttpPost("upload")]
        public async Task<IActionResult> UploadDocument([FromForm] IFormFile file, [FromForm] string title, [FromForm] long categoryId)
        {
            var memberId = GetCurrentMemberId();
            if (memberId == 0)
                return Unauthorized(new { message = "Thiếu token hoặc token không hợp lệ!" });

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Vui lòng chọn file!" });

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(UploadPath, uniqueFileName);
            Directory.CreateDirectory(UploadPath);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var memberParam = new SqlParameter("@MemberId", SqlDbType.BigInt) { Value = memberId };
            var titleParam = new SqlParameter("@DocumentTitle", SqlDbType.NVarChar, 200) { Value = title };
            var fileParam = new SqlParameter("@FilePath", SqlDbType.NVarChar, 500) { Value = $"/uploads/{uniqueFileName}" };
            var categoryParam = new SqlParameter("@CategoryId", SqlDbType.BigInt) { Value = categoryId };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_add_document @MemberId, @CategoryId, @DocumentTitle, @FilePath",
                memberParam, categoryParam, titleParam, fileParam
            );


            return Json(new { message = "Tải lên thành công!" });
        }


        // Pending documents API (Admin)
       /* [HttpGet("pending")]
        public async Task<IActionResult> PendingDocuments()
        {
            var memberId = GetCurrentMemberId();
            if (memberId == 0) return Unauthorized();

            var hasPermission = await _permissionService.HasPermissionAsync(memberId, 201);
            var hasPermissionSuperAdmin = await _permissionService.HasPermissionAsync(memberId, 301);
            if (!hasPermission && !hasPermissionSuperAdmin) return Forbid();

           var docs = await _context.Database.SqlQueryRaw<PendingDocumentDto>(
            "EXEC sp_get_pending_documents"
            ).ToListAsync();

            return Json(docs);
        }
        */
        [HttpGet("pending")]
        public async Task<IActionResult> PendingDocuments()
        {
            var memberId = GetCurrentMemberId();
            if (memberId == 0) return Unauthorized();

            var hasPermission = await _permissionService.HasPermissionAsync(memberId, 201);
            var hasPermissionSuperAdmin = await _permissionService.HasPermissionAsync(memberId, 301);
            if (!hasPermission && !hasPermissionSuperAdmin) return Forbid();

            var docs = await _context.Documents
                .Include(d => d.Member)
                .Include(d => d.Category)
                .Where(d => d.DocumentStatus == 0)
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => new {
                    d.DocumentId,
                    d.DocumentTitle,
                    AuthorName = d.Member.FullName,
                    CategoryName = d.Category.CategoryName ?? "Chưa có danh mục",
                    d.CreatedAt
                })
                .ToListAsync();

            return Ok(docs);
        }

        public class PendingDocumentDto
        {
            public long DocumentId { get; set; }
            public string DocumentTitle { get; set; }
            public DateTime CreatedAt { get; set; }
            public int DocumentStatus { get; set; }
            public string? RejectedReason { get; set; }
            public string CategoryName { get; set; }
            public string AuthorName { get; set; }
        }

        public class DocumentDto
        {
            public long DocumentId { get; set; }
            public string DocumentTitle { get; set; }
            public string FilePath { get; set; }
            public DateTime? CreatedAt { get; set; }
            public int DocumentStatus { get; set; }
            public DateTime? ApprovedDate { get; set; }
            public string? RejectedReason { get; set; }
            public long? CategoryId { get; set; }
            public string? CategoryName { get; set; }
            public string? AuthorName { get; set; }
        }

        // Chi tiết document
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDocument(long id)
        {
            var param = new SqlParameter("@DocumentId", id);

            // Dùng SqlQueryRaw để map trực tiếp vào DTO
            var docList = await _context.Database
                .SqlQueryRaw<DocumentDto>("EXEC sp_get_document_by_id @DocumentId", param)
                .ToListAsync();

            var doc = docList.FirstOrDefault();

            if (doc == null) return NotFound();

            return Json(doc);
        }


        // Duyệt document (Admin / SuperAdmin)
        [HttpPost("approve/{id}")]
        public async Task<IActionResult> ApproveDocument(long id, [FromBody] ApproveRequest req)
        {
            var memberId = GetCurrentMemberId();
            if (memberId == 0) return Unauthorized(new { message = "Thiếu token hoặc token không hợp lệ!" });

            var hasPermission = await _permissionService.HasPermissionAsync(memberId, 201); 
            var hasPermissionSuperAdmin = await _permissionService.HasPermissionAsync(memberId, 301);
            if (!hasPermission && !hasPermissionSuperAdmin) return StatusCode(403, new { message = "Bạn không có quyền duyệt tài liệu." });

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_approve_document @DocumentId, @Status, @Reason, @ApprovedBy",
                new SqlParameter("@DocumentId", id),
                new SqlParameter("@Status", req.Approve ? 1 : 2),
                new SqlParameter("@Reason", req.Reason ?? (object)DBNull.Value),
                new SqlParameter("@ApprovedBy", memberId)
            );

            return Json(new { message = req.Approve ? "Đã duyệt!" : "Đã từ chối!" });
        }

        public class ApproveRequest
        {
            public bool Approve { get; set; }
            public string? Reason { get; set; }
        }


        // [GET] /document/public
        [HttpGet("public")]
        public async Task<IActionResult> GetPublicDocuments()
        {
            // Lấy danh sách tài liệu đã được duyệt (DocumentStatus = 1)
            var documents = await _context.Documents
                .Include(d => d.Member)
                .Include(d => d.Category)
                .Where(d => d.DocumentStatus == 1)
                .OrderByDescending(d => d.ApprovedDate)
                .Select(d => new DocumentDto
                {
                    DocumentId = d.DocumentId,
                    DocumentTitle = d.DocumentTitle,
                    FilePath = d.FilePath,
                    CreatedAt = d.CreatedAt,
                    ApprovedDate = d.ApprovedDate,
                    CategoryName = d.Category.CategoryName ?? "Chưa có danh mục",
                    AuthorName = d.Member.FullName
                })
                .ToListAsync();

            return Ok(documents);
        }

        [HttpGet("public/{id}")]
        public async Task<IActionResult> GetPublicDocumentDetail(long id)
        {
            var doc = await _context.Documents
                .Include(d => d.Member)
                .Include(d => d.Category)
                .Where(d => d.DocumentId == id && d.DocumentStatus == 1)
                .Select(d => new DocumentDto
                {
                    DocumentId = d.DocumentId,
                    DocumentTitle = d.DocumentTitle,
                    FilePath = d.FilePath,
                    CreatedAt = d.CreatedAt,
                    ApprovedDate = d.ApprovedDate,
                    CategoryName = d.Category.CategoryName ?? "Chưa có danh mục",
                    AuthorName = d.Member.FullName
                })
                .FirstOrDefaultAsync();

            if (doc == null)
                return NotFound(new { message = "Không tìm thấy tài liệu hoặc chưa được duyệt." });

            return Ok(doc);
        }





        #endregion
    }
}
