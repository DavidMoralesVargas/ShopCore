using Microsoft.AspNetCore.Components.Authorization;

namespace Tienda_Online.Frontend.AuthenticactionProvider
{
    public class AuthenticationProviderTest:AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            throw new NotImplementedException();
        }
    }
}
