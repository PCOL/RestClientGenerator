namespace RestClient;

using System;
using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Method)]
public class OutputCodeAttribute
    : Attribute
{
}
