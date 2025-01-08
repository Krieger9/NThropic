using ClaudeApi.Messages;

namespace Sanctuary.Cache
{
    public class RequestKeyEqualityComparer : IEqualityComparer<Tuple<List<Message>, List<ContentBlock>, string>>
    {
        public bool Equals(Tuple<List<Message>, List<ContentBlock>, string>? x, Tuple<List<Message>, List<ContentBlock>, string>? y)
        {
            if (x == null || y == null)
                return x == y;

            return ListEquals(x.Item1, y.Item1) &&
                   ListEquals(x.Item2, y.Item2) &&
                   string.Equals(x.Item3, y.Item3, StringComparison.Ordinal);
        }

        private static bool ListEquals<T>(List<T>? x, List<T>? y)
        {
            if (x == null || y == null)
                return x == y;

            if (x.Count != y.Count)
                return false;

            for (int i = 0; i < x.Count; i++)
            {
                if (!EqualityComparer<T>.Default.Equals(x[i], y[i]))
                    return false;
            }

            return true;
        }

        public int GetHashCode(Tuple<List<Message>, List<ContentBlock>, string> obj)
        {
            ArgumentNullException.ThrowIfNull(obj);

            int hash = 17;
            hash = hash * 31 + GetListHashCode(obj.Item1);
            hash = hash * 31 + GetListHashCode(obj.Item2);
            hash = hash * 31 + (obj.Item3?.GetHashCode() ?? 0);

            return hash;
        }

        private static int GetListHashCode<T>(List<T> list)
        {
            if (list == null)
                return 0;

            int hash = 17;
            foreach (var item in list)
            {
                hash = hash * 31 + (item != null ? EqualityComparer<T>.Default.GetHashCode(item) : 0);
            }

            return hash;
        }
    }
}