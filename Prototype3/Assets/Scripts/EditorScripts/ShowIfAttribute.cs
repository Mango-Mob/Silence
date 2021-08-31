using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum ShowIfCond
{
    Equal,
    NotEqual,
    MoreThan,
    LessThan,
    MoreThanOrEqual,
    LessThanOrEqual,
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
public class ShowIfAttribute : PropertyAttribute
{
    public string comparedPropertyName { get; private set; } = "";

    public object targetValue { get; private set; }

    public bool readOnlyInInspector { get; private set; } = false;
    public ShowIfCond condition { get; private set; } = ShowIfCond.Equal;

    public ShowIfAttribute(string comparedPropertyName, bool showAsReadOnly = false)
    {
        this.comparedPropertyName = comparedPropertyName;
        this.targetValue = true;
        this.readOnlyInInspector = showAsReadOnly;
    }
    public ShowIfAttribute(string comparedPropertyName, object targetValue, ShowIfCond condition = ShowIfCond.Equal, bool showAsReadOnly = false)
    {
        this.comparedPropertyName = comparedPropertyName;
        this.targetValue = targetValue;
        this.condition = condition;
        this.readOnlyInInspector = showAsReadOnly;
    }
}