//https://stackoverflow.com/questions/19695629/c-sharp-sortedlist-with-byte-array-as-key

using System;
using System.Collections.Generic;

class ByteComparer : IComparer<byte[]>
{
    public int Compare(byte[] x, byte[] y)
    {
        var len = Math.Min(x.Length, y.Length);
        for (var i = 0; i < len; i++)
        {
            var c = x[i].CompareTo(y[i]);
            if (c != 0)
            {
                return c;
            }
        }

        return x.Length.CompareTo(y.Length);
    }
}


//https://stackoverflow.com/questions/5716423/c-sharp-sortable-collection-which-allows-duplicate-keys
public class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
{
    #region IComparer<TKey> Members

    public int Compare(TKey x, TKey y)
    {
        int result = x.CompareTo(y);

        if (result == 0)
            return 1;   // Handle equality as beeing greater
        else
            return result;
    }

    #endregion
}

//https://stackoverflow.com/questions/7815930/sortedlist-desc-order
class DescComparer<TKey> : IComparer<TKey> where TKey : IComparable
{
    public int Compare(TKey x, TKey y)
    {
        int result = x.CompareTo(y);

        if (result == 0)
            return 1;   // Handle equality as beeing greater

        if (x == null) return -1;
        if (y == null) return 1;
        return Comparer<TKey>.Default.Compare(y, x);
    }
}