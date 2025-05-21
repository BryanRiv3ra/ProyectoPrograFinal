using System;

namespace Proyecto_Final.Data.Models
{
    public enum TransactionType
    {
        Purchase,
        Sale,
        Adjustment,
        Return,
        Transfer
    }
    
    public class Transaction
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public TransactionType Type { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Reference { get; set; }
        public string Notes { get; set; }
        public DateTime TransactionDate { get; set; }
        
        // Propiedad de navegación
        public virtual Product Product { get; set; }
        
        public Transaction()
        {
            TransactionDate = DateTime.Now;
        }
    }
}