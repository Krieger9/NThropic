using System.Text;

public static class AsyncEnumerableExtensions
{
    public static async Task<string> ToSingleStringAsync(this IAsyncEnumerable<string> source)
    {
        var stringBuilder = new StringBuilder();
        await foreach (var item in source)
        {
            stringBuilder.Append(item);
        }
        return stringBuilder.ToString();
    }
}
