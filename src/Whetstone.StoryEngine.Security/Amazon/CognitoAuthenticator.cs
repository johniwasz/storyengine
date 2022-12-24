using Amazon.AspNetCore.Identity.Cognito.Extensions;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Cache;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Security.Claims;


namespace Whetstone.StoryEngine.Security.Amazon
{

    internal enum GetUserStatus : long
    {
        UserExists,
        UserDoesNotExist,
        ErrorGettingUser
    }

    internal enum UserConfirmationStatus
    {
        UserConfirmed,
        UserNotConfirmed,
        UserDoesNotExist,
        ErrorGettingUser
    }

    public class CognitoAuthenticator : IAuthenticator
    {
        private readonly IAmazonCognitoIdentityProvider _identityProvider;
        private readonly CognitoConfig _cogOptions;
        private readonly ILogger<CognitoAuthenticator> _logger;
        private readonly CognitoClientSecret _clientSecret;

        private readonly IUserContextRetriever _userContextRetriever;
        private readonly IJwtTokenParser _tokenParser;

        private readonly IDistributedCache _cache;

        public static LazyConcurrentDictionary<CognitoConfig, TokenValidationParameters> TokenValidatorDict =
            new LazyConcurrentDictionary<CognitoConfig, TokenValidationParameters>();


        public CognitoAuthenticator(IAmazonCognitoIdentityProvider identityProvider,
            IUserContextRetriever userContextRetriever,
            IOptions<CognitoConfig> cogOptions,
            IJwtTokenParser tokenParser,
            IDistributedCache cache,
            ILogger<CognitoAuthenticator> logger)
        {
            _cogOptions = cogOptions?.Value ?? throw new ArgumentNullException(nameof(cogOptions));
            _identityProvider = identityProvider ?? throw new ArgumentNullException(nameof(identityProvider));
            _tokenParser = tokenParser ?? throw new ArgumentNullException(nameof(tokenParser));


            AWSCognitoClientOptions awsOpts = _cogOptions.ToAwsCognitoClientOptions();

            _clientSecret = new CognitoClientSecret(awsOpts);

            _cache = cache ?? throw new ArgumentNullException(nameof(cache));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _userContextRetriever = userContextRetriever ?? throw new ArgumentNullException(nameof(userContextRetriever));
        }




        public async Task<TokenResult> AuthenticateAsync(AuthCredentials creds)
        {
            if (creds == null)
                throw new ArgumentNullException(nameof(creds));

            TokenResult result = null;
            var request = new InitiateAuthRequest
            {
                ClientId = _cogOptions.UserPoolClientId,
                //UserPoolId = _cogOptions.UserPoolId,
                AuthFlow = AuthFlowType.USER_PASSWORD_AUTH
            };

            request.AuthParameters.Add("USERNAME", creds.UserName);
            request.AuthParameters.Add("PASSWORD", creds.UserSecret);
            request.AuthParameters.Add("SECRET_HASH", _clientSecret.ComputeHash(creds.UserName));

            string publicErrorMessage = $"User {creds.UserName} not authenticated";

            try
            {
                var response = await _identityProvider.InitiateAuthAsync(request);


                result = new TokenResult
                {
                    AuthToken = response.AuthenticationResult.AccessToken,
                    RefreshToken = response.AuthenticationResult.RefreshToken,
                    ExpiresIn = response.AuthenticationResult.ExpiresIn,
                    IdToken = response.AuthenticationResult.IdToken,
                    TokenType = response.AuthenticationResult.TokenType
                };

                _logger.LogInformation(
                    $"User {creds.UserName} logged into Cognito with user pool {_cogOptions.UserPoolId} and user pool client id {_cogOptions.UserPoolClientId}");
            }
            catch (UserNotFoundException ex)
            {

                string internalError =
                    $"User not does not exist. User name: {creds.UserName} user pool {_cogOptions.UserPoolId} and user pool client id {_cogOptions.UserPoolClientId}";

                throw new UserNotAuthenticatedException(publicErrorMessage, internalError, ex);
            }
            catch (UserNotConfirmedException confirmEx)
            {

                string errText = $"User {creds.UserName} is not confirmed. Reroute to the confirmation page.";

                throw new UserConfirmationPendingException("User is not confirmed", errText, confirmEx);
            }
            catch (NotAuthorizedException ex)
            {

                string internalError =
                    $"User {creds.UserName} not found in user pool {_cogOptions.UserPoolId} and user pool client id {_cogOptions.UserPoolClientId}";

                throw new UserNotAuthenticatedException(publicErrorMessage, internalError, ex);
            }
            catch (Exception ex)
            {

                string internalError = $"Unexpected error authenticating user. User name: {creds.UserName} user pool {_cogOptions.UserPoolId} and user pool client id {_cogOptions.UserPoolClientId}";
                throw new UserNotAuthenticatedException(publicErrorMessage, internalError, ex);
            }


            // Get the permissions
            try
            {
                // Parse the access token and get the sub.
                var jwtToken = _tokenParser.ParseAuthToken(result.AuthToken);

                IEnumerable<ClaimsIdentity> claimIdentities = await this.GetUserClaimsAsync(jwtToken);
                List<string> permissions = new List<string>();

                foreach (var claimId in claimIdentities)
                {
                    var foundPerms = claimId.Claims.Where(x => x.Type.Equals(SoniBridgeClaimTypes.Permission)).Select(x => x.Value);
                    permissions.AddRange(foundPerms);
                }

                result.Permissions = permissions;

            }
            catch (Exception ex)
            {
                string internalError = $"Unexpected error authenticating user. Could not get permissions for {creds.UserName}";
                throw new UserNotAuthenticatedException(publicErrorMessage, internalError, ex);
            }



            return result;

        }



        public async Task RequestNewConfirmationCode(ConfirmationCodeResendRequest resendRequest)
        {
            if (resendRequest == null)
                throw new ArgumentNullException(nameof(resendRequest));

            if (string.IsNullOrWhiteSpace(resendRequest.UserName))
                throw new ArgumentNullException(nameof(resendRequest), "UserName cannot be null");

            var userConfStatus = await IsUserConfirmedAsync(resendRequest.UserName);

            if (userConfStatus.HasValue)
            {
                switch (userConfStatus.Value)
                {

                    case UserConfirmationStatus.UserDoesNotExist:
                        _logger.LogWarning($"Resend of email confirmation code of confirmed user {resendRequest.UserName} requested");
                        break;
                    case UserConfirmationStatus.UserConfirmed:
                        // This could be a hacker attempting to get access to the user's account.
                        _logger.LogWarning($"Confirmed user is requesting a new confirmation code {resendRequest.UserName}");
                        break;
                    case UserConfirmationStatus.ErrorGettingUser:
                        _logger.LogError($"Error retrieving user {resendRequest.UserName}");
                        break;
                    case UserConfirmationStatus.UserNotConfirmed:
                        // Only request a new confirmation code if the user does not exist
                        // and is not yet confirmed.
                        await RequestNewConfirmationCodeInternal(resendRequest);
                        break;
                }
            }
            else
            {
                _logger.LogError($"UserConfirmationStatus could not be resolved for user {resendRequest.UserName}");
            }

        }


        private async Task<UserConfirmationStatus?> IsUserConfirmedAsync(string userName)
        {
            UserConfirmationStatus? confStatus = null;

            var (adminUser, status) = await GetUser(userName);

            switch (status)
            {
                case GetUserStatus.UserExists:

                    // Check if the user has been confirmed. If the user has, then do not send a confirmation code.
                    // Currently, only email confirmation is supported. 
                    // This will need to be revisited if phone confirmation is required.
                    confStatus = adminUser.IsEmailConfirmed()
                        ? UserConfirmationStatus.UserConfirmed
                        : UserConfirmationStatus.UserNotConfirmed;
                    break;
                case GetUserStatus.ErrorGettingUser:
                    confStatus = UserConfirmationStatus.ErrorGettingUser;
                    break;
                case GetUserStatus.UserDoesNotExist:
                    confStatus = UserConfirmationStatus.UserDoesNotExist;
                    break;
            }

            return confStatus;
        }

        private async Task RequestNewConfirmationCodeInternal(ConfirmationCodeResendRequest resendRequest)
        {
            ResendConfirmationCodeRequest resendConfirmationRequest = new ResendConfirmationCodeRequest
            {
                ClientId = _cogOptions.UserPoolClientId,
                SecretHash = _clientSecret.ComputeHash(resendRequest.UserName),
                Username = resendRequest.UserName
            };

            try
            {
                ResendConfirmationCodeResponse resendResp = await _identityProvider.ResendConfirmationCodeAsync(resendConfirmationRequest);


                _logger.LogInformation(
                    $"Resending confirmation code fore user {resendRequest.UserName} for pool client {_cogOptions.UserPoolClientId}");
            }
            catch (Exception ex)
            {
                string errorInfo =
                    $"Unexpected error requesting new confirmation code for user {resendRequest.UserName} and pool client {_cogOptions.UserPoolClientId}";

                throw new UnexpectedSecurityException(errorInfo, ex);
            }


        }

        private async Task<(AdminGetUserResponse adminUser, GetUserStatus status)> GetUser(string userName)
        {
            GetUserStatus retStatus;
            AdminGetUserResponse userResp = null;

            AdminGetUserRequest adminGetUser = new AdminGetUserRequest
            {
                Username = userName,
                UserPoolId = _cogOptions.UserPoolId
            };

            try
            {
                userResp = await _identityProvider.AdminGetUserAsync(adminGetUser);
                retStatus = GetUserStatus.UserExists;
            }
            catch (UserNotFoundException)
            {
                retStatus = GetUserStatus.UserDoesNotExist;

                _logger.LogWarning($"User {userName} does not exist");
            }
            catch (Exception ex)
            {
                retStatus = GetUserStatus.ErrorGettingUser;
                _logger.LogWarning($"User {userName} not found", ex);
            }


            return (userResp, retStatus);

        }

        public async Task<ConfirmNewUserResponse> ConfirmAccountAsync(ConfirmNewUserRequest confirmUserRequest)
        {

            if (confirmUserRequest == null)
                throw new ArgumentNullException(nameof(confirmUserRequest));

            if (string.IsNullOrWhiteSpace(confirmUserRequest.UserName))
                throw new ArgumentNullException(nameof(confirmUserRequest), "UserName cannot be null");

            if (string.IsNullOrWhiteSpace(confirmUserRequest.ConfirmationCode))
                throw new ArgumentNullException(nameof(confirmUserRequest), "ConfirmationCode cannot be null");

            ConfirmNewUserResponse resp = new ConfirmNewUserResponse();

            var userConfirmationStatus = await IsUserConfirmedAsync(confirmUserRequest.UserName);


            bool confirmUser = false;
            if (userConfirmationStatus.HasValue)
            {

                switch (userConfirmationStatus.Value)
                {
                    case UserConfirmationStatus.UserDoesNotExist:
                        resp.Status = UserConfimationCodeStatus.Invalid;
                        _logger.LogError($"Issue validating confirmation code: User {confirmUserRequest.UserName} does not exist.");
                        break;
                    case UserConfirmationStatus.UserConfirmed:
                        _logger.LogError($"Issue validating confirmation code: User {confirmUserRequest.UserName} already confirmed.");
                        break;
                    case UserConfirmationStatus.ErrorGettingUser:
                        _logger.LogError($"Issue validating confirmation code: Unexpected error retrieving user {confirmUserRequest.UserName}");
                        break;
                    case UserConfirmationStatus.UserNotConfirmed:
                        confirmUser = true;
                        break;
                }


            }
            else
            {
                _logger.LogError($"Error retrieving user confirmation status when attempting to confirm user {confirmUserRequest.UserName}");
            }



            if (confirmUser)
            {


                ConfirmSignUpRequest confirmSignUpRequest = new ConfirmSignUpRequest
                {
                    ClientId = _cogOptions.UserPoolClientId,
                    ConfirmationCode = confirmUserRequest.ConfirmationCode,
                    Username = confirmUserRequest.UserName,
                    SecretHash = _clientSecret.ComputeHash(confirmUserRequest.UserName)
                };


                try
                {
                    ConfirmSignUpResponse confirmResponse =
                        await _identityProvider.ConfirmSignUpAsync(confirmSignUpRequest);

                    resp.Status = UserConfimationCodeStatus.Confirmed;
                    _logger.LogInformation(
                        $"User {confirmUserRequest.UserName} successfully confirmed account with Cognito user pool {_cogOptions.UserPoolId}");
                }
                catch (ExpiredCodeException expiredEx)
                {
                    // Curiously, Amazon throws an ExpiredCodeException if the provided confirmation code is invalid, then the text "invalid code" appears in the response
                    // message.

                    // This error is also thrown if the user name is not registered and the error message contains the text "invalid code"

                    // This error is also thrown if the user is already confirmed and the error message will contain the text "invalid code"

                    string errorText;
                    if (expiredEx.Message.Contains("invalid code", StringComparison.OrdinalIgnoreCase))
                    {
                        resp.Status = UserConfimationCodeStatus.Invalid;
                        errorText =
                            $"Confirmation code for new user {confirmUserRequest.UserName} in Cognito user pool {_cogOptions.UserPoolId} is invalid";
                    }
                    else
                    {
                        resp.Status = UserConfimationCodeStatus.Expired;
                        errorText =
                            $"Confirmation code for new user {confirmUserRequest.UserName} in Cognito user pool {_cogOptions.UserPoolId} is expired";
                    }


                    _logger.LogWarning(expiredEx, errorText);

                }
                catch (CodeMismatchException mismatchEx)
                {
                    string errorText =
                        $"Confirmation code for new user {confirmUserRequest.UserName} in Cognito user pool {_cogOptions.UserPoolId} is invalid";
                    resp.Status = UserConfimationCodeStatus.Invalid;

                    _logger.LogWarning(mismatchEx, errorText);

                }
                catch (Exception ex)
                {
                    string errorInfo =
                        $"Error confirming new user {confirmUserRequest.UserName} in Cognito user pool {_cogOptions.UserPoolId}";
                    throw new UnexpectedSecurityException(errorInfo, ex);
                }
            }

            return resp;
        }


        public async Task SignUpAsync(SignUpRequest createNewRequest)
        {
            if (createNewRequest == null)
                throw new ArgumentNullException(nameof(createNewRequest));

            if (string.IsNullOrWhiteSpace(createNewRequest.UserName))
                throw new ArgumentNullException(nameof(createNewRequest), "UserName cannot be null");

            global::Amazon.CognitoIdentityProvider.Model.SignUpRequest signUpReq =
                new global::Amazon.CognitoIdentityProvider.Model.SignUpRequest
                {
                    ClientId = _cogOptions.UserPoolClientId,
                    Username = createNewRequest.UserName,
                    Password = createNewRequest.UserSecret,
                    SecretHash = _clientSecret.ComputeHash(createNewRequest.UserName)
                };

            var emailAttribute = new AttributeType
            {
                Name = "email",
                Value = createNewRequest.UserName
            };
            signUpReq.UserAttributes.Add(emailAttribute);


            SignUpResponse createResponse = null;

            try
            {

                createResponse =
                    await _identityProvider.SignUpAsync(signUpReq);

                _logger.LogInformation(
                    $"Request to create new user {createNewRequest.UserName} sent to Cognito user pool {_cogOptions.UserPoolId}");
            }
            catch (UsernameExistsException)
            {


                string errText =
                    $"{createNewRequest.UserName} already exists in Cognito user pool {_cogOptions.UserPoolId}";

                _logger.LogError(errText);
                // The UI should not know that the user already exists. This would give hackers a known user account.
                // throw new UnexpectedSecurityException(errText);

                // TODO Send the user a message. This may be a hacking attempt.
            }
            catch (InvalidParameterException paramEx)
            {

                string errText =
                    $"Possible duplicate alias issued for {createNewRequest.UserName} in Cognito user pool {_cogOptions.UserPoolId}";

                _logger.LogError(paramEx, errText);

            }
            catch (Exception ex)
            {

                string errMsg = $"Error processing request to create new user {createNewRequest.UserName} in Cognito user pool {_cogOptions.UserPoolId}";

                throw new UnexpectedSecurityException(errMsg, ex);
            }

            // Get the user id by the sub




            if (createResponse != null)
            {
                // Add the user sub and user name to the database.
                DataUser newUser = new DataUser
                {
                    CognitoSub = createResponse.UserSub
                };

                try
                {
                    await using var userContext = await _userContextRetriever.GetUserDataContextAsync();
                    await userContext.CreateUserAccountAsync(newUser, createNewRequest.UserName);
                }
                catch (Exception ex)
                {
                    string errMsg = $"Database error processing request to create new user {createNewRequest.UserName} in database";
                    throw new UnexpectedSecurityException(errMsg, ex);
                }
            }

        }


        public async Task<TokenResult> RefreshTokenAsync(RefreshSessionRequest refreshRequest)
        {

            if (refreshRequest == null)
            {
                throw new ArgumentNullException(nameof(refreshRequest));
            }

            if (string.IsNullOrWhiteSpace(refreshRequest.RefreshToken))
            {
                throw new ArgumentNullException(nameof(refreshRequest), "RefreshToken property cannot be null");
            }

            if (string.IsNullOrWhiteSpace(refreshRequest.AuthToken))
            {
                throw new ArgumentNullException(nameof(refreshRequest), "AuthToken property cannot be null");
            }

            TokenResult result;

            var request = new InitiateAuthRequest
            {
                ClientId = _cogOptions.UserPoolClientId,
                AuthFlow = AuthFlowType.REFRESH_TOKEN_AUTH
            };


            request.AuthParameters.Add("REFRESH_TOKEN", refreshRequest.RefreshToken);

            try
            {

                string userSub = GetUserId(refreshRequest.AuthToken);

                string secretHash = _clientSecret.ComputeHash(userSub);

                request.AuthParameters.Add("SECRET_HASH", secretHash);

                var cognitoResponse = await _identityProvider.InitiateAuthAsync(request);

                result = new TokenResult
                {
                    AuthToken = cognitoResponse.AuthenticationResult.AccessToken,
                    RefreshToken = cognitoResponse.AuthenticationResult.RefreshToken ?? refreshRequest.RefreshToken,
                    ExpiresIn = cognitoResponse.AuthenticationResult.ExpiresIn,
                    IdToken = cognitoResponse.AuthenticationResult.IdToken,
                    TokenType = cognitoResponse.AuthenticationResult.TokenType
                };

                _logger.LogInformation(
                    $"Token refreshed with {_cogOptions.UserPoolId} and user pool client id {_cogOptions.UserPoolClientId}");
            }
            catch (NotAuthorizedException authEx)
            {
                string errorInfo = $"Authorization exception using token with user pool {_cogOptions.UserPoolId} and user pool client id {_cogOptions.UserPoolClientId}: {refreshRequest.RefreshToken}";
                throw new UnexpectedSecurityException(errorInfo, authEx);
            }
            catch (Exception ex)
            {
                string errorInfo = $"Error refreshing token with user pool {_cogOptions.UserPoolId} and user pool client id {_cogOptions.UserPoolClientId}: {refreshRequest.RefreshToken}";
                throw new UnexpectedSecurityException(errorInfo, ex);

            }

            return result;
        }

        private string GetUserId(string authToken)
        {

            var handler = new JwtSecurityTokenHandler();

            var parsedToken = handler.ReadToken(authToken) as JwtSecurityToken;

            return parsedToken.Subject;
        }


        public async Task SignOutAsync(SignOutRequest request)
        {

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.AuthToken))
                throw new ArgumentNullException(nameof(request), "AuthToken property cannot be null");

            GlobalSignOutRequest signOutRequest = new GlobalSignOutRequest
            {
                AccessToken = request.AuthToken
            };


            try
            {
                var signOutResponse = await _identityProvider.GlobalSignOutAsync(signOutRequest);
                _logger.LogInformation($"Cognito user log out status code response: {signOutResponse.HttpStatusCode} with user pool {_cogOptions.UserPoolId} and user pool client id {_cogOptions.UserPoolClientId}");
            }
            catch (Exception ex)
            {
                string errorInfo =
                    $"Error on user sign out with auth token with user pool {_cogOptions.UserPoolId} and user pool client id {_cogOptions.UserPoolClientId} and auth token {request.AuthToken}";
                throw new UnauthorizedAccessException(errorInfo, ex);
            }
        }

        public async Task<IEnumerable<ClaimsIdentity>> GetUserClaimsAsync(JwtSecurityToken token)
        {

            string container = "jwt:cog";

            List<ClaimsIdentity> claimsIdentities = null;

            if (token == null)
                throw new ArgumentNullException(nameof(token));

            if (string.IsNullOrWhiteSpace(token.Subject))
                throw new ArgumentNullException(nameof(token), "Subject cannot be null or empty");



            string sub = token.Subject;

            List<Claim> userClaims = _cache.Get<List<Claim>>(container, sub);


            if (userClaims == null)
            {

                using (var context = await _userContextRetriever.GetUserDataContextAsync())
                {

                    var foundSecInfo = context.Users
                        .Join(context.UserGroupXRefs,
                            uid => uid.Id,
                            group => group.UserId,
                            (uid, group) => new
                            {
                                group.Group,
                                UserId = uid.Id,
                                UserSub = uid.CognitoSub

                            }).Join(context.GroupRoleXRefs,
                            userGroup => userGroup.Group.Id,
                            roleXRef => roleXRef.GroupId,
                            (groupXRef, roleXRef) => new
                            {
                                roleXRef.Group,
                                roleXRef.Role,
                                groupXRef.UserSub,
                                groupXRef.UserId
                            }).Join(context.Organizations,
                            groupRole => groupRole.Group.OrganizationId,
                            org => org.Id,
                            (groupRole, org) => new
                            {
                                Organization = org,
                                groupRole.Role,
                                groupRole.UserSub,
                                groupRole.UserId
                            }).Join(context.FunctionalEntitlementRoleXRefs,
                            roleXRef => roleXRef.Role.Id,
                            funcRoleXRef => funcRoleXRef.RoleId,
                            (roleXRef, funcRoleXRef) =>
                                new
                                {
                                    Entitlement = funcRoleXRef.FunctionalEntitlement.Claim,
                                    roleXRef.UserSub,
                                    roleXRef.UserId,
                                    OrganizationId = roleXRef.Organization.Id,
                                    RoleName = roleXRef.Role.Name,
                                    roleXRef.Organization.SubscriptionLevelId,
                                    IsOrganizationEnabled = roleXRef.Organization.IsEnabled
                                })
                        .Where(orgRoles => orgRoles.UserSub.Equals(sub))
                        .Select(element => new { element.RoleName, element.OrganizationId, element.UserId, element.Entitlement, element.SubscriptionLevelId, element.IsOrganizationEnabled })
                        .Distinct();



                    // assign the user id to a claim

                    var rootItem = foundSecInfo.FirstOrDefault();

                    if (rootItem != null)
                    {


                        userClaims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Sid, rootItem.UserId.ToString(), ClaimValueTypes.Sid),

                            new Claim(SoniBridgeClaimTypes.Organization, rootItem.OrganizationId.ToString(), ClaimValueTypes.String),


                            new Claim(SoniBridgeClaimTypes.SubscriptionLevel,
                            rootItem.SubscriptionLevelId.ToString(), ClaimValueTypes.String),

                            new Claim(SoniBridgeClaimTypes.AccountStatus,
                            rootItem.IsOrganizationEnabled ? "enabled" : "disabled", ClaimValueTypes.String)
                        };


                        var entitlements = foundSecInfo.Select(x => x.Entitlement).Distinct();

                        // Add permissions
                        foreach (var entitlement in entitlements)
                        {
                            userClaims.Add(new Claim(SoniBridgeClaimTypes.Permission, entitlement, ClaimValueTypes.String));
                        }


                        // Add to cache
                        await _cache.SetAsync(container, sub, userClaims, new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = token.ValidTo - DateTime.UtcNow });

                    }
                }
            }

            claimsIdentities = new List<ClaimsIdentity>();
            ClaimsIdentity roleIdentity = new ClaimsIdentity(userClaims, SoniBridgeClaimTypes.SoniBridgeIssuer, "EngineIdentity", null);
            claimsIdentities.Add(roleIdentity);


            return claimsIdentities;
        }

        public static TokenValidationParameters GetTokenValidationParameters(CognitoConfig cognitoConfig)
        {
            return TokenValidatorDict.GetOrAdd(cognitoConfig, cogConfig =>
            {
                SigningKeyRetriever signingRetriever = new SigningKeyRetriever(cognitoConfig);
                return signingRetriever.GetTokenValidationParameters();
            });
        }


    }
}
