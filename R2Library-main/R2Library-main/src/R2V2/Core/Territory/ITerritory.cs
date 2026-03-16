namespace R2V2.Core.Territory
{
    public interface ITerritory
    {
        bool RecordStatus { get; set; }
        string Code { get; set; }
        string Name { get; set; }
        int Id { get; set; }
    }
}