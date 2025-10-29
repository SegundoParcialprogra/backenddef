namespace backenddef.Dtos
{
    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class OrderCreateDto
    {
        public int EmpresaId { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderReadDto
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string? ClienteName { get; set; }
        public int EmpresaId { get; set; }
        public string? EmpresaName { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = null!;
        public List<OrderItemReadDto> Items { get; set; } = new();
    }

    public class OrderItemReadDto
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    public class OrderStatusUpdateDto
    {
        public string Status { get; set; } = null!;
    }
}