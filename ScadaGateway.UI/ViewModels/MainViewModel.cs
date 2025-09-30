using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ScadaGateway.Core.Models;
using ScadaGateway.Core.Services;
using ScadaGateway.UI.Dialogs;
using ScadaGateway.UI.ViewModels.Dialogs;
using ScadaGateway.UI.Views;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace ScadaGateway.UI.ViewModels
{
    public class LogEntry
    {
        public DateTime Time { get; set; } = DateTime.Now;
        public string Source { get; set; } = "";
        public string Content { get; set; } = "";
    }

    public partial class MainViewModel : ObservableObject
    {
        public ObservableCollection<ChannelViewModel> Channels { get; } = new();
        public ObservableCollection<PointViewModel> SelectedPoints { get; } = new();
        public ObservableCollection<object> SelectedMappings { get; } = new();
        public ObservableCollection<LogEntry> Logs { get; } = new();

        private readonly DriverManager _driverManager;
        private readonly MappingEngine _mapping;
        private readonly GatewayService _gateway;
        private readonly PersistenceService _persistence;

        public IRelayCommand<string> AddChannelCommand { get; }
        public IRelayCommand NewExternalChannelCommand { get; }
        public IRelayCommand SaveProjectCommand { get; }
        public IRelayCommand LoadProjectCommand { get; }
        public IRelayCommand ClearLogsCommand { get; }
        public IRelayCommand ExitCommand { get; }
        public IRelayCommand<ChannelViewModel> AddDeviceCommand { get; }
        public IRelayCommand<ChannelViewModel> EditChannelCommand { get; }
        public IRelayCommand<ChannelViewModel> DeleteChannelCommand { get; }
        public MainViewModel()
        {
            _driverManager = new DriverManager();
            var driversFolder = Path.Combine(AppContext.BaseDirectory, "drivers");
            _driverManager.LoadDriversFromFolder(driversFolder);

            _mapping = new MappingEngine();
            _gateway = new GatewayService(_driverManager, _mapping);
            _persistence = new PersistenceService();

            AddChannelCommand = new RelayCommand<string>(OnAddChannel);
            NewExternalChannelCommand = new RelayCommand(() => OnAddChannel("Mock"));
            SaveProjectCommand = new RelayCommand(OnSaveProject);
            LoadProjectCommand = new RelayCommand(OnLoadProject);
            ClearLogsCommand = new RelayCommand(() => Logs.Clear());
            ExitCommand = new RelayCommand(() => Application.Current.Shutdown());

            AddDeviceCommand = new RelayCommand<ChannelViewModel>(OnAddDevice);
            EditChannelCommand = new RelayCommand<ChannelViewModel>(OnEditChannel);
            DeleteChannelCommand = new RelayCommand<ChannelViewModel>(OnDeleteChannel);
        }
        private void OnDeleteChannel(ChannelViewModel? channelVm)
        {
            if (channelVm == null) return;

            if (MessageBox.Show($"Delete channel {channelVm.DisplayName}?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Channels.Remove(channelVm);
                _gateway.Channels.Remove(channelVm.Model);

                Logs.Add(new LogEntry { Source = "UI", Content = $"Deleted Channel {channelVm.DisplayName}" });
            }
        }
        private void OnEditChannel(ChannelViewModel ch)
        {
            try
            {
                if (ch == null) return;

                var dlg = new ModbusChannelDialog(ch.Model);
                if (dlg.ShowDialog() == true)
                {
                    ch.Model.Name = dlg.ChannelName;
                    ch.Model.Enabled = dlg.Enabled;

                    ch.Model.Config.Clear();
                    foreach (var kv in dlg.GetConfig())
                        ch.Model.Config[kv.Key] = kv.Value;

                    ch.RefreshDisplayName();
                    Logs.Add(new LogEntry { Source = "UI", Content = $"Channel {ch.Model.Name} updated." });
                }
            }
            catch (Exception ex)
            {
                Logs.Add(new LogEntry { Source = "UI", Content = "Edit channel error: " + ex.Message });
            }
        }
        private void OnAddDevice(ChannelViewModel? channelVm)
        {
            if (channelVm == null) return;

            var win = new AddDeviceWindow { Owner = Application.Current.MainWindow };
            if (win.ShowDialog() == true)
            {
                var dev = win.Device;

                // tạo 4 nhóm DataType
                dev.DataTypeGroups.Add(new DataTypeGroup { Name = "T1 - Coil", Function = "Coil" });
                dev.DataTypeGroups.Add(new DataTypeGroup { Name = "T2 - Discrete Input", Function = "Discrete" });
                dev.DataTypeGroups.Add(new DataTypeGroup { Name = "T3 - Input Register", Function = "InputRegister" });
                dev.DataTypeGroups.Add(new DataTypeGroup { Name = "T4 - Holding Register", Function = "HoldingRegister" });

                channelVm.Model.Devices.Add(dev);
                channelVm.Devices.Add(new DeviceViewModel(dev));

                Logs.Add(new LogEntry { Source = "UI", Content = $"Added Device {dev.Name} to Channel {channelVm.DisplayName}" });
            }
        }
        private async void OnAddChannel(string? protocol)
        {
            if (string.IsNullOrEmpty(protocol)) return;
            try
            {
                var dlg = ChannelDialogFactory.Create(protocol);
                if (dlg == null)
                {
                    // nếu dialog không có — fallback: tạo channel ngay
                    var id = Guid.NewGuid().ToString("N").Substring(0, 8);
                    var chFallback = _gateway.CreateChannel(protocol, id, protocol, null);
                    Channels.Add(new ChannelViewModel(chFallback));
                    await _gateway.StartChannelAsync(chFallback);
                    Logs.Add(new LogEntry { Source = "UI", Content = $"Channel {chFallback.Name} ({chFallback.Protocol}) added & started." });
                    return;
                }

                // show dialog (owner is main window; we open modal)
                var result = dlg.ShowDialog();
                if (result != true) return; // user cancel

                var cfg = dlg.GetConfig();
                var name = dlg.ChannelName ?? protocol;
                var id2 = Guid.NewGuid().ToString("N").Substring(0, 8);
                var ch = _gateway.CreateChannel(protocol, id2, name, cfg);
                Channels.Add(new ChannelViewModel(ch));
                await _gateway.StartChannelAsync(ch);

                Logs.Add(new LogEntry { Source = "UI", Content = $"Channel {ch.Name} ({ch.Protocol}) added & started." });
            }
            catch (Exception ex)
            {
                Logs.Add(new LogEntry { Source = "UI", Content = "Add channel error: " + ex.ToString() });
                //Logs.Add(new LogEntry { Source = "UI", Content = "Add channel error: " + ex.Message });
            }
        }


        private void OnSaveProject()
        {
            try
            {
                var dlg = new SaveFileDialog
                {
                    Title = "Save Project",
                    Filter = "Project Files (*.xml)|*.xml",
                    FileName = "project.xml"
                };
                if (dlg.ShowDialog() == true)
                {
                    var dto = PersistenceService.ToDto(_gateway.Channels, _gateway.Mappings);
                    _persistence.SaveProject(dto, dlg.FileName);
                    Logs.Add(new LogEntry { Source = "UI", Content = $"Saved project to {dlg.FileName}" });
                }
            }
            catch (Exception ex)
            {
                Logs.Add(new LogEntry { Source = "UI", Content = "Save error: " + ex.Message });
            }
        }

        private void OnLoadProject()
        {
            try
            {
                var dlg = new OpenFileDialog
                {
                    Title = "Open Project",
                    Filter = "Project Files (*.xml)|*.xml"
                };
                if (dlg.ShowDialog() == true && File.Exists(dlg.FileName))
                {
                    var dto = _persistence.LoadProject(dlg.FileName);
                    var (chs, maps) = PersistenceService.FromDto(dto);

                    Channels.Clear();
                    _gateway.Channels.Clear();
                    _mapping.Clear();

                    foreach (var ch in chs)
                    {
                        Channels.Add(new ChannelViewModel(ch));
                        _gateway.Channels.Add(ch);
                    }
                    foreach (var m in maps)
                        _mapping.RegisterMapping(m, _gateway.LookupPoint, _gateway.WritePoint);

                    Logs.Add(new LogEntry
                    {
                        Source = "UI",
                        Content = $"Loaded project from {dlg.FileName}, channels={chs.Count}"
                    });
                }
            }
            catch (Exception ex)
            {
                Logs.Add(new LogEntry { Source = "UI", Content = "Load error: " + ex.Message });
            }
        }

        public void OnSelectedTreeItemChanged(object? selection)
        {
            SelectedPoints.Clear();

            if (selection is DeviceViewModel dv)
            {
                foreach (var g in dv.DataTypeGroups)
                    foreach (var p in g.Points)
                        SelectedPoints.Add(p);
            }
            else if (selection is ChannelViewModel ch)
            {
                foreach (var d in ch.Devices)
                    foreach (var g in d.DataTypeGroups)
                        foreach (var p in g.Points)
                            SelectedPoints.Add(p);
            }
            else if (selection is PointViewModel pv)
            {
                SelectedPoints.Add(pv);
            }
        }
    }
}
