namespace System;

public abstract class ValueType
{
    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }

        var thisType = GetType();
        var thatType = obj.GetType();
        if (thisType != thatType)
        {
            return false;
        }

        // TODO: proper compare, both bitwise and field wise

        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string ToString()
    {
        return GetType().ToString();
    }
}