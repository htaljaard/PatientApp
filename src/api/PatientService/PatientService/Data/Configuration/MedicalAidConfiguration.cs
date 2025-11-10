using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientService.API.Domain.ValueObjects;

namespace PatientService.API.Data.Configuration;

public class MedicalAidConfiguration : IEntityTypeConfiguration<MedicalAidDetails>
{
    void IEntityTypeConfiguration<MedicalAidDetails>.Configure(EntityTypeBuilder<MedicalAidDetails> builder)
    {
        builder.HasKey(m => m.Id);

        builder.OwnsMany(m => m.PrivateHealthFundAccounts, pha =>
        {
            pha.WithOwner().HasForeignKey("MedicalAidDetailsId");
            pha.HasKey("Id");
            pha.Property(p => p.ProviderName).IsRequired();
            pha.Property(p => p.AccountNumber).IsRequired();
            pha.HasIndex(p => new { p.ProviderName, p.AccountNumber }).IsUnique();
        });
    }
}
