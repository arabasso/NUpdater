using System;
using System.Reflection;
using Microsoft.Win32;

namespace NUpdater
{
    public class RegistryConfiguration : IDisposable
    {
        private readonly RegistryKey _software;
        private RegistryKey _key;

        public RegistryConfiguration()
        {
            var app = Assembly.GetExecutingAssembly().GetName().Name;

            _software = Registry.CurrentUser.OpenSubKey("Software", true);

            _key = _software?.CreateSubKey(app);
        }
        public void Dispose()
        {
            _software?.Close();
        }

        private T GetValue<T>(string name)
        {
            return (T)Convert.ChangeType(_key.GetValue(name, default(T)), typeof(T));
        }

        public bool StartMinimized
        {
            get
            {
                return GetValue<bool>(nameof(StartMinimized));
            }
            set
            {
                _key.SetValue(nameof(StartMinimized), value);
            }
        }
    }
}
