namespace ScadaGateway.Core.Models
{
    public class GatewaySetting
    {
        public string GatewayName { get; set; } = "XpertGateway";
        public int DefaultPollingInterval { get; set; } = 1000; // ms
        public bool AutoStart { get; set; } = true;
    }
}
