using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Zomo.Core
{
    /*
     * A simple class that you construct with a file path.
     * You can store and retrieve vars from. 
     */
    public class Config : IDisposable
    {
        private Dictionary<string, object> _data = new Dictionary<string, object>();
        private string _configFilePath;
        
        public Config(string filePath)
        {
            _configFilePath = filePath;
            Load();
        }

        public bool HasVar(string name)
        {
            return _data.ContainsKey(name);
        }
        
        public void Store(string name, object value)
        {
            _data[name] = value;
        }

        public T Get<T>(string name)
        {
            return (T)_data[name];
        }

        public void Save()
        {
            using (var textWriter = File.CreateText(_configFilePath))
            {
                textWriter.Write(JsonConvert.SerializeObject(_data));
            }
        }

        public bool Load()
        {
            if (File.Exists(_configFilePath))
            {
                _data = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(_configFilePath));
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            Save();
        }
    }
}