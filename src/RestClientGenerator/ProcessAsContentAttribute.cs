﻿namespace RestClient;

using System;

/// <summary>
/// A parameter attribute which specifies that the parameter is provided in the requests content.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class ProcessAsContentAttribute
    : Attribute
{
}
