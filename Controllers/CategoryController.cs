using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using AssociationPortal.Data;

namespace AssociationPortal.Controllers
{
    [Route("category")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _context.Database.SqlQueryRaw<CategoryDto>(
                "EXEC sp_get_all_categories"
            ).ToListAsync();

            return Json(categories);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CategoryCreateRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.CategoryName))
                return BadRequest("Tên danh mục không được để trống!");

            var result = await _context.Database.SqlQueryRaw<string>(
                "EXEC sp_add_category @MemberId, @CategoryName, @Description",
                new SqlParameter("@MemberId", req.MemberId ?? (object)DBNull.Value),
                new SqlParameter("@CategoryName", req.CategoryName),
                new SqlParameter("@Description", req.Description ?? (object)DBNull.Value)
            ).ToListAsync();

            return Ok(result.FirstOrDefault() ?? "Lỗi khi thêm danh mục!");
        }

        public class CategoryCreateRequest
        {
            public long? MemberId { get; set; }
            public string CategoryName { get; set; }
            public string? Description { get; set; }
        }

        public class CategoryDto
        {
            public long CategoryId { get; set; }
            public string CategoryName { get; set; }
            public string? Description { get; set; }
            public int Status { get; set; }
        }

        [HttpGet("create")]
        public IActionResult CreatePage()
        {
            return View("Create");
        }

    }
}
