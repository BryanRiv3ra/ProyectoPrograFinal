using System;
using System.Data;
using System.Data.SQLite;
using System.Configuration;

namespace Proyecto_Final.Data.Database
{
    public class DatabaseContext : IDisposable
    {
        private SQLiteConnection _connection;
        
        public DatabaseContext()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["InventoryDB"].ConnectionString;
            _connection = new SQLiteConnection(connectionString);
        }
        
        public void Open()
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
        }
        
        public void Close()
        {
            if (_connection.State != ConnectionState.Closed)
            {
                _connection.Close();
            }
        }
        
        public SQLiteCommand CreateCommand()
        {
            return _connection.CreateCommand();
        }
        
        public SQLiteTransaction BeginTransaction()
        {
            Open();
            return _connection.BeginTransaction();
        }
        
        public void Dispose()
        {
            Close();
            _connection.Dispose();
        }
    }
}