using Clinica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinica.Infrastructure.Data.Configurations;

public class AtencionConfiguration : IEntityTypeConfiguration<Atencion>
{
    public void Configure(EntityTypeBuilder<Atencion> builder)
    {
        builder.ToTable("Atenciones");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CodigoAtencion)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.CodigoAtencion)
            .IsUnique();

        builder.Property(x => x.FechaInicio)
            .IsRequired();

        builder.Property(x => x.MotivoConsulta)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(x => x.Observaciones)
            .HasColumnType("text");

        builder.Property(x => x.DiagnosticoResumen)
            .HasColumnType("text");

        builder.Property(x => x.Indicaciones)
            .HasColumnType("text");

        builder.Property(x => x.Tratamiento)
            .HasColumnType("text");

        builder.Property(x => x.Estado)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(x => x.CostoFinal)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(x => x.MontoPagado)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(x => x.SaldoPendiente)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.HasIndex(x => x.PacienteId);
        builder.HasIndex(x => x.DoctorId);
        builder.HasIndex(x => x.CitaId);

        builder.HasOne(x => x.Paciente)
            .WithMany(x => x.Atenciones)
            .HasForeignKey(x => x.PacienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Doctor)
            .WithMany(x => x.Atenciones)
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ServicioClinico)
            .WithMany(x => x.Atenciones)
            .HasForeignKey(x => x.ServicioClinicoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Cita)
            .WithOne(x => x.Atencion)
            .HasForeignKey<Atencion>(x => x.CitaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.HistorialClinico)
            .WithMany()
            .HasForeignKey(x => x.HistorialClinicoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UsuarioRegistro)
            .WithMany()
            .HasForeignKey(x => x.UsuarioRegistroId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.Pagos)
            .WithOne(x => x.Atencion)
            .HasForeignKey(x => x.AtencionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}