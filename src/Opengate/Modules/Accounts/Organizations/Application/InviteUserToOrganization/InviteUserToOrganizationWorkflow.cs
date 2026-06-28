using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Opengate.Modules.Accounts.Organizations.Domain.Exceptions.Invites;
using Opengate.Modules.Accounts.Organizations.Domain.Models;
using Opengate.Modules.Accounts.Users.Models;
using Opengate.Modules.Shared.Adapters.Databases;
using Opengate.Modules.Shared.Submodules.Integrations.EmailSenders.EmailsTemplates.OrganizationInvite;

namespace Opengate.Modules.Accounts.Organizations.Application.InviteUserToOrganization;

public class InviteUserToOrganizationWorkflow(
        UserManager<ApplicationUser> userManager,
        IOrganizationInviteEmailSender emailSender,
        ApplicationDbContext db,
        IUserStore<ApplicationUser> userStore
        )
{
    private readonly UserManager<ApplicationUser> userManager = userManager;
    private readonly IOrganizationInviteEmailSender emailSender = emailSender;
    private readonly ApplicationDbContext db = db;
    private readonly IUserStore<ApplicationUser> userStore = userStore;

    public Aff<Unit> Execute(InviteUserToOrganizationInput input, CancellationToken ct = default)
    => from now in Eff(() => DateTimeOffset.UtcNow)
       from _1 in CheckUserAlreadyInvited(input, now, ct)

       from sender in FetchSenderUser(input, ct)
       from _2 in CheckValidSenderUser(input, sender, ct)
       from organization in FetchOrganization(input, ct)

       from invite in CreateOrganizationInvite(input)
       from createdUser in Createuser(input)

       from _ in Save(invite, organization, createdUser, ct)

       select unit;

    private Aff<ApplicationUser> FetchSenderUser(InviteUserToOrganizationInput input, CancellationToken ct = default)
    => Aff(async () =>
    {
        var user = await userManager.FindByIdAsync(input.SenderUserId.ToString())
            ?? throw new InviteUserToOrganizationException(
                error: InviteUserToOrganizationError.SenderNotFound,
                organizationId: input.OrganizationId.ToString(),
                targetEmail: input.TargetEmail.ToString()
        );

        return user;
    });

    private Aff<Organization> FetchOrganization(InviteUserToOrganizationInput input, CancellationToken ct = default)
    => Aff(async () =>
    {
        var org = await db.Organizations
            .FirstOrDefaultAsync(o => o.Id == input.OrganizationId, ct)
            ?? throw new InviteUserToOrganizationException(
                error: InviteUserToOrganizationError.OrganizationNotFound,
                organizationId: input.OrganizationId.ToString(),
                targetEmail: input.TargetEmail.ToString()
            );

        return org.ToOrganization();
    });

    private Eff<OrganizationInvite> CreateOrganizationInvite(InviteUserToOrganizationInput input)
    => Eff(() => OrganizationInvite.Create(
            organizationId: input.OrganizationId,
            email: input.TargetEmail.ToString(),
            phoneNumber: null
            ));

    private Eff<ApplicationUser> Createuser(InviteUserToOrganizationInput input)
        => Eff(() => new ApplicationUser
        {
            Email = input.TargetEmail.ToString(),
            UserName = input.TargetEmail.ToString(),
            FirstName = input.TargetUserFirstName,
            OrganizationId = input.OrganizationId.ToString(),
            LastName = input.TargetUserLastName,
        });

    private Aff<Unit> Save(
        OrganizationInvite invite,
        Organization organization,
        ApplicationUser createdUser,
        CancellationToken ct = default
        )
    => Aff(async () =>
    {
        await db.OrganizationInvites.AddAsync(invite);
        var emailStore = (IUserEmailStore<ApplicationUser>)userStore;

        var emailSendResponse = await emailSender.Send(new InviteUserToOrganizationInviteInput(
                TargetEmail: invite.Email,
                OrganizationName: organization.Name,
                InviteCode: invite.GetInviteCode(),
                ExpiresAt: invite.ExpiresAt
            )).Run();

        emailSendResponse.ThrowIfFail();

        await userStore.SetUserNameAsync(createdUser, createdUser.Email, CancellationToken.None);

        await emailStore.SetEmailAsync(createdUser, createdUser.Email, CancellationToken.None);

        var result = await userManager.CreateAsync(createdUser);

        if (result.Succeeded)
        {
            // TODO: Define a role to user
            // await userManager.AddToRoleAsync(user, Roles.User);

            await db.SaveChangesAsync(ct);
        }
        else
        {
            throw new InviteUserToOrganizationException(
                error: InviteUserToOrganizationError.UserCreationFailed,
                organizationId: organization.Id.ToString(),
                targetEmail: createdUser.Email ?? ""
            );
        }

        return unit;
    });

    private Aff<Unit> CheckValidSenderUser(InviteUserToOrganizationInput input, ApplicationUser user, CancellationToken ct = default)
        => Aff(async () =>
        {
            if (string.IsNullOrWhiteSpace(user.OrganizationId))
            {
                throw new InviteUserToOrganizationException(
                                error: InviteUserToOrganizationError.SenderHasNoOrganization,
                                organizationId: "",
                                targetEmail: input.TargetEmail.ToString()
                );
            }

            return unit;
        });

    private Aff<Unit> CheckUserAlreadyInvited(InviteUserToOrganizationInput input, DateTimeOffset offsetNow, CancellationToken ct = default)
    => Aff(async () =>
    {
        var now = offsetNow.UtcDateTime;

        var targetEmail = input.TargetEmail.ToString().ToLower();

        var alreadyInvited = await db.OrganizationInvites
                .AnyAsync(x => x.OrganizationId == input.OrganizationId
                            && x.Email.ToLower() == targetEmail
                            && x.IsDeleted == false
                            && x.ExpiresAt > now, ct);

        if (alreadyInvited)
        {
            throw new InviteUserToOrganizationException(
                            error: InviteUserToOrganizationError.InviteAlreadySent,
                            organizationId: input.OrganizationId.ToString(),
                            targetEmail: input.TargetEmail.ToString()
            );
        }

        var userAlreadyMember = await db.Users
                .AnyAsync(x => x.OrganizationId == input.OrganizationId.ToString()
                            && string.Equals(x.Email!.ToLower(), targetEmail), ct);

        if (userAlreadyMember)
        {
            throw new InviteUserToOrganizationException(
                            error: InviteUserToOrganizationError.UserAlreadyMember,
                            organizationId: input.OrganizationId.ToString(),
                            targetEmail: input.TargetEmail.ToString()
            );
        }

        return unit;
    });
}