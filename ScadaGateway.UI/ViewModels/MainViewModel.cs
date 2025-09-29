using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ScadaGateway.Core.Models;
using ScadaGateway.Core.Services;
using ScadaGateway.UI.Dialogs;
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
        }

        private async void OnAddChannel(string protocol)
        {
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
                Logs.Add(new LogEntry { Source = "UI", Content = "Add channel error: " + ex.Message });
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
                foreach (var p in dv.Points) SelectedPoints.Add(p);
            }
            else if (selection is ChannelViewModel ch)
            {
                foreach (var d in ch.Devices)
                    foreach (var p in d.Points) SelectedPoints.Add(p);
            }
            else if (selection is PointViewModel pv)
            {
                SelectedPoints.Add(pv);
            }
        }
    }
}
