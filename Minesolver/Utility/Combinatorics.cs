namespace Minesolver.Utility {
    internal static class Combinatorics {
        //public static IEnumerable<T[]> Combinations<T>(this IEnumerable<T> source) {
        //    if(source == null) throw new ArgumentNullException(nameof(source));

        //    T[] sourceBuffer = source.ToArray();
        //    return Enumerable
        //        .Range(0, 1 << sourceBuffer.Length)
        //        .Select(mask =>
        //            sourceBuffer.Where((_, i) => (mask & (1 << i)) != 0)
        //            .ToArray()
        //        );
        //}

        //public static IEnumerable<T[]> Combinations<T>(this IEnumerable<T> source, int length) {
        //    if(source == null) throw new ArgumentNullException(nameof(source));
        //    if(length < 0 || length > source.Count()) throw new ArgumentOutOfRangeException(nameof(source));

        //    return source.Combinations()
        //        .Where(comb => comb.Length == length);
        //}

        
        private static IEnumerable<int[]> Combinations(int m, int n) {
            int[] result = new int[m];
            Stack<int> stack = new Stack<int>(m);
            stack.Push(0);

            while(stack.Count > 0) {
                int index = stack.Count - 1;
                int value = stack.Pop();

                while(value < n) {
                    result[index++] = value++;
                    stack.Push(value);

                    if(index == m) {
                        yield return (int[])result.Clone();
                        break;
                    }
                }
            }
        }

        public static IEnumerable<T[]> Combinations<T>(this IEnumerable<T> source, int m) {
            T[] array = source.ToArray();
            if(array.Length < m) throw new ArgumentException("Array length can't be less than number of selected elements");
            if(m < 1) throw new ArgumentException("Number of selected elements can't be less than 1");

            T[] result = new T[m];
            foreach(int[] j in Combinations(m, array.Length)) {
                for(int i = 0; i < m; i++) {
                    result[i] = array[j[i]];
                }
                yield return result;
            }
        }
    }
}
