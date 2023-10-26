namespace RestClient
{
    using System;

    public abstract class RestClientContext
    {
        public TClient GetClient<TClient>()
        {
            return (TClient)GetClient(typeof(TClient));
        }

        public object GetClient(Type clientType)
        {
            clientType.ThrowIfArgumentNull(nameof(clientType));
            clientType.ThrowIfNotInterface(nameof(clientType));

            return null;
        }
    }
}
