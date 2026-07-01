using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Opengate.Modules.Projects.Submodules.Checkouts.Adapters.Entities;

namespace Opengate.Modules.Projects.Submodules.Checkouts.Adapters.Configurations;

// public class CheckoutConfigurationEntityConfiguration : IEntityTypeConfiguration<CheckoutConfigurationEntity>
// {
//     public void Configure(EntityTypeBuilder<CheckoutConfigurationEntity> b)
//     {
//         b.ToTable("CheckoutConfigurations");
//
//
//         b.HasKey(x => x.Id);
//
//         b.Property(x => x.Name)
//             .HasMaxLength(255)
//             .IsRequired();
//
//         b.Property(x => x.Description)
//             .HasMaxLength(1024)
//             .IsRequired();
//
//         b.Property(x => x.ProjectId)
//             .IsRequired();
//
//         b.Property(x => x.OrganizationId)
//             .IsRequired();
//
//         b.Property(x => x.CreatorUserId)
//             .IsRequired();
//
//         b.ComplexProperty(x => x.ReceiverBankAccountDetails, r =>
//         {
//             r.Property(x => x.BankName)
//                 .HasMaxLength(512)
//                 .IsRequired();
//
//             r.Property(x => x.BankAccountType)
//                 .IsRequired();
//
//             r.Property(x => x.Name)
//                 .HasMaxLength(512)
//                 .IsRequired();
//
//             r.Property(x => x.Document)
//                 .HasMaxLength(512)
//                 .IsRequired();
//
//             r.Property(x => x.DocumentType)
//                 .HasMaxLength(512)
//                 .IsRequired();
//
//             r.Property(x => x.AccountNumber)
//                 .HasMaxLength(512)
//                 .IsRequired();
//
//             r.Property(x => x.Agency)
//                 .HasMaxLength(512)
//                 .IsRequired();
//
//             r.Property(x => x.KeyType)
//                 .IsRequired();
//
//             r.Property(x => x.PixKey)
//                 .HasMaxLength(255)
//                 .IsRequired();
//         });
//
//         b.ComplexProperty(x => x.PaymentDetails, p =>
//         {
//             p.Property(x => x.Currency)
//                 .HasMaxLength(3)
//                 .IsRequired();
//
//             p.Property(x => x.ServiceType)
//                 .IsRequired();
//
//             p.Property(x => x.PaymentTypes)
//                 .IsRequired();
//         });
//
//         b.ComplexProperty(x => x.Webhook, h =>
//         {
//             h.ToJson();
//         });
//
//         b.Property(x => x.Webhook)
//             .IsRequired();
//
//         b.Property(x => x.CreatedBy)
//             .IsRequired();
//
//         b.Property(x => x.CreatedAt)
//             .IsRequired();
//
//         b.Property(x => x.UpdatedAt)
//             .IsRequired();
//
//         b.Property(x => x.DeletedAt)
//             .IsRequired();
//     }
// }