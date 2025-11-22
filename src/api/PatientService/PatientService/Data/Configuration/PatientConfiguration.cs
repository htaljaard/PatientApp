using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PatientService.API.Domain.Entities;

namespace PatientService.API.Data.Configuration;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    void IEntityTypeConfiguration<Patient>.Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.Email).IsUnique();

        builder.HasOne(p => p.MedicalAidDetails)
               .WithOne()
               .HasForeignKey<Patient>(p => p.Id)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.Email)
               .IsUnique();


    }
}
