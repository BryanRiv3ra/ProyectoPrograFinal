namespace Proyecto_Final.Utils
{
    public static class Constants
    {
        // Configuración de la aplicación
        public const string AppName = "Sistema de Control de Inventarios A2A";
        public const string AppVersion = "1.0.0";
        
        // Configuración de agentes
        public const int AgentUpdateIntervalMs = 5000; // 5 segundos
        
        // Umbrales de inventario
        public const int DefaultLowStockThreshold = 10;
        public const int DefaultMaxStockThreshold = 100;
        
        // Formatos de fecha
        public const string DateFormat = "yyyy-MM-dd";
        public const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        
        // Rutas de archivos
        public const string ReportFolderPath = "reports";
        public const string LogFolderPath = "logs";
    }
}