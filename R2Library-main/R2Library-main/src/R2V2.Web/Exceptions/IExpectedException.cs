namespace R2V2.Web.Exceptions
{
    public interface IExpectedException
    {
        //Guid Id { get; }
        int HttpStatusCode { get; }
        string UserFriendlyMessage { get; }
        int HttpSubStatusCode { get; }
    }
}