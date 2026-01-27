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

            // Seed Data - 3 metodos de login activos para personas con discapacidad
            // Metodos deprecados (3, 4, 6, 7) se mantienen para compatibilidad pero con IsActive = false
            builder.HasData(
                new LoginMethod
                {
                    Id = 1,
                    Code = "STANDARD",
                    Name = "Email y Contrasena",
                    Description = "Login visual con nombre de usuario y contrasena",
                    MinAutonomyLevel = 1,
                    RequiresEmail = false,
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
                    Description = "Login con nombre de usuario y PIN de 4 digitos",
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
                // Metodo deprecado - mantenido para compatibilidad
                new LoginMethod
                {
                    Id = 3,
                    Code = "EMOJI_SEQUENCE",
                    Name = "Secuencia de Emojis (Deprecado)",
                    Description = "Login seleccionando 4 emojis en orden - DEPRECADO",
                    MinAutonomyLevel = 2,
                    RequiresEmail = false,
                    RequiresPassword = false,
                    RequiresPin = false,
                    RequiresEmojiSequence = true,
                    RequiresColorShape = false,
                    RequiresProfileSelect = false,
                    RequiresSupervisor = false,
                    DisplayOrder = 3,
                    IsActive = false
                },
                // Metodo deprecado - mantenido para compatibilidad
                new LoginMethod
                {
                    Id = 4,
                    Code = "COLOR_SHAPE",
                    Name = "Colores y Formas (Deprecado)",
                    Description = "Login seleccionando 4 colores y formas en orden - DEPRECADO",
                    MinAutonomyLevel = 2,
                    RequiresEmail = false,
                    RequiresPassword = false,
                    RequiresPin = false,
                    RequiresEmojiSequence = false,
                    RequiresColorShape = true,
                    RequiresProfileSelect = false,
                    RequiresSupervisor = false,
                    DisplayOrder = 4,
                    IsActive = false
                },
                new LoginMethod
                {
                    Id = 5,
                    Code = "ASSISTED",
                    Name = "Login Asistido",
                    Description = "Login asistido donde un familiar o profesional autoriza el acceso",
                    MinAutonomyLevel = 3,
                    RequiresEmail = false,
                    RequiresPassword = false,
                    RequiresPin = false,
                    RequiresEmojiSequence = false,
                    RequiresColorShape = false,
                    RequiresProfileSelect = false,
                    RequiresSupervisor = true,
                    DisplayOrder = 3,
                    IsActive = true
                },
                // Metodo deprecado - mantenido para compatibilidad
                new LoginMethod
                {
                    Id = 6,
                    Code = "TRUSTED_DEVICE",
                    Name = "Dispositivo Confiable (Deprecado)",
                    Description = "Login automatico en dispositivos previamente autorizados - DEPRECADO",
                    MinAutonomyLevel = 3,
                    RequiresEmail = false,
                    RequiresPassword = false,
                    RequiresPin = false,
                    RequiresEmojiSequence = false,
                    RequiresColorShape = false,
                    RequiresProfileSelect = false,
                    RequiresSupervisor = false,
                    DisplayOrder = 6,
                    IsActive = false
                },
                // Metodo deprecado - mantenido para compatibilidad
                new LoginMethod
                {
                    Id = 7,
                    Code = "PROFILE_SELECT",
                    Name = "Seleccion de Perfil (Deprecado)",
                    Description = "Login seleccionando nombre y avatar del usuario - DEPRECADO",
                    MinAutonomyLevel = 3,
                    RequiresEmail = false,
                    RequiresPassword = false,
                    RequiresPin = false,
                    RequiresEmojiSequence = false,
                    RequiresColorShape = false,
                    RequiresProfileSelect = true,
                    RequiresSupervisor = false,
                    DisplayOrder = 7,
                    IsActive = false
                }
            );
        }
    }
}
