namespace RestClient
{
    using System;

    /// <summary>
    /// An attribute to indicate REST clients.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RestClientAttribute
        : Attribute
    {
        public string ClientTypeName { get; set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="RestClientAttribute"/> class.
        /// </summary>
        /// <param name="clientType">The client type.</param>
        public RestClientAttribute(Type clientType)
        {
            clientType.ThrowIfArgumentNull(nameof(clientType));
            clientType.ThrowIfNotInterface(nameof(clientType));

            this.ClientTypeName = clientType.FullName;
        }
    }
}
