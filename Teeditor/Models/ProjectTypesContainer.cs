using System.Collections.Generic;
using Teeditor.Common.Models.IO;

namespace Teeditor.Models
{
    internal static class ProjectTypesContainer
    {
        public static IReadOnlyCollection<ProjectType> Items { get; }

        static ProjectTypesContainer()
        {
            var items = Get();
            Items = new List<ProjectType>(items);
        }

        private static IEnumerable<ProjectType> Get()
        {
            yield return new ProjectType("TeeWorlds Vanilla Map", ".map", "A project for creating a map for the 2D retro multiplayer shooter TeeWorlds.", "test", "New map");
        }
    }
}
