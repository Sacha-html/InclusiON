using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InclusiON.Entities.Models;

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

            // Seed Data - 7 metodos de login adaptados por nivel de autonomia
            builder.HasData(
                new LoginMethod
                {
                    Id = 1,
                    Code = "STANDARD",
                    Name = "Email y Contrasena",
                    Description = "Login tradicional con email y contrasena",
                    MinAutonomyLevel = 1,
                    RequiresEmail = true,
                    RequiresPassword = true,
                    RequiresPin = false,
                    RequiresEmojiSequence = false,
                    RequiresColorShape = false,
                    RequiresProfileSelect = false,
                    RequiresSupervisor = false,
                    DisplayOrder = 1,
                    IsActive = true
                },
                new LoginMethod
                {
                    Id = 2,
                    Code = "PIN",
                    Name = "PIN Numerico",
                    Description = "Login con nombre de usuario y PIN de 4-6 digitos",
                    MinAutonomyLevel = 1,
                    RequiresEmail = false,
                    RequiresPassword = false,
                    RequiresPin = true,
                    RequiresEmojiSequence = false,
                    RequiresColorShape = false,
                    RequiresProfileSelect = false,
                    RequiresSupervisor = false,
                    DisplayOrder = 2,
                    IsActive = true
                },
                new LoginMethod
                {
                    Id = 3,
                    Code = "EMOJI_SEQUENCE",
                    Name = "Secuencia de Emojis",
                    Description = "Login seleccionando 4 emojis en orden",
                    MinAutonomyLevel = 2,
                    RequiresEmail = false,
                    RequiresPassword = false,
                    RequiresPin = false,
                    RequiresEmojiSequence = true,
                    RequiresColorShape = false,
                    RequiresProfileSelect = false,
                    RequiresSupervisor = false,
                    DisplayOrder = 3,
                    IsActive = true
                },
                new LoginMethod
                {
                    Id = 4,
                    Code = "COLOR_SHAPE",
                    Name = "Colores y Formas",
                    Description = "Login seleccionando 4 colores y formas en orden",
                    MinAutonomyLevel = 2,
                    RequiresEmail = false,
                    RequiresPassword = false,
                    RequiresPin = false,
                    RequiresEmojiSequence = false,
                    RequiresColorShape = true,
                    RequiresProfileSelect = false,
                    RequiresSupervisor = false,
                    DisplayOrder = 4,
                    IsActive = true
                },
                new LoginMethod
                {
                    Id = 5,
                    Code = "SUPERVISED",
                    Name = "Supervisado",
                    Description = "Login requiere desbloqueo por familiar o profesional",
                    MinAutonomyLevel = 3,
                    RequiresEmail = false,
                    RequiresPassword = false,
                    RequiresPin = false,
                    RequiresEmojiSequence = false,
                    RequiresColorShape = false,
                    RequiresProfileSelect = false,
                    RequiresSupervisor = true,
                    DisplayOrder = 5,
                    IsActive = true
                },
                new LoginMethod
                {
                    Id = 6,
                    Code = "TRUSTED_DEVICE",
                    Name = "Dispositivo Confiable",
                    Description = "Login automatico en dispositivos previamente autorizados",
                    MinAutonomyLevel = 3,
                    RequiresEmail = false,
                    RequiresPassword = false,
                    RequiresPin = false,
                    RequiresEmojiSequence = false,
                    RequiresColorShape = false,
                    RequiresProfileSelect = false,
                    RequiresSupervisor = false,
                    DisplayOrder = 6,
                    IsActive = true
                },
                new LoginMethod
                {
                    Id = 7,
                    Code = "PROFILE_SELECT",
                    Name = "Seleccion de Perfil",
                    Description = "Login seleccionando nombre y avatar del usuario",
                    MinAutonomyLevel = 3,
                    RequiresEmail = false,
                    RequiresPassword = false,
                    RequiresPin = false,
                    RequiresEmojiSequence = false,
                    RequiresColorShape = false,
                    RequiresProfileSelect = true,
                    RequiresSupervisor = false,
                    DisplayOrder = 7,
                    IsActive = true
                }
            );
        }
    }
}
