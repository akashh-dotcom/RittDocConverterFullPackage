#region

using System.Collections.Generic;

#endregion

namespace R2V2.Core.Tabers
{
    public class MainEntry : IMainEntry
    {
        public virtual int MainEntryKey { get; set; }
        public virtual string Name { get; set; }
        public virtual IList<Sense> Senses { get; set; }
    }
}