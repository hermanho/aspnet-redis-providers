﻿using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Web.Configuration;
using System.Web.Hosting;

namespace Microsoft.Web.Redis
{
    internal class ProviderConfiguration
    {
        public TimeSpan RequestTimeout { get; set; }
        public TimeSpan SessionTimeout { get; set; }
        public int Port { get; set; }
        public string Host { get; set; }
        public string AccessKey { get; set; }
        public TimeSpan RetryTimeout { get; set; }
        public bool ThrowOnError { get; set; }
        public bool UseSsl { get; set; }
        public int DatabaseId { get; set; }
        public string ApplicationName { get; set; }
        public int ConnectionTimeoutInMilliSec { get; set; }
        public int OperationTimeoutInMilliSec { get; set; }
        public string ConnectionString { get; set; }

        /* Empty constructor required for testing */
        internal ProviderConfiguration()
        {}

        internal static ProviderConfiguration ProviderConfigurationForSessionState(NameValueCollection config)
        {
            ProviderConfiguration configuration = new ProviderConfiguration(config);
            
            configuration.ThrowOnError = GetBoolSettings(config, "throwOnError", true);
            int retryTimeoutInMilliSec = GetIntSettings(config, "retryTimeoutInMilliseconds", 5000);
            configuration.RetryTimeout = new TimeSpan(0, 0, 0, 0, retryTimeoutInMilliSec);
            
            // Get request timeout from config
            HttpRuntimeSection httpRuntimeSection = ConfigurationManager.GetSection("system.web/httpRuntime") as HttpRuntimeSection;
            configuration.RequestTimeout = httpRuntimeSection.ExecutionTimeout;

            // Get session timeout from config
            SessionStateSection sessionStateSection = (SessionStateSection)WebConfigurationManager.GetSection("system.web/sessionState");
            configuration.SessionTimeout = sessionStateSection.Timeout;

            LogUtility.LogInfo("Host: {0}, Port: {1}, ThrowOnError: {2}, UseSsl: {3}, RetryTimeout: {4}, DatabaseId: {5}, ApplicationName: {6}, RequestTimeout: {7}, SessionTimeout: {8}",
                                            configuration.Host, configuration.Port, configuration.ThrowOnError, configuration.UseSsl, configuration.RetryTimeout, configuration.DatabaseId, configuration.ApplicationName, configuration.RequestTimeout, configuration.SessionTimeout);
            return configuration;
        }

        internal static ProviderConfiguration ProviderConfigurationForOutputCache(NameValueCollection config)
        {
            ProviderConfiguration configuration = new ProviderConfiguration(config);
            
            // No retry login for output cache provider
            configuration.RetryTimeout = TimeSpan.Zero;
            
            // Session state specific attribute which are not applicable to output cache
            configuration.ThrowOnError = true;
            configuration.RequestTimeout = TimeSpan.Zero;
            configuration.SessionTimeout = TimeSpan.Zero;

            LogUtility.LogInfo("Host: {0}, Port: {1}, UseSsl: {2}, DatabaseId: {3}, ApplicationName: {4}",
                                            configuration.Host, configuration.Port, configuration.UseSsl, configuration.DatabaseId, configuration.ApplicationName);
            return configuration;
        }

        private ProviderConfiguration(NameValueCollection config)
        {
            // Get connection host, port and password.
            // host, port, accessKey and ssl are firest fetched from appSettings if not found there than taken from web.config
            ConnectionString = GetStringSettings(config, "connectionString", null);
            Host = GetStringSettings(config, "host", "127.0.0.1");
            Port = GetIntSettings(config, "port", 0);
            AccessKey = GetStringSettings(config, "accessKey", null);
            UseSsl = GetBoolSettings(config, "ssl", true);
            
            // All below parameters are only fetched from web.config
            DatabaseId = GetIntSettings(config, "databaseId", 0);
            ApplicationName = GetStringSettings(config, "applicationName", null);
            if (ApplicationName == null)
            {
                try
                {
                    ApplicationName = HostingEnvironment.ApplicationVirtualPath;
                    if (String.IsNullOrEmpty(ApplicationName))
                    {
                        ApplicationName = System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName;

                        int indexOfDot = ApplicationName.IndexOf('.');
                        if (indexOfDot != -1)
                        {
                            ApplicationName = ApplicationName.Remove(indexOfDot);
                        }
                    }

                    if (String.IsNullOrEmpty(ApplicationName))
                    {
                        ApplicationName = "/";
                    }

                }
                catch (Exception e)
                {
                    ApplicationName = "/";
                    LogUtility.LogInfo(e.Message);
                }
            }

            ConnectionTimeoutInMilliSec = GetIntSettings(config, "connectionTimeoutInMilliseconds", 0);
            OperationTimeoutInMilliSec = GetIntSettings(config, "operationTimeoutInMilliseconds", 0);
        }

        // 1) Use key available inside AppSettings
        // 2) Use literal value as given in config
        // 3) Both are null than use default value.
        private static string GetStringSettings(NameValueCollection config, string attrName, string defaultVal)
        {
            string literalValue = GetFromConfig(config, attrName);
            if (string.IsNullOrEmpty(literalValue))
            {
                return defaultVal;
            }

            string appSettingsValue = GetFromAppSetting(literalValue);
            if (!string.IsNullOrEmpty(appSettingsValue))
            {
                return appSettingsValue;
            }
            return literalValue;
        }

        // 1) Check if literal value is valid integer than use it as it is
        // 2) Use app setting value corrosponding to this string
        // 3) Both are null than use default value.
        private static int GetIntSettings(NameValueCollection config, string attrName, int defaultVal)
        {
            string literalValue = null;
            try
            {
                literalValue = GetFromConfig(config, attrName);
                if (literalValue == null)
                {
                    return defaultVal;
                }
                return int.Parse(literalValue);
            }
            catch (FormatException)
            {}

            string appSettingsValue = GetFromAppSetting(literalValue);
            if (appSettingsValue == null)
            {
                // This will blow up but gives right message to customer
                return int.Parse(literalValue);
            }
            return int.Parse(appSettingsValue);
        }

        // 1) Check if literal value is valid bool than use it as it is
        // 2) Use app setting value corrosponding to this string
        // 3) Both are null than use default value.
        private static bool GetBoolSettings(NameValueCollection config, string attrName, bool defaultVal)
        {
            string literalValue = null;
            try
            {
                literalValue = GetFromConfig(config, attrName);
                if (literalValue == null)
                {
                    return defaultVal;
                }
                return bool.Parse(literalValue);
            }
            catch (FormatException)
            { }

            string appSettingsValue = GetFromAppSetting(literalValue);
            if (appSettingsValue == null)
            {
                // This will blow up but gives right message to customer
                return bool.Parse(literalValue);
            }
            return bool.Parse(appSettingsValue);
        }

        // Reads value from app settings (mostly azure app settings)
        private static string GetFromAppSetting(string attrName)
        {
            if (!string.IsNullOrEmpty(attrName))
            {
                string paramFromAppSetting = ConfigurationManager.AppSettings[attrName];
                if (!string.IsNullOrEmpty(paramFromAppSetting))
                {
                    return paramFromAppSetting;
                }
            }
            return null;
        }

        // Reads string value from web.config session state section
        private static string GetFromConfig(NameValueCollection config, string attrName)
        {
            string[] attrValues = config.GetValues(attrName);
            if (attrValues != null && attrValues.Length > 0 && !string.IsNullOrEmpty(attrValues[0]))
            {
                return attrValues[0];
            }
            return null;
        }
    }
}