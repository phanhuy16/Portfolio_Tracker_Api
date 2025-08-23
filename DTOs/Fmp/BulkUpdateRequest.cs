namespace server.DTOs.Fmp
{
    public class BulkUpdateRequest
    {
        public List<string> Symbols { get; set; } = new List<string>();
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
