namespace Minesolver.Utility {
    internal static class Combinatorics {        
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
            if(m < 0) throw new ArgumentOutOfRangeException(nameof(m));

            T[] result = new T[m];
            if(m == 0) {
                yield return result;
            } else {
                foreach(int[] j in Combinations(m, array.Length)) {
                    for(int i = 0; i < m; i++) {
                        result[i] = array[j[i]];
                    }
                    yield return result;
                }
            }
        }

        public static int Combination(int n, int k) {
            if(n < 0) throw new ArgumentOutOfRangeException(nameof(n));
            if(k < 0 || k > n) return 0;

            int c = 1;
            for(int num = n; num > Math.Max(k, n - k); num--) {
                c *= num;
            }
            for(int den = Math.Min(k, n - k); den > 1; den--) {
                c /= den;
            }
            return c;
        }
    }
}
