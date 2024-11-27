public interface ICacheKey
{
    bool Equals(object? obj);
    int GetHashCode();
}
