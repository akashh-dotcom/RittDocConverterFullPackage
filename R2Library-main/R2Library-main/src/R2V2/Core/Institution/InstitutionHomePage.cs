#region

using System;

#endregion

namespace R2V2.Core.Institution
{
    [Serializable]
    public class InstitutionHomePage : IInstitutionHomePage
    {
        public static InstitutionHomePage Discipline = new InstitutionHomePage
            { Id = HomePage.Discipline, Description = "Browse by Discipline" };

        public static InstitutionHomePage Titles = new InstitutionHomePage
            { Id = HomePage.Titles, Description = "Browse by Titles" };

        public static InstitutionHomePage AtoZIndex = new InstitutionHomePage
            { Id = HomePage.AtoZIndex, Description = "Browse by A-Z Index" };

        public HomePage Id { get; protected set; }

        public string Description { get; protected set; }
    }
}