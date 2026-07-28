using InclusiON.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InclusiON.Data.Configurations
{
    public class ActivityEmbeddingConfiguration : IEntityTypeConfiguration<ActivityEmbedding>
    {
        public void Configure(EntityTypeBuilder<ActivityEmbedding> builder)
        {
            builder.ToTable("ActivityEmbeddings");

            builder.HasKey(p => p.ActivityId);

            builder.Property(p => p.Model)
                .IsRequired()
                .HasMaxLength(100);

            // Embedding column is vector(384) — created via raw SQL in migration, not mapped by EF
            builder.Ignore(p => p.Embedding);

            builder.HasOne(p => p.Activity)
                .WithOne(a => a.Embedding)
                .HasForeignKey<ActivityEmbedding>(p => p.ActivityId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
