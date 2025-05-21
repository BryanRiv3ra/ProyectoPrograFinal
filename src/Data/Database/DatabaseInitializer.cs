using System;
using System.IO;
using System.Data.SQLite;

namespace Proyecto_Final.Data.Database
{
    public static class DatabaseInitializer
    {
        public static void Initialize()
        {
            string dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
            string dbFilePath = Path.Combine(dataDirectory, "InventoryDatabase.db");
            
            // Crear el directorio si no existe
            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
            }
            
            // Crear la base de datos si no existe
            if (!File.Exists(dbFilePath))
            {
                SQLiteConnection.CreateFile(dbFilePath);
                CreateTables();
            }
        }
        
        private static void CreateTables()
        {
            using (var context = new DatabaseContext())
            {
                context.Open();
                
                // Crear tabla de productos
                using (var command = context.CreateCommand())
                {
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Products (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Code TEXT NOT NULL UNIQUE,
                            Name TEXT NOT NULL,
                            Description TEXT,
                            Price REAL NOT NULL,
                            Category TEXT,
                            Supplier TEXT,
                            CreatedAt DATETIME NOT NULL,
                            UpdatedAt DATETIME NOT NULL
                        );";
                    command.ExecuteNonQuery();
                }
                
                // Crear tabla de inventario
                using (var command = context.CreateCommand())
                {
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Inventory (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            ProductId INTEGER NOT NULL,
                            Quantity INTEGER NOT NULL DEFAULT 0,
                            MinimumStock INTEGER NOT NULL DEFAULT 0,
                            MaximumStock INTEGER NOT NULL DEFAULT 0,
                            Location TEXT,
                            LastUpdated DATETIME NOT NULL,
                            FOREIGN KEY (ProductId) REFERENCES Products(Id)
                        );";
                    command.ExecuteNonQuery();
                }
                
                // Crear tabla de transacciones
                using (var command = context.CreateCommand())
                {
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Transactions (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            ProductId INTEGER NOT NULL,
                            Type INTEGER NOT NULL,
                            Quantity INTEGER NOT NULL,
                            UnitPrice REAL NOT NULL,
                            Reference TEXT,
                            Notes TEXT,
                            TransactionDate DATETIME NOT NULL,
                            FOREIGN KEY (ProductId) REFERENCES Products(Id)
                        );";
                    command.ExecuteNonQuery();
                }
                
                // Crear tabla de alertas
                using (var command = context.CreateCommand())
                {
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Alerts (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            ProductId INTEGER NOT NULL,
                            Type INTEGER NOT NULL,
                            Message TEXT NOT NULL,
                            Status INTEGER NOT NULL,
                            CreatedAt DATETIME NOT NULL,
                            ResolvedAt DATETIME,
                            FOREIGN KEY (ProductId) REFERENCES Products(Id)
                        );";
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}