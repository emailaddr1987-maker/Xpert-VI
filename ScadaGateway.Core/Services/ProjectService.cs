using System;
using System.IO;
using System.Xml.Serialization;
using ScadaGateway.Core.Models;
using ScadaGateway.Core.Dto;

namespace ScadaGateway.Core.Services
{
    public static class ProjectService
    {
        public static void Save(ProjectDto project, string filePath)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(ProjectDto));
                using var writer = new StreamWriter(filePath);
                serializer.Serialize(writer, project);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to save project", ex);
            }
        }

        public static ProjectDto Load(string filePath)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(ProjectDto));
                using var reader = new StreamReader(filePath);
                return (ProjectDto)serializer.Deserialize(reader)!;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to load project", ex);
            }
        }
    }
}
