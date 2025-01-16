namespace Ttc.Model.Clubs;

public class CacheResponse<T>
{
    public ICollection<T> Data { get; set; }
    public DateTime LastChange { get; set; }

    public CacheResponse(ICollection<T> data, DateTime lastChange)
    {
        Data = data;
        LastChange = lastChange;
    }
}
