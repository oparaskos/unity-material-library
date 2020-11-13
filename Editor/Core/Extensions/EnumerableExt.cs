using System.Collections.Generic;
using System.Linq;

namespace HestiaMaterialImporter.Extensions
{
    public static class EnumerableExt
    {
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunksize)
        {
            while (source.Any())
            {
                yield return source.Take(chunksize);
                source = source.Skip(chunksize);
            }
        }
    }
}