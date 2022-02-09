namespace Teeditor.Common.Models.IO
{
    public class ProjectType
    {
        public string Name { get; }
        public string Extension { get; }
        public string Description { get; }
        public string Icon { get; }
        public string GeneratedName { get; }

        public ProjectType(string name, string extension, string description, string icon, string generatedName)
        {
            Name = name;
            Extension = extension;
            Description = description;
            Icon = icon;
            GeneratedName = generatedName;
        }
    }
}
