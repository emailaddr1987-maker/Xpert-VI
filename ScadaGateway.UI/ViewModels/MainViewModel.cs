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
using System.Linq;
using System.Windows;
using System.Collections.Specialized;
using System.Text.Json;


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
        private GatewaySetting _settings = new GatewaySetting();
        // thêm property để bật/tắt auto scroll
        public bool AutoScrollLogs { get; set; } = true;
        public IRelayCommand GatewaySettingCommand { get; }
        public IRelayCommand SaveLogsCommand { get; }
        public IRelayCommand ShowLicenceCommand { get; }
        public IRelayCommand ShowAboutCommand { get; }
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

            GatewaySettingCommand = new RelayCommand(OnGatewaySetting);
            SaveLogsCommand = new RelayCommand(OnSaveLogs);
            ShowLicenceCommand = new RelayCommand(OnShowLicence);
            ShowAboutCommand = new RelayCommand(OnShowAbout);

            Logs.CollectionChanged += Logs_CollectionChanged;
        }
        // Auto Scroll
        private void Logs_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (AutoScrollLogs && e.Action == NotifyCollectionChangedAction.Add)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // giả sử bạn đặt tên ListBox là LogListBox trong XAML
                    if (Application.Current.MainWindow is MainWindow main && main.LogListBox.Items.Count > 0)
                        main.LogListBox.ScrollIntoView(main.LogListBox.Items[^1]);
                });
            }
        }

        private void OnGatewaySetting()
        {
            var dlg = new GatewaySettingDialog { Owner = Application.Current.MainWindow };
            dlg.DataContext = _settings;
            if (dlg.ShowDialog() == true)
            {
                File.WriteAllText("gateway-settings.json", JsonSerializer.Serialize(_settings));
                Logs.Add(new LogEntry { Source = "UI", Content = "Gateway settings updated." });
            }
        }

        private void OnSaveLogs()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Save Logs",
                Filter = "Text Files (*.txt)|*.txt",
                FileName = $"logs_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
            };
            if (dlg.ShowDialog() == true)
            {
                File.WriteAllLines(dlg.FileName,
                    Logs.Select(l => $"[{l.Time:yyyy-MM-dd HH:mm:ss}] {l.Source}: {l.Content}"));
                Logs.Add(new LogEntry { Source = "UI", Content = $"Logs saved to {dlg.FileName}" });
            }
        }

        private void OnShowLicence()
        {
            var dlg = new LicenceWindow { Owner = Application.Current.MainWindow };
            dlg.ShowDialog();
        }

        private void OnShowAbout()
        {
            var dlg = new AboutWindow { Owner = Application.Current.MainWindow };
            dlg.ShowDialog();
        }
        // ---------- Channel CRUD ----------

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

        private void OnEditChannel(ChannelViewModel? ch)
        {
            try
            {
                if (ch == null) return;

                var dlg = new ModbusChannelDialog(ch.Model);
                if (dlg.ShowDialog() == true)
                {
                    var newName = (dlg.ChannelName ?? ch.Model.Name).Trim();

                    // kiểm tra tên kênh mới có trùng với kênh khác không
                    if (Channels.Any(c => !ReferenceEquals(c, ch) &&
                                          string.Equals(c.Model.Name, newName, StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show($"A channel named '{newName}' already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    ch.Model.Name = newName;
                    ch.Model.Enabled = dlg.Enabled;
                    // đồng bộ sang ViewModel
                    ch.Name = newName;
                    ch.Enabled = dlg.Enabled;

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
                var dev = win.Device ?? new Device();
                if (dev == null) return;
                // normalize name/id
                dev.Name = (dev.Name ?? "").Trim();
                if (string.IsNullOrEmpty(dev.Name))
                    dev.Name = $"Device{channelVm.Devices.Count + 1}";

                if (string.IsNullOrEmpty(dev.Id))
                    dev.Id = Guid.NewGuid().ToString("N").Substring(0, 8);

                // 1) kiểm tra trùng Name trong cùng channel
                if (channelVm.Devices.Any(d => string.Equals(d.Name, dev.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show($"A device named '{dev.Name}' already exists in channel '{channelVm.DisplayName}'.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 2) kiểm tra trùng Id trong cùng channel
                if (channelVm.Devices.Any(d => string.Equals(d.Model.Id, dev.Id, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show($"A device with Id '{dev.Id}' already exists in channel '{channelVm.DisplayName}'.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // nếu Device chưa có DataTypeGroups (ví dụ dialog trả về device trống), tạo 4 nhóm mặc định
               
                if (dev.DataTypeGroups == null || dev.DataTypeGroups.Count == 0)
                {
                    dev.DataTypeGroups.Add(new DataTypeGroup { Name = "T1 - Coil", Function = "Coil" });
                    dev.DataTypeGroups.Add(new DataTypeGroup { Name = "T2 - Discrete Input", Function = "Discrete" });
                    dev.DataTypeGroups.Add(new DataTypeGroup { Name = "T3 - Input Register", Function = "InputRegister" });
                    dev.DataTypeGroups.Add(new DataTypeGroup { Name = "T4 - Holding Register", Function = "HoldingRegister" });
                }
                // Thêm vào model và viewmodel
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
                var name = (dlg.ChannelName ?? protocol).Trim();

                // kiểm tra không cho phép trùng tên channel
                if (Channels.Any(c => string.Equals(c.Model.Name, name, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show($"A channel named '{name}' already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var id2 = Guid.NewGuid().ToString("N").Substring(0, 8);
                var ch = _gateway.CreateChannel(protocol, id2, name, cfg);

                // gán Enabled theo dialog
                try { ch.Enabled = dlg.Enabled; } catch { /* ignore if not supported */ }

                Channels.Add(new ChannelViewModel(ch));
                await _gateway.StartChannelAsync(ch);

                // refresh display name (ensure VM shows Enable/Disable)
                Channels.Last().RefreshDisplayName();

                Logs.Add(new LogEntry { Source = "UI", Content = $"Channel {ch.Name} ({ch.Protocol}) added & started." });
            }
            catch (Exception ex)
            {
                Logs.Add(new LogEntry { Source = "UI", Content = "Add channel error: " + ex.ToString() });
            }
        }

        // ---------- Save / Load ----------

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

        // Called from TreeView selection changed
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
