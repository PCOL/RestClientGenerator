namespace RestClientGenerator;

using System;

[AttributeUsage(AttributeTargets.Method)]
public class HttpGetAttribute
    : Attribute
{
    public string Path { get; }

    public HttpGetAttribute(string path)
    {
        this.Path = path;
    }
}
