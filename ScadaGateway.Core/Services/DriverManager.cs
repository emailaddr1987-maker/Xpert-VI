using ScadaGateway.Core.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace ScadaGateway.Core.Services
{
    public class DriverManager
    {
        private readonly Dictionary<string, IChannelDriver> _drivers = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Load driver DLLs from a folder (scan for types implementing IChannelDriver).</summary>
        public void LoadDriversFromFolder(string folder)
        {
            if (!Directory.Exists(folder)) return;

            foreach (var dll in Directory.GetFiles(folder, "*.dll"))
            {
                try
                {
                    var asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(dll));
                    var types = asm.GetTypes().Where(t => typeof(IChannelDriver).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
                    foreach (var t in types)
                    {
                        var inst = (IChannelDriver?)Activator.CreateInstance(t);
                        if (inst != null)
                        {
                            _drivers[inst.Protocol] = inst;
                        }
                    }
                }
                catch
                {
                    // ignore bad assemblies; optionally log
                }
            }
        }

        /// <summary>Register driver programmatically (useful in dev).</summary>
        public void RegisterDriver(IChannelDriver driver) => _drivers[driver.Protocol] = driver;

        public IChannelDriver? GetDriver(string protocol) => _drivers.TryGetValue(protocol, out var d) ? d : null;

        public IEnumerable<string> AvailableProtocols => _drivers.Keys;
    }
}
