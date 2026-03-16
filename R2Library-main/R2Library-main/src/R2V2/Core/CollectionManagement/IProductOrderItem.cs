namespace R2V2.Core.CollectionManagement
{
    public interface IProductOrderItem : IOrderItem
    {
        IProduct Product { get; }

        bool Agree { get; set; }
    }
}