using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserApi.Data;
using UserApi.Models;

namespace UserApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RolesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _context.Roles
                .Select(role => new
                {
                    roleId = role.RoleId,
                    roleName = role.RoleName
                })
                .ToListAsync();

            return Ok(new
            {
                status = new { code = "200", description = "Success" },
                data = roles
            });
        }

    }
}
