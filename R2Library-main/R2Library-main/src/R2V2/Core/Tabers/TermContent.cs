namespace R2V2.Core.Tabers
{
    public class TermContent : ITermContent
    {
        public virtual int TermContentKey { get; set; }

        // Implement ITermContent.Id property
        public virtual int Id
        {
            get => TermContentKey;
            set => TermContentKey = value;
        }

        public virtual string Term { get; set; }
        public virtual string Content { get; set; }
        public virtual string SectionId { get; set; }
    }
}