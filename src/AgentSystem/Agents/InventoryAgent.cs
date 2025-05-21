using System;
using System.Collections.Generic;
using System.Linq;
using Proyecto_Final.AgentSystem.Core;
using Proyecto_Final.Data.Database.Repositories;
using Proyecto_Final.Data.Models;
using Proyecto_Final.Utils;

namespace Proyecto_Final.AgentSystem.Agents
{
    public class InventoryAgent : Agent
    {
        private readonly InventoryRepository _inventoryRepository;
        private readonly ProductRepository _productRepository;
        private readonly int _lowStockThreshold;
        private DateTime _lastCheckTime;
        
        public InventoryAgent(DatabaseContext dbContext, int lowStockThreshold = 0)
            : base("inventory_agent", "Agente de Inventario", "Monitorea y gestiona el inventario")
        {
            _inventoryRepository = new InventoryRepository(dbContext);
            _productRepository = new ProductRepository(dbContext);
            _lowStockThreshold = lowStockThreshold > 0 ? lowStockThreshold : Constants.DefaultLowStockThreshold;
            _lastCheckTime = DateTime.MinValue;
        }
        
        protected override void ProcessMessage(Message message)
        {
            switch (message.Subject)
            {
                case "check_stock_level":
                    ProcessCheckStockLevel(message);
                    break;
                    
                case "update_inventory":
                    ProcessUpdateInventory(message);
                    break;
                    
                case "get_product_stock":
                    ProcessGetProductStock(message);
                    break;
                    
                case "get_low_stock_items":
                    ProcessGetLowStockItems(message);
                    break;
                    
                default:
                    // Mensaje no reconocido
                    Console.WriteLine($"Mensaje no reconocido: {message.Subject}");
                    break;
            }
        }
        
        protected override void PeriodicBehavior()
        {
            // Verificar niveles bajos de stock periódicamente (cada 5 minutos)
            if ((DateTime.Now - _lastCheckTime).TotalMinutes >= 5)
            {
                CheckLowStockLevels();
                _lastCheckTime = DateTime.Now;
            }
        }
        
        private void ProcessCheckStockLevel(Message message)
        {
            int productId = message.GetContent<int>("product_id");
            
            var inventory = _inventoryRepository.GetByProductId(productId);
            var response = message.CreateResponse();
            
            if (inventory != null)
            {
                response.AddContent("product_id", productId);
                response.AddContent("quantity", inventory.Quantity);
                response.AddContent("minimum_stock", inventory.MinimumStock);
                response.AddContent("is_low_stock", inventory.Quantity <= inventory.MinimumStock);
            }
            else
            {
                response.AddContent("error", $"No se encontró inventario para el producto con ID {productId}");
            }
            
            SendMessage(response);
        }
        
        private void ProcessUpdateInventory(Message message)
        {
            int productId = message.GetContent<int>("product_id");
            int quantity = message.GetContent<int>("quantity");
            string operation = message.GetContent<string>("operation", "set");
            
            var inventory = _inventoryRepository.GetByProductId(productId);
            var response = message.CreateResponse();
            
            if (inventory != null)
            {
                int oldQuantity = inventory.Quantity;
                
                // Actualizar la cantidad según la operación
                switch (operation.ToLower())
                {
                    case "add":
                        inventory.Quantity += quantity;
                        break;
                        
                    case "subtract":
                        inventory.Quantity = Math.Max(0, inventory.Quantity - quantity);
                        break;
                        
                    case "set":
                    default:
                        inventory.Quantity = Math.Max(0, quantity);
                        break;
                }
                
                inventory.LastUpdated = DateTime.Now;
                _inventoryRepository.Update(inventory);
                
                response.AddContent("success", true);
                response.AddContent("product_id", productId);
                response.AddContent("old_quantity", oldQuantity);
                response.AddContent("new_quantity", inventory.Quantity);
                
                // Verificar si el nivel de stock es bajo después de la actualización
                if (inventory.Quantity <= inventory.MinimumStock)
                {
                    NotifyLowStock(inventory);
                }
            }
            else
            {
                response.AddContent("success", false);
                response.AddContent("error", $"No se encontró inventario para el producto con ID {productId}");
            }
            
            SendMessage(response);
        }
        
        private void ProcessGetProductStock(Message message)
        {
            int productId = message.GetContent<int>("product_id");
            
            var inventory = _inventoryRepository.GetByProductId(productId);
            var response = message.CreateResponse();
            
            if (inventory != null)
            {
                response.AddContent("product_id", productId);
                response.AddContent("product_name", inventory.Product?.Name ?? "Desconocido");
                response.AddContent("quantity", inventory.Quantity);
                response.AddContent("minimum_stock", inventory.MinimumStock);
                response.AddContent("maximum_stock", inventory.MaximumStock);
                response.AddContent("location", inventory.Location);
                response.AddContent("last_updated", inventory.LastUpdated);
            }
            else
            {
                response.AddContent("error", $"No se encontró inventario para el producto con ID {productId}");
            }
            
            SendMessage(response);
        }
        
        private void ProcessGetLowStockItems(Message message)
        {
            int threshold = message.GetContent<int>("threshold", _lowStockThreshold);
            
            var lowStockItems = _inventoryRepository.GetLowStockItems(threshold);
            var response = message.CreateResponse();
            
            var itemsData = lowStockItems.Select(item => new
            {
                ProductId = item.ProductId,
                ProductName = item.Product?.Name ?? "Desconocido",
                Quantity = item.Quantity,
                MinimumStock = item.MinimumStock,
                Location = item.Location
            }).ToList();
            
            response.AddContent("low_stock_items", itemsData);
            response.AddContent("count", itemsData.Count);
            
            SendMessage(response);
        }
        
        private void CheckLowStockLevels()
        {
            var lowStockItems = _inventoryRepository.GetLowStockItems();
            
            foreach (var item in lowStockItems)
            {
                NotifyLowStock(item);
            }
        }
        
        private void NotifyLowStock(Inventory inventory)
        {
            // Crear un mensaje de notificación para el agente de alertas
            var alertMessage = new Message
            {
                ReceiverAgentId = "alert_agent",
                Type = MessageType.Notification,
                Subject = "low_stock_alert"
            };
            
            alertMessage.AddContent("product_id", inventory.ProductId);
            alertMessage.AddContent("product_name", inventory.Product?.Name ?? "Desconocido");
            alertMessage.AddContent("current_quantity", inventory.Quantity);
            alertMessage.AddContent("minimum_stock", inventory.MinimumStock);
            
            SendMessage(alertMessage);
            
            // También notificar al agente de predicción para que calcule cuándo reordenar
            var predictionMessage = new Message
            {
                ReceiverAgentId = "prediction_agent",
                Type = MessageType.Request,
                Subject = "calculate_reorder"
            };
            
            predictionMessage.AddContent("product_id", inventory.ProductId);
            
            SendMessage(predictionMessage);
        }
    }
}