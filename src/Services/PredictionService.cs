using System;
using System.Collections.Generic;
using System.Linq;
using Proyecto_Final.Data.Models;

namespace Proyecto_Final.Services
{
    public class PredictionService
    {
        // Constantes para cálculos de predicción
        private const int DEFAULT_LEAD_TIME_DAYS = 7; // Tiempo promedio de entrega en días
        private const double SAFETY_STOCK_FACTOR = 1.5; // Factor para stock de seguridad
        
        // Historial de ventas simulado (en un sistema real, esto vendría de la base de datos)
        private Dictionary<int, List<SalesRecord>> _salesHistory;
        
        public PredictionService()
        {
            // Inicializar el historial de ventas simulado
            _salesHistory = new Dictionary<int, List<SalesRecord>>();
        }
        
        /// <summary>
        /// Calcula el punto de reorden y días hasta necesitar reordenar
        /// </summary>
        public ReorderPrediction CalculateReorderPoint(Inventory inventory, Product product)
        {
            // Obtener o generar el historial de ventas
            var salesHistory = GetSalesHistory(product.Id);
            
            // Calcular la demanda diaria promedio
            double averageDailyDemand = CalculateAverageDailyDemand(salesHistory);
            
            // Calcular el tiempo de entrega (lead time)
            int leadTimeDays = GetLeadTimeDays(product);
            
            // Calcular el punto de reorden = (demanda diaria promedio * tiempo de entrega) + stock de seguridad
            double safetyStock = averageDailyDemand * SAFETY_STOCK_FACTOR;
            int reorderPoint = (int)Math.Ceiling(averageDailyDemand * leadTimeDays + safetyStock);
            
            // Calcular días hasta necesitar reordenar
            int daysUntilReorder = 0;
            if (inventory.Quantity > reorderPoint)
            {
                daysUntilReorder = (int)Math.Floor((inventory.Quantity - reorderPoint) / averageDailyDemand);
            }
            
            // Calcular cantidad sugerida para ordenar
            int suggestedOrderQuantity = CalculateEconomicOrderQuantity(product, averageDailyDemand);
            
            return new ReorderPrediction
            {
                ReorderPoint = reorderPoint,
                DaysUntilReorder = daysUntilReorder,
                SuggestedOrderQuantity = suggestedOrderQuantity,
                AverageDailyDemand = averageDailyDemand
            };
        }
        
        /// <summary>
        /// Predice la demanda futura para un producto
        /// </summary>
        public DemandPrediction PredictDemand(Product product, int daysAhead)
        {
            // Obtener o generar el historial de ventas
            var salesHistory = GetSalesHistory(product.Id);
            
            // Calcular la demanda diaria promedio
            double averageDailyDemand = CalculateAverageDailyDemand(salesHistory);
            
            // Calcular la tendencia (crecimiento o decrecimiento)
            double trend = CalculateTrend(salesHistory);
            
            // Predecir la demanda futura considerando la tendencia
            double predictedDemand = averageDailyDemand * daysAhead * (1 + trend);
            
            // Calcular nivel de confianza (simulado)
            double confidenceLevel = CalculateConfidenceLevel(salesHistory);
            
            return new DemandPrediction
            {
                PredictedDemand = (int)Math.Ceiling(predictedDemand),
                ConfidenceLevel = confidenceLevel,
                Trend = trend
            };
        }
        
        #region Métodos auxiliares
        
        private List<SalesRecord> GetSalesHistory(int productId)
        {
            // En un sistema real, esto obtendría datos de la base de datos
            // Para este ejemplo, generamos datos simulados si no existen
            if (!_salesHistory.ContainsKey(productId))
            {
                GenerateSimulatedSalesHistory(productId);
            }
            
            return _salesHistory[productId];
        }
        
        private void GenerateSimulatedSalesHistory(int productId)
        {
            // Generar historial de ventas simulado para los últimos 90 días
            var random = new Random(productId); // Usar productId como semilla para consistencia
            var history = new List<SalesRecord>();
            
            DateTime startDate = DateTime.Now.AddDays(-90);
            
            for (int i = 0; i < 90; i++)
            {
                var date = startDate.AddDays(i);
                
                // Generar cantidad vendida con variación aleatoria
                int baseDemand = 5 + (productId % 10); // Demanda base varía según el producto
                int variation = random.Next(-3, 4); // Variación de -3 a +3
                
                // Añadir tendencia (crecimiento o decrecimiento)
                double trendFactor = 1.0 + (0.01 * (i / 30.0)); // Pequeño crecimiento con el tiempo
                
                // Añadir estacionalidad (más ventas los fines de semana)
                double seasonalFactor = (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) ? 1.3 : 1.0;
                
                int quantity = (int)Math.Max(0, Math.Round((baseDemand + variation) * trendFactor * seasonalFactor));
                
                history.Add(new SalesRecord
                {
                    Date = date,
                    ProductId = productId,
                    Quantity = quantity
                });
            }
            
            _salesHistory[productId] = history;
        }
        
        private double CalculateAverageDailyDemand(List<SalesRecord> salesHistory)
        {
            // Calcular la demanda diaria promedio de los últimos 30 días
            var recentSales = salesHistory.OrderByDescending(s => s.Date).Take(30).ToList();
            
            if (recentSales.Count == 0)
                return 1.0; // Valor predeterminado si no hay datos
                
            return recentSales.Average(s => s.Quantity);
        }
        
        private int GetLeadTimeDays(Product product)
        {
            // En un sistema real, esto podría variar según el proveedor
            // Para este ejemplo, usamos un valor constante
            return DEFAULT_LEAD_TIME_DAYS;
        }
        
        private double CalculateTrend(List<SalesRecord> salesHistory)
        {
            if (salesHistory.Count < 60)
                return 0.0;
                
            // Dividir el historial en dos períodos y comparar
            var firstPeriod = salesHistory.OrderBy(s => s.Date).Take(30).ToList();
            var secondPeriod = salesHistory.OrderByDescending(s => s.Date).Take(30).ToList();
            
            double firstAvg = firstPeriod.Average(s => s.Quantity);
            double secondAvg = secondPeriod.Average(s => s.Quantity);
            
            // Calcular el cambio porcentual
            if (firstAvg == 0)
                return 0.0;
                
            return (secondAvg - firstAvg) / firstAvg;
        }
        
        private double CalculateConfidenceLevel(List<SalesRecord> salesHistory)
        {
            // En un sistema real, esto se calcularía con métodos estadísticos
            // Para este ejemplo, simulamos un nivel de confianza basado en la cantidad de datos
            double baseConfidence = 0.7; // 70% base
            
            // Más datos = mayor confianza
            double dataFactor = Math.Min(1.0, salesHistory.Count / 90.0);
            
            // Menos variabilidad = mayor confianza
            var recentSales = salesHistory.OrderByDescending(s => s.Date).Take(30).ToList();
            double stdDev = CalculateStandardDeviation(recentSales.Select(s => (double)s.Quantity).ToList());
            double mean = recentSales.Average(s => s.Quantity);
            double cvFactor = (mean > 0) ? Math.Min(1.0, 1.0 / (stdDev / mean)) : 0.5;
            
            return Math.Min(0.95, baseConfidence * (0.5 + 0.5 * dataFactor) * (0.5 + 0.5 * cvFactor));
        }
        
        private double CalculateStandardDeviation(List<double> values)
        {
            double avg = values.Average();
            double sumOfSquaresOfDifferences = values.Select(val => (val - avg) * (val - avg)).Sum();
            double standardDeviation = Math.Sqrt(sumOfSquaresOfDifferences / values.Count);
            return standardDeviation;
        }
        
        private int CalculateEconomicOrderQuantity(Product product, double dailyDemand)
        {
            // Fórmula simplificada para la Cantidad Económica de Pedido (EOQ)
            // En un sistema real, esto consideraría costos de pedido y almacenamiento
            double annualDemand = dailyDemand * 365;
            double orderCost = 50; // Costo fijo por realizar un pedido (simulado)
            double holdingCost = product.Price * 0.2; // Costo de mantener inventario (20% del precio)
            
            double eoq = Math.Sqrt((2 * annualDemand * orderCost) / holdingCost);
            return (int)Math.Ceiling(eoq);
        }
        
        #endregion
    }
    
    // Clases para los resultados de predicción
    
    public class ReorderPrediction
    {
        public int ReorderPoint { get; set; }
        public int DaysUntilReorder { get; set; }
        public int SuggestedOrderQuantity { get; set; }
        public double AverageDailyDemand { get; set; }
    }
    
    public class DemandPrediction
    {
        public int PredictedDemand { get; set; }
        public double ConfidenceLevel { get; set; }
        public double Trend { get; set; }
    }
    
    // Clase para representar registros de ventas
    public class SalesRecord
    {
        public DateTime Date { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}