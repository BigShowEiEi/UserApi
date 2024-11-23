namespace UserApi.DTOs.Request
{
    public class PermissionRequestDto
    {
        public string PermissionId { get; set; }
        public bool IsReadable { get; set; }
        public bool IsWritable { get; set; }
        public bool IsDeletable { get; set; }
    }
}
