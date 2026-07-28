using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Domain.Models;

namespace InclusiON.Data.Configurations
{
    public class LoginMethodConfiguration : IEntityTypeConfiguration<LoginMethod>
    {
        public void Configure(EntityTypeBuilder<LoginMethod> builder)
        {
            builder.ToTable("LoginMethods");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Id)
                .ValueGeneratedOnAdd();

            builder.Property(m => m.Code)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(m => m.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(m => m.Description)
                .HasMaxLength(255);

            builder.Property(m => m.IsActive)
                .HasDefaultValue(true);

            builder.HasIndex(m => m.Code)
                .IsUnique();

            builder.HasData(
                new LoginMethod
                {
                    Id = 1,
                    Code = "STANDARD",
                    Name = "Email y contraseña",
                    Description = "Login visual con nombre de usuario y contraseña",
                    MinAutonomyLevel = 1,
                    RequiresEmail = false,
                    RequiresPassword = true,
                    RequiresPin = false,
                    RequiresSupervisor = false,
                    DisplayOrder = 1,
                    IsActive = true
                },
                new LoginMethod
                {
                    Id = 2,
                    Code = "PIN",
                    Name = "PIN Numerico",
                    Description = "Login con nombre de usuario y PIN de 4 digitos",
                    MinAutonomyLevel = 1,
                    RequiresEmail = false,
                    RequiresPassword = false,
                    RequiresPin = true,
                    RequiresSupervisor = false,
                    DisplayOrder = 2,
                    IsActive = true
                },
                new LoginMethod
                {
                    Id = 3,
                    Code = "ASSISTED",
                    Name = "Login Asistido",
                    Description = "Login asistido donde un familiar o profesional autoriza el acceso",
                    MinAutonomyLevel = 3,
                    RequiresEmail = false,
                    RequiresPassword = false,
                    RequiresPin = false,
                    RequiresSupervisor = true,
                    DisplayOrder = 3,
                    IsActive = true
                }
            );
        }
    }
}
