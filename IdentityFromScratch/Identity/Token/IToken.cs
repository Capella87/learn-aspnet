using Microsoft.IdentityModel.Tokens;

namespace IdentityFromScratch.Identity.Token;

public interface IToken
{
    public string Token { get; set; }
}

public interface IToken<T> : IToken where T : SecurityToken
{
    public T? SecurityToken { get; protected set; }
}
