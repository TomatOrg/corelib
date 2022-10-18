namespace System;

public struct RuntimeTypeHandle
{
    
    public IntPtr Value { get; }

    internal static object CreateInstanceForAnotherGenericParameter(Type type, Type genericParameter)
    {
        if (!type.IsGenericTypeDefinition)
        {
            type = type.GetGenericTypeDefinition();
        }
        return Activator.CreateInstance(type.MakeGenericType(genericParameter));
    }
    
    
    internal static object CreateInstanceForAnotherGenericParameter(Type type, Type genericParameter1, Type genericParameter2)
    {
        if (!type.IsGenericTypeDefinition)
        {
            type = type.GetGenericTypeDefinition();
        }
        return Activator.CreateInstance(type.MakeGenericType(genericParameter1, genericParameter2));
    }
    
}