using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

using Opengate.Modules.Accounts.Users.Adapters.Endpoints.Dtos;
using Opengate.Modules.Accounts.Users.Domain.Enum;
using Opengate.Modules.Accounts.Users.Domain.Ports.AuthenticationTokenGerators;
using Opengate.Modules.Accounts.Users.Models;
using Opengate.Modules.Shared.Utils.HttpUtils;

namespace Opengate.Modules.Accounts.Users.Adapters.Endpoints;

public static class IdentityEndpointExtensions
{
    // Validate the email address using DataAnnotations like the UserValidator does when RequireUniqueEmail = true.
    private static readonly EmailAddressAttribute EmailAddressAttribute = new();

    /// <summary>
    /// Add endpoints for registering, logging in, and logging out using ASP.NET Core Identity.
    /// </summary>
    /// <typeparam name="TUser">The user type.</typeparam>
    /// <param name="endpoints">
    /// The <see cref="IEndpointRouteBuilder"/> to add the identity endpoints to.
    /// Call <see cref="EndpointRouteBuilderExtensions.MapGroup(IEndpointRouteBuilder, string)"/> to add a prefix to all the endpoints.
    /// </param>
    /// <returns>An <see cref="IEndpointConventionBuilder"/> to further customize the added endpoints.</returns>
    public static IEndpointConventionBuilder MapCustomIdentityApi<TUser>(this IEndpointRouteBuilder endpoints)
        where TUser : class, new()
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var timeProvider = endpoints.ServiceProvider.GetRequiredService<TimeProvider>();
        var bearerTokenOptions = endpoints.ServiceProvider.GetRequiredService<IOptionsMonitor<BearerTokenOptions>>();
        var linkGenerator = endpoints.ServiceProvider.GetRequiredService<LinkGenerator>();

        // We'll figure out a unique endpoint name based on the final route pattern during endpoint generation.
        string? confirmEmailEndpointName = null;

        var routeGroup = endpoints.MapGroup("")
            .WithTags("Identity");

        // NOTE: We cannot inject UserManager<ApplicationUser> directly because the ApplicationUser generic parameter is currently unsupported by RDG.
        // https://github.com/dotnet/aspnetcore/issues/47338
        routeGroup.MapPost("/register", async Task<Results<Ok, ValidationProblem>>
            (
            [FromBody] RegisterUserRequest registration,
            HttpContext context,
            [FromServices] IServiceProvider sp) =>
        {
            var emailSender = sp.GetRequiredService<IEmailSender<ApplicationUser>>();

            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

            if (!userManager.SupportsUserEmail)
            {
                throw new NotSupportedException($"{nameof(MapCustomIdentityApi)} requires a user store with email support.");
            }

            var userStore = sp.GetRequiredService<IUserStore<ApplicationUser>>();
            var emailStore = (IUserEmailStore<ApplicationUser>)userStore;
            var email = registration.Email;

            if (string.IsNullOrEmpty(email) || !EmailAddressAttribute.IsValid(email))
            {
                return CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidEmail(email)));
            }

            var user = new ApplicationUser()
            {
                FirstName = registration.FirstName,
                LastName = registration.LastName,
            };

            await userStore.SetUserNameAsync(user, email, CancellationToken.None);
            await emailStore.SetEmailAsync(user, email, CancellationToken.None);
            var result = await userManager.CreateAsync(user, registration.Password);

            if (!result.Succeeded)
            {
                return CreateValidationProblem(result);
            }

            await SendConfirmationEmailAsync(user, userManager, context, email, emailSender);
            return TypedResults.Ok();
        }).WithSummary("Register a new user")
        .WithDescription("Create a new user account with email and password.")
        ;

        routeGroup.MapPost("/login", async Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, ProblemHttpResult>>
            (
            [FromBody] LoginRequest login,
            [FromQuery] bool? useCookies,
            [FromQuery] bool? useSessionCookies,
            [FromServices] IAuthenticationTokenGenerator tokenGenerator,
            [FromServices] IServiceProvider sp) =>
        {
            var signInManager = sp.GetRequiredService<SignInManager<ApplicationUser>>();
            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

            var isPersistent = false;

            var user = await userManager.FindByEmailAsync(login.Email);


            if (user is null)
            {
                return TypedResults.Problem("UserNotFound", statusCode: StatusCodes.Status401Unauthorized);
            }

            var result = await signInManager.CheckPasswordSignInAsync(
                user: user,
                password: login.Password,
                lockoutOnFailure: true);

            if (result.RequiresTwoFactor)
            {
                if (!string.IsNullOrEmpty(login.TwoFactorCode))
                {
                    result = await signInManager.TwoFactorAuthenticatorSignInAsync(login.TwoFactorCode, isPersistent, rememberClient: isPersistent);
                }
                else if (!string.IsNullOrEmpty(login.TwoFactorRecoveryCode))
                {
                    result = await signInManager.TwoFactorRecoveryCodeSignInAsync(login.TwoFactorRecoveryCode);
                }
            }

            if (!result.Succeeded)
            {
                return TypedResults.Problem(result.ToString(), statusCode: StatusCodes.Status401Unauthorized);
            }

            var roles = await userManager.GetRolesAsync(user);
            var token = await tokenGenerator.GenerateTokenAsync(new(user, roles));

            var response = new AccessTokenResponse()
            {
                AccessToken = token.AcessToken,
                ExpiresIn = (long)token.ExpiresIn().TotalSeconds,
                RefreshToken = token.RefreshToken,
            };

            return TypedResults.Ok(response);

        })
        .WithSummary("Login")
        .WithDescription("Login with email and password")

        ;

        routeGroup.MapPost("/refresh", async Task<Results<Ok<AccessTokenResponse>, UnauthorizedHttpResult, SignInHttpResult, ChallengeHttpResult>>
            (
             HttpContext context,
            [FromBody] RefreshRequest refreshRequest,
            [FromServices] IAuthenticationTokenGenerator tokenGenerator,
            [FromServices] IServiceProvider sp
        ) =>
        {
            var tokenInfo = await tokenGenerator.ReadTokenAsync(new(refreshRequest.RefreshToken, false));

            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

            var user = await userManager.FindByIdAsync(tokenInfo.UserId);

            if (user is null)
            {
                return TypedResults.Unauthorized();
            }

            var roles = await userManager.GetRolesAsync(user);

            var token = await tokenGenerator.RefreshTokenAsync(new(user, refreshRequest.RefreshToken, roles));

            var response = new AccessTokenResponse()
            {
                AccessToken = token.AcessToken,
                ExpiresIn = (long)token.ExpiresIn().TotalSeconds,
                RefreshToken = token.RefreshToken,
            };

            return TypedResults.Ok(response);
        }).WithSummary("Refresh access token")
        .WithDescription("Refresh an expired access token using a refresh token.")
        ;

        routeGroup.MapGet("/confirmEmail", async Task<Results<ContentHttpResult, UnauthorizedHttpResult>>
            (
            [FromQuery] string userId,
            [FromQuery] string code,
            [FromQuery] string? changedEmail,
            [FromServices] IServiceProvider sp
            ) =>
        {
            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
            if (await userManager.FindByIdAsync(userId) is not { } user)
            {
                // We could respond with a 404 instead of a 401 like Identity UI, but that feels like unnecessary information.
                return TypedResults.Unauthorized();
            }

            try
            {
                code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            }
            catch (FormatException)
            {
                return TypedResults.Unauthorized();
            }

            IdentityResult result;

            if (string.IsNullOrEmpty(changedEmail))
            {
                result = await userManager.ConfirmEmailAsync(user, code);
            }
            else
            {
                // As with Identity UI, email and user name are one and the same. So when we update the email,
                // we need to update the user name.
                result = await userManager.ChangeEmailAsync(user, changedEmail, code);

                if (result.Succeeded)
                {
                    result = await userManager.SetUserNameAsync(user, changedEmail);
                }
            }

            if (!result.Succeeded)
            {
                return TypedResults.Unauthorized();
            }

            return TypedResults.Text("Thank you for confirming your email.");
        }).WithSummary("Confirm user's email")
        .WithDescription("Confirm a user's email address with a confirmation code.")

        .Add(endpointBuilder =>
        {
            var finalPattern = ((RouteEndpointBuilder)endpointBuilder).RoutePattern.RawText;
            confirmEmailEndpointName = $"{nameof(MapCustomIdentityApi)}-{finalPattern}";
            endpointBuilder.Metadata.Add(new EndpointNameMetadata(confirmEmailEndpointName));
        });

        routeGroup.MapPost("/resendConfirmationEmail", async Task<Ok>
            (
            [FromBody] ResendConfirmationEmailRequest resendRequest,
            HttpContext context,
            [FromServices] IServiceProvider sp) =>
        {
            var emailSender = sp.GetRequiredService<IEmailSender<ApplicationUser>>();

            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
            if (await userManager.FindByEmailAsync(resendRequest.Email) is not { } user)
            {
                return TypedResults.Ok();
            }

            await SendConfirmationEmailAsync(user, userManager, context, resendRequest.Email, emailSender);
            return TypedResults.Ok();
        }).WithSummary("Resend confirmation email")
        .WithDescription("Resend the email confirmation link to a user's email address.")
        ;

        routeGroup.MapPost("/forgotPassword", async Task<Results<Ok, ValidationProblem>>
            (
            [FromBody] ForgotPasswordRequest resetRequest,
            [FromServices] IServiceProvider sp) =>
        {
            var emailSender = sp.GetRequiredService<IEmailSender<ApplicationUser>>();
            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(resetRequest.Email);

            if (user is not null && await userManager.IsEmailConfirmedAsync(user))
            {
                var code = await userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                await emailSender.SendPasswordResetCodeAsync(user, resetRequest.Email, HtmlEncoder.Default.Encode(code));
            }

            // Don't reveal that the user does not exist or is not confirmed, so don't return a 200 if we would have
            // returned a 400 for an invalid code given a valid user email.
            return TypedResults.Ok();
        }).WithSummary("Request a password reset")
        .WithDescription("Request a password reset for a user by providing their email address.")
        ;

        routeGroup.MapPost("/resetPassword", async Task<Results<Ok, ValidationProblem>>
            (
            [FromBody] ResetPasswordRequest resetRequest,
            [FromServices] IServiceProvider sp) =>
        {
            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

            var user = await userManager.FindByEmailAsync(resetRequest.Email);

            if (user is null || !(await userManager.IsEmailConfirmedAsync(user)))
            {
                // Don't reveal that the user does not exist or is not confirmed, so don't return a 200 if we would have
                // returned a 400 for an invalid code given a valid user email.
                return CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidToken()));
            }

            IdentityResult result;
            try
            {
                var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetRequest.ResetCode));
                result = await userManager.ResetPasswordAsync(user, code, resetRequest.NewPassword);
            }
            catch (FormatException)
            {
                result = IdentityResult.Failed(userManager.ErrorDescriber.InvalidToken());
            }

            if (!result.Succeeded)
            {
                return CreateValidationProblem(result);
            }

            return TypedResults.Ok();
        }).WithSummary("Reset user password")
        .WithDescription("Reset a user's password using a reset code.")
        ;

        var accountGroup = routeGroup.MapGroup("/manage").RequireAuthorization();

        accountGroup.MapPost("/2fa", async Task<Results<Ok<TwoFactorResponse>, ValidationProblem, NotFound>>
            (
            ClaimsPrincipal claimsPrincipal,
            [FromBody] TwoFactorRequest tfaRequest,
            [FromServices] IServiceProvider sp) =>
        {
            var signInManager = sp.GetRequiredService<SignInManager<ApplicationUser>>();
            var userManager = signInManager.UserManager;
            if (await userManager.GetUserAsync(claimsPrincipal) is not { } user)
            {
                return TypedResults.NotFound();
            }

            if (tfaRequest.Enable == true)
            {
                if (tfaRequest.ResetSharedKey)
                {
                    return CreateValidationProblem("CannotResetSharedKeyAndEnable",
                        "Resetting the 2fa shared key must disable 2fa until a 2fa token based on the new shared key is validated.");
                }

                if (string.IsNullOrEmpty(tfaRequest.TwoFactorCode))
                {
                    return CreateValidationProblem("RequiresTwoFactor",
                        "No 2fa token was provided by the request. A valid 2fa token is required to enable 2fa.");
                }

                if (!await userManager.VerifyTwoFactorTokenAsync(user, userManager.Options.Tokens.AuthenticatorTokenProvider, tfaRequest.TwoFactorCode))
                {
                    return CreateValidationProblem("InvalidTwoFactorCode",
                        "The 2fa token provided by the request was invalid. A valid 2fa token is required to enable 2fa.");
                }

                await userManager.SetTwoFactorEnabledAsync(user, true);
            }
            else if (tfaRequest.Enable == false || tfaRequest.ResetSharedKey)
            {
                await userManager.SetTwoFactorEnabledAsync(user, false);
            }

            if (tfaRequest.ResetSharedKey)
            {
                await userManager.ResetAuthenticatorKeyAsync(user);
            }

            string[]? recoveryCodes = null;
            if (tfaRequest.ResetRecoveryCodes || (tfaRequest.Enable == true && await userManager.CountRecoveryCodesAsync(user) == 0))
            {
                var recoveryCodesEnumerable = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
                recoveryCodes = recoveryCodesEnumerable?.ToArray();
            }

            if (tfaRequest.ForgetMachine)
            {
                await signInManager.ForgetTwoFactorClientAsync();
            }

            var key = await userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(key))
            {
                await userManager.ResetAuthenticatorKeyAsync(user);
                key = await userManager.GetAuthenticatorKeyAsync(user);

                if (string.IsNullOrEmpty(key))
                {
                    throw new NotSupportedException("The user manager must produce an authenticator key after reset.");
                }
            }

            return TypedResults.Ok(new TwoFactorResponse
            {
                SharedKey = key,
                RecoveryCodes = recoveryCodes,
                RecoveryCodesLeft = recoveryCodes?.Length ?? await userManager.CountRecoveryCodesAsync(user),
                IsTwoFactorEnabled = await userManager.GetTwoFactorEnabledAsync(user),
                IsMachineRemembered = await signInManager.IsTwoFactorClientRememberedAsync(user),
            });
        }).WithSummary("Manage two-factor authentication (2FA)")
        .WithDescription("Enable or disable two-factor authentication for the current user.")
        ;

        accountGroup.MapGet("/info", async Task<Results<Ok<UserInfoResponse>, ValidationProblem, NotFound>>
            (
            ClaimsPrincipal claimsPrincipal,
            [FromServices] IServiceProvider sp) =>
        {
            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

            if (await userManager.GetUserAsync(claimsPrincipal) is not { } user)
            {
                return TypedResults.NotFound();
            }

            var res = new UserInfoResponse()
            {
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsEmailConfirmed = user.EmailConfirmed,
                OrganizationId = user.OrganizationId,
            };

            return TypedResults.Ok(res);
        }).WithSummary("Get user information")
        .WithDescription("Get the current user's information.")
        ;

        accountGroup.MapPost("/info", async Task<Results<Ok<UserInfoResponse>, ValidationProblem, NotFound>>
            (
            ClaimsPrincipal claimsPrincipal,
            [FromBody] UpdateUserRequest infoRequest,
            HttpContext context,
            [FromServices] IServiceProvider sp) =>
        {
            var emailSender = sp.GetRequiredService<IEmailSender<ApplicationUser>>();

            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
            if (await userManager.GetUserAsync(claimsPrincipal) is not { } user)
            {
                return TypedResults.NotFound();
            }

            if (!string.IsNullOrEmpty(infoRequest.NewEmail) && !EmailAddressAttribute.IsValid(infoRequest.NewEmail))
            {
                return CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidEmail(infoRequest.NewEmail)));
            }

            if (!string.IsNullOrEmpty(infoRequest.NewPassword))
            {
                if (string.IsNullOrEmpty(infoRequest.OldPassword))
                {
                    return CreateValidationProblem("OldPasswordRequired",
                        "The old password is required to set a new password. If the old password is forgotten, use /resetPassword.");
                }

                var changePasswordResult = await userManager.ChangePasswordAsync(user, infoRequest.OldPassword, infoRequest.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    return CreateValidationProblem(changePasswordResult);
                }
            }

            if (!string.IsNullOrEmpty(infoRequest.NewEmail))
            {
                var email = await userManager.GetEmailAsync(user);

                if (email != infoRequest.NewEmail)
                {
                    await SendConfirmationEmailAsync(user, userManager, context, infoRequest.NewEmail, emailSender, isChange: true);
                }
            }

            if (infoRequest.FirstName is not null)
            {
                user.FirstName = infoRequest.FirstName;
            }

            if (infoRequest.LastName is not null)
            {
                user.LastName = infoRequest.LastName;
            }

            await userManager.UpdateAsync(user);

            return TypedResults.Ok(await CreateInfoResponseAsync(user, userManager));
        }).WithSummary("Update user information")
        .WithDescription("Update the current user's information, such as email and password.")
        ;

        async Task SendConfirmationEmailAsync(
            ApplicationUser user,
            UserManager<ApplicationUser> userManager,
            HttpContext context,
            string email,
            IEmailSender<ApplicationUser> emailSender,
            bool isChange = false
            )
        {
            if (confirmEmailEndpointName is null)
            {
                throw new NotSupportedException("No email confirmation endpoint was registered!");
            }

            var code = isChange
                ? await userManager.GenerateChangeEmailTokenAsync(user, email)
                : await userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var userId = await userManager.GetUserIdAsync(user);
            var routeValues = new RouteValueDictionary()
            {
                ["userId"] = userId,
                ["code"] = code,
            };

            if (isChange)
            {
                // This is validated by the /confirmEmail endpoint on change.
                routeValues.Add("changedEmail", email);
            }

            var confirmEmailUrl = linkGenerator.GetUriByName(context, confirmEmailEndpointName, routeValues)
                ?? throw new NotSupportedException($"Could not find endpoint named '{confirmEmailEndpointName}'.");

            await emailSender.SendConfirmationLinkAsync(user, email, HtmlEncoder.Default.Encode(confirmEmailUrl));
        }

        return new IdentityEndpointsConventionBuilder(routeGroup);
    }

    private static ValidationProblem CreateValidationProblem(string errorCode, string errorDescription) =>
        TypedResults.ValidationProblem(new Dictionary<string, string[]> {
            { errorCode, [errorDescription] }
        });

    private static ValidationProblem CreateValidationProblem(IdentityResult result)
    {
        // We expect a single error code and description in the normal case.
        // This could be golfed with GroupBy and ToDictionary, but perf! :P
        Debug.Assert(!result.Succeeded);
        var errorDictionary = new Dictionary<string, string[]>(1);

        foreach (var error in result.Errors)
        {
            string[] newDescriptions;

            if (errorDictionary.TryGetValue(error.Code, out var descriptions))
            {
                newDescriptions = new string[descriptions.Length + 1];
                System.Array.Copy(descriptions, newDescriptions, descriptions.Length);
                newDescriptions[descriptions.Length] = error.Description;
            }
            else
            {
                newDescriptions = [error.Description];
            }

            errorDictionary[error.Code] = newDescriptions;
        }

        return TypedResults.ValidationProblem(errorDictionary);
    }

    private static async Task<UserInfoResponse> CreateInfoResponseAsync(ApplicationUser user, UserManager<ApplicationUser> userManager)
    {
        return new()
        {
            Email = await userManager.GetEmailAsync(user) ?? throw new NotSupportedException("Users must have an email."),
            OrganizationId = user.OrganizationId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsEmailConfirmed = await userManager.IsEmailConfirmedAsync(user),
        };
    }

    // Wrap RouteGroupBuilder with a non-public type to avoid a potential future behavioral breaking change.
    private sealed class IdentityEndpointsConventionBuilder(RouteGroupBuilder inner) : IEndpointConventionBuilder
    {
        private IEndpointConventionBuilder InnerAsConventionBuilder => inner;

        public void Add(Action<EndpointBuilder> convention) => InnerAsConventionBuilder.Add(convention);
        public void Finally(Action<EndpointBuilder> finallyConvention) => InnerAsConventionBuilder.Finally(finallyConvention);
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    private sealed class FromBodyAttribute : Attribute, IFromBodyMetadata
    {
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    private sealed class FromServicesAttribute : Attribute, IFromServiceMetadata
    {
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    private sealed class FromQueryAttribute : Attribute, IFromQueryMetadata
    {
        public string? Name => null;
    }
}