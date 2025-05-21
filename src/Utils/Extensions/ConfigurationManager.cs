using System;
using System.Configuration;

namespace Proyecto_Final.Utils.Extensions
{
    public static class ConfigurationManagerExtensions
    {
        public static string GetSetting(string key, string defaultValue = "")
        {
            string value = ConfigurationManager.AppSettings[key];
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }
        
        public static int GetSettingInt(string key, int defaultValue = 0)
        {
            string value = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrEmpty(value))
                return defaultValue;
                
            if (int.TryParse(value, out int result))
                return result;
                
            return defaultValue;
        }
        
        public static bool GetSettingBool(string key, bool defaultValue = false)
        {
            string value = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrEmpty(value))
                return defaultValue;
                
            if (bool.TryParse(value, out bool result))
                return result;
                
            return defaultValue;
        }
        
        public static void SaveSetting(string key, string value)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings[key].Value = value;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}