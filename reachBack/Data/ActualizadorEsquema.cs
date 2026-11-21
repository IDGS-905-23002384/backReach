using Microsoft.EntityFrameworkCore;

namespace reachBack.Data;

public static class ActualizadorEsquema
{
    private static readonly string[] Pasos =
    {
        """
        IF OBJECT_ID(N'[Notificaciones]', N'U') IS NULL
        BEGIN
            CREATE TABLE [Notificaciones] (
                [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Notificaciones] PRIMARY KEY,
                [UsuarioId] int NOT NULL CONSTRAINT [FK_Notificaciones_Usuarios_UsuarioId] REFERENCES [Usuarios] ([Id]) ON DELETE CASCADE,
                [Titulo] nvarchar(120) NOT NULL,
                [Mensaje] nvarchar(400) NOT NULL,
                [Tipo] nvarchar(20) NOT NULL,
                [PedidoId] int NULL,
                [Leida] bit NOT NULL,
                [Fecha] datetime2 NOT NULL
            );
        END
        """,
        """
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Notificaciones_UsuarioId_Leida')
            CREATE INDEX [IX_Notificaciones_UsuarioId_Leida] ON [Notificaciones] ([UsuarioId], [Leida]);
        """,
        """
        IF OBJECT_ID(N'[Sucursales]', N'U') IS NULL
            CREATE TABLE [Sucursales] (
                [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Sucursales] PRIMARY KEY,
                [Nombre] nvarchar(120) NOT NULL,
                [Direccion] nvarchar(250) NOT NULL,
                [Ciudad] nvarchar(80) NOT NULL,
                [Telefono] nvarchar(20) NOT NULL,
                [Horario] nvarchar(120) NOT NULL,
                [Latitud] decimal(9,6) NOT NULL,
                [Longitud] decimal(9,6) NOT NULL,
                [Activa] bit NOT NULL
            );
        """,
        """
        IF COL_LENGTH(N'Direcciones', N'Latitud') IS NULL
            ALTER TABLE [Direcciones] ADD [Latitud] decimal(9,6) NULL, [Longitud] decimal(9,6) NULL;
        """,
        """
        IF COL_LENGTH(N'Pedidos', N'EnvioLatitud') IS NULL
            ALTER TABLE [Pedidos] ADD [EnvioLatitud] decimal(9,6) NULL, [EnvioLongitud] decimal(9,6) NULL;
        """,
        """
        IF COL_LENGTH(N'Pedidos', N'SucursalId') IS NULL
            ALTER TABLE [Pedidos] ADD [SucursalId] int NULL CONSTRAINT [FK_Pedidos_Sucursales_SucursalId] REFERENCES [Sucursales] ([Id]);
        """,
        """
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Pedidos_SucursalId')
            CREATE INDEX [IX_Pedidos_SucursalId] ON [Pedidos] ([SucursalId]);
        """,
        """
        UPDATE [MetodosEnvio] SET [Descripcion] = N'Elige la sucursal más cercana. Sin costo.', [Paqueteria] = N'Sucursal Reach'
        WHERE [RequiereDireccion] = 0 AND [Descripcion] LIKE N'Blvd. Adolfo%';
        """
    };

    public static async Task AplicarAsync(ApplicationDbContext db, ILogger logger)
    {
        foreach (var sql in Pasos)
        {
            await db.Database.ExecuteSqlRawAsync(sql);
        }
        logger.LogInformation("Esquema verificado ({Pasos} pasos)", Pasos.Length);
    }
}
