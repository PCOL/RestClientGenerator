namespace RestClient.Generator;

/// <summary>
/// Defines the accessability of a class or member.
/// </summary>
public enum Accessability
{
    /// <summary>
    /// The member is public.
    /// </summary>
    Public,

    /// <summary>
    /// The member is protected.
    /// </summary>
    Protected,

    /// <summary>
    /// The member is private.
    /// </summary>
    Private,

    /// <summary>
    /// The member is internal.
    /// </summary>
    Internal,

    /// <summary>
    /// The member is protected internal.
    /// </summary>
    ProtectedInternal,
}
