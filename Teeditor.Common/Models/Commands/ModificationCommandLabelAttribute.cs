using System;

namespace Teeditor.Common.Models.Commands
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ModificationCommandLabelAttribute : Attribute
    {
        public string Label { get; set; }

        public ModificationCommandLabelAttribute(string label)
        {
            Label = label;
        }
    }
}
