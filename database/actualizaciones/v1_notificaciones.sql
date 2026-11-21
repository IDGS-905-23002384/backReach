/* Reach v1 - Notificaciones
   Solo es necesario si tu base ReachTiendaDb ya existía antes de esta versión.
   La API también lo aplica sola al arrancar. */
USE ReachTiendaDb;
GO
IF OBJECT_ID(N'Notificaciones', N'U') IS NULL
BEGIN
    CREATE TABLE Notificaciones (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Notificaciones PRIMARY KEY,
        UsuarioId int NOT NULL CONSTRAINT FK_Notificaciones_Usuarios_UsuarioId REFERENCES Usuarios (Id) ON DELETE CASCADE,
        Titulo nvarchar(120) NOT NULL,
        Mensaje nvarchar(400) NOT NULL,
        Tipo nvarchar(20) NOT NULL,
        PedidoId int NULL,
        Leida bit NOT NULL,
        Fecha datetime2 NOT NULL
    );
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Notificaciones_UsuarioId_Leida')
    CREATE INDEX IX_Notificaciones_UsuarioId_Leida ON Notificaciones (UsuarioId, Leida);
GO
