using System.Collections.Generic;

namespace ScadaGateway.UI.Dialogs
{
    public interface IChannelConfigDialog
    {
        bool? ShowDialog(); // open window
        Dictionary<string, string> GetConfig();
        string ChannelName { get; }
        string Protocol { get; }
        bool Enabled { get; }
    }
}
