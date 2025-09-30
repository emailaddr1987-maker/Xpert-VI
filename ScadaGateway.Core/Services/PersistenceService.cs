using ScadaGateway.Core.Dto;
using ScadaGateway.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace ScadaGateway.Core.Services
{
    public class PersistenceService
    {
        public void SaveProject(ProjectDto project, string filePath)
        {
            var serializer = new XmlSerializer(typeof(ProjectDto));
            using var writer = new StreamWriter(filePath);
            serializer.Serialize(writer, project);
        }

        public ProjectDto LoadProject(string filePath)
        {
            var serializer = new XmlSerializer(typeof(ProjectDto));
            using var reader = new StreamReader(filePath);
            return (ProjectDto)serializer.Deserialize(reader)!;
        }

        public static ProjectDto ToDto(IEnumerable<Channel> channels, IEnumerable<Mapping> mappings)
        {
            return new ProjectDto
            {
                Name = "GatewayProject",
                Channels = channels.Select(ch => new ChannelDto
                {
                    Id = ch.Id,
                    Name = ch.Name,
                    Protocol = ch.Protocol,
                    Enabled = ch.Enabled,
                    Config = ch.Config.Select(kv => new KeyValuePairDto { Key = kv.Key, Value = kv.Value }).ToList(),
                    Devices = ch.Devices.Select(d => new DeviceDto
                    {
                        Id = d.Id,
                        Name = d.Name,
                        // ✅ Thêm DataTypeGroups
                        DataTypeGroups = d.DataTypeGroups.Select(g => new DataTypeGroupDto
                        {
                            Name = g.Name,
                            Function = g.Function,
                            Points = g.Points.Select(p => new PointDto
                            {
                                Id = p.Id,
                                Name = p.Name,
                                DataType = p.DataType.ToString(),
                                Meta = p.Meta.Select(m => new KeyValuePairDto { Key = m.Key, Value = m.Value }).ToList()
                            }).ToList()
                        }).ToList(),
                        // vẫn giữ lại Points nếu bạn dùng trực tiếp
                        Points = d.Points.Select(p => new PointDto
                        {
                            Id = p.Id,
                            Name = p.Name,
                            DataType = p.DataType.ToString(),
                            Meta = p.Meta.Select(m => new KeyValuePairDto { Key = m.Key, Value = m.Value }).ToList()
                        }).ToList()
                    }).ToList()
                }).ToList(),
                Mappings = mappings.Select(m => new MappingDto
                {
                    Id = m.Id,
                    SourcePointId = m.SourcePointId,
                    DestinationPointId = m.DestinationPointId,
                    TwoWay = m.TwoWay,
                    Interval = m.Interval,
                    Expression = m.Expression
                }).ToList()
            };
        }

        public static (List<Channel>, List<Mapping>) FromDto(ProjectDto dto)
        {
            var channels = dto.Channels.Select(ch => new Channel
            {
                Id = ch.Id,
                Name = ch.Name,
                Protocol = ch.Protocol,
                Enabled = ch.Enabled,
            }).ToList();

            foreach (var ch in channels)
            {
                var chDto = dto.Channels.First(c => c.Id == ch.Id);

                foreach (var kv in chDto.Config)
                    ch.Config[kv.Key] = kv.Value;

                foreach (var d in chDto.Devices)
                {
                    var dev = new Device { Id = d.Id, Name = d.Name };

                    // ✅ Load DataTypeGroups
                    foreach (var gDto in d.DataTypeGroups)
                    {
                        var group = new DataTypeGroup { Name = gDto.Name, Function = gDto.Function };
                        foreach (var p in gDto.Points)
                        {
                            var pt = new Point
                            {
                                Id = p.Id,
                                Name = p.Name,
                                DataType = Enum.TryParse<PointDataType>(p.DataType, out var dt) ? dt : PointDataType.String,
                                Meta = p.Meta.ToDictionary(m => m.Key, m => m.Value)
                            };
                            group.Points.Add(pt);
                        }
                        dev.DataTypeGroups.Add(group);
                    }

                    // giữ lại Points nếu dùng trực tiếp
                    foreach (var p in d.Points)
                    {
                        var pt = new Point
                        {
                            Id = p.Id,
                            Name = p.Name,
                            DataType = Enum.TryParse<PointDataType>(p.DataType, out var dt) ? dt : PointDataType.String,
                            Meta = p.Meta.ToDictionary(m => m.Key, m => m.Value)
                        };
                        dev.Points.Add(pt);
                    }

                    ch.Devices.Add(dev);
                }
            }

            var mappings = dto.Mappings.Select(m => new Mapping
            {
                Id = m.Id,
                SourcePointId = m.SourcePointId,
                DestinationPointId = m.DestinationPointId,
                TwoWay = m.TwoWay,
                Interval = m.Interval,
                Expression = m.Expression
            }).ToList();

            return (channels, mappings);
        }
    }
}
