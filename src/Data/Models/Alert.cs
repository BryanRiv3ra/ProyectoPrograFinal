using System;

namespace Proyecto_Final.Data.Models
{
    public enum AlertType
    {
        LowStock,
        StockOut,
        PriceChange,
        QualityIssue,
        ExpirationDate,
        SystemError,
        Other
    }
    
    public enum AlertSeverity
    {
        Info,
        Warning,
        Critical
    }
    
    public class Alert
    {
        public int Id { get; set; }
        public AlertType Type { get; set; }
        public AlertSeverity Severity { get; set; }
        public string Message { get; set; }
        public int? ProductId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsResolved { get; set; }
        public bool IsExpired { get; set; }
        public DateTime? ResolvedAt { get; set; }
        
        // Propiedad de navegación
        public virtual Product Product { get; set; }
        
        public Alert()
        {
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
            IsResolved = false;
            IsExpired = false;
        }
    }
}