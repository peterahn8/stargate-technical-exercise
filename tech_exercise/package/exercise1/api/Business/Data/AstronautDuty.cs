using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace StargateAPI.Business.Data
{
    [Table("AstronautDuty")]
    public class AstronautDuty
    {
        public int Id { get; set; }

        public int PersonId { get; set; }

        public string Rank { get; set; } = string.Empty;

        public string DutyTitle { get; set; } = string.Empty;

        public DateTime DutyStartDate { get; set; }

        public DateTime? DutyEndDate { get; set; }

        // Always set for a stored row; null! tells the compiler EF fills it in.
        public virtual Person Person { get; set; } = null!;
    }

    public class AstronautDutyConfiguration : IEntityTypeConfiguration<AstronautDuty>
    {
        public void Configure(EntityTypeBuilder<AstronautDuty> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            // Kept explicitly: the filtered index below doesn't cover a person's past duties.
            builder.HasIndex(x => x.PersonId);
            // At most one current duty (no end date) per person, enforced by the database (RULE-3, RULE-4, D14).
            builder.HasIndex(x => x.PersonId, "IX_AstronautDuty_PersonId_Current")
                .IsUnique()
                .HasFilter("\"DutyEndDate\" IS NULL");
        }
    }
}
