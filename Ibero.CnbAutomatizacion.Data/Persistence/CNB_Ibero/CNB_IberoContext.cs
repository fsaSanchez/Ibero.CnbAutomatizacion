using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Ibero.CnbAutomatizacion.Data.Persistence.CNB_Ibero;

public partial class CNB_IberoContext : DbContext
{
    public CNB_IberoContext()
    {
    }

    public CNB_IberoContext(DbContextOptions<CNB_IberoContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ArchivoCorreo> ArchivoCorreos { get; set; }

    public virtual DbSet<BitacoraGeneral> BitacoraGenerals { get; set; }

    public virtual DbSet<BitacoraPublicacion> BitacoraPublicacions { get; set; }

    public virtual DbSet<ConfiguracionSistema> ConfiguracionSistemas { get; set; }

    public virtual DbSet<CorreoRaw> CorreoRaws { get; set; }

    public virtual DbSet<FotoPersona> FotoPersonas { get; set; }

    public virtual DbSet<PersonaDesaparecidum> PersonaDesaparecida { get; set; }

    public virtual DbSet<VwCorreosPendiente> VwCorreosPendientes { get; set; }

    public virtual DbSet<VwEstadisticasSistema> VwEstadisticasSistemas { get; set; }

    public virtual DbSet<VwPersonasPublicablesHoy> VwPersonasPublicablesHoys { get; set; }

 

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ArchivoCorreo>(entity =>
        {
            entity.HasKey(e => e.IdArchivoCorreo).HasName("pk_archivo_correo");

            entity.ToTable("archivo_correo", "operacion");

            entity.HasIndex(e => e.Activo, "ix_archivo_correo_activo");

            entity.HasIndex(e => e.IdCorreoRaw, "ix_archivo_correo_id_correo_raw");

            entity.HasIndex(e => e.RutaDisco, "uk_archivo_correo_ruta").IsUnique();

            entity.Property(e => e.IdArchivoCorreo).HasColumnName("id_archivo_correo");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.FechaActualizacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.IdCorreoRaw).HasColumnName("id_correo_raw");
            entity.Property(e => e.NombreArchivo)
                .HasMaxLength(255)
                .HasColumnName("nombre_archivo");
            entity.Property(e => e.RutaDisco)
                .HasMaxLength(500)
                .HasColumnName("ruta_disco");
            entity.Property(e => e.TamanoBytes).HasColumnName("tamano_bytes");
            entity.Property(e => e.TipoContenido)
                .HasMaxLength(100)
                .HasColumnName("tipo_contenido");

            entity.HasOne(d => d.IdCorreoRawNavigation).WithMany(p => p.ArchivoCorreos)
                .HasForeignKey(d => d.IdCorreoRaw)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_archivo_correo_correo_raw");
        });

        modelBuilder.Entity<BitacoraGeneral>(entity =>
        {
            entity.HasKey(e => e.IdBitacoraGeneral).HasName("pk_bitacora_general");

            entity.ToTable("bitacora_general", "bitacoras");

            entity.HasIndex(e => e.FechaAccion, "ix_bitacora_general_fecha_accion").IsDescending();

            entity.HasIndex(e => e.IdCorreoRaw, "ix_bitacora_general_id_correo");

            entity.HasIndex(e => e.IdPersonaDesaparecida, "ix_bitacora_general_id_persona");

            entity.HasIndex(e => e.TipoAccion, "ix_bitacora_general_tipo_accion");

            entity.Property(e => e.IdBitacoraGeneral).HasColumnName("id_bitacora_general");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.EstadoAccion)
                .HasMaxLength(50)
                .HasColumnName("estado_accion");
            entity.Property(e => e.FechaAccion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("fecha_accion");
            entity.Property(e => e.IdCorreoRaw).HasColumnName("id_correo_raw");
            entity.Property(e => e.IdPersonaDesaparecida).HasColumnName("id_persona_desaparecida");
            entity.Property(e => e.IpOrigen)
                .HasMaxLength(50)
                .HasColumnName("ip_origen");
            entity.Property(e => e.MensajeError).HasColumnName("mensaje_error");
            entity.Property(e => e.TipoAccion)
                .HasMaxLength(100)
                .HasColumnName("tipo_accion");
            entity.Property(e => e.Usuario)
                .HasMaxLength(255)
                .HasColumnName("usuario");

            entity.HasOne(d => d.IdCorreoRawNavigation).WithMany(p => p.BitacoraGenerals)
                .HasForeignKey(d => d.IdCorreoRaw)
                .HasConstraintName("fk_bitacora_general_correo_raw");

            entity.HasOne(d => d.IdPersonaDesaparecidaNavigation).WithMany(p => p.BitacoraGenerals)
                .HasForeignKey(d => d.IdPersonaDesaparecida)
                .HasConstraintName("fk_bitacora_general_persona");
        });

        modelBuilder.Entity<BitacoraPublicacion>(entity =>
        {
            entity.HasKey(e => e.IdBitacoraPublicacion).HasName("pk_bitacora_publicacion");

            entity.ToTable("bitacora_publicacion", "bitacoras");

            entity.HasIndex(e => e.EstadoPublicacion, "ix_bitacora_publicacion_estado");

            entity.HasIndex(e => e.FechaIntento, "ix_bitacora_publicacion_fecha_intento").IsDescending();

            entity.HasIndex(e => e.IdPersonaDesaparecida, "ix_bitacora_publicacion_id_persona");

            entity.HasIndex(e => e.TipoRedSocial, "ix_bitacora_publicacion_tipo_red");

            entity.Property(e => e.IdBitacoraPublicacion).HasColumnName("id_bitacora_publicacion");
            entity.Property(e => e.EstadoPublicacion)
                .HasMaxLength(50)
                .HasColumnName("estado_publicacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaIntento)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("fecha_intento");
            entity.Property(e => e.FechaPublicacionReal).HasColumnName("fecha_publicacion_real");
            entity.Property(e => e.IdPersonaDesaparecida).HasColumnName("id_persona_desaparecida");
            entity.Property(e => e.IdPublicacionExterna).HasColumnName("id_publicacion_externa");
            entity.Property(e => e.MensajeError).HasColumnName("mensaje_error");
            entity.Property(e => e.TipoPublicacion)
                .HasMaxLength(50)
                .HasColumnName("tipo_publicacion");
            entity.Property(e => e.TipoRedSocial)
                .HasMaxLength(50)
                .HasColumnName("tipo_red_social");
            entity.Property(e => e.UsuarioPublicador)
                .HasMaxLength(255)
                .HasColumnName("usuario_publicador");

            entity.HasOne(d => d.IdPersonaDesaparecidaNavigation).WithMany(p => p.BitacoraPublicacions)
                .HasForeignKey(d => d.IdPersonaDesaparecida)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_bitacora_publicacion_persona");
        });

        modelBuilder.Entity<ConfiguracionSistema>(entity =>
        {
            entity.HasKey(e => e.IdConfiguracion).HasName("pk_configuracion_sistema");

            entity.ToTable("configuracion_sistema", "catalogos");

            entity.HasIndex(e => e.Clave, "UQ__configur__71DCA3DB7050DB3F").IsUnique();

            entity.HasIndex(e => e.Clave, "ix_configuracion_clave");

            entity.Property(e => e.IdConfiguracion).HasColumnName("id_configuracion");
            entity.Property(e => e.Clave)
                .HasMaxLength(100)
                .HasColumnName("clave");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .HasColumnName("descripcion");
            entity.Property(e => e.Editable)
                .HasDefaultValue(true)
                .HasColumnName("editable");
            entity.Property(e => e.FechaActualizacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.TipoValor)
                .HasMaxLength(50)
                .HasColumnName("tipo_valor");
            entity.Property(e => e.Valor).HasColumnName("valor");
        });

        modelBuilder.Entity<CorreoRaw>(entity =>
        {
            entity.HasKey(e => e.IdCorreoRaw).HasName("pk_correo_raw");

            entity.ToTable("correo_raw", "operacion");

            entity.HasIndex(e => e.Activo, "ix_correo_raw_activo");

            entity.HasIndex(e => e.EstadoProcesamiento, "ix_correo_raw_estado_procesamiento");

            entity.HasIndex(e => e.FechaRecepcion, "ix_correo_raw_fecha_recepcion").IsDescending();

            entity.HasIndex(e => e.IdExterno, "uk_correo_raw_id_externo").IsUnique();

            entity.Property(e => e.IdCorreoRaw).HasColumnName("id_correo_raw");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Asunto).HasColumnName("asunto");
            entity.Property(e => e.CuerpoCorreo).HasColumnName("cuerpo_correo");
            entity.Property(e => e.Destinatario)
                .HasMaxLength(255)
                .HasColumnName("destinatario");
            entity.Property(e => e.EstadoProcesamiento)
                .HasMaxLength(50)
                .HasDefaultValue("pendiente")
                .HasColumnName("estado_procesamiento");
            entity.Property(e => e.FechaActualizacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaProcesamiento).HasColumnName("fecha_procesamiento");
            entity.Property(e => e.FechaRecepcion).HasColumnName("fecha_recepcion");
            entity.Property(e => e.IdExterno)
                .HasMaxLength(200)
                .HasColumnName("id_externo");
            entity.Property(e => e.MensajeError).HasColumnName("mensaje_error");
            entity.Property(e => e.Reintentos)
                .HasDefaultValue(0)
                .HasColumnName("reintentos");
            entity.Property(e => e.Remitente)
                .HasMaxLength(255)
                .HasColumnName("remitente");
        });

        modelBuilder.Entity<FotoPersona>(entity =>
        {
            entity.HasKey(e => e.IdFotoPersona).HasName("pk_foto_persona");

            entity.ToTable("foto_persona", "operacion");

            entity.HasIndex(e => e.Activo, "ix_foto_persona_activo");

            entity.HasIndex(e => e.IdPersonaDesaparecida, "ix_foto_persona_id_persona");

            entity.HasIndex(e => e.Principal, "ix_foto_persona_principal");

            entity.HasIndex(e => e.RutaDisco, "uk_foto_persona_ruta").IsUnique();

            entity.Property(e => e.IdFotoPersona).HasColumnName("id_foto_persona");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.FechaActualizacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.IdLaserfiche)
                .HasMaxLength(20)
                .HasColumnName("id_laserfiche");
            entity.Property(e => e.IdPersonaDesaparecida).HasColumnName("id_persona_desaparecida");
            entity.Property(e => e.NombreArchivo)
                .HasMaxLength(255)
                .HasColumnName("nombre_archivo");
            entity.Property(e => e.Principal)
                .HasDefaultValue(true)
                .HasColumnName("principal");
            entity.Property(e => e.RutaDisco)
                .HasMaxLength(500)
                .HasColumnName("ruta_disco");
            entity.Property(e => e.TamanoBytes).HasColumnName("tamano_bytes");
            entity.Property(e => e.TipoContenido)
                .HasMaxLength(100)
                .HasColumnName("tipo_contenido");

            entity.HasOne(d => d.IdPersonaDesaparecidaNavigation).WithMany(p => p.FotoPersonas)
                .HasForeignKey(d => d.IdPersonaDesaparecida)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_foto_persona_persona_desaparecida");
        });

        modelBuilder.Entity<PersonaDesaparecidum>(entity =>
        {
            entity.HasKey(e => e.IdPersonaDesaparecida).HasName("pk_persona_desaparecida");

            entity.ToTable("persona_desaparecida", "operacion");

            entity.HasIndex(e => e.FolioUnicoIdentificacion, "UQ__persona___3826DC07AACC13C7").IsUnique();

            entity.HasIndex(e => e.Activo, "ix_persona_desaparecida_activo");

            entity.HasIndex(e => e.EstadoProcesamiento, "ix_persona_desaparecida_estado");

            entity.HasIndex(e => e.FechaCreacion, "ix_persona_desaparecida_fecha_creacion").IsDescending();

            entity.HasIndex(e => e.FechaHechos, "ix_persona_desaparecida_fecha_hechos").IsDescending();

            entity.HasIndex(e => e.FlagPublicadoFacebook, "ix_persona_desaparecida_flag_publicado");

            entity.HasIndex(e => e.FolioUnicoIdentificacion, "ix_persona_desaparecida_folio");

            entity.HasIndex(e => e.LugarHechos, "ix_persona_desaparecida_lugar_hechos");

            entity.HasIndex(e => e.Nombre, "ix_persona_desaparecida_nombre");

            entity.Property(e => e.IdPersonaDesaparecida).HasColumnName("id_persona_desaparecida");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.AutoridadesCompetentes).HasColumnName("autoridades_competentes");
            entity.Property(e => e.CamposIncompletos).HasColumnName("campos_incompletos");
            entity.Property(e => e.CaracteristicasFisicas).HasColumnName("caracteristicas_fisicas");
            entity.Property(e => e.CarpetaInvestigacion)
                .HasMaxLength(100)
                .HasColumnName("carpeta_investigacion");
            entity.Property(e => e.Discapacidad).HasColumnName("discapacidad");
            entity.Property(e => e.EdadActual).HasColumnName("edad_actual");
            entity.Property(e => e.EdadMomentoDesaparicion).HasColumnName("edad_momento_desaparicion");
            entity.Property(e => e.EstadoProcesamiento)
                .HasMaxLength(50)
                .HasDefaultValue("completo")
                .HasColumnName("estado_procesamiento");
            entity.Property(e => e.FechaActualizacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaHechos).HasColumnName("fecha_hechos");
            entity.Property(e => e.FechaPercate).HasColumnName("fecha_percate");
            entity.Property(e => e.FechaPublicacionFacebook).HasColumnName("fecha_publicacion_facebook");
            entity.Property(e => e.FlagPublicadoFacebook).HasColumnName("flag_publicado_facebook");
            entity.Property(e => e.FolioUnicoIdentificacion)
                .HasMaxLength(100)
                .HasColumnName("folio_unico_identificacion");
            entity.Property(e => e.Genero)
                .HasMaxLength(50)
                .HasColumnName("genero");
            entity.Property(e => e.IdCorreoRaw).HasColumnName("id_correo_raw");
            entity.Property(e => e.Idioma)
                .HasMaxLength(100)
                .HasColumnName("idioma");
            entity.Property(e => e.IntentoPublicacionFacebook)
                .HasDefaultValue(0)
                .HasColumnName("intento_publicacion_facebook");
            entity.Property(e => e.LugarHechos)
                .HasMaxLength(255)
                .HasColumnName("lugar_hechos");
            entity.Property(e => e.LugarNacimiento)
                .HasMaxLength(255)
                .HasColumnName("lugar_nacimiento");
            entity.Property(e => e.Nacionalidad)
                .HasMaxLength(100)
                .HasColumnName("nacionalidad");
            entity.Property(e => e.Nombre)
                .HasMaxLength(255)
                .HasColumnName("nombre");
            entity.Property(e => e.PrendasVestir).HasColumnName("prendas_vestir");
            entity.Property(e => e.SenasParticulares).HasColumnName("senas_particulares");
            entity.Property(e => e.Sexo)
                .HasMaxLength(50)
                .HasColumnName("sexo");

            entity.HasOne(d => d.IdCorreoRawNavigation).WithMany(p => p.PersonaDesaparecida)
                .HasForeignKey(d => d.IdCorreoRaw)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_persona_desaparecida_correo_raw");
        });

        modelBuilder.Entity<VwCorreosPendiente>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_correos_pendientes", "operacion");

            entity.Property(e => e.Asunto).HasColumnName("asunto");
            entity.Property(e => e.EstadoProcesamiento)
                .HasMaxLength(50)
                .HasColumnName("estado_procesamiento");
            entity.Property(e => e.FechaRecepcion).HasColumnName("fecha_recepcion");
            entity.Property(e => e.IdCorreoRaw)
                .ValueGeneratedOnAdd()
                .HasColumnName("id_correo_raw");
            entity.Property(e => e.MensajeError).HasColumnName("mensaje_error");
            entity.Property(e => e.Reintentos).HasColumnName("reintentos");
            entity.Property(e => e.Remitente)
                .HasMaxLength(255)
                .HasColumnName("remitente");
        });

        modelBuilder.Entity<VwEstadisticasSistema>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_estadisticas_sistema", "operacion");

            entity.Property(e => e.CorreosPendientes).HasColumnName("correos_pendientes");
            entity.Property(e => e.PersonasCompletas).HasColumnName("personas_completas");
            entity.Property(e => e.PersonasIncompletas).HasColumnName("personas_incompletas");
            entity.Property(e => e.PersonasPublicadas).HasColumnName("personas_publicadas");
            entity.Property(e => e.PublicacionesFallidasHoy).HasColumnName("publicaciones_fallidas_hoy");
            entity.Property(e => e.TotalPersonas).HasColumnName("total_personas");
        });

        modelBuilder.Entity<VwPersonasPublicablesHoy>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_personas_publicables_hoy", "operacion");

            entity.Property(e => e.CaracteristicasFisicas).HasColumnName("caracteristicas_fisicas");
            entity.Property(e => e.EdadActual).HasColumnName("edad_actual");
            entity.Property(e => e.EstadoProcesamiento)
                .HasMaxLength(50)
                .HasColumnName("estado_procesamiento");
            entity.Property(e => e.FechaCreacionDia).HasColumnName("fecha_creacion_dia");
            entity.Property(e => e.FlagPublicadoFacebook).HasColumnName("flag_publicado_facebook");
            entity.Property(e => e.FolioUnicoIdentificacion)
                .HasMaxLength(100)
                .HasColumnName("folio_unico_identificacion");
            entity.Property(e => e.IdPersonaDesaparecida).HasColumnName("id_persona_desaparecida");
            entity.Property(e => e.LugarHechos)
                .HasMaxLength(255)
                .HasColumnName("lugar_hechos");
            entity.Property(e => e.Nombre)
                .HasMaxLength(255)
                .HasColumnName("nombre");
            entity.Property(e => e.RutaFoto)
                .HasMaxLength(500)
                .HasColumnName("ruta_foto");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
