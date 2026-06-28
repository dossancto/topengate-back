using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Opengate.Modules.Accounts.Organizations.Domain.Enums;
using Opengate.Modules.Accounts.Organizations.Domain.Exceptions.Invites;
using Opengate.Modules.Accounts.Organizations.Domain.Models;
using Opengate.Modules.Accounts.Users.Models;
using Opengate.Modules.Shared.Adapters.Databases;

namespace Opengate.Modules.Accounts.Organizations.Application.ChangeInviteStatus;

public class ChangeInviteStatusWorkflow(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager
        )
{
    private readonly ApplicationDbContext db = db;
    private readonly UserManager<ApplicationUser> userManager = userManager;

    public Aff<ChangeInviteStatusOutput> Execute(ChangeInviteStatusInput input, CancellationToken cancellationToken = default)
    =>
    // Fetch
    from user in FetchUser(input, cancellationToken)
    from invite in FetchInvite(input, cancellationToken)
    from organization in FetchOrganization(invite, cancellationToken)

        // Business logic
    from updatedInvite in ChangeInviteStatus(input, invite)
    from updatedUser in ChangeUserToOrganization(user, organization)

        // Persist
    from _ in Save(updatedInvite, updatedUser, cancellationToken)

        // Format output
    select new ChangeInviteStatusOutput();

    private Aff<OrganizationInvite> FetchInvite(ChangeInviteStatusInput input, CancellationToken cancellationToken)
    => Aff(async () =>
    {
        var now = DateTimeOffset.UtcNow;

        var invite = await db.OrganizationInvites
        .Where(x =>
                x.Id == input.InviteId
                && x.Email.ToLower() == input.UserEmail.ToLower()
                && x.Status == OrganizationInviteStatus.Pending
                && x.ExpiresAt > now)
        .FirstOrDefaultAsync(cancellationToken)
        ?? throw new ChangeInviteStatusException(
                inviteId: input.InviteId,
                error: ChangeInviteStatusError.InviteNotFound
            );

        return invite;
    });

    private Aff<ApplicationUser> FetchUser(ChangeInviteStatusInput input, CancellationToken cancellationToken)
    => Aff(async () =>
    {
        var user = await userManager.FindByIdAsync(input.UserId)
        ?? throw new KeyNotFoundException("User not found.");

        return user;
    });

    private static Eff<ApplicationUser> ChangeUserToOrganization(ApplicationUser user, Organization organization)
    => Eff(() =>
    {
        user.OrganizationId = organization.Id.ToString();

        return user;
    });

    private Aff<Organization> FetchOrganization(OrganizationInvite invite, CancellationToken cancellationToken)
    => Aff(async () =>
    {
        var organization = await db.Organizations
            .Where(x => x.Id == invite.OrganizationId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Organization not found.");

        return organization.ToOrganization();
    });

    private Aff<Unit> Save(
        OrganizationInvite updatedInvite,
        ApplicationUser updatedUser,
        CancellationToken cancellationToken
        )
    => Aff(async () =>
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        db.OrganizationInvites.Update(updatedInvite);

        await db.SaveChangesAsync(cancellationToken);

        var updateUserRes = await Aff(async () => await userManager.UpdateAsync(updatedUser)).Run();

        if (updateUserRes.IsFail)
        {
            await transaction.RollbackAsync(cancellationToken);

            updateUserRes.ThrowIfFail();
        }

        await transaction.CommitAsync(cancellationToken);

        return unit;
    });

    /// <summary>
    /// Change the status of the invite based on the input.
    /// </summary>
    private static Eff<OrganizationInvite> ChangeInviteStatus(ChangeInviteStatusInput input, OrganizationInvite invite)
    => Eff(() =>
    {
        var updatedInvite = input.Status switch
        {
            OrganizationInviteStatus.Accepted => invite.Accept(),
            OrganizationInviteStatus.Rejected => invite.Reject(),
            _ => throw new ChangeInviteStatusException(
                    inviteId: input.InviteId,
                    error: ChangeInviteStatusError.InvalidStatus
                )
        };

        return updatedInvite;
    });
}