namespace R2V2.Core.Search.FacetData
{
    public abstract class FacetDataBase : IFacetData
    {
        public int Id { get; set; }
        public int Count { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }

        public override string ToString()
        {
            return ToString("FacetDataBase");
        }

        protected string ToString(string className)
        {
            return $"{className} = [Name: {Name}, Count: {Count}, Id: {Id}, Code: {Code}]";
        }
    }
}