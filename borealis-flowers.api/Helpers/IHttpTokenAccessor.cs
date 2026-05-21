namespace borealis_flowers.api.Helpers;

public interface IHttpTokenAccessor
{
    public Task<string?> GetBearerToken();
}
