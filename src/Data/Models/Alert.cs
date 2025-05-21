using System;

namespace Proyecto_Final.Data.Models
{
    public enum AlertType
    {
        LowStock,
        OutOfStock,
        ExcessStock,
        PriceChange,
        ExpirationDate
    }
    
    public enum AlertStatus
    {
        New,
        Acknowledged,
        Resolved,
        Ignored
    }
    
    public class Alert
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public AlertType Type { get; set; }
        public string Message { get; set; }
        public AlertStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        
        // Propiedad de navegación
        public virtual Product Product { get; set; }
        
        public Alert()
        {
            CreatedAt = DateTime.Now;
            Status = AlertStatus.New;
        }
    }
}