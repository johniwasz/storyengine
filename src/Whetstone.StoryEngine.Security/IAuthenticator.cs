using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Whetstone.StoryEngine.Security
{
    public interface IAuthenticator
    {

        Task<TokenResult> AuthenticateAsync(AuthCredentials creds);


        Task SignOutAsync(SignOutRequest request);


        Task<TokenResult> RefreshTokenAsync(RefreshSessionRequest refreshRequest);


        public Task SignUpAsync(SignUpRequest createRequest);


        public Task<ConfirmNewUserResponse> ConfirmAccountAsync(ConfirmNewUserRequest confirmUserRequest);


        public Task RequestNewConfirmationCode(ConfirmationCodeResendRequest resendRequest);


        public Task<IEnumerable<ClaimsIdentity>> GetUserClaimsAsync(JwtSecurityToken token);
    }
}
