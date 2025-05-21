using System;
using System.Collections.Generic;
using System.Linq;
using Proyecto_Final.AgentSystem.Core;
using Proyecto_Final.Data.Database.Repositories;
using Proyecto_Final.Data.Models;

namespace Proyecto_Final.AgentSystem.Agents
{
    public class AlertAgent : Agent
    {
        private readonly List<Alert> _activeAlerts;
        private readonly AlertRepository _alertRepository;
        
        public AlertAgent(DatabaseContext dbContext)
            : base("alert_agent", "Agente de Alertas", "Gestiona y notifica alertas del sistema")
        {
            _activeAlerts = new List<Alert>();
            _alertRepository = new AlertRepository(dbContext);
            
            // Cargar alertas activas al iniciar
            LoadActiveAlerts();
        }
        
        private void LoadActiveAlerts()
        {
            var activeAlerts = _alertRepository.GetActiveAlerts();
            _activeAlerts.Clear();
            _activeAlerts.AddRange(activeAlerts);
        }
        
        protected override void ProcessMessage(Message message)
        {
            switch (message.Subject)
            {
                case "low_stock_alert":
                    ProcessLowStockAlert(message);
                    break;
                    
                case "get_active_alerts":
                    ProcessGetActiveAlerts(message);
                    break;
                    
                case "dismiss_alert":
                    ProcessDismissAlert(message);
                    break;
                    
                case "create_alert":
                    ProcessCreateAlert(message);
                    break;
                    
                default:
                    // Mensaje no reconocido
                    Console.WriteLine($"Mensaje no reconocido: {message.Subject}");
                    break;
            }
        }
        
        protected override void PeriodicBehavior()
        {
            // Verificar alertas antiguas y marcarlas como expiradas si es necesario
            var now = DateTime.Now;
            var expiredAlerts = _activeAlerts.Where(a => 
                !a.IsResolved && 
                (now - a.CreatedAt).TotalDays > 7).ToList();
                
            foreach (var alert in expiredAlerts)
            {
                alert.IsExpired = true;
                _alertRepository.Update(alert);
            }
            
            // Eliminar alertas expiradas de la lista de alertas activas
            _activeAlerts.RemoveAll(a => a.IsExpired);
        }
        
        private void ProcessLowStockAlert(Message message)
        {
            int productId = message.GetContent<int>("product_id");
            string productName = message.GetContent<string>("product_name", "Producto desconocido");
            int currentQuantity = message.GetContent<int>("current_quantity");
            int minimumStock = message.GetContent<int>("minimum_stock");
            
            // Verificar si ya existe una alerta activa para este producto
            var existingAlert = _activeAlerts.FirstOrDefault(a => 
                a.ProductId == productId && 
                a.Type == AlertType.LowStock && 
                !a.IsResolved);
                
            if (existingAlert != null)
            {
                // Actualizar la alerta existente
                existingAlert.Message = $"Stock bajo para {productName}: {currentQuantity} unidades (mínimo: {minimumStock})";
                existingAlert.UpdatedAt = DateTime.Now;
                _alertRepository.Update(existingAlert);
                
                return;
            }
            
            // Crear una nueva alerta
            var alert = new Alert
            {
                Type = AlertType.LowStock,
                Severity = AlertSeverity.Warning,
                Message = $"Stock bajo para {productName}: {currentQuantity} unidades (mínimo: {minimumStock})",
                ProductId = productId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsResolved = false,
                IsExpired = false
            };
            
            _alertRepository.Add(alert);
            _activeAlerts.Add(alert);
            
            // Notificar a otros agentes o sistemas sobre la nueva alerta
            NotifyNewAlert(alert);
        }
        
        private void ProcessGetActiveAlerts(Message message)
        {
            var response = message.CreateResponse();
            
            // Filtrar por tipo si se especifica
            string alertTypeStr = message.GetContent<string>("alert_type", null);
            AlertType? alertType = null;
            
            if (!string.IsNullOrEmpty(alertTypeStr) && Enum.TryParse<AlertType>(alertTypeStr, out var parsedType))
            {
                alertType = parsedType;
            }
            
            var filteredAlerts = _activeAlerts
                .Where(a => !a.IsResolved && !a.IsExpired)
                .Where(a => alertType == null || a.Type == alertType.Value)
                .ToList();
                
            response.AddContent("alerts", filteredAlerts);
            response.AddContent("count", filteredAlerts.Count);
            
            SendMessage(response);
        }
        
        private void ProcessDismissAlert(Message message)
        {
            int alertId = message.GetContent<int>("alert_id");
            var response = message.CreateResponse();
            
            var alert = _activeAlerts.FirstOrDefault(a => a.Id == alertId);
            
            if (alert != null)
            {
                alert.IsResolved = true;
                alert.ResolvedAt = DateTime.Now;
                alert.UpdatedAt = DateTime.Now;
                
                _alertRepository.Update(alert);
                _activeAlerts.Remove(alert);
                
                response.AddContent("success", true);
                response.AddContent("alert_id", alertId);
            }
            else
            {
                response.AddContent("success", false);
                response.AddContent("error", $"No se encontró una alerta activa con ID {alertId}");
            }
            
            SendMessage(response);
        }
        
        private void ProcessCreateAlert(Message message)
        {
            string alertTypeStr = message.GetContent<string>("alert_type", "Other");
            string severityStr = message.GetContent<string>("severity", "Info");
            string alertMessage = message.GetContent<string>("message", "");
            int? productId = message.GetContent<int?>("product_id", null);
            
            if (string.IsNullOrEmpty(alertMessage))
            {
                var response = message.CreateResponse();
                response.AddContent("success", false);
                response.AddContent("error", "El mensaje de alerta no puede estar vacío");
                SendMessage(response);
                return;
            }
            
            // Parsear tipo y severidad
            AlertType alertType;
            AlertSeverity severity;
            
            if (!Enum.TryParse<AlertType>(alertTypeStr, out alertType))
            {
                alertType = AlertType.Other;
            }
            
            if (!Enum.TryParse<AlertSeverity>(severityStr, out severity))
            {
                severity = AlertSeverity.Info;
            }
            
            // Crear la alerta
            var alert = new Alert
            {
                Type = alertType,
                Severity = severity,
                Message = alertMessage,
                ProductId = productId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsResolved = false,
                IsExpired = false
            };
            
            _alertRepository.Add(alert);
            _activeAlerts.Add(alert);
            
            // Notificar a otros agentes
            NotifyNewAlert(alert);
            
            // Responder al remitente
            var response = message.CreateResponse();
            response.AddContent("success", true);
            response.AddContent("alert_id", alert.Id);
            SendMessage(response);
        }
        
        private void NotifyNewAlert(Alert alert)
        {
            // Enviar mensaje de notificación a la interfaz de usuario o a otros agentes
            var notificationMessage = new Message
            {
                ReceiverAgentId = "*", // Broadcast a todos los agentes
                Type = MessageType.Notification,
                Subject = "new_alert_notification"
            };
            
            notificationMessage.AddContent("alert_id", alert.Id);
            notificationMessage.AddContent("alert_type", alert.Type.ToString());
            notificationMessage.AddContent("alert_severity", alert.Severity.ToString());
            notificationMessage.AddContent("alert_message", alert.Message);
            notificationMessage.AddContent("product_id", alert.ProductId);
            
            SendMessage(notificationMessage);
        }
    }
}