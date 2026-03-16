#region

using System;
using System.ComponentModel;

#endregion

namespace R2V2.Core.Institution
{
    public enum HomePage
    {
        [Description("disciplines")] Discipline = 0,
        [Description("publications")] Titles = 1,
        [Description("publications")] AtoZIndex = 2
    }

    public static class HomePageExtentions
    {
        public static string GetDescription(this HomePage value)
        {
            var field = value.GetType().GetField(value.ToString());

            var attribute
                = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute))
                    as DescriptionAttribute;

            return attribute == null ? value.ToString() : attribute.Description;
        }
    }
}