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

            builder.Property(p => p.EmbeddingJson)
                .IsRequired();

            builder.HasOne(p => p.Activity)
                .WithOne(a => a.Embedding)
                .HasForeignKey<ActivityEmbedding>(p => p.ActivityId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
