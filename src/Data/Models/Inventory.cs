using System;

namespace Proyecto_Final.Data.Models
{
    public class Inventory
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int MinimumStock { get; set; }
        public int MaximumStock { get; set; }
        public string Location { get; set; }
        public DateTime LastUpdated { get; set; }
        
        // Propiedad de navegación (para Entity Framework o similar)
        public virtual Product Product { get; set; }
        
        public Inventory()
        {
            LastUpdated = DateTime.Now;
        }
    }
}