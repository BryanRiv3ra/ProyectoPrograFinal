using System;
using System.Collections.Generic;
using System.Linq;
using Proyecto_Final.Data.Models;

namespace Proyecto_Final.Data.Database.Repositories
{
    public class AlertRepository : IRepository<Alert>
    {
        private readonly DatabaseContext _context;
        
        public AlertRepository(DatabaseContext context)
        {
            _context = context;
        }
        
        public Alert GetById(int id)
        {
            _context.Open();
            Alert alert = null;

            using (var command = _context.CreateCommand())
            {
                command.CommandText = "SELECT * FROM Alerts WHERE Id = @Id";
                command.Parameters.AddWithValue("@Id", id);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        alert = new Alert
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Type = (AlertType)Enum.Parse(typeof(AlertType), reader["Type"].ToString()),
                            Severity = (AlertSeverity)Enum.Parse(typeof(AlertSeverity), reader["Severity"].ToString()),
                            Message = reader["Message"].ToString(),
                            ProductId = reader["ProductId"] != DBNull.Value ? Convert.ToInt32(reader["ProductId"]) : (int?)null,
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                            UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"]),
                            IsResolved = Convert.ToBoolean(reader["IsResolved"]),
                            IsExpired = Convert.ToBoolean(reader["IsExpired"]),
                            ResolvedAt = reader["ResolvedAt"] != DBNull.Value ? Convert.ToDateTime(reader["ResolvedAt"]) : (DateTime?)null
                        };
                    }
                }
            }

            return alert;
        }

        public IEnumerable<Alert> GetAll()
        {
            _context.Open();
            var alerts = new List<Alert>();

            using (var command = _context.CreateCommand())
            {
                command.CommandText = "SELECT * FROM Alerts ORDER BY CreatedAt DESC";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var alert = new Alert
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Type = (AlertType)Enum.Parse(typeof(AlertType), reader["Type"].ToString()),
                            Severity = (AlertSeverity)Enum.Parse(typeof(AlertSeverity), reader["Severity"].ToString()),
                            Message = reader["Message"].ToString(),
                            ProductId = reader["ProductId"] != DBNull.Value ? Convert.ToInt32(reader["ProductId"]) : (int?)null,
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                            UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"]),
                            IsResolved = Convert.ToBoolean(reader["IsResolved"]),
                            IsExpired = Convert.ToBoolean(reader["IsExpired"]),
                            ResolvedAt = reader["ResolvedAt"] != DBNull.Value ? Convert.ToDateTime(reader["ResolvedAt"]) : (DateTime?)null
                        };
                        
                        alerts.Add(alert);
                    }
                }
            }

            return alerts;
        }

        public void Add(Alert entity)
        {
            _context.Open();

            using (var command = _context.CreateCommand())
            {
                command.CommandText = @"
                    INSERT INTO Alerts (Type, Severity, Message, ProductId, CreatedAt, UpdatedAt, IsResolved, IsExpired, ResolvedAt)
                    VALUES (@Type, @Severity, @Message, @ProductId, @CreatedAt, @UpdatedAt, @IsResolved, @IsExpired, @ResolvedAt);
                    SELECT last_insert_rowid();";

                command.Parameters.AddWithValue("@Type", entity.Type.ToString());
                command.Parameters.AddWithValue("@Severity", entity.Severity.ToString());
                command.Parameters.AddWithValue("@Message", entity.Message);
                command.Parameters.AddWithValue("@ProductId", entity.ProductId.HasValue ? (object)entity.ProductId.Value : DBNull.Value);
                command.Parameters.AddWithValue("@CreatedAt", entity.CreatedAt);
                command.Parameters.AddWithValue("@UpdatedAt", entity.UpdatedAt);
                command.Parameters.AddWithValue("@IsResolved", entity.IsResolved);
                command.Parameters.AddWithValue("@IsExpired", entity.IsExpired);
                command.Parameters.AddWithValue("@ResolvedAt", entity.ResolvedAt.HasValue ? (object)entity.ResolvedAt.Value : DBNull.Value);

                entity.Id = Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public void Update(Alert entity)
        {
            _context.Open();
            entity.UpdatedAt = DateTime.Now;

            using (var command = _context.CreateCommand())
            {
                command.CommandText = @"
                    UPDATE Alerts 
                    SET Type = @Type, 
                        Severity = @Severity, 
                        Message = @Message, 
                        ProductId = @ProductId, 
                        UpdatedAt = @UpdatedAt, 
                        IsResolved = @IsResolved, 
                        IsExpired = @IsExpired, 
                        ResolvedAt = @ResolvedAt
                    WHERE Id = @Id";

                command.Parameters.AddWithValue("@Id", entity.Id);
                command.Parameters.AddWithValue("@Type", entity.Type.ToString());
                command.Parameters.AddWithValue("@Severity", entity.Severity.ToString());
                command.Parameters.AddWithValue("@Message", entity.Message);
                command.Parameters.AddWithValue("@ProductId", entity.ProductId.HasValue ? (object)entity.ProductId.Value : DBNull.Value);
                command.Parameters.AddWithValue("@UpdatedAt", entity.UpdatedAt);
                command.Parameters.AddWithValue("@IsResolved", entity.IsResolved);
                command.Parameters.AddWithValue("@IsExpired", entity.IsExpired);
                command.Parameters.AddWithValue("@ResolvedAt", entity.ResolvedAt.HasValue ? (object)entity.ResolvedAt.Value : DBNull.Value);

                command.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            _context.Open();

            using (var command = _context.CreateCommand())
            {
                command.CommandText = "DELETE FROM Alerts WHERE Id = @Id";
                command.Parameters.AddWithValue("@Id", id);
                command.ExecuteNonQuery();
            }
        }

        public IEnumerable<Alert> GetActiveAlerts()
        {
            _context.Open();
            var alerts = new List<Alert>();

            using (var command = _context.CreateCommand())
            {
                command.CommandText = "SELECT * FROM Alerts WHERE IsResolved = 0 AND IsExpired = 0 ORDER BY CreatedAt DESC";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var alert = new Alert
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Type = (AlertType)Enum.Parse(typeof(AlertType), reader["Type"].ToString()),
                            Severity = (AlertSeverity)Enum.Parse(typeof(AlertSeverity), reader["Severity"].ToString()),
                            Message = reader["Message"].ToString(),
                            ProductId = reader["ProductId"] != DBNull.Value ? Convert.ToInt32(reader["ProductId"]) : (int?)null,
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                            UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"]),
                            IsResolved = Convert.ToBoolean(reader["IsResolved"]),
                            IsExpired = Convert.ToBoolean(reader["IsExpired"]),
                            ResolvedAt = reader["ResolvedAt"] != DBNull.Value ? Convert.ToDateTime(reader["ResolvedAt"]) : (DateTime?)null
                        };
                        
                        alerts.Add(alert);
                    }
                }
            }

            return alerts;
        }
        
        public IEnumerable<Alert> GetAlertsByProduct(int productId)
        {
            _context.Open();
            var alerts = new List<Alert>();

            using (var command = _context.CreateCommand())
            {
                command.CommandText = "SELECT * FROM Alerts WHERE ProductId = @ProductId ORDER BY CreatedAt DESC";
                command.Parameters.AddWithValue("@ProductId", productId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var alert = new Alert
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Type = (AlertType)Enum.Parse(typeof(AlertType), reader["Type"].ToString()),
                            Severity = (AlertSeverity)Enum.Parse(typeof(AlertSeverity), reader["Severity"].ToString()),
                            Message = reader["Message"].ToString(),
                            ProductId = reader["ProductId"] != DBNull.Value ? Convert.ToInt32(reader["ProductId"]) : (int?)null,
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                            UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"]),
                            IsResolved = Convert.ToBoolean(reader["IsResolved"]),
                            IsExpired = Convert.ToBoolean(reader["IsExpired"]),
                            ResolvedAt = reader["ResolvedAt"] != DBNull.Value ? Convert.ToDateTime(reader["ResolvedAt"]) : (DateTime?)null
                        };
                        
                        alerts.Add(alert);
                    }
                }
            }

            return alerts;
        }
        
        public void SaveChanges()
        {
            // No es necesario para SQLite directo, pero se mantiene por consistencia con la interfaz
        }
    }
}