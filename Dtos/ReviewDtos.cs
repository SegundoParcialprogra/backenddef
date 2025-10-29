namespace backenddef.Dtos
{
    public class ReviewCreateDto
    {
        public int ProductId { get; set; }
        public int Rating { get; set; } // 1 a 5
        public string? Comment { get; set; }
    }

    public class ReviewReadDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int ClienteId { get; set; }
        public string? ClienteName { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}