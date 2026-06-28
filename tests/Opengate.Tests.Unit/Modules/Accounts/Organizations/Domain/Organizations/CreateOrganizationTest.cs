using Opengate.Modules.Accounts.Organizations.Domain.Models;
using Opengate.Modules.Shared.Domain.ValueObjects.Emails;

namespace Opengate.Tests.Unit.Modules.Accounts.Organizations.Domain.Organizations;

public class CreateOrganizationTest
{
    [Fact]
    public void CreateOrganizationTest_ShouldCreateOrganization()
    {
        //Given
        var org = Organization.Create(
            name: "Organization Name",
            description: "Description",
            ownerEmail: Email.Parse("owner@EMAIL.com"),
            ownerPhoneNumber: "1234567890",
            document: "1234567890",
            documentType: "document",
            country: "Br",
            logo: "logo"
        );

        //Then
        org.Slug.ToString().ShouldBe("organization-name");
        org.Name.ShouldBe("Organization Name");
        org.OwnerEmail.ToString().ShouldBe("OWNER@EMAIL.COM");
        org.Country.ShouldBe("BR");
    }
}