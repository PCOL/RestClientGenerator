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
        /// <summary>
        /// Gets or sets the client type name.
        /// </summary>
        public string ClientTypeName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not debugging logs are enabled.
        /// </summary>
        public bool DebuggingLogs { get; set; }

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
