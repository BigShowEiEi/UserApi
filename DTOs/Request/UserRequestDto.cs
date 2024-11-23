namespace UserApi.DTOs.Request
{
    public class UserRequestDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string RoleId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public List<PermissionRequestDto> Permissions { get; set; } // Use PermissionRequestDto here
    }
}
