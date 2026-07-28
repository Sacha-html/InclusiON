using InclusiON.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InclusiON.Data.Configurations;

public class PersonEmbeddingConfiguration : IEntityTypeConfiguration<PersonEmbedding>
{
    public void Configure(EntityTypeBuilder<PersonEmbedding> builder)
    {
        builder.ToTable("PersonEmbeddings");

        builder.HasKey(p => p.PersonId);

        builder.Property(p => p.Model)
            .IsRequired()
            .HasMaxLength(100);

        // Embedding column is vector(384) — created via raw SQL in migration, not mapped by EF
        builder.Ignore(p => p.Embedding);

        builder.HasOne(p => p.Person)
            .WithOne(p => p.Embedding)
            .HasForeignKey<PersonEmbedding>(p => p.PersonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
