namespace UserApi.DTOs.Responses
{
    public class UserResponseDto
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public RoleDto Role { get; set; }
        public string Username { get; set; }
        public List<PermissionResponseDto> Permissions { get; set; } // Change to PermissionResponseDto
        public string CreatedDate { get; set; }
    }

    public class RoleDto
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
    }

}