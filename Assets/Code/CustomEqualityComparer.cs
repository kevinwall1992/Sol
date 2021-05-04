using System;
using System.Collections.Generic;

public class CustomEqualityComparer<T> : IEqualityComparer<T>
{
    Func<T, T, bool> Equals_;
    Func<T, int> GetHashCode_;

    public CustomEqualityComparer(Func<T, T, bool> Equals_,
                                  Func<T, int> GetHashCode_)
    {
        this.Equals_ = Equals_;
        this.GetHashCode_ = GetHashCode_;
    }

    public bool Equals(T a, T b)
    {
        return Equals_(a, b);
    }

    public int GetHashCode(T element)
    {
        return GetHashCode_(element);
    }
}