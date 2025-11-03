using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using AssociationPortal.Data;
using AssociationPortal.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class PermissionService
{
    private readonly ApplicationDbContext _context;

    public PermissionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<int>> GetPermissionsByMemberIdAsync(long memberId)
    {
        var param = new SqlParameter("@MemberId", memberId);

        var result = await _context.PermissionResults
            .FromSqlRaw("EXEC sp_get_member_permissions @MemberId", param)
            .ToListAsync();

        // Chỉ lấy ra ActionCode
        var permissions = result
            .Select(p => p.ActionCode)
            .ToList();

        return permissions;
    }
}
