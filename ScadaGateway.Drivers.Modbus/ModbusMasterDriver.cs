// ScadaGateway.Drivers.Modbus/ModbusMasterDriver.cs
using ScadaGateway.Core.Contracts;
using ScadaGateway.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ScadaGateway.Drivers.Modbus
{
    public class ModbusMasterDriver : IChannelDriver
    {
        public string Protocol => "ModbusMaster";

        class RuntimeConnection
        {
            public TcpClient? Tcp { get; set; }
            public SerialPort? Serial { get; set; }
            public CancellationTokenSource? Cts { get; set; }
            public Task? Worker { get; set; }
            public object? MasterObject { get; set; } // nếu tích hợp NModbus, lưu instance master ở đây
        }

        // lưu runtime connection theo channel.Id
        private readonly ConcurrentDictionary<string, RuntimeConnection> _runtime = new();

        public Channel CreateChannel(string id, string name, IDictionary<string, string>? config = null)
        {
            var ch = new Channel
            {
                Id = id,
                Name = name,
                Protocol = Protocol,
                Enabled = true
            };

            // copy config (Channel.Config là Dictionary<string,string>)
            if (config != null)
            {
                foreach (var kv in config)
                    ch.Config[kv.Key] = kv.Value;
            }

            // (tùy) có thể khởi tạo 1 device/point mặc định hoặc để mapping/scan tạo sau
            return ch;
        }

        public Task<IEnumerable<Point>> DiscoverPointsAsync(Channel channel, CancellationToken ct)
        {
            // default: trả về các point đã có sẵn trên model
            var pts = new List<Point>();
            foreach (var d in channel.Devices)
                pts.AddRange(d.Points);
            return Task.FromResult<IEnumerable<Point>>(pts);
        }

        public Task<object?> ReadPointAsync(Channel channel, Point point, CancellationToken ct)
        {
            // TODO: nếu tích hợp NModbus, đọc register/coil ở đây theo point.Meta["FunctionCode"] và ["Address"]...
            // Hiện tại trả về value model (không thực hiện Modbus read)
            return Task.FromResult<object?>(point.Value);
        }

        public Task<bool> WritePointAsync(Channel channel, Point point, object? value, CancellationToken ct)
        {
            // TODO: khi tích hợp NModbus, viết giá trị ra thiết bị ở đây
            try
            {
                point.SetValue(value);
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public Task StartAsync(Channel channel, CancellationToken ct)
        {
            var cfg = channel.Config;
            // connection type logic: ưu tiên ConnectionType config, fallback ModbusType/TCP
            var connType = cfg.ContainsKey("ConnectionType") ? cfg["ConnectionType"] : cfg.GetValueOrDefault("ModbusType", "TCP");

            var run = new RuntimeConnection { Cts = CancellationTokenSource.CreateLinkedTokenSource(ct) };
            _runtime[channel.Id] = run;

            if (string.Equals(connType, "Serial", StringComparison.OrdinalIgnoreCase))
            {
                // serial params (sử dụng defaults nếu không có)
                var portName = cfg.GetValueOrDefault("ComPort", "COM1");
                var baud = int.TryParse(cfg.GetValueOrDefault("BaudRate", "9600"), out var b) ? b : 9600;
                var dataBits = int.TryParse(cfg.GetValueOrDefault("DataBits", "8"), out var db) ? db : 8;
                var parity = Parity.None;
                Enum.TryParse(cfg.GetValueOrDefault("Parity", "None"), true, out parity);
                var stopBits = StopBits.One;
                Enum.TryParse(cfg.GetValueOrDefault("StopBits", "One"), true, out stopBits);

                var sp = new SerialPort(portName, baud, parity, dataBits, stopBits);
                sp.ReadTimeout = 1000;
                sp.WriteTimeout = 1000;
                sp.Open();
                run.Serial = sp;

                // background worker placeholder (nếu muốn poll điểm định kỳ)
                run.Worker = Task.Run(async () =>
                {
                    while (!run.Cts!.IsCancellationRequested)
                    {
                        // TODO: nếu muốn poll registers, thực hiện ở đây (dùng NModbus master)
                        await Task.Delay(1000, run.Cts.Token).ContinueWith(_ => { });
                    }
                }, run.Cts.Token);
            }
            else
            {
                // TCP client
                var server = cfg.GetValueOrDefault("ServerAddress", "127.0.0.1");
                var port = int.TryParse(cfg.GetValueOrDefault("ClientPort", cfg.GetValueOrDefault("Port", "502")), out var p) ? p : 502;
                var tcp = new TcpClient();

                run.Worker = Task.Run(async () =>
                {
                    try
                    {
                        await tcp.ConnectAsync(server, port);
                        run.Tcp = tcp;
                        // TODO: tạo Modbus IP master nếu dùng NModbus và lưu vào run.MasterObject
                        while (!run.Cts!.IsCancellationRequested)
                        {
                            await Task.Delay(1000, run.Cts.Token).ContinueWith(_ => { });
                        }
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception) { /* ignore/connect errors */ }
                    finally
                    {
                        try { tcp.Close(); } catch { }
                    }
                }, run.Cts.Token);
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(Channel channel)
        {
            if (_runtime.TryRemove(channel.Id, out var run))
            {
                try
                {
                    run.Cts?.Cancel();
                    run.Worker?.Wait(500);
                }
                catch { }

                try { run.Tcp?.Close(); } catch { }
                try { if (run.Serial?.IsOpen == true) run.Serial?.Close(); } catch { }
                run.MasterObject = null;
            }
            return Task.CompletedTask;
        }
    }
}
