using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NzbDrone.Common;
using NzbDrone.Common.Cache;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Configuration.Events;

namespace NzbDrone.Core.Configuration
{
    public interface IConfigFileProvider
    {
        Dictionary<string, object> GetConfigDictionary();
        void SaveConfigDictionary(Dictionary<string, object> configValues);

        int Port { get; }
        bool LaunchBrowser { get; }
        bool AuthenticationEnabled { get; }
        string Username { get; }
        string Password { get; }
        string LogLevel { get; }
    }

    public class ConfigFileProvider : IConfigFileProvider
    {
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IMessageAggregator _messageAggregator;
        private readonly ICached<string> _cache;

        private readonly string _configFile;

        public ConfigFileProvider(IAppFolderInfo appFolderInfo, ICacheManger cacheManger, IMessageAggregator messageAggregator)
        {
            _appFolderInfo = appFolderInfo;
            _cache = cacheManger.GetCache<string>(GetType());
            _messageAggregator = messageAggregator;
            _configFile = _appFolderInfo.GetConfigPath();
        }

        public Dictionary<string, object> GetConfigDictionary()
        {
            var dict = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

            var type = GetType();
            var properties = type.GetProperties();

            foreach (var propertyInfo in properties)
            {
                var value = propertyInfo.GetValue(this, null);

                dict.Add(propertyInfo.Name, value);
            }

            return dict;
        }

        public void SaveConfigDictionary(Dictionary<string, object> configValues)
        {
            _cache.Clear();

            var allWithDefaults = GetConfigDictionary();

            foreach (var configValue in configValues)
            {
                object currentValue;
                allWithDefaults.TryGetValue(configValue.Key, out currentValue);
                if (currentValue == null) continue;

                var equal = configValue.Value.ToString().Equals(currentValue.ToString());

                if (!equal)
                {
                    SetValue(configValue.Key.FirstCharToUpper(), configValue.Value.ToString());
                }
            }

            _messageAggregator.PublishEvent(new ConfigFileSavedEvent());
        }

        public int Port
        {
            get { return GetValueInt("Port", 8989); }
        }

        public bool LaunchBrowser
        {
            get { return GetValueBoolean("LaunchBrowser", true); }
        }

        public bool AuthenticationEnabled
        {
            get { return GetValueBoolean("AuthenticationEnabled", false); }
        }

        public string Username
        {
            get { return GetValue("Username", ""); }
        }

        public string Password
        {
            get { return GetValue("Password", ""); }
        }

        public string LogLevel
        {
            get { return GetValue("LogLevel", "Info"); }
        }

        public int GetValueInt(string key, int defaultValue)
        {
            return Convert.ToInt32(GetValue(key, defaultValue));
        }

        public bool GetValueBoolean(string key, bool defaultValue)
        {
            return Convert.ToBoolean(GetValue(key, defaultValue));
        }

        public T GetValueEnum<T>(string key, T defaultValue)
        {
            return (T)Enum.Parse(typeof(T), GetValue(key, defaultValue), true);
        }

        public string GetValue(string key, object defaultValue)
        {
            return _cache.Get(key, () =>
                {
                    EnsureDefaultConfigFile();

                    var xDoc = XDocument.Load(_configFile);
                    var config = xDoc.Descendants("Config").Single();

                    var parentContainer = config;

                    var valueHolder = parentContainer.Descendants(key).ToList();

                    if (valueHolder.Count() == 1)
                        return valueHolder.First().Value;

                    //Save the value
                    SetValue(key, defaultValue);

                    //return the default value
                    return defaultValue.ToString();
                });
        }

        public void SetValue(string key, object value)
        {
            EnsureDefaultConfigFile();

            var xDoc = XDocument.Load(_configFile);
            var config = xDoc.Descendants("Config").Single();

            var parentContainer = config;

            var keyHolder = parentContainer.Descendants(key);

            if (keyHolder.Count() != 1)
            {
                parentContainer.Add(new XElement(key, value));
            }

            else
            {
                parentContainer.Descendants(key).Single().Value = value.ToString();
            }

            _cache.Set(key, value.ToString());

            xDoc.Save(_configFile);
        }

        public void SetValue(string key, Enum value)
        {
            SetValue(key, value.ToString().ToLower());
        }

        private void EnsureDefaultConfigFile()
        {
            if (!File.Exists(_configFile))
            {
                var xDoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
                xDoc.Add(new XElement("Config"));
                xDoc.Save(_configFile);
            }
        }
    }
}
