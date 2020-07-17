using NUnit.Framework;

namespace Zomo.Core.Tests
{
    public class ConfigTests
    {
        private Config _config;
        private const string ConfigFileLocation = "testconfig.txt";

        [SetUp]
        public void Setup()
        {
            _config = new Config(ConfigFileLocation);
        }

        /*
         * Does config correctly save?
         */
        [Test]
        public void Save_Works()
        {
            const string varName = "Save_Works";
            const string varValue = "Hello";

            _config.Store(varName, varValue);
            _config.Dispose();

            _config = new Config(ConfigFileLocation);
            string value = _config.Get<string>(varName);

            Assert.AreEqual(varValue, value);
        }

        /*
         * Does config correctly load?
         */
        [Test]
        public void Load_Works()
        {
            const string varName = "Load_Works";
            const string varValue = "Hello";
            
            _config.Store(varName, varValue);

            //This should wipe that value
            _config.Load();
            
            Assert.False(_config.HasVar(varName));
        }
    }
}