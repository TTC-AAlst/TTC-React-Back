namespace Ttc.DataEntities.Core;

public interface IUserProvider
{
    string Name { get; }
    int? PlayerId { get; }
}