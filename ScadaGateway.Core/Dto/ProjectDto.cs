using System;
using System.Collections.Generic;

namespace ScadaGateway.Core.Dto
{
    public class ProjectDto
    {
        public string Name { get; set; } = "DefaultProject";
        public List<ChannelDto> Channels { get; set; } = new();
        public List<MappingDto> Mappings { get; set; } = new();
    }

    public class ChannelDto
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Protocol { get; set; } = "";
        public bool Enabled { get; set; } = true;
        public List<DeviceDto> Devices { get; set; } = new();
        public List<KeyValuePairDto> Config { get; set; } = new();
    }

    public class DeviceDto
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public List<PointDto> Points { get; set; } = new();

        // ✅ Bổ sung
        public List<DataTypeGroupDto> DataTypeGroups { get; set; } = new();
    }

    public class DataTypeGroupDto
    {
        public string Name { get; set; } = "";
        public string Function { get; set; } = "";
        public List<PointDto> Points { get; set; } = new();
    }

    public class PointDto
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string DataType { get; set; } = "String";
        public List<KeyValuePairDto> Meta { get; set; } = new();
    }

    public class MappingDto
    {
        public string Id { get; set; } = "";
        public string SourcePointId { get; set; } = "";
        public string DestinationPointId { get; set; } = "";
        public bool TwoWay { get; set; }
        public TimeSpan? Interval { get; set; }
        public string? Expression { get; set; }
    }

    public class KeyValuePairDto
    {
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
    }
}
