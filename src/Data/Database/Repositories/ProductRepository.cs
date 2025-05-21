using System;
using System.Collections.Generic;
using System.Data.SQLite;
using Proyecto_Final.Data.Models;

namespace Proyecto_Final.Data.Database.Repositories
{
    public class ProductRepository : IRepository<Product>
    {
        private readonly DatabaseContext _context;

        public ProductRepository(DatabaseContext context)
        {
            _context = context;
        }

        public Product GetById(int id)
        {
            _context.Open();
            Product product = null;

            using (var command = _context.CreateCommand())
            {
                command.CommandText = "SELECT * FROM Products WHERE Id = @Id";
                command.Parameters.AddWithValue("@Id", id);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        product = new Product
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Code = reader["Code"].ToString(),
                            Name = reader["Name"].ToString(),
                            Description = reader["Description"].ToString(),
                            Price = Convert.ToDecimal(reader["Price"]),
                            Category = reader["Category"].ToString(),
                            Supplier = reader["Supplier"].ToString(),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                            UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"])
                        };
                    }
                }
            }

            return product;
        }

        public IEnumerable<Product> GetAll()
        {
            _context.Open();
            var products = new List<Product>();

            using (var command = _context.CreateCommand())
            {
                command.CommandText = "SELECT * FROM Products";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var product = new Product
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Code = reader["Code"].ToString(),
                            Name = reader["Name"].ToString(),
                            Description = reader["Description"].ToString(),
                            Price = Convert.ToDecimal(reader["Price"]),
                            Category = reader["Category"].ToString(),
                            Supplier = reader["Supplier"].ToString(),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                            UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"])
                        };
                        products.Add(product);
                    }
                }
            }

            return products;
        }

        public void Add(Product entity)
        {
            _context.Open();

            using (var command = _context.CreateCommand())
            {
                command.CommandText = @"
                    INSERT INTO Products (Code, Name, Description, Price, Category, Supplier, CreatedAt, UpdatedAt)
                    VALUES (@Code, @Name, @Description, @Price, @Category, @Supplier, @CreatedAt, @UpdatedAt);
                    SELECT last_insert_rowid();";

                command.Parameters.AddWithValue("@Code", entity.Code);
                command.Parameters.AddWithValue("@Name", entity.Name);
                command.Parameters.AddWithValue("@Description", entity.Description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Price", entity.Price);
                command.Parameters.AddWithValue("@Category", entity.Category ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Supplier", entity.Supplier ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CreatedAt", entity.CreatedAt);
                command.Parameters.AddWithValue("@UpdatedAt", entity.UpdatedAt);

                entity.Id = Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public void Update(Product entity)
        {
            _context.Open();
            entity.UpdatedAt = DateTime.Now;

            using (var command = _context.CreateCommand())
            {
                command.CommandText = @"
                    UPDATE Products 
                    SET Code = @Code, 
                        Name = @Name, 
                        Description = @Description, 
                        Price = @Price, 
                        Category = @Category, 
                        Supplier = @Supplier, 
                        UpdatedAt = @UpdatedAt
                    WHERE Id = @Id";

                command.Parameters.AddWithValue("@Id", entity.Id);
                command.Parameters.AddWithValue("@Code", entity.Code);
                command.Parameters.AddWithValue("@Name", entity.Name);
                command.Parameters.AddWithValue("@Description", entity.Description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Price", entity.Price);
                command.Parameters.AddWithValue("@Category", entity.Category ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Supplier", entity.Supplier ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@UpdatedAt", entity.UpdatedAt);

                command.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            _context.Open();

            using (var command = _context.CreateCommand())
            {
                command.CommandText = "DELETE FROM Products WHERE Id = @Id";
                command.Parameters.AddWithValue("@Id", id);
                command.ExecuteNonQuery();
            }
        }

        public void SaveChanges()
        {
            // No es necesario para SQLite directo, pero se mantiene por consistencia con la interfaz
        }
    }
}