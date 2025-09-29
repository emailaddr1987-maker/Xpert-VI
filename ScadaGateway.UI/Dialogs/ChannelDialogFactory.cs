using System;

namespace ScadaGateway.UI.Dialogs
{
    public static class ChannelDialogFactory
    {
        public static IChannelConfigDialog? Create(string protocol)
        {
            return protocol switch
            {
                "ModbusMaster" => new ModbusChannelDialog(),
                // add cases later for other protocols:
                // "ModbusSlave" => new ModbusSlaveDialog(),
                // "IEC104Master" => new IEC104Dialog(),
                _ => null
            };
        }
    }
}
