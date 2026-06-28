using Microsoft.EntityFrameworkCore;

using Opengate.Modules.Accounts.Organizations.Adapters.Entities;
using Opengate.Modules.Accounts.Organizations.Domain.Exceptions;
using Opengate.Modules.Accounts.Organizations.Domain.Models;
using Opengate.Modules.Shared.Adapters.Databases;
using Opengate.Modules.Shared.Domain.ValueObjects.Emails;
using Opengate.Modules.Shared.Submodules.Integrations.EmailSenders.EmailsTemplates.OrganizationInvite;

namespace Opengate.Modules.Accounts.Organizations.Application.InitOrganization;

/// <summary>
/// Workflow to init a new organization.
/// Creates a new organization and sends an invite to the owner.
/// </summary>
public class InitOrganizationWorkflow(
        IOrganizationInviteEmailSender emailSender,
        ApplicationDbContext db
        )
{
    private readonly IOrganizationInviteEmailSender emailSender = emailSender;
    private readonly ApplicationDbContext db = db;

    public Aff<InitOrganizationResponse> Execute(InitOrganizationInput input)
        => from newOrganization in BuildNewOrganization(input)
           from _2 in ValidateOrganization(newOrganization)
           from ownerinvite in BuildNewOwnerInvite(newOrganization)
           from _1 in SaveAndSendInvite(newOrganization, ownerinvite)
           select new InitOrganizationResponse(newOrganization.Id);

    private Aff<Unit> ValidateOrganization(Organization org)
    => Aff(async () =>
    {
        var slug = org.Slug.ToString();
        var exists = await db.Organizations.AnyAsync(x => x.Slug == slug);

        if (exists)
        {
            throw InitOrganizationException.OrganizationAlreadyExists(org.Slug.ToString());
        }

        return unit;
    });

    private Aff<Unit> SaveAndSendInvite(Organization org, OrganizationInvite invite)
    => Aff(async () =>
    {
        await using var transaction = await db.Database.BeginTransactionAsync();

        var orgMOdel = OrganizationEntity.From(org);

        await db.Organizations.AddAsync(orgMOdel);
        await db.OrganizationInvites.AddAsync(invite);
        await db.SaveChangesAsync();

        var emailSendResponse = await emailSender.Send(new InviteUserToOrganizationInviteInput(
                TargetEmail: invite.Email,
                OrganizationName: org.Name,
                InviteCode: invite.GetInviteCode(),
                ExpiresAt: invite.ExpiresAt
            )).Run();

        if (emailSendResponse.IsFail)
        {
            await transaction.RollbackAsync();

            emailSendResponse.ThrowIfFail();
        }

        await transaction.CommitAsync();

        return unit;
    });

    private static Eff<OrganizationInvite> BuildNewOwnerInvite(Organization organization)
    => Eff(() => OrganizationInvite.Create(
            organizationId: organization.Id,
            email: organization.OwnerEmail.ToString(),
            phoneNumber: organization.OwnerPhoneNumber
    ));

    private static Eff<Organization> BuildNewOrganization(InitOrganizationInput input)
    => Eff(() => Organization.Create(
            name: input.Name,
            description: input.Description,
            ownerEmail: Email.Parse(input.OwnerEmail),
            ownerPhoneNumber: input.OwnerPhoneNumber,
            document: input.Document,
            documentType: input.DocumentType,
            country: input.Country,
            logo: input.Logo
    ));
}