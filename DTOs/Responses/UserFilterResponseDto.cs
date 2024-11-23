namespace UserApi.DTOs.Responses
{
    public class UserFilterResponseDto
    {
        public List<UserResponseDto> DataSource { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
    }

}
