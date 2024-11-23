namespace UserApi.DTOs.Request
{
    public class UserFilterRequestDto
    {
        public string OrderBy { get; set; }
        public string OrderDirection { get; set; } // "asc" or "desc"
        public int? PageNumber { get; set; } // Current page
        public int? PageSize { get; set; } // Number of items per page
        public string Search { get; set; } // Search keyword
    }
}
