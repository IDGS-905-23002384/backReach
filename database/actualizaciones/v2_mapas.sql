/* Reach v2 - Mapas (sucursales, ubicación de direcciones y rastreo)
   Solo es necesario si tu base ReachTiendaDb ya existía antes de esta versión.
   La API también lo aplica sola al arrancar. */
USE ReachTiendaDb;
GO
IF OBJECT_ID(N'Sucursales', N'U') IS NULL
    CREATE TABLE Sucursales (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Sucursales PRIMARY KEY,
        Nombre nvarchar(120) NOT NULL,
        Direccion nvarchar(250) NOT NULL,
        Ciudad nvarchar(80) NOT NULL,
        Telefono nvarchar(20) NOT NULL,
        Horario nvarchar(120) NOT NULL,
        Latitud decimal(9,6) NOT NULL,
        Longitud decimal(9,6) NOT NULL,
        Activa bit NOT NULL
    );
GO
IF COL_LENGTH(N'Direcciones', N'Latitud') IS NULL
    ALTER TABLE Direcciones ADD Latitud decimal(9,6) NULL, Longitud decimal(9,6) NULL;
GO
IF COL_LENGTH(N'Pedidos', N'EnvioLatitud') IS NULL
    ALTER TABLE Pedidos ADD EnvioLatitud decimal(9,6) NULL, EnvioLongitud decimal(9,6) NULL;
GO
IF COL_LENGTH(N'Pedidos', N'SucursalId') IS NULL
    ALTER TABLE Pedidos ADD SucursalId int NULL CONSTRAINT FK_Pedidos_Sucursales_SucursalId REFERENCES Sucursales (Id);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Pedidos_SucursalId')
    CREATE INDEX IX_Pedidos_SucursalId ON Pedidos (SucursalId);
GO
IF NOT EXISTS (SELECT 1 FROM Sucursales)
    INSERT INTO Sucursales (Nombre, Direccion, Ciudad, Telefono, Horario, Latitud, Longitud, Activa) VALUES
     (N'Reach León Centro', N'Blvd. Adolfo López Mateos 1502, Col. Los Gavilanes', N'León, Gto.', N'4777123400', N'Lun a Sáb 10:00 a 20:00 · Dom 11:00 a 17:00', 21.124300, -101.671900, 1),
     (N'Reach Plaza Mayor', N'Blvd. Juan Alonso de Torres 2002, Valle del Campestre', N'León, Gto.', N'4777123401', N'Lun a Dom 11:00 a 21:00', 21.159300, -101.694800, 1),
     (N'Reach Guanajuato Capital', N'Av. Juárez 120, Zona Centro', N'Guanajuato, Gto.', N'4737321500', N'Lun a Sáb 10:00 a 19:00', 21.017000, -101.256500, 1);
GO
UPDATE MetodosEnvio SET Descripcion = N'Elige la sucursal más cercana. Sin costo.', Paqueteria = N'Sucursal Reach'
WHERE RequiereDireccion = 0 AND Descripcion LIKE N'Blvd. Adolfo%';
GO
