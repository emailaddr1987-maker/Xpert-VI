using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ScadaGateway.Core.Models;
using ScadaGateway.Core.Dto;
using ScadaGateway.Core.Services;
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

        public IRelayCommand<string> AddChannelCommand { get; }
        public IRelayCommand NewExternalChannelCommand { get; }
        public IRelayCommand SaveProjectCommand { get; }
        public IRelayCommand LoadProjectCommand { get; }
        public IRelayCommand ClearLogsCommand { get; }
        public IRelayCommand ExitCommand { get; }

        public MainViewModel()
        {
            _driverManager = new DriverManager();

            // load drivers folder relative to exe
            var driversFolder = Path.Combine(AppContext.BaseDirectory, "drivers");
            _driverManager.LoadDriversFromFolder(driversFolder);

            _mapping = new MappingEngine();
            _gateway = new GatewayService(_driverManager, _mapping);

            AddChannelCommand = new RelayCommand<string>(OnAddChannel);
            NewExternalChannelCommand = new RelayCommand(() => OnAddChannel("Mock"));
            SaveProjectCommand = new RelayCommand(OnSaveProject);
            LoadProjectCommand = new RelayCommand(OnLoadProject);
            ClearLogsCommand = new RelayCommand(() => Logs.Clear());
            ExitCommand = new RelayCommand(() => Application.Current.Shutdown());

            // auto load project.xml if exists
            var proj = Path.Combine(AppContext.BaseDirectory, "project.xml");
            if (File.Exists(proj))
            {
                try
                {
                    var dto = ProjectService.Load(proj);
                    LoadFromDto(dto);
                    Logs.Add(new LogEntry { Source = "Main", Content = $"Loaded project.xml, channels={dto.Channels.Count}" });
                }
                catch (Exception ex)
                {
                    Logs.Add(new LogEntry { Source = "Main", Content = "Load error: " + ex.Message });
                }
            }
        }

        private async void OnAddChannel(string? protocol)
        {
            if (string.IsNullOrWhiteSpace(protocol)) return;
            try
            {
                var id = Guid.NewGuid().ToString("N").Substring(0, 8);
                var ch = _gateway.CreateChannel(protocol, id, protocol, new System.Collections.Generic.Dictionary<string, string>());
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
                var dto = new ProjectDto();
                foreach (var chVm in Channels)
                {
                    var chDto = new ChannelDto { Name = chVm.DisplayName, Protocol = chVm.Protocol };
                    foreach (var devVm in chVm.Devices)
                    {
                        var devDto = new DeviceDto { Name = devVm.Name };
                        foreach (var ptVm in devVm.Points)
                        {
                            devDto.Points.Add(new PointDto
                            {
                                Name = ptVm.Name,
                                DataType = ptVm.Type
                            });
                        }
                        chDto.Devices.Add(devDto);
                    }
                    dto.Channels.Add(chDto);
                }

                var dialog = new SaveFileDialog
                {
                    Filter = "XML files (*.xml)|*.xml",
                    FileName = "project.xml"
                };
                if (dialog.ShowDialog() == true)
                {
                    ProjectService.Save(dto, dialog.FileName);
                    Logs.Add(new LogEntry { Source = "UI", Content = $"Saved project to {dialog.FileName}" });
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
                var dialog = new OpenFileDialog { Filter = "XML files (*.xml)|*.xml" };
                if (dialog.ShowDialog() == true)
                {
                    var dto = ProjectService.Load(dialog.FileName);
                    LoadFromDto(dto);
                    Logs.Add(new LogEntry { Source = "UI", Content = $"Project loaded from {dialog.FileName}" });
                }
            }
            catch (Exception ex)
            {
                Logs.Add(new LogEntry { Source = "UI", Content = "Load error: " + ex.Message });
            }
        }

        private void LoadFromDto(ProjectDto dto)
        {
            Channels.Clear();
            foreach (var chDto in dto.Channels)
            {
                var chVm = new ChannelViewModel
                {
                    Name = chDto.Name,
                    Protocol = chDto.Protocol,
                    Enabled = true
                };

                foreach (var devDto in chDto.Devices)
                {
                    var devVm = new DeviceViewModel { Name = devDto.Name };
                    foreach (var ptDto in devDto.Points)
                    {
                        devVm.Points.Add(new PointViewModel
                        {
                            Name = ptDto.Name,
                            Type = ptDto.DataType
                        });
                    }
                    chVm.Devices.Add(devVm);
                }
                Channels.Add(chVm);
            }
        }

        // called from code-behind when selection in TreeView changes
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
