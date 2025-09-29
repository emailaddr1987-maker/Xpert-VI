using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Reflection;
using ScadaGateway.Core.Contracts;

namespace ScadaGateway.Core.Services
{
    public class DriverManager
    {
        private readonly Dictionary<string, IChannelDriver> _drivers = new();

        public void LoadDriversFromFolder(string folder)
        {
            if (!Directory.Exists(folder)) return;
            foreach (var dll in Directory.GetFiles(folder, "*.dll"))
            {
                try
                {
                    var asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(dll));
                    var types = asm.GetTypes().Where(t => typeof(IChannelDriver).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);
                    foreach (var t in types)
                    {
                        var inst = (IChannelDriver?)Activator.CreateInstance(t);
                        if (inst != null) _drivers[inst.Protocol] = inst;
                    }
                }
                catch { /* ignore bad assemblies */ }
            }
        }

        // allow registration manually (useful in dev)
        public void RegisterDriver(IChannelDriver driver) => _drivers[driver.Protocol] = driver;
        public IChannelDriver? GetDriver(string protocol) => _drivers.TryGetValue(protocol, out var d) ? d : null;
        public IEnumerable<string> AvailableProtocols => _drivers.Keys;
    }
}
