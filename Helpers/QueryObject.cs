namespace server.Helpers
{
    public class QueryObject
    {
        public string SortBy { get; set; } = string.Empty;
        public string searchTerm { get; set; } = string.Empty;
        public bool IsDescending { get; set; } = false;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
