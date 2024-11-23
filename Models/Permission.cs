namespace UserApi.Models
{
    public class Permission
    {
        public string PermissionId { get; set; }
        public string PermissionName { get; set; }
        public string UserId { get; set; }
        public bool IsReadable { get; set; }
        public bool IsWritable { get; set; }
        public bool IsDeletable { get; set; }
        public User User { get; set; }
    }
}