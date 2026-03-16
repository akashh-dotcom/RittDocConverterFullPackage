namespace R2V2.Core.CollectionManagement
{
    public class CheckoutResult
    {
        public CheckoutResult(bool successfull, Cart cart)
        {
            Successful = successfull;
            Cart = cart;
        }

        public Cart Cart { get; private set; }
        public bool Successful { get; private set; }
    }
}