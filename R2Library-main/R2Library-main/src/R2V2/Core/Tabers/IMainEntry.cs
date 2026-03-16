#region

using System.Collections.Generic;

#endregion

namespace R2V2.Core.Tabers
{
    public interface IMainEntry
    {
        int MainEntryKey { get; set; }
        string Name { get; set; }
        IList<Sense> Senses { get; set; }
    }
}