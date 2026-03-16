namespace Sushi.Core
{
    /// <summary>
    ///     Defines an interface for components that can verify a requestor's authority
    ///     to view usage for the specified customer.
    /// </summary>
    public interface IAuthorizationAuthority
    {
        /// <summary>
        ///     Gets a value indicating whether or not the indicated requestor is authorized
        ///     to view usage statistics for the specified customer.
        /// </summary>
        /// <param name="requestor">The requesting party.</param>
        /// <param name="targetCustomer">The target customer.</param>
        /// <returns>True if the requestor is authorized to view usage statistics for the specified customer; otherwise, false.</returns>
        bool IsRequestorAuthorized(Requestor requestor, CustomerReference targetCustomer);
    }
}