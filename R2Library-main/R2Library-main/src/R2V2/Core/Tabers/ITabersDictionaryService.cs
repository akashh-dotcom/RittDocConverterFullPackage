namespace R2V2.Core.Tabers
{
    public interface ITabersDictionaryService
    {
        IMainEntry GetMainEntry(string name);
        ITermContent GetTermContent(int termContentId);
        ITermContent GetTermContentFuzzy(string term);
    }
}