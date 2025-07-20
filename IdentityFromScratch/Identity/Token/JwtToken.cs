using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace IdentityFromScratch.Identity.Token;

public class JwtToken : IToken<JsonWebToken>
{
    public string Token { get; set; }

    private SecurityToken? _securityToken;

    public JsonWebToken? SecurityToken
    {
        get => (JsonWebToken?)_securityToken;
        set => _securityToken = value;
    }

    public JwtToken(string accessToken)
    {
        Token = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
        _securityToken = new JsonWebToken(accessToken) ?? throw new ArgumentNullException(nameof(accessToken), "Access token cannot be null.");
    }

    public JwtToken(JsonWebToken jsonWebToken)
    {
        _securityToken = jsonWebToken ?? throw new ArgumentNullException(nameof(jsonWebToken));
        Token = jsonWebToken.EncodedToken ?? throw new ArgumentNullException(nameof(jsonWebToken), "JsonWebToken cannot be null.");
    }

    public JwtToken(string accessToken, JsonWebToken jsonWebToken)
    {
        Token = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
        _securityToken = jsonWebToken ?? throw new ArgumentNullException(nameof(jsonWebToken));
    }
}
