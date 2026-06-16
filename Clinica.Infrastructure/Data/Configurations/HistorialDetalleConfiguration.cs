using Clinica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinica.Infrastructure.Data.Configurations;

public class HistorialDetalleConfiguration : IEntityTypeConfiguration<HistorialDetalle>
{
    public void Configure(EntityTypeBuilder<HistorialDetalle> builder)
    {
        builder.ToTable("HistorialDetalles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CodigoDetalle)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.CodigoDetalle)
            .IsUnique();

        builder.Property(x => x.TipoMovimiento)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Titulo)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Descripcion)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(x => x.FechaRegistro)
            .IsRequired();

        builder.HasIndex(x => x.HistorialClinicoId);
        builder.HasIndex(x => x.FechaRegistro);
        builder.HasIndex(x => x.TipoMovimiento);

        builder.HasOne(x => x.HistorialClinico)
            .WithMany(x => x.Detalles)
            .HasForeignKey(x => x.HistorialClinicoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Cita)
            .WithMany(x => x.HistorialDetalles)
            .HasForeignKey(x => x.CitaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Atencion)
            .WithMany(x => x.HistorialDetalles)
            .HasForeignKey(x => x.AtencionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Pago)
            .WithMany(x => x.HistorialDetalles)
            .HasForeignKey(x => x.PagoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Usuario)
            .WithMany()
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}