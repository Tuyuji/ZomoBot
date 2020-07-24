using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Zomo.Core.Common
{
    /*
     * A simple class that you construct with a file path.
     * You can store and retrieve vars from. 
     */
    public class Config : IDisposable
    {
        private dynamic _data;
        private readonly string _configFilePath;
        
        public Config(string filePath)
        {
            _configFilePath = filePath;
            Load();
        }

        public bool HasVar(string name)
        {
            return _data[name] != null;
        }
        
        public void Store(string name, object value)
        {
            _data[name] = JToken.FromObject(value);
        }

        public T Get<T>(string name)
        {
            return _data[name].ToObject<T>();
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
                _data = JsonConvert.DeserializeObject(File.ReadAllText(_configFilePath));
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