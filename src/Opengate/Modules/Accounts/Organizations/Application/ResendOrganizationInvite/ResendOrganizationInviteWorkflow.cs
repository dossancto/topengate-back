using Microsoft.EntityFrameworkCore;

using Opengate.Modules.Accounts.Organizations.Domain.Enums;
using Opengate.Modules.Accounts.Organizations.Domain.Exceptions.Invites;
using Opengate.Modules.Accounts.Organizations.Domain.Models;
using Opengate.Modules.Shared.Adapters.Databases;
using Opengate.Modules.Shared.Submodules.Integrations.EmailSenders.EmailsTemplates.OrganizationInvite;

namespace Opengate.Modules.Accounts.Organizations.Application.ResendOrganizationInvite;

/// <summary>
/// Workflow for resending an organization invite.
/// </summary>
public class ResendOrganizationInviteWorkflow(
        IOrganizationInviteEmailSender emailSender,
        ApplicationDbContext db
        )
{
    private readonly IOrganizationInviteEmailSender _emailSender = emailSender;

    private readonly ApplicationDbContext _db = db;

    /// <summary>
    /// Executes the workflow to resend an organization invite.
    /// </summary>
    /// <param name="input">Input containing invite and organization IDs.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Output of the resend invite operation.</returns>
    public Aff<ResendOrganizationInviteOutput> Execute(
        ResendOrganizationInviteInput input,
        CancellationToken ct = default)
    => from now in Eff(() => DateTimeOffset.UtcNow)
       from organization in FetchOrganization(input, ct)
       from invite in FetchInvite(input, ct)

       from updatedInvite in Reinvite(invite)

       from _ in Save(organization, updatedInvite, ct)

       select new ResendOrganizationInviteOutput();

    /// <summary>
    /// Fetches the organization invite by input parameters.
    /// </summary>
    /// <param name="input">Input containing invite and organization IDs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The organization invite if found.</returns>
    /// <exception cref="ResendOrganizationInviteExceptionException">Thrown if invite is not found.</exception>
    private Aff<OrganizationInvite> FetchInvite(ResendOrganizationInviteInput input, CancellationToken cancellationToken = default)
    => Aff(async () =>
    {
        var invite = await _db.OrganizationInvites
        .Where(invite => invite.Id == input.InviteId
            && invite.OrganizationId == input.OrganizationId
            && invite.Status == OrganizationInviteStatus.Pending
            )
        .FirstOrDefaultAsync(cancellationToken)
        ;

        if (invite is null)
        {
            throw new ResendOrganizationInviteExceptionException(
                error: ResendOrganizationInviteExceptionError.InviteNotFound,
                organizationId: input.OrganizationId.ToString(),
                inviteId: input.InviteId.ToString()
            );
        }

        return invite;
    });

    /// <summary>
    /// Fetches the organization by input parameters.
    /// </summary>
    /// <param name="input">Input containing organization ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The organization if found.</returns>
    /// <exception cref="ResendOrganizationInviteExceptionException">Thrown if organization is not found.</exception>
    private Aff<Organization> FetchOrganization(ResendOrganizationInviteInput input, CancellationToken ct = default)
    => Aff(async () =>
    {
        var org = await _db.Organizations
            .FirstOrDefaultAsync(o => o.Id == input.OrganizationId, ct);

        if (org is null)
        {
            throw new ResendOrganizationInviteExceptionException(
                    error: ResendOrganizationInviteExceptionError.InviteNotFound,
                    organizationId: input.OrganizationId.ToString(),
                    inviteId: input.InviteId.ToString()
                );
        }

        return org.ToOrganization();
    });

    /// <summary>
    /// Creates a new reinvited organization invite.
    /// </summary>
    /// <param name="invite">The original organization invite.</param>
    /// <returns>The updated organization invite.</returns>
    private static Eff<OrganizationInvite> Reinvite(OrganizationInvite invite)
    => Eff(() => invite.Resend());

    /// <summary>
    /// Saves the updated invite and sends the invite email.
    /// </summary>
    /// <param name="organization">The organization.</param>
    /// <param name="invite">The updated organization invite.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Unit value on success.</returns>
    private Aff<Unit> Save(Organization organization, OrganizationInvite invite, CancellationToken ct = default)
    => Aff(async () =>
    {
        _db.OrganizationInvites.Update(invite);

        var res = await _emailSender.Send(new(
                TargetEmail: invite.Email,
                OrganizationName: organization.Name,
                InviteCode: invite.GetInviteCode(),
                ExpiresAt: default
        )).Run();

        res.ThrowIfFail();

        await _db.SaveChangesAsync(ct);

        return unit;
    });
}