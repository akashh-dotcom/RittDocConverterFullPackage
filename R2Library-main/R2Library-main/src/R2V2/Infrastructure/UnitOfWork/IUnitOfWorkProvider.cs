namespace R2V2.Infrastructure.UnitOfWork
{
    public interface IUnitOfWorkProvider : IUnitOfWork
    {
        IUnitOfWork Start();
        IUnitOfWork Start(UnitOfWorkScope scope);
    }
}