namespace R2V2.Core.SuperType
{
    public interface IEntity : IEntity<int>
    {
    }

    public interface IEntity<T>
    {
        T Id { get; set; }
    }
}