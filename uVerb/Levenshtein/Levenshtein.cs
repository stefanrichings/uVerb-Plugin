using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Levenshtein {
    public static int EditDistance <T> (IEnumerable<T> x, IEnumerable<T> y) where T : IEquatable<T>
    {
        IList<T> first = x as IList<T> ?? new List<T>(x);
        IList<T> second = y as IList<T> ?? new List<T>(y);

        int n = first.Count, m = second.Count;
        if (n == 0) return m;
        if (m == 0) return n;

        int curRow = 0, nextRow = 1;
        int[][] rows = new int[][] { new int[m + 1], new int[m + 1] };

        for (int j = 0; j <= m; ++j)
        {
            rows[curRow][j] = j;
        }

        for (int i = 1; i <= n; ++i)
        {
            rows[nextRow][0] = i;
            for (int j = 1; j <= m; ++j)
            {
                int dist1 = rows[curRow][j] + 1;
                int dist2 = rows[nextRow][j - 1] + 1;
                int dist3 = rows[curRow][j - 1] + (first[i - 1].Equals(second[j - 1]) ? 0 : 1);
                rows[nextRow][j] = Mathf.Min(dist1, Mathf.Min(dist2, dist3));
            }

            if (curRow == 0)
            {
                curRow = 1;
                nextRow = 0;
            }

            else
            {
                curRow = 0;
                nextRow = 1;
            }
        }

        return rows[curRow][m];
    }
}
