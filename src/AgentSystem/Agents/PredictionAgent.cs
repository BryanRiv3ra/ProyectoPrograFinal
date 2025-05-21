using System;
using System.Collections.Generic;
using System.Linq;
using Proyecto_Final.AgentSystem.Core;
using Proyecto_Final.Data.Database.Repositories;
using Proyecto_Final.Data.Models;
using Proyecto_Final.Services;

namespace Proyecto_Final.AgentSystem.Agents
{
    public class PredictionAgent : Agent
    {
        private readonly InventoryRepository _inventoryRepository;
        private readonly ProductRepository _productRepository;
        private readonly PredictionService _predictionService;
        
        public PredictionAgent(DatabaseContext dbContext)
            : base("prediction_agent", "Agente de Predicción", "Predice tendencias y necesidades de inventario")
        {
            _inventoryRepository = new InventoryRepository(dbContext);
            _productRepository = new ProductRepository(dbContext);
            _predictionService = new PredictionService();
        }
        
        protected override void ProcessMessage(Message message)
        {
            switch (message.Subject)
            {
                case "calculate_reorder":
                    ProcessCalculateReorder(message);
                    break;
                    
                case "predict_demand":
                    ProcessPredictDemand(message);
                    break;
                    
                case "get_prediction_report":
                    ProcessGetPredictionReport(message);
                    break;
                    
                default:
                    // Mensaje no reconocido
                    Console.WriteLine($"Mensaje no reconocido: {message.Subject}");
                    break;
            }
        }
        
        protected override void PeriodicBehavior()
        {
            // Cada día, analizar productos que podrían necesitar reordenarse pronto
            if (DateTime.Now.Hour == 1 && DateTime.Now.Minute < 5) // Ejecutar alrededor de la 1 AM
            {
                AnalyzeInventoryForReordering();
            }
        }
        
        private void ProcessCalculateReorder(Message message)
        {
            int productId = message.GetContent<int>("product_id");
            
            var inventory = _inventoryRepository.GetByProductId(productId);
            var product = _productRepository.GetById(productId);
            
            if (inventory == null || product == null)
            {
                var errorResponse = message.CreateResponse();
                errorResponse.AddContent("success", false);
                errorResponse.AddContent("error", $"No se encontró inventario o producto con ID {productId}");
                SendMessage(errorResponse);
                return;
            }
            
            // Calcular predicción de reorden
            var reorderPrediction = _predictionService.CalculateReorderPoint(inventory, product);
            
            // Crear respuesta
            var response = message.CreateResponse();
            response.AddContent("product_id", productId);
            response.AddContent("product_name", product.Name);
            response.AddContent("current_stock", inventory.Quantity);
            response.AddContent("reorder_point", reorderPrediction.ReorderPoint);
            response.AddContent("days_until_reorder", reorderPrediction.DaysUntilReorder);
            response.AddContent("suggested_order_quantity", reorderPrediction.SuggestedOrderQuantity);
            
            SendMessage(response);
            
            // Si es necesario reordenar pronto, enviar alerta
            if (reorderPrediction.DaysUntilReorder <= 7)
            {
                SendReorderAlert(product, inventory, reorderPrediction);
            }
        }
        
        private void ProcessPredictDemand(Message message)
        {
            int productId = message.GetContent<int>("product_id");
            int daysAhead = message.GetContent<int>("days_ahead", 30);
            
            var product = _productRepository.GetById(productId);
            
            if (product == null)
            {
                var errorResponse = message.CreateResponse();
                errorResponse.AddContent("success", false);
                errorResponse.AddContent("error", $"No se encontró producto con ID {productId}");
                SendMessage(errorResponse);
                return;
            }
            
            // Predecir demanda
            var demandPrediction = _predictionService.PredictDemand(product, daysAhead);
            
            // Crear respuesta
            var response = message.CreateResponse();
            response.AddContent("product_id", productId);
            response.AddContent("product_name", product.Name);
            response.AddContent("days_ahead", daysAhead);
            response.AddContent("predicted_demand", demandPrediction.PredictedDemand);
            response.AddContent("confidence_level", demandPrediction.ConfidenceLevel);
            response.AddContent("trend", demandPrediction.Trend);
            
            SendMessage(response);
        }
        
        private void ProcessGetPredictionReport(Message message)
        {
            int daysAhead = message.GetContent<int>("days_ahead", 30);
            string category = message.GetContent<string>("category", null);
            
            var products = _productRepository.GetAll();
            
            // Filtrar por categoría si se especifica
            if (!string.IsNullOrEmpty(category))
            {
                products = products.Where(p => p.Category == category).ToList();
            }
            
            var predictionReports = new List<object>();
            
            foreach (var product in products)
            {
                var inventory = _inventoryRepository.GetByProductId(product.Id);
                
                if (inventory != null)
                {
                    var reorderPrediction = _predictionService.CalculateReorderPoint(inventory, product);
                    var demandPrediction = _predictionService.PredictDemand(product, daysAhead);
                    
                    predictionReports.Add(new
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        CurrentStock = inventory.Quantity,
                        MinimumStock = inventory.MinimumStock,
                        ReorderPoint = reorderPrediction.ReorderPoint,
                        DaysUntilReorder = reorderPrediction.DaysUntilReorder,
                        SuggestedOrderQuantity = reorderPrediction.SuggestedOrderQuantity,
                        PredictedDemand = demandPrediction.PredictedDemand,
                        Trend = demandPrediction.Trend
                    });
                }
            }
            
            // Crear respuesta
            var response = message.CreateResponse();
            response.AddContent("predictions", predictionReports);
            response.AddContent("count", predictionReports.Count);
            response.AddContent("days_ahead", daysAhead);
            response.AddContent("generated_at", DateTime.Now);
            
            SendMessage(response);
        }
        
        private void AnalyzeInventoryForReordering()
        {
            var inventoryItems = _inventoryRepository.GetAll();
            
            foreach (var item in inventoryItems)
            {
                var product = _productRepository.GetById(item.ProductId);
                
                if (product != null)
                {
                    var reorderPrediction = _predictionService.CalculateReorderPoint(item, product);
                    
                    // Si es necesario reordenar en los próximos 7 días, enviar alerta
                    if (reorderPrediction.DaysUntilReorder <= 7)
                    {
                        SendReorderAlert(product, item, reorderPrediction);
                    }
                }
            }
        }
        
        private void SendReorderAlert(Product product, Inventory inventory, ReorderPrediction prediction)
        {
            var alertMessage = new Message
            {
                ReceiverAgentId = "alert_agent",
                Type = MessageType.Request,
                Subject = "create_alert"
            };
            
            alertMessage.AddContent("alert_type", "LowStock");
            alertMessage.AddContent("severity", "Warning");
            alertMessage.AddContent("product_id", product.Id);
            
            string message;
            
            if (prediction.DaysUntilReorder <= 0)
            {
                message = $"¡Reordenar ahora! El producto {product.Name} está por debajo del punto de reorden. " +
                          $"Stock actual: {inventory.Quantity}, Punto de reorden: {prediction.ReorderPoint}. " +
                          $"Cantidad sugerida para ordenar: {prediction.SuggestedOrderQuantity} unidades.";
            }
            else
            {
                message = $"Reordenar pronto: El producto {product.Name} necesitará reordenarse en {prediction.DaysUntilReorder} días. " +
                          $"Stock actual: {inventory.Quantity}, Punto de reorden: {prediction.ReorderPoint}. " +
                          $"Cantidad sugerida para ordenar: {prediction.SuggestedOrderQuantity} unidades.";
            }
            
            alertMessage.AddContent("message", message);
            
            SendMessage(alertMessage);
        }
    }
}