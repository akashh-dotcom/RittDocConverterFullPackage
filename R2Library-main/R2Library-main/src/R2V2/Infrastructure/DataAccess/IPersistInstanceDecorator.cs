namespace R2V2.Infrastructure.DataAccess
{
    public interface IPersistInstanceDecorator
    {
        void Execute<T>(T instance);
    }
}