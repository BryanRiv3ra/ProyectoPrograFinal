using System;
using System.Collections.Generic;
using System.Data.SQLite;
using Proyecto_Final.Data.Models;

namespace Proyecto_Final.Data.Database.Repositories
{
    public class InventoryRepository : IRepository<Inventory>
    {
        private readonly DatabaseContext _context;
        private readonly ProductRepository _productRepository;

        public InventoryRepository(DatabaseContext context)
        {
            _context = context;
            _productRepository = new ProductRepository(context);
        }

        public Inventory GetById(int id)
        {
            _context.Open();
            Inventory inventory = null;

            using (var command = _context.CreateCommand())
            {
                command.CommandText = "SELECT * FROM Inventory WHERE Id = @Id";
                command.Parameters.AddWithValue("@Id", id);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int productId = Convert.ToInt32(reader["ProductId"]);
                        
                        inventory = new Inventory
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            ProductId = productId,
                            Quantity = Convert.ToInt32(reader["Quantity"]),
                            MinimumStock = Convert.ToInt32(reader["MinimumStock"]),
                            MaximumStock = Convert.ToInt32(reader["MaximumStock"]),
                            Location = reader["Location"].ToString(),
                            LastUpdated = Convert.ToDateTime(reader["LastUpdated"]),
                            Product = _productRepository.GetById(productId)
                        };
                    }
                }
            }

            return inventory;
        }

        public IEnumerable<Inventory> GetAll()
        {
            _context.Open();
            var inventories = new List<Inventory>();

            using (var command = _context.CreateCommand())
            {
                command.CommandText = "SELECT * FROM Inventory";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int productId = Convert.ToInt32(reader["ProductId"]);
                        
                        var inventory = new Inventory
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            ProductId = productId,
                            Quantity = Convert.ToInt32(reader["Quantity"]),
                            MinimumStock = Convert.ToInt32(reader["MinimumStock"]),
                            MaximumStock = Convert.ToInt32(reader["MaximumStock"]),
                            Location = reader["Location"].ToString(),
                            LastUpdated = Convert.ToDateTime(reader["LastUpdated"]),
                            Product = _productRepository.GetById(productId)
                        };
                        inventories.Add(inventory);
                    }
                }
            }

            return inventories;
        }

        public void Add(Inventory entity)
        {
            _context.Open();

            using (var command = _context.CreateCommand())
            {
                command.CommandText = @"
                    INSERT INTO Inventory (ProductId, Quantity, MinimumStock, MaximumStock, Location, LastUpdated)
                    VALUES (@ProductId, @Quantity, @MinimumStock, @MaximumStock, @Location, @LastUpdated);
                    SELECT last_insert_rowid();";

                command.Parameters.AddWithValue("@ProductId", entity.ProductId);
                command.Parameters.AddWithValue("@Quantity", entity.Quantity);
                command.Parameters.AddWithValue("@MinimumStock", entity.MinimumStock);
                command.Parameters.AddWithValue("@MaximumStock", entity.MaximumStock);
                command.Parameters.AddWithValue("@Location", entity.Location ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@LastUpdated", entity.LastUpdated);

                entity.Id = Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public void Update(Inventory entity)
        {
            _context.Open();
            entity.LastUpdated = DateTime.Now;

            using (var command = _context.CreateCommand())
            {
                command.CommandText = @"
                    UPDATE Inventory 
                    SET ProductId = @ProductId, 
                        Quantity = @Quantity, 
                        MinimumStock = @MinimumStock, 
                        MaximumStock = @MaximumStock, 
                        Location = @Location, 
                        LastUpdated = @LastUpdated
                    WHERE Id = @Id";

                command.Parameters.AddWithValue("@Id", entity.Id);
                command.Parameters.AddWithValue("@ProductId", entity.ProductId);
                command.Parameters.AddWithValue("@Quantity", entity.Quantity);
                command.Parameters.AddWithValue("@MinimumStock", entity.MinimumStock);
                command.Parameters.AddWithValue("@MaximumStock", entity.MaximumStock);
                command.Parameters.AddWithValue("@Location", entity.Location ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@LastUpdated", entity.LastUpdated);

                command.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            _context.Open();

            using (var command = _context.CreateCommand())
            {
                command.CommandText = "DELETE FROM Inventory WHERE Id = @Id";
                command.Parameters.AddWithValue("@Id", id);
                command.ExecuteNonQuery();
            }
        }

        public void SaveChanges()
        {
            // No es necesario para SQLite directo, pero se mantiene por consistencia con la interfaz
        }
        
        // Métodos específicos para Inventario
        public Inventory GetByProductId(int productId)
        {
            _context.Open();
            Inventory inventory = null;

            using (var command = _context.CreateCommand())
            {
                command.CommandText = "SELECT * FROM Inventory WHERE ProductId = @ProductId";
                command.Parameters.AddWithValue("@ProductId", productId);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        inventory = new Inventory
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            ProductId = productId,
                            Quantity = Convert.ToInt32(reader["Quantity"]),
                            MinimumStock = Convert.ToInt32(reader["MinimumStock"]),
                            MaximumStock = Convert.ToInt32(reader["MaximumStock"]),
                            Location = reader["Location"].ToString(),
                            LastUpdated = Convert.ToDateTime(reader["LastUpdated"]),
                            Product = _productRepository.GetById(productId)
                        };
                    }
                }
            }

            return inventory;
        }
        
        public IEnumerable<Inventory> GetLowStockItems(int threshold = 0)
        {
            _context.Open();
            var inventories = new List<Inventory>();

            using (var command = _context.CreateCommand())
            {
                if (threshold == 0)
                {
                    command.CommandText = "SELECT * FROM Inventory WHERE Quantity <= MinimumStock";
                }
                else
                {
                    command.CommandText = "SELECT * FROM Inventory WHERE Quantity <= @Threshold";
                    command.Parameters.AddWithValue("@Threshold", threshold);
                }

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int productId = Convert.ToInt32(reader["ProductId"]);
                        
                        var inventory = new Inventory
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            ProductId = productId,
                            Quantity = Convert.ToInt32(reader["Quantity"]),
                            MinimumStock = Convert.ToInt32(reader["MinimumStock"]),
                            MaximumStock = Convert.ToInt32(reader["MaximumStock"]),
                            Location = reader["Location"].ToString(),
                            LastUpdated = Convert.ToDateTime(reader["LastUpdated"]),
                            Product = _productRepository.GetById(productId)
                        };
                        inventories.Add(inventory);
                    }
                }
            }

            return inventories;
        }
    }
}