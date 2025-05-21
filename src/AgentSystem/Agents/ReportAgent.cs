using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.Json;
using Proyecto_Final.AgentSystem.Core;
using Proyecto_Final.Data.Database.Repositories;
using Proyecto_Final.Data.Models;
using Proyecto_Final.Utils;

namespace Proyecto_Final.AgentSystem.Agents
{
    public class ReportAgent : Agent
    {
        private readonly ProductRepository _productRepository;
        private readonly InventoryRepository _inventoryRepository;
        private readonly TransactionRepository _transactionRepository;
        private readonly string _reportsFolder;
        
        public ReportAgent(DatabaseContext dbContext, string reportsFolder = null)
            : base("report_agent", "Agente de Reportes", "Genera reportes del sistema de inventario")
        {
            _productRepository = new ProductRepository(dbContext);
            _inventoryRepository = new InventoryRepository(dbContext);
            _transactionRepository = new TransactionRepository(dbContext);
            
            // Establecer carpeta de reportes
            _reportsFolder = reportsFolder ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");
            
            // Crear la carpeta si no existe
            if (!Directory.Exists(_reportsFolder))
            {
                Directory.CreateDirectory(_reportsFolder);
            }
        }
        
        protected override void ProcessMessage(Message message)
        {
            switch (message.Subject)
            {
                case "generate_inventory_report":
                    ProcessGenerateInventoryReport(message);
                    break;
                    
                case "generate_transactions_report":
                    ProcessGenerateTransactionsReport(message);
                    break;
                    
                case "generate_low_stock_report":
                    ProcessGenerateLowStockReport(message);
                    break;
                    
                default:
                    // Mensaje no reconocido
                    Console.WriteLine($"Mensaje no reconocido: {message.Subject}");
                    break;
            }
        }
        
        protected override void PeriodicBehavior()
        {
            // Generar reportes automáticos semanales (cada lunes)
            if (DateTime.Now.DayOfWeek == DayOfWeek.Monday && DateTime.Now.Hour == 8 && DateTime.Now.Minute < 15)
            {
                GenerateWeeklyReports();
            }
        }
        
        private void ProcessGenerateInventoryReport(Message message)
        {
            string format = message.GetContent<string>("format", "json");
            string category = message.GetContent<string>("category", null);
            
            var inventory = _inventoryRepository.GetAll();
            
            // Filtrar por categoría si se especifica
            if (!string.IsNullOrEmpty(category))
            {
                inventory = inventory.Where(i => i.Product?.Category == category).ToList();
            }
            
            // Preparar datos del reporte
            var reportData = inventory.Select(i => new
            {
                ProductId = i.ProductId,
                ProductCode = i.Product?.Code,
                ProductName = i.Product?.Name,
                Category = i.Product?.Category,
                Quantity = i.Quantity,
                MinimumStock = i.MinimumStock,
                MaximumStock = i.MaximumStock,
                Location = i.Location,
                LastUpdated = i.LastUpdated,
                Status = i.Quantity <= i.MinimumStock ? "Bajo" : 
                         i.Quantity >= i.MaximumStock ? "Exceso" : "Normal"
            }).ToList();
            
            // Generar el reporte
            string reportPath = GenerateReport("Inventario", reportData, format);
            
            // Responder con la ruta del reporte
            var response = message.CreateResponse();
            response.AddContent("success", true);
            response.AddContent("report_path", reportPath);
            response.AddContent("report_format", format);
            response.AddContent("item_count", reportData.Count);
            
            SendMessage(response);
        }
        
        private void ProcessGenerateTransactionsReport(Message message)
        {
            string format = message.GetContent<string>("format", "json");
            DateTime startDate = message.GetContent<DateTime>("start_date", DateTime.Now.AddDays(-30));
            DateTime endDate = message.GetContent<DateTime>("end_date", DateTime.Now);
            TransactionType? type = null;
            
            string typeStr = message.GetContent<string>("transaction_type", null);
            if (!string.IsNullOrEmpty(typeStr) && Enum.TryParse<TransactionType>(typeStr, out var parsedType))
            {
                type = parsedType;
            }
            
            var transactions = _transactionRepository.GetByDateRange(startDate, endDate);
            
            // Filtrar por tipo si se especifica
            if (type.HasValue)
            {
                transactions = transactions.Where(t => t.Type == type.Value).ToList();
            }
            
            // Preparar datos del reporte
            var reportData = transactions.Select(t => new
            {
                Id = t.Id,
                Date = t.Date,
                ProductId = t.ProductId,
                ProductName = t.Product?.Name,
                Type = t.Type.ToString(),
                Quantity = t.Quantity,
                UnitPrice = t.UnitPrice,
                TotalValue = t.Quantity * t.UnitPrice,
                Reference = t.Reference,
                Notes = t.Notes
            }).ToList();
            
            // Generar el reporte
            string reportPath = GenerateReport("Transacciones", reportData, format);
            
            // Responder con la ruta del reporte
            var response = message.CreateResponse();
            response.AddContent("success", true);
            response.AddContent("report_path", reportPath);
            response.AddContent("report_format", format);
            response.AddContent("transaction_count", reportData.Count);
            response.AddContent("start_date", startDate);
            response.AddContent("end_date", endDate);
            
            SendMessage(response);
        }
        
        private void ProcessGenerateLowStockReport(Message message)
        {
            string format = message.GetContent<string>("format", "json");
            int threshold = message.GetContent<int>("threshold", Constants.DefaultLowStockThreshold);
            
            var lowStockItems = _inventoryRepository.GetLowStockItems(threshold);
            
            // Preparar datos del reporte
            var reportData = lowStockItems.Select(i => new
            {
                ProductId = i.ProductId,
                ProductCode = i.Product?.Code,
                ProductName = i.Product?.Name,
                Category = i.Product?.Category,
                CurrentQuantity = i.Quantity,
                MinimumStock = i.MinimumStock,
                Deficit = i.MinimumStock - i.Quantity,
                Location = i.Location,
                LastUpdated = i.LastUpdated
            }).ToList();
            
            // Generar el reporte
            string reportPath = GenerateReport("StockBajo", reportData, format);
            
            // Responder con la ruta del reporte
            var response = message.CreateResponse();
            response.AddContent("success", true);
            response.AddContent("report_path", reportPath);
            response.AddContent("report_format", format);
            response.AddContent("item_count", reportData.Count);
            
            SendMessage(response);
        }
        
        private void GenerateWeeklyReports()
        {
            // Generar reportes semanales automáticos
            try
            {
                // Reporte de inventario
                var inventoryMessage = new Message
                {
                    ReceiverAgentId = "report_agent",
                    Type = MessageType.Request,
                    Subject = "generate_inventory_report"
                };
                inventoryMessage.AddContent("format", "json");
                ProcessGenerateInventoryReport(inventoryMessage);
                
                // Reporte de transacciones de la última semana
                var transactionsMessage = new Message
                {
                    ReceiverAgentId = "report_agent",
                    Type = MessageType.Request,
                    Subject = "generate_transactions_report"
                };
                transactionsMessage.AddContent("format", "json");
                transactionsMessage.AddContent("start_date", DateTime.Now.AddDays(-7));
                ProcessGenerateTransactionsReport(transactionsMessage);
                
                // Reporte de stock bajo
                var lowStockMessage = new Message
                {
                    ReceiverAgentId = "report_agent",
                    Type = MessageType.Request,
                    Subject = "generate_low_stock_report"
                };
                lowStockMessage.AddContent("format", "json");
                ProcessGenerateLowStockReport(lowStockMessage);
                
                // Notificar que se han generado los reportes semanales
                var notificationMessage = new Message
                {
                    ReceiverAgentId = "*", // Broadcast a todos los agentes
                    Type = MessageType.Notification,
                    Subject = "weekly_reports_generated"
                };
                notificationMessage.AddContent("timestamp", DateTime.Now);
                notificationMessage.AddContent("reports_folder", _reportsFolder);
                
                SendMessage(notificationMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al generar reportes semanales: {ex.Message}");
            }
        }
        
        private string GenerateReport(string reportName, object data, string format)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = $"{reportName}_{timestamp}.{format.ToLower()}";
            string filePath = Path.Combine(_reportsFolder, fileName);
            
            switch (format.ToLower())
            {
                case "json":
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };
                    string jsonContent = JsonSerializer.Serialize(data, options);
                    File.WriteAllText(filePath, jsonContent, Encoding.UTF8);
                    break;
                    
                case "csv":
                    GenerateCsvReport(filePath, data);
                    break;
                    
                default:
                    throw new NotSupportedException($"Formato de reporte no soportado: {format}");
            }
            
            return filePath;
        }
        
        private void GenerateCsvReport(string filePath, object data)
        {
            // Implementación simple de generación de CSV
            // En un proyecto real, se podría usar una biblioteca como CsvHelper
            
            if (data is IEnumerable<object> items && items.Any())
            {
                using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    // Obtener propiedades del primer elemento para los encabezados
                    var firstItem = items.First();
                    var properties = firstItem.GetType().GetProperties();
                    
                    // Escribir encabezados
                    writer.WriteLine(string.Join(",", properties.Select(p => $"\"{p.Name}\"")));
                    
                    // Escribir datos
                    foreach (var item in items)
                    {
                        var values = properties.Select(p => 
                        {
                            var value = p.GetValue(item);
                            if (value == null)
                                return "\"\"";
                                
                            // Escapar comillas en strings
                            if (value is string strValue)
                                return $"\"{strValue.Replace("\"", "\"\"")}\"";
                                
                            return $"\"{value}\"";
                        });
                        
                        writer.WriteLine(string.Join(",", values));
                    }
                }
            }
        }
    }
}