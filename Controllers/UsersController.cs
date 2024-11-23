using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UserApi.Data;
using UserApi.DTOs.Request;
using UserApi.DTOs.Responses;
using UserApi.Models;

namespace UserApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

// Add new user
        [HttpPost]
        public async Task<IActionResult> CreateUser(UserRequestDto request)
        {
            // Generate a unique ID for the user
            var userId = Guid.NewGuid().ToString();

            // Map the UserRequestDto to a User model
            var user = new User
            {
                Id = userId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = request.Phone,
                RoleId = request.RoleId,
                Username = request.Username,
                Password = request.Password
            };

            // Add the user to the Users table
            _context.Users.Add(user);

            // Process permissions and generate PermissionName
            foreach (var permissionDto in request.Permissions)
            {
                var permissionId = Guid.NewGuid().ToString();
                var permissionName = GeneratePermissionName(permissionDto); // Ensure permissionDto is of type PermissionRequestDto

                var permission = new Permission
                {
                    PermissionId = permissionId,
                    UserId = userId,
                    IsReadable = permissionDto.IsReadable,
                    IsWritable = permissionDto.IsWritable,
                    IsDeletable = permissionDto.IsDeletable,
                    PermissionName = permissionName
                };

                _context.Permissions.Add(permission);
            }

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Return the response
            return Ok(new
            {
                Status = new { Code = "200", Description = "User created successfully" },
                Data = new
                {
                    userId = user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.Phone,
                    Role = new { user.RoleId, RoleName = "" }, // Fill RoleName based on DB lookup if needed
                    user.Username,
                    Permissions = request.Permissions.Select(p => new
                    {
                        p.PermissionId,
                        PermissionName = GeneratePermissionName(p)
                    }).ToList()
                }
            });
        }

        // Helper function to generate PermissionName
        private string GeneratePermissionName(PermissionRequestDto permission)
        {
            var nameParts = new List<string>();
            if (permission.IsReadable) nameParts.Add("Readable");
            if (permission.IsWritable) nameParts.Add("Writable");
            if (permission.IsDeletable) nameParts.Add("Deletable");
            return string.Join(", ", nameParts);
        }

// Delete user
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                // Find the user by Id
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new
                    {
                        status = new { code = "404", description = "User not found" },
                        data = new { result = false, message = "User not found with the given Id" }
                    });
                }

                // Remove associated permissions
                var permissions = await _context.Permissions.Where(p => p.UserId == id).ToListAsync();
                if (permissions.Any())
                {
                    _context.Permissions.RemoveRange(permissions);
                }

                // Remove the user
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    status = new { code = "200", description = "User deleted successfully" },
                    data = new { result = true, message = "User and associated permissions deleted successfully" }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = new { code = "500", description = "Internal Server Error" },
                    data = new { result = false, message = ex.Message }
                });
            }
        }

// Get user by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            try
            {
                // Fetch user with related role and permissions
                var user = await _context.Users
                    .Include(u => u.Role) // Include Role information
                    .Include(u => u.Permissions) // Include Permissions
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound(new
                    {
                        status = new { code = "404", description = "User not found" },
                        data = new { }
                    });
                }

                // Map response data
                var response = new
                {
                    status = new { code = "200", description = "User found" },
                    data = new
                    {
                        userId = user.Id,
                        firstName = user.FirstName,
                        lastName = user.LastName,
                        email = user.Email,
                        phone = user.Phone,
                        role = new
                        {
                            roleId = user.Role.RoleId,
                            roleName = user.Role.RoleName
                        },
                        username = user.Username,
                        permissions = user.Permissions.Select(p => new
                        {
                            permissionId = p.PermissionId,
                            permissionName = p.PermissionName
                        }).ToList()
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                return StatusCode(500, new
                {
                    status = new { code = "500", description = "Internal Server Error" },
                    data = new { message = ex.Message }
                });
            }
        }

// Data Table
    [HttpPost("DataTable")]
        public async Task<IActionResult> GetUsers([FromBody] UserFilterRequestDto request)
        {
            try
            {
                // Default values for pagination
                int pageNumber = request.PageNumber ?? 1;
                int pageSize = request.PageSize ?? 10;

                // Query to fetch users with related Role and Permissions
                var query = _context.Users
                    .Include(u => u.Role)
                    .Include(u => u.Permissions)
                    .AsQueryable();

                // Apply search filter
                if (!string.IsNullOrEmpty(request.Search))
                {
                    query = query.Where(u => 
                        u.FirstName.Contains(request.Search) ||
                        u.LastName.Contains(request.Search) ||
                        u.Email.Contains(request.Search) ||
                        u.Username.Contains(request.Search));
                }

                // Apply ordering
                if (!string.IsNullOrEmpty(request.OrderBy))
                {
                    if (request.OrderDirection?.ToLower() == "desc")
                    {
                        query = request.OrderBy.ToLower() switch
                        {
                            "firstname" => query.OrderByDescending(u => u.FirstName),
                            "lastname" => query.OrderByDescending(u => u.LastName),
                            "email" => query.OrderByDescending(u => u.Email),
                            _ => query.OrderByDescending(u => u.FirstName) // Default ordering
                        };
                    }
                    else
                    {
                        query = request.OrderBy.ToLower() switch
                        {
                            "firstname" => query.OrderBy(u => u.FirstName),
                            "lastname" => query.OrderBy(u => u.LastName),
                            "email" => query.OrderBy(u => u.Email),
                            _ => query.OrderBy(u => u.FirstName) // Default ordering
                        };
                    }
                }

                // Calculate total count
                int totalCount = await query.CountAsync();

                // Apply pagination
                var users = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Map data to DTOs
                var dataSource = users.Select(user => new UserResponseDto
                {
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.Phone,
                    Role = new RoleDto
                    {
                        RoleId = user.Role.RoleId,
                        RoleName = user.Role.RoleName
                    },
                    Username = user.Username,
                    Permissions = user.Permissions.Select(p => new PermissionResponseDto
                    {
                        PermissionId = p.PermissionId,
                        PermissionName = p.PermissionName
                    }).ToList(),
                    CreatedDate = user.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss") // Use CreatedDate
                }).ToList();

                // Build the response
                var response = new UserFilterResponseDto
                {
                    DataSource = dataSource,
                    Page = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = new { code = "500", description = "Internal Server Error" },
                    data = new { message = ex.Message }
                });
            }
        }

// Edit user
        [HttpPut("{id}")]
        public async Task<IActionResult> EditUser(string id, [FromBody] UserEditRequestDto request)
        {
            try
            {
                // Find the user by Id
                var user = await _context.Users
                    .Include(u => u.Permissions)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound(new
                    {
                        status = new { code = "404", description = "User not found" },
                        data = new { }
                    });
                }

                // Update user fields
                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.Email = request.Email;
                user.Phone = request.Phone;
                user.RoleId = request.RoleId;
                user.Username = request.Username;
                user.Password = request.Password;

                // Update permissions
                foreach (var permissionDto in request.Permission)
                {
                    var existingPermission = user.Permissions
                        .FirstOrDefault(p => p.PermissionId == permissionDto.PermissionId);

                    if (existingPermission != null)
                    {
                        existingPermission.IsReadable = permissionDto.IsReadable;
                        existingPermission.IsWritable = permissionDto.IsWritable;
                        existingPermission.IsDeletable = permissionDto.IsDeletable;
                    }
                    else
                    {
                        // Add new permission
                        var newPermission = new Permission
                        {
                            PermissionId = Guid.NewGuid().ToString(),
                            UserId = user.Id,
                            IsReadable = permissionDto.IsReadable,
                            IsWritable = permissionDto.IsWritable,
                            IsDeletable = permissionDto.IsDeletable,
                            PermissionName = GeneratePermissionName(permissionDto)
                        };
                        _context.Permissions.Add(newPermission);
                    }
                }

                // Remove permissions not in the request
                var permissionIds = request.Permission.Select(p => p.PermissionId).ToList();
                var permissionsToRemove = user.Permissions
                    .Where(p => !permissionIds.Contains(p.PermissionId))
                    .ToList();

                _context.Permissions.RemoveRange(permissionsToRemove);

                // Save changes
                await _context.SaveChangesAsync();

                // Map response data
                var response = new UserEditResponseDto
                {
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.Phone,
                    Role = new RoleDto
                    {
                        RoleId = user.RoleId,
                        RoleName = _context.Roles
                            .Where(r => r.RoleId == user.RoleId)
                            .Select(r => r.RoleName)
                            .FirstOrDefault()
                    },
                    Username = user.Username,
                    Permissions = user.Permissions.Select(p => new PermissionResponseDto
                    {
                        PermissionId = p.PermissionId,
                        PermissionName = p.PermissionName
                    }).ToList()
                };

                return Ok(new
                {
                    status = new { code = "200", description = "User updated successfully" },
                    data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = new { code = "500", description = "Internal Server Error" },
                    data = new { message = ex.Message }
                });
            }
        }


    }




}
