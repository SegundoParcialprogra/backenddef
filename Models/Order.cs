using System.ComponentModel.DataAnnotations;

namespace backenddef.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClienteId { get; set; }
        public User? Cliente { get; set; }

        [Required]
        public int EmpresaId { get; set; }
        public User? Empresa { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Nuevo;

        public ICollection<OrderItem>? Items { get; set; }
    }
}