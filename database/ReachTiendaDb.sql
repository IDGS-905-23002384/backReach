/* Reach Tienda - Script de base de datos (SQL Server)
   Alternativa a la creación automática que hace la API al iniciar.
   Si usas este script, ejecútalo completo antes de arrancar la API. */
IF DB_ID(N'ReachTiendaDb') IS NULL CREATE DATABASE ReachTiendaDb;
GO
USE ReachTiendaDb;
GO
SET NOCOUNT ON;
GO

CREATE TABLE Usuarios (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Usuarios PRIMARY KEY,
    Nombre nvarchar(120) NOT NULL,
    Email nvarchar(160) NOT NULL,
    PasswordHash nvarchar(100) NOT NULL,
    Telefono nvarchar(20) NOT NULL,
    Rol nvarchar(20) NOT NULL,
    Activo bit NOT NULL,
    IntentosFallidos int NOT NULL,
    BloqueadoHasta datetime2 NULL,
    FechaRegistro datetime2 NOT NULL
);
CREATE UNIQUE INDEX IX_Usuarios_Email ON Usuarios (Email);

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
CREATE INDEX IX_Notificaciones_UsuarioId_Leida ON Notificaciones (UsuarioId, Leida);

CREATE TABLE Categorias (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Categorias PRIMARY KEY,
    Nombre nvarchar(80) NOT NULL,
    Slug nvarchar(80) NOT NULL,
    TipoComponente nvarchar(20) NULL,
    Orden int NOT NULL
);
CREATE UNIQUE INDEX IX_Categorias_Slug ON Categorias (Slug);

CREATE TABLE Proveedores (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Proveedores PRIMARY KEY,
    NombreEmpresa nvarchar(150) NOT NULL,
    ContactoPersona nvarchar(120) NOT NULL,
    Telefono nvarchar(20) NOT NULL,
    Email nvarchar(160) NOT NULL,
    Activo bit NOT NULL
);

CREATE TABLE Productos (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Productos PRIMARY KEY,
    Sku nvarchar(40) NOT NULL,
    Nombre nvarchar(200) NOT NULL,
    Marca nvarchar(80) NOT NULL,
    Descripcion nvarchar(2000) NOT NULL,
    Precio decimal(18,2) NOT NULL,
    PrecioAnterior decimal(18,2) NULL,
    Stock int NOT NULL,
    StockMinimo int NOT NULL,
    TipoInventario nvarchar(30) NOT NULL,
    ImagenUrl nvarchar(500) NOT NULL,
    EspecificacionesTecnicas nvarchar(4000) NOT NULL,
    Envio bit NOT NULL,
    Activo bit NOT NULL,
    Destacado bit NOT NULL,
    FechaAlta datetime2 NOT NULL,
    CategoriaId int NOT NULL CONSTRAINT FK_Productos_Categorias_CategoriaId REFERENCES Categorias (Id),
    ProveedorId int NULL CONSTRAINT FK_Productos_Proveedores_ProveedorId REFERENCES Proveedores (Id) ON DELETE SET NULL,
    RowVersion rowversion NOT NULL
);
CREATE UNIQUE INDEX IX_Productos_Sku ON Productos (Sku);
CREATE INDEX IX_Productos_CategoriaId_Activo ON Productos (CategoriaId, Activo);
CREATE INDEX IX_Productos_ProveedorId ON Productos (ProveedorId);

CREATE TABLE EspecificacionesComponente (
    ProductoId int NOT NULL CONSTRAINT PK_EspecificacionesComponente PRIMARY KEY
        CONSTRAINT FK_EspecificacionesComponente_Productos_ProductoId REFERENCES Productos (Id) ON DELETE CASCADE,
    Socket nvarchar(20) NULL,
    SocketsSoportados nvarchar(100) NULL,
    TipoMemoria nvarchar(10) NULL,
    FormFactor nvarchar(20) NULL,
    FormFactorsSoportados nvarchar(60) NULL,
    Interfaz nvarchar(20) NULL,
    ConsumoW int NULL,
    CapacidadW int NULL,
    FuenteRecomendadaW int NULL,
    TdpMaxW int NULL,
    LongitudMm int NULL,
    LongitudMaxGpuMm int NULL,
    AlturaMm int NULL,
    AlturaMaxCoolerMm int NULL,
    RadiadorMm int NULL,
    RadiadorMaxMm int NULL,
    SlotsRam int NULL,
    ModulosRam int NULL,
    CapacidadGb int NULL,
    RanurasM2 int NULL,
    GraficosIntegrados bit NULL,
    IncluyeDisipador bit NULL
);

CREATE TABLE CarritoItems (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_CarritoItems PRIMARY KEY,
    UsuarioId int NOT NULL CONSTRAINT FK_CarritoItems_Usuarios_UsuarioId REFERENCES Usuarios (Id) ON DELETE CASCADE,
    ProductoId int NOT NULL CONSTRAINT FK_CarritoItems_Productos_ProductoId REFERENCES Productos (Id) ON DELETE CASCADE,
    Cantidad int NOT NULL,
    EsEnsamblePersonalizado bit NOT NULL,
    EnsambleId nvarchar(40) NULL,
    EnsambleNombre nvarchar(120) NULL,
    FechaAgregado datetime2 NOT NULL
);
CREATE INDEX IX_CarritoItems_UsuarioId ON CarritoItems (UsuarioId);
CREATE INDEX IX_CarritoItems_ProductoId ON CarritoItems (ProductoId);

CREATE TABLE Direcciones (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Direcciones PRIMARY KEY,
    UsuarioId int NOT NULL CONSTRAINT FK_Direcciones_Usuarios_UsuarioId REFERENCES Usuarios (Id) ON DELETE CASCADE,
    Alias nvarchar(40) NOT NULL,
    NombreRecibe nvarchar(120) NOT NULL,
    Telefono nvarchar(20) NOT NULL,
    Calle nvarchar(150) NOT NULL,
    NumeroExterior nvarchar(20) NOT NULL,
    NumeroInterior nvarchar(20) NULL,
    Colonia nvarchar(120) NOT NULL,
    Ciudad nvarchar(120) NOT NULL,
    Estado nvarchar(80) NOT NULL,
    CodigoPostal nvarchar(5) NOT NULL,
    Referencias nvarchar(250) NULL,
    EsPredeterminada bit NOT NULL,
    Latitud decimal(9,6) NULL,
    Longitud decimal(9,6) NULL
);
CREATE INDEX IX_Direcciones_UsuarioId ON Direcciones (UsuarioId);

CREATE TABLE MetodosEnvio (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_MetodosEnvio PRIMARY KEY,
    Nombre nvarchar(80) NOT NULL,
    Descripcion nvarchar(250) NOT NULL,
    Paqueteria nvarchar(80) NOT NULL,
    Costo decimal(18,2) NOT NULL,
    DiasMin int NOT NULL,
    DiasMax int NOT NULL,
    AplicaEnvioGratis bit NOT NULL,
    RequiereDireccion bit NOT NULL,
    Activo bit NOT NULL
);

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

CREATE TABLE Pedidos (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Pedidos PRIMARY KEY,
    Folio nvarchar(30) NOT NULL,
    UsuarioId int NOT NULL CONSTRAINT FK_Pedidos_Usuarios_UsuarioId REFERENCES Usuarios (Id),
    Fecha datetime2 NOT NULL,
    FechaActualizacion datetime2 NOT NULL,
    Subtotal decimal(18,2) NOT NULL,
    CostoManoObra decimal(18,2) NOT NULL,
    CostoEnvio decimal(18,2) NOT NULL,
    Iva decimal(18,2) NOT NULL,
    Total decimal(18,2) NOT NULL,
    Estado nvarchar(30) NOT NULL,
    EstadoArmado nvarchar(30) NOT NULL,
    EstadoEnvio nvarchar(30) NOT NULL,
    IncluyeEnsamble bit NOT NULL,
    MetodoEnvioId int NOT NULL CONSTRAINT FK_Pedidos_MetodosEnvio_MetodoEnvioId REFERENCES MetodosEnvio (Id),
    Paqueteria nvarchar(80) NULL,
    NumeroGuia nvarchar(60) NULL,
    FechaEstimadaEntrega datetime2 NULL,
    EnvioNombreRecibe nvarchar(120) NULL,
    EnvioTelefono nvarchar(20) NULL,
    EnvioDireccion nvarchar(400) NULL,
    EnvioCiudad nvarchar(120) NULL,
    EnvioEstado nvarchar(80) NULL,
    EnvioCodigoPostal nvarchar(5) NULL,
    EnvioReferencias nvarchar(250) NULL,
    MetodoPago nvarchar(30) NOT NULL,
    TarjetaMarca nvarchar(20) NULL,
    TarjetaUltimos4 nvarchar(4) NULL,
    Notas nvarchar(500) NULL,
    EnvioLatitud decimal(9,6) NULL,
    EnvioLongitud decimal(9,6) NULL,
    SucursalId int NULL CONSTRAINT FK_Pedidos_Sucursales_SucursalId REFERENCES Sucursales (Id)
);
CREATE UNIQUE INDEX IX_Pedidos_Folio ON Pedidos (Folio);
CREATE INDEX IX_Pedidos_Fecha ON Pedidos (Fecha);
CREATE INDEX IX_Pedidos_UsuarioId ON Pedidos (UsuarioId);
CREATE INDEX IX_Pedidos_MetodoEnvioId ON Pedidos (MetodoEnvioId);
CREATE INDEX IX_Pedidos_SucursalId ON Pedidos (SucursalId);

CREATE TABLE PedidoDetalles (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_PedidoDetalles PRIMARY KEY,
    PedidoId int NOT NULL CONSTRAINT FK_PedidoDetalles_Pedidos_PedidoId REFERENCES Pedidos (Id) ON DELETE CASCADE,
    ProductoId int NOT NULL CONSTRAINT FK_PedidoDetalles_Productos_ProductoId REFERENCES Productos (Id),
    Sku nvarchar(40) NOT NULL,
    NombreProducto nvarchar(200) NOT NULL,
    ImagenUrl nvarchar(500) NOT NULL,
    Cantidad int NOT NULL,
    PrecioUnitario decimal(18,2) NOT NULL,
    EsEnsamble bit NOT NULL,
    EnsambleId nvarchar(40) NULL,
    EnsambleNombre nvarchar(120) NULL
);
CREATE INDEX IX_PedidoDetalles_PedidoId ON PedidoDetalles (PedidoId);
CREATE INDEX IX_PedidoDetalles_ProductoId ON PedidoDetalles (ProductoId);

CREATE TABLE PedidoSeguimientos (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_PedidoSeguimientos PRIMARY KEY,
    PedidoId int NOT NULL CONSTRAINT FK_PedidoSeguimientos_Pedidos_PedidoId REFERENCES Pedidos (Id) ON DELETE CASCADE,
    Titulo nvarchar(80) NOT NULL,
    Descripcion nvarchar(300) NOT NULL,
    Fecha datetime2 NOT NULL
);
CREATE INDEX IX_PedidoSeguimientos_PedidoId ON PedidoSeguimientos (PedidoId);

CREATE TABLE MovimientosKardex (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_MovimientosKardex PRIMARY KEY,
    ProductoId int NOT NULL CONSTRAINT FK_MovimientosKardex_Productos_ProductoId REFERENCES Productos (Id),
    TipoMovimiento nvarchar(10) NOT NULL,
    Cantidad int NOT NULL,
    StockResultante int NOT NULL,
    Fecha datetime2 NOT NULL,
    Motivo nvarchar(200) NOT NULL,
    PedidoId int NULL,
    AdministradorResponsable nvarchar(160) NOT NULL
);
CREATE INDEX IX_MovimientosKardex_Fecha ON MovimientosKardex (Fecha);
CREATE INDEX IX_MovimientosKardex_ProductoId ON MovimientosKardex (ProductoId);

CREATE TABLE FichasTecnicas (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_FichasTecnicas PRIMARY KEY,
    NombreEquipoPrearmado nvarchar(120) NOT NULL,
    CategoriaUso nvarchar(60) NOT NULL,
    Descripcion nvarchar(500) NOT NULL,
    CostoManoObra decimal(18,2) NOT NULL,
    PrecioSugerido decimal(18,2) NOT NULL,
    Activo bit NOT NULL
);

CREATE TABLE FichaTecnicaDetalles (
    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_FichaTecnicaDetalles PRIMARY KEY,
    FichaTecnicaId int NOT NULL CONSTRAINT FK_FichaTecnicaDetalles_FichasTecnicas_FichaTecnicaId REFERENCES FichasTecnicas (Id) ON DELETE CASCADE,
    ProductoId int NOT NULL CONSTRAINT FK_FichaTecnicaDetalles_Productos_ProductoId REFERENCES Productos (Id),
    Cantidad int NOT NULL
);
CREATE INDEX IX_FichaTecnicaDetalles_FichaTecnicaId ON FichaTecnicaDetalles (FichaTecnicaId);
CREATE INDEX IX_FichaTecnicaDetalles_ProductoId ON FichaTecnicaDetalles (ProductoId);
GO

SET IDENTITY_INSERT Categorias ON;
INSERT INTO Categorias (Id, Nombre, Slug, TipoComponente, Orden) VALUES (1, N'Procesadores', N'procesadores', N'CPU', 1);
INSERT INTO Categorias (Id, Nombre, Slug, TipoComponente, Orden) VALUES (2, N'Tarjetas Madre', N'tarjetas-madre', N'MOTHERBOARD', 2);
INSERT INTO Categorias (Id, Nombre, Slug, TipoComponente, Orden) VALUES (3, N'Memoria RAM', N'memoria-ram', N'RAM', 3);
INSERT INTO Categorias (Id, Nombre, Slug, TipoComponente, Orden) VALUES (4, N'Almacenamiento', N'almacenamiento', N'STORAGE', 4);
INSERT INTO Categorias (Id, Nombre, Slug, TipoComponente, Orden) VALUES (5, N'Tarjetas de Video', N'tarjetas-de-video', N'GPU', 5);
INSERT INTO Categorias (Id, Nombre, Slug, TipoComponente, Orden) VALUES (6, N'Enfriamiento', N'enfriamiento', N'COOLER', 6);
INSERT INTO Categorias (Id, Nombre, Slug, TipoComponente, Orden) VALUES (7, N'Gabinetes', N'gabinetes', N'CASE', 7);
INSERT INTO Categorias (Id, Nombre, Slug, TipoComponente, Orden) VALUES (8, N'Fuentes de Poder', N'fuentes-de-poder', N'PSU', 8);
INSERT INTO Categorias (Id, Nombre, Slug, TipoComponente, Orden) VALUES (9, N'Monitores', N'monitores', NULL, 9);
INSERT INTO Categorias (Id, Nombre, Slug, TipoComponente, Orden) VALUES (10, N'Teclados', N'teclados', NULL, 10);
INSERT INTO Categorias (Id, Nombre, Slug, TipoComponente, Orden) VALUES (11, N'Mouse', N'mouse', NULL, 11);
INSERT INTO Categorias (Id, Nombre, Slug, TipoComponente, Orden) VALUES (12, N'Audífonos', N'audifonos', NULL, 12);
SET IDENTITY_INSERT Categorias OFF;
GO

SET IDENTITY_INSERT Proveedores ON;
INSERT INTO Proveedores (Id, NombreEmpresa, ContactoPersona, Telefono, Email, Activo) VALUES (1, N'Ingram Micro México', N'Laura Méndez', N'5552638000', N'ventas@ingrammicro.com.mx', 1);
INSERT INTO Proveedores (Id, NombreEmpresa, ContactoPersona, Telefono, Email, Activo) VALUES (2, N'CT Internacional', N'Jorge Ramírez', N'6622590000', N'contacto@ctonline.mx', 1);
INSERT INTO Proveedores (Id, NombreEmpresa, ContactoPersona, Telefono, Email, Activo) VALUES (3, N'Grupo CVA', N'Mariana Torres', N'3330440000', N'ventas@grupocva.com', 1);
INSERT INTO Proveedores (Id, NombreEmpresa, ContactoPersona, Telefono, Email, Activo) VALUES (4, N'Exel del Norte', N'Ricardo Salinas', N'8181500000', N'ventas@exeldelnorte.com.mx', 1);
SET IDENTITY_INSERT Proveedores OFF;
GO

SET IDENTITY_INSERT Productos ON;
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (1, N'AMD-R5-5600', N'AMD Ryzen 5 5600', N'AMD', N'Procesador de 6 núcleos ideal para gaming de entrada con excelente relación precio-rendimiento. Incluye disipador Wraith Stealth.', 2199, 2599, 25, 5, N'Almacen', N'/img/productos/amd-r5-5600.webp', N'Socket: AM4
Núcleos / Hilos: 6 / 12
Frecuencia base: 3.5 GHz
Frecuencia turbo: 4.4 GHz
Caché: 35 MB
TDP: 65 W
Gráficos integrados: No
Disipador incluido: Sí', 1, 1, 0, DATEADD(MINUTE, 0, DATEADD(DAY, -30, SYSUTCDATETIME())), 1, 1);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (2, N'AMD-R7-5700X3D', N'AMD Ryzen 7 5700X3D', N'AMD', N'La mejor mejora para plataformas AM4: tecnología 3D V-Cache para máximo rendimiento en juegos.', 4299, 4899, 14, 5, N'Almacen', N'/img/productos/amd-r7-5700x3d.webp', N'Socket: AM4
Núcleos / Hilos: 8 / 16
Frecuencia base: 3.0 GHz
Frecuencia turbo: 4.1 GHz
Caché: 100 MB (3D V-Cache)
TDP: 105 W
Gráficos integrados: No
Disipador incluido: No', 1, 1, 0, DATEADD(MINUTE, 1, DATEADD(DAY, -30, SYSUTCDATETIME())), 1, 2);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (3, N'AMD-R5-7600X', N'AMD Ryzen 5 7600X', N'AMD', N'Arquitectura Zen 4 con soporte DDR5 y PCIe 5.0. Plataforma AM5 lista para futuras actualizaciones.', 3899, 4599, 18, 5, N'Almacen', N'/img/productos/amd-r5-7600x.webp', N'Socket: AM5
Núcleos / Hilos: 6 / 12
Frecuencia base: 4.7 GHz
Frecuencia turbo: 5.3 GHz
Caché: 38 MB
TDP: 105 W
Gráficos integrados: Sí
Disipador incluido: No', 1, 1, 0, DATEADD(MINUTE, 2, DATEADD(DAY, -30, SYSUTCDATETIME())), 1, 1);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (4, N'AMD-R7-7800X3D', N'AMD Ryzen 7 7800X3D', N'AMD', N'Uno de los procesadores más rápidos para gaming gracias a su enorme caché 3D V-Cache.', 8499, 9499, 9, 5, N'Almacen', N'/img/productos/amd-r7-7800x3d.webp', N'Socket: AM5
Núcleos / Hilos: 8 / 16
Frecuencia base: 4.2 GHz
Frecuencia turbo: 5.0 GHz
Caché: 104 MB (3D V-Cache)
TDP: 120 W
Gráficos integrados: Sí
Disipador incluido: No', 1, 1, 1, DATEADD(MINUTE, 3, DATEADD(DAY, -30, SYSUTCDATETIME())), 1, 1);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (5, N'AMD-R7-9700X', N'AMD Ryzen 7 9700X', N'AMD', N'Zen 5 de alta eficiencia: 8 núcleos con gran rendimiento por watt para gaming y creación.', 6799, NULL, 12, 5, N'Almacen', N'/img/productos/amd-r7-9700x.webp', N'Socket: AM5
Núcleos / Hilos: 8 / 16
Frecuencia base: 3.8 GHz
Frecuencia turbo: 5.5 GHz
Caché: 40 MB
TDP: 65 W
Gráficos integrados: Sí
Disipador incluido: No', 1, 1, 0, DATEADD(MINUTE, 4, DATEADD(DAY, -30, SYSUTCDATETIME())), 1, 3);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (6, N'AMD-R9-9900X', N'AMD Ryzen 9 9900X', N'AMD', N'12 núcleos Zen 5 para edición de video, renderizado 3D y streaming sin compromisos.', 8999, 10499, 7, 5, N'Almacen', N'/img/productos/amd-r9-9900x.webp', N'Socket: AM5
Núcleos / Hilos: 12 / 24
Frecuencia base: 4.4 GHz
Frecuencia turbo: 5.6 GHz
Caché: 76 MB
TDP: 120 W
Gráficos integrados: Sí
Disipador incluido: No', 1, 1, 0, DATEADD(MINUTE, 5, DATEADD(DAY, -30, SYSUTCDATETIME())), 1, 3);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (7, N'INT-I5-12400F', N'Intel Core i5-12400F', N'Intel', N'Procesador de 12.ª generación con excelente rendimiento en juegos. Requiere tarjeta de video dedicada. Incluye disipador Laminar.', 2099, 2499, 30, 5, N'Almacen', N'/img/productos/int-i5-12400f.webp', N'Socket: LGA1700
Núcleos / Hilos: 6 / 12
Frecuencia base: 2.5 GHz
Frecuencia turbo: 4.4 GHz
Caché: 18 MB
TDP: 65 W
Gráficos integrados: No
Disipador incluido: Sí', 1, 1, 0, DATEADD(MINUTE, 6, DATEADD(DAY, -30, SYSUTCDATETIME())), 1, 2);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (8, N'INT-I5-14600K', N'Intel Core i5-14600K', N'Intel', N'14 núcleos híbridos (6P + 8E) desbloqueado para overclocking. Ideal para gaming y multitarea.', 5299, 5999, 15, 5, N'Almacen', N'/img/productos/int-i5-14600k.webp', N'Socket: LGA1700
Núcleos / Hilos: 14 / 20
Frecuencia base: 3.5 GHz
Frecuencia turbo: 5.3 GHz
Caché: 24 MB
TDP: 125 W
Gráficos integrados: Sí
Disipador incluido: No', 1, 1, 1, DATEADD(MINUTE, 7, DATEADD(DAY, -30, SYSUTCDATETIME())), 1, 1);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (9, N'INT-I7-14700K', N'Intel Core i7-14700K', N'Intel', N'20 núcleos para productividad extrema, streaming y juegos a altas tasas de cuadros.', 7499, 8499, 10, 5, N'Almacen', N'/img/productos/int-i7-14700k.webp', N'Socket: LGA1700
Núcleos / Hilos: 20 / 28
Frecuencia base: 3.4 GHz
Frecuencia turbo: 5.6 GHz
Caché: 33 MB
TDP: 125 W
Gráficos integrados: Sí
Disipador incluido: No', 1, 1, 0, DATEADD(MINUTE, 8, DATEADD(DAY, -30, SYSUTCDATETIME())), 1, 2);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (10, N'INT-U5-245K', N'Intel Core Ultra 5 245K', N'Intel', N'Arquitectura Arrow Lake con NPU integrada para IA y plataforma LGA1851 con DDR5.', 5699, NULL, 11, 5, N'Almacen', N'/img/productos/int-u5-245k.webp', N'Socket: LGA1851
Núcleos / Hilos: 14 / 14
Frecuencia base: 4.2 GHz
Frecuencia turbo: 5.2 GHz
Caché: 24 MB
TDP: 125 W
Gráficos integrados: Sí
Disipador incluido: No', 1, 1, 0, DATEADD(MINUTE, 9, DATEADD(DAY, -30, SYSUTCDATETIME())), 1, 4);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (11, N'INT-U7-265K', N'Intel Core Ultra 7 265K', N'Intel', N'20 núcleos Arrow Lake, alto rendimiento multinúcleo con menor consumo que la generación anterior.', 7899, 8999, 8, 5, N'Almacen', N'/img/productos/int-u7-265k.webp', N'Socket: LGA1851
Núcleos / Hilos: 20 / 20
Frecuencia base: 3.9 GHz
Frecuencia turbo: 5.5 GHz
Caché: 36 MB
TDP: 125 W
Gráficos integrados: Sí
Disipador incluido: No', 1, 1, 0, DATEADD(MINUTE, 10, DATEADD(DAY, -30, SYSUTCDATETIME())), 1, 4);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (12, N'ASU-PRIME-B550M-A', N'ASUS PRIME B550M-A WIFI II', N'ASUS', N'Tarjeta madre Micro-ATX para Ryzen AM4 con PCIe 4.0, Wi-Fi 6 y Bluetooth.', 2399, 2799, 16, 5, N'Almacen', N'/img/productos/asu-prime-b550m-a.webp', N'Socket: AM4
Chipset: B550
Formato: mATX
Memoria: DDR4, 4 ranuras (máx. 128 GB)
Ranuras M.2: 2
Wi-Fi: Wi-Fi 6', 1, 1, 0, DATEADD(MINUTE, 11, DATEADD(DAY, -30, SYSUTCDATETIME())), 2, 1);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (13, N'MSI-B550-TOMAHAWK', N'MSI MAG B550 TOMAHAWK MAX WIFI', N'MSI', N'ATX robusta con VRM reforzado, ideal para Ryzen 5000 y overclocking de memoria.', 3299, 3699, 10, 5, N'Almacen', N'/img/productos/msi-b550-tomahawk.webp', N'Socket: AM4
Chipset: B550
Formato: ATX
Memoria: DDR4, 4 ranuras (máx. 128 GB)
Ranuras M.2: 2
Wi-Fi: Wi-Fi 6E', 1, 1, 0, DATEADD(MINUTE, 12, DATEADD(DAY, -30, SYSUTCDATETIME())), 2, 2);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (14, N'GIG-B650M-ELITE', N'GIGABYTE B650M AORUS ELITE AX', N'GIGABYTE', N'Micro-ATX premium para AM5 con DDR5, PCIe 5.0 en M.2 y Wi-Fi 6E.', 3999, 4499, 14, 5, N'Almacen', N'/img/productos/gig-b650m-elite.webp', N'Socket: AM5
Chipset: B650
Formato: mATX
Memoria: DDR5, 4 ranuras (máx. 192 GB)
Ranuras M.2: 2
Wi-Fi: Wi-Fi 6E', 1, 1, 1, DATEADD(MINUTE, 13, DATEADD(DAY, -30, SYSUTCDATETIME())), 2, 3);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (15, N'ASU-TUF-B650-PLUS', N'ASUS TUF GAMING B650-PLUS WIFI', N'ASUS', N'Durabilidad TUF en formato ATX para Ryzen 7000/9000 con tres ranuras M.2.', 4499, NULL, 12, 5, N'Almacen', N'/img/productos/asu-tuf-b650-plus.webp', N'Socket: AM5
Chipset: B650
Formato: ATX
Memoria: DDR5, 4 ranuras (máx. 192 GB)
Ranuras M.2: 3
Wi-Fi: Wi-Fi 6', 1, 1, 0, DATEADD(MINUTE, 14, DATEADD(DAY, -30, SYSUTCDATETIME())), 2, 1);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (16, N'ASR-B650I-LIGHT', N'ASRock B650I Lightning WiFi', N'ASRock', N'Mini-ITX para equipos compactos de alto rendimiento con AM5.', 4699, 5199, 6, 5, N'Almacen', N'/img/productos/asr-b650i-light.webp', N'Socket: AM5
Chipset: B650
Formato: Mini-ITX
Memoria: DDR5, 2 ranuras (máx. 96 GB)
Ranuras M.2: 2
Wi-Fi: Wi-Fi 6E', 1, 1, 0, DATEADD(MINUTE, 15, DATEADD(DAY, -30, SYSUTCDATETIME())), 2, 4);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (17, N'MSI-B760M-A-D4', N'MSI PRO B760M-A WIFI DDR4', N'MSI', N'Plataforma Intel 12.ª-14.ª gen con memoria DDR4 para armados económicos.', 2799, 3199, 15, 5, N'Almacen', N'/img/productos/msi-b760m-a-d4.webp', N'Socket: LGA1700
Chipset: B760
Formato: mATX
Memoria: DDR4, 4 ranuras (máx. 128 GB)
Ranuras M.2: 2
Wi-Fi: Wi-Fi 6E', 1, 1, 0, DATEADD(MINUTE, 16, DATEADD(DAY, -30, SYSUTCDATETIME())), 2, 2);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (18, N'ASU-TUF-Z790-PLUS', N'ASUS TUF GAMING Z790-PLUS WIFI', N'ASUS', N'Chipset Z790 para overclocking de procesadores K con DDR5 y cuatro M.2.', 5499, 6299, 8, 5, N'Almacen', N'/img/productos/asu-tuf-z790-plus.webp', N'Socket: LGA1700
Chipset: Z790
Formato: ATX
Memoria: DDR5, 4 ranuras (máx. 192 GB)
Ranuras M.2: 4
Wi-Fi: Wi-Fi 6', 1, 1, 1, DATEADD(MINUTE, 17, DATEADD(DAY, -30, SYSUTCDATETIME())), 2, 1);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (19, N'GIG-Z890-ELITE', N'GIGABYTE Z890 AORUS ELITE WIFI7', N'GIGABYTE', N'Tarjeta madre Z890 para Intel Core Ultra con Wi-Fi 7 y PCIe 5.0.', 6999, NULL, 6, 5, N'Almacen', N'/img/productos/gig-z890-elite.webp', N'Socket: LGA1851
Chipset: Z890
Formato: ATX
Memoria: DDR5, 4 ranuras (máx. 256 GB)
Ranuras M.2: 4
Wi-Fi: Wi-Fi 7', 1, 1, 0, DATEADD(MINUTE, 18, DATEADD(DAY, -30, SYSUTCDATETIME())), 2, 3);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (20, N'MSI-B860M-A', N'MSI PRO B860M-A WIFI', N'MSI', N'Micro-ATX para Core Ultra serie 200 con DDR5 y gran relación calidad-precio.', 3999, 4399, 10, 5, N'Almacen', N'/img/productos/msi-b860m-a.webp', N'Socket: LGA1851
Chipset: B860
Formato: mATX
Memoria: DDR5, 4 ranuras (máx. 256 GB)
Ranuras M.2: 2
Wi-Fi: Wi-Fi 6E', 1, 1, 0, DATEADD(MINUTE, 19, DATEADD(DAY, -30, SYSUTCDATETIME())), 2, 4);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (21, N'KIN-FURY-D4-16', N'Kingston FURY Beast 16GB (2x8) DDR4 3200', N'Kingston', N'Kit de doble canal DDR4 confiable para equipos de oficina y gaming de entrada.', 799, 949, 40, 5, N'Almacen', N'/img/productos/kin-fury-d4-16.webp', N'Tipo: DDR4
Capacidad: 16 GB (2 x 8 GB)
Velocidad: 3200 MT/s
Latencia: CL16
RGB: No', 1, 1, 0, DATEADD(MINUTE, 20, DATEADD(DAY, -30, SYSUTCDATETIME())), 3, 1);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (22, N'COR-LPX-D4-32', N'Corsair Vengeance LPX 32GB (2x16) DDR4 3600', N'Corsair', N'Perfil bajo y alto rendimiento para plataformas AM4 e Intel DDR4.', 1499, 1699, 22, 5, N'Almacen', N'/img/productos/cor-lpx-d4-32.webp', N'Tipo: DDR4
Capacidad: 32 GB (2 x 16 GB)
Velocidad: 3600 MT/s
Latencia: CL18
RGB: No', 1, 1, 0, DATEADD(MINUTE, 21, DATEADD(DAY, -30, SYSUTCDATETIME())), 3, 2);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (23, N'KIN-FURY-D5-32', N'Kingston FURY Beast 32GB (2x16) DDR5 6000', N'Kingston', N'DDR5 a 6000 MT/s con perfiles EXPO/XMP, el punto ideal para Ryzen 7000/9000.', 2299, 2699, 28, 5, N'Almacen', N'/img/productos/kin-fury-d5-32.webp', N'Tipo: DDR5
Capacidad: 32 GB (2 x 16 GB)
Velocidad: 6000 MT/s
Latencia: CL36
RGB: No', 1, 1, 1, DATEADD(MINUTE, 22, DATEADD(DAY, -30, SYSUTCDATETIME())), 3, 1);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (24, N'COR-VEN-RGB-D5-32', N'Corsair Vengeance RGB 32GB (2x16) DDR5 6000', N'Corsair', N'Iluminación RGB direccionable y latencia CL30 para máximo desempeño.', 2599, 2999, 20, 5, N'Almacen', N'/img/productos/cor-ven-rgb-d5-32.webp', N'Tipo: DDR5
Capacidad: 32 GB (2 x 16 GB)
Velocidad: 6000 MT/s
Latencia: CL30
RGB: Sí', 1, 1, 0, DATEADD(MINUTE, 23, DATEADD(DAY, -30, SYSUTCDATETIME())), 3, 2);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (25, N'GSK-TZ5-D5-64', N'G.Skill Trident Z5 RGB 64GB (2x32) DDR5 6400', N'G.Skill', N'64 GB de alto rendimiento para edición de video, máquinas virtuales y desarrollo.', 4799, 5399, 8, 5, N'Almacen', N'/img/productos/gsk-tz5-d5-64.webp', N'Tipo: DDR5
Capacidad: 64 GB (2 x 32 GB)
Velocidad: 6400 MT/s
Latencia: CL32
RGB: Sí', 1, 1, 0, DATEADD(MINUTE, 24, DATEADD(DAY, -30, SYSUTCDATETIME())), 3, 3);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (26, N'XPG-LAN-D5-16', N'XPG Lancer 16GB (2x8) DDR5 5600', N'XPG', N'Entrada a DDR5 con disipador de aluminio y garantía de por vida.', 1199, NULL, 25, 5, N'Almacen', N'/img/productos/xpg-lan-d5-16.webp', N'Tipo: DDR5
Capacidad: 16 GB (2 x 8 GB)
Velocidad: 5600 MT/s
Latencia: CL46
RGB: No', 1, 1, 0, DATEADD(MINUTE, 25, DATEADD(DAY, -30, SYSUTCDATETIME())), 3, 4);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (27, N'KIN-NV3-1TB', N'Kingston NV3 1TB NVMe PCIe 4.0', N'Kingston', N'SSD NVMe Gen4 económico y rápido para arranques y cargas instantáneas.', 1099, 1299, 45, 5, N'Almacen', N'/img/productos/kin-nv3-1tb.webp', N'Capacidad: 1 TB
Interfaz: M.2 NVMe
Formato: M.2 2280
Lectura: 6000 MB/s
Escritura: 4000 MB/s', 1, 1, 0, DATEADD(MINUTE, 26, DATEADD(DAY, -30, SYSUTCDATETIME())), 4, 1);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (28, N'SAM-990PRO-2TB', N'Samsung 990 PRO 2TB NVMe PCIe 4.0', N'Samsung', N'Uno de los SSD Gen4 más rápidos del mercado, ideal para gaming y creación de contenido.', 3299, 3799, 15, 5, N'Almacen', N'/img/productos/sam-990pro-2tb.webp', N'Capacidad: 2 TB
Interfaz: M.2 NVMe
Formato: M.2 2280
Lectura: 7450 MB/s
Escritura: 6900 MB/s', 1, 1, 1, DATEADD(MINUTE, 27, DATEADD(DAY, -30, SYSUTCDATETIME())), 4, 2);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (29, N'WD-SN850X-1TB', N'WD_BLACK SN850X 1TB NVMe PCIe 4.0', N'Western Digital', N'Rendimiento de primer nivel para juegos con tiempos de carga mínimos.', 1799, 2099, 20, 5, N'Almacen', N'/img/productos/wd-sn850x-1tb.webp', N'Capacidad: 1 TB
Interfaz: M.2 NVMe
Formato: M.2 2280
Lectura: 7300 MB/s
Escritura: 6300 MB/s', 1, 1, 0, DATEADD(MINUTE, 28, DATEADD(DAY, -30, SYSUTCDATETIME())), 4, 3);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (30, N'CRU-P3P-2TB', N'Crucial P3 Plus 2TB NVMe PCIe 4.0', N'Crucial', N'Gran capacidad Gen4 a precio accesible para tu biblioteca de juegos.', 2299, 2599, 18, 5, N'Almacen', N'/img/productos/cru-p3p-2tb.webp', N'Capacidad: 2 TB
Interfaz: M.2 NVMe
Formato: M.2 2280
Lectura: 5000 MB/s
Escritura: 4200 MB/s', 1, 1, 0, DATEADD(MINUTE, 29, DATEADD(DAY, -30, SYSUTCDATETIME())), 4, 1);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (31, N'KIN-A400-480', N'Kingston A400 480GB SSD SATA', N'Kingston', N'SSD SATA de 2.5 pulgadas para revivir equipos o como unidad secundaria.', 599, 699, 50, 5, N'Almacen', N'/img/productos/kin-a400-480.webp', N'Capacidad: 480 GB
Interfaz: SATA
Formato: 2.5 pulgadas
Lectura: 500 MB/s
Escritura: 450 MB/s', 1, 1, 0, DATEADD(MINUTE, 30, DATEADD(DAY, -30, SYSUTCDATETIME())), 4, 4);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (32, N'SEA-BARRA-2TB', N'Seagate BarraCuda 2TB HDD 7200 RPM', N'Seagate', N'Disco duro de 3.5 pulgadas para almacenamiento masivo de archivos y respaldos.', 1199, NULL, 24, 5, N'Almacen', N'/img/productos/sea-barra-2tb.webp', N'Capacidad: 2 TB
Interfaz: SATA
Formato: 3.5 pulgadas
Lectura: 220 MB/s
Escritura: 220 MB/s', 1, 1, 0, DATEADD(MINUTE, 31, DATEADD(DAY, -30, SYSUTCDATETIME())), 4, 4);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (33, N'MSI-RTX4060-V2X', N'MSI GeForce RTX 4060 VENTUS 2X 8GB', N'MSI', N'Compacta y eficiente, perfecta para jugar en 1080p con DLSS 3 y Ray Tracing.', 6299, 6999, 20, 5, N'Almacen', N'/img/productos/msi-rtx4060-v2x.webp', N'GPU: NVIDIA GeForce RTX 4060
Memoria: 8 GB GDDR6
Longitud: 199 mm
Consumo (TBP): 115 W
Fuente recomendada: 550 W
Interfaz: PCI Express 4.0 x16', 1, 1, 0, DATEADD(MINUTE, 32, DATEADD(DAY, -30, SYSUTCDATETIME())), 5, 1);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (34, N'ASU-RTX4060TI-DUAL', N'ASUS Dual GeForce RTX 4060 Ti OC 8GB', N'ASUS', N'Excelente rendimiento 1080p/1440p con diseño de doble ventilador Axial-tech.', 7999, 8799, 14, 5, N'Almacen', N'/img/productos/asu-rtx4060ti-dual.webp', N'GPU: NVIDIA GeForce RTX 4060 Ti
Memoria: 8 GB GDDR6
Longitud: 227 mm
Consumo (TBP): 160 W
Fuente recomendada: 550 W
Interfaz: PCI Express 4.0 x16', 1, 1, 0, DATEADD(MINUTE, 33, DATEADD(DAY, -30, SYSUTCDATETIME())), 5, 2);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (35, N'RTX4070-SUP', N'ASUS Dual GeForce RTX 4070 SUPER OC 12GB', N'ASUS', N'Rendimiento sobresaliente en 1440p con DLSS 3.5 y gran eficiencia energética.', 13899, 15499, 8, 5, N'Almacen', N'/img/productos/rtx4070-sup.webp', N'GPU: NVIDIA GeForce RTX 4070 SUPER
Memoria: 12 GB GDDR6X
Longitud: 267 mm
Consumo (TBP): 220 W
Fuente recomendada: 650 W
Interfaz: PCI Express 4.0 x16', 1, 1, 1, DATEADD(MINUTE, 34, DATEADD(DAY, -30, SYSUTCDATETIME())), 5, 1);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (36, N'GIG-RTX4070TIS-OC', N'GIGABYTE GeForce RTX 4070 Ti SUPER GAMING OC 16GB', N'GIGABYTE', N'16 GB de VRAM y sistema de enfriamiento WINDFORCE para jugar en 1440p y 4K.', 18999, 20999, 6, 5, N'Almacen', N'/img/productos/gig-rtx4070tis-oc.webp', N'GPU: NVIDIA GeForce RTX 4070 Ti SUPER
Memoria: 16 GB GDDR6X
Longitud: 336 mm
Consumo (TBP): 285 W
Fuente recomendada: 750 W
Interfaz: PCI Express 4.0 x16', 1, 1, 0, DATEADD(MINUTE, 35, DATEADD(DAY, -30, SYSUTCDATETIME())), 5, 3);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (37, N'ZOT-RTX5070-TWIN', N'ZOTAC GAMING GeForce RTX 5070 Twin Edge OC 12GB', N'ZOTAC', N'Arquitectura Blackwell con DLSS 4 y Multi Frame Generation en formato compacto.', 13499, NULL, 12, 5, N'Almacen', N'/img/productos/zot-rtx5070-twin.webp', N'GPU: NVIDIA GeForce RTX 5070
Memoria: 12 GB GDDR7
Longitud: 230 mm
Consumo (TBP): 250 W
Fuente recomendada: 650 W
Interfaz: PCI Express 5.0 x16', 1, 1, 1, DATEADD(MINUTE, 36, DATEADD(DAY, -30, SYSUTCDATETIME())), 5, 2);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (38, N'ASU-RTX5080-TUF', N'ASUS TUF Gaming GeForce RTX 5080 OC 16GB', N'ASUS', N'Rendimiento 4K extremo con construcción de grado militar TUF.', 27999, 29999, 4, 5, N'Almacen', N'/img/productos/asu-rtx5080-tuf.webp', N'GPU: NVIDIA GeForce RTX 5080
Memoria: 16 GB GDDR7
Longitud: 348 mm
Consumo (TBP): 360 W
Fuente recomendada: 850 W
Interfaz: PCI Express 5.0 x16', 1, 1, 0, DATEADD(MINUTE, 37, DATEADD(DAY, -30, SYSUTCDATETIME())), 5, 1);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (39, N'SAP-RX7600-PULSE', N'Sapphire PULSE AMD Radeon RX 7600 8GB', N'Sapphire', N'Gran opción para gaming 1080p con FSR y AMD HYPR-RX.', 5699, 6299, 16, 5, N'Almacen', N'/img/productos/sap-rx7600-pulse.webp', N'GPU: AMD Radeon RX 7600
Memoria: 8 GB GDDR6
Longitud: 240 mm
Consumo (TBP): 165 W
Fuente recomendada: 550 W
Interfaz: PCI Express 4.0 x16', 1, 1, 0, DATEADD(MINUTE, 38, DATEADD(DAY, -30, SYSUTCDATETIME())), 5, 3);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (40, N'PWC-RX7800XT-HH', N'PowerColor Hellhound AMD Radeon RX 7800 XT 16GB', N'PowerColor', N'16 GB de VRAM para 1440p con iluminación tenue y enfriamiento silencioso.', 10999, 12499, 9, 5, N'Almacen', N'/img/productos/pwc-rx7800xt-hh.webp', N'GPU: AMD Radeon RX 7800 XT
Memoria: 16 GB GDDR6
Longitud: 322 mm
Consumo (TBP): 263 W
Fuente recomendada: 700 W
Interfaz: PCI Express 4.0 x16', 1, 1, 0, DATEADD(MINUTE, 39, DATEADD(DAY, -30, SYSUTCDATETIME())), 5, 4);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (41, N'SAP-RX9070XT-PULSE', N'Sapphire PULSE AMD Radeon RX 9070 XT 16GB', N'Sapphire', N'Arquitectura RDNA 4 con FSR 4 y ray tracing mejorado para 1440p y 4K.', 15999, 17499, 7, 5, N'Almacen', N'/img/productos/sap-rx9070xt-pulse.webp', N'GPU: AMD Radeon RX 9070 XT
Memoria: 16 GB GDDR6
Longitud: 320 mm
Consumo (TBP): 304 W
Fuente recomendada: 750 W
Interfaz: PCI Express 4.0 x16', 1, 1, 1, DATEADD(MINUTE, 40, DATEADD(DAY, -30, SYSUTCDATETIME())), 5, 3);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (42, N'INT-ARC-B580', N'Intel Arc B580 Limited Edition 12GB', N'Intel', N'12 GB de VRAM a precio accesible con XeSS 2 y codificación AV1.', 5499, NULL, 10, 5, N'Almacen', N'/img/productos/int-arc-b580.webp', N'GPU: Intel Arc B580
Memoria: 12 GB GDDR6
Longitud: 272 mm
Consumo (TBP): 190 W
Fuente recomendada: 600 W
Interfaz: PCI Express 4.0 x16', 1, 1, 0, DATEADD(MINUTE, 41, DATEADD(DAY, -30, SYSUTCDATETIME())), 5, 4);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (43, N'CM-HYPER212-BLK', N'Cooler Master Hyper 212 Black Edition', N'Cooler Master', N'El clásico disipador de torre con ventilador SickleFlow de 120 mm.', 699, 799, 30, 5, N'Almacen', N'/img/productos/cm-hyper212-blk.webp', N'Tipo: Disipador por aire
Sockets compatibles: AM4, AM5, LGA1700, LGA1851
TDP soportado: 150 W
Altura: 159 mm', 1, 1, 0, DATEADD(MINUTE, 42, DATEADD(DAY, -30, SYSUTCDATETIME())), 6, 1);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (44, N'DPC-AK400', N'DeepCool AK400', N'DeepCool', N'Torre compacta de 4 heatpipes con gran capacidad de disipación y bajo ruido.', 649, NULL, 28, 5, N'Almacen', N'/img/productos/dpc-ak400.webp', N'Tipo: Disipador por aire
Sockets compatibles: AM4, AM5, LGA1700, LGA1851
TDP soportado: 220 W
Altura: 155 mm', 1, 1, 0, DATEADD(MINUTE, 43, DATEADD(DAY, -30, SYSUTCDATETIME())), 6, 2);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (45, N'TRM-PA120SE', N'Thermalright Peerless Assassin 120 SE', N'Thermalright', N'Doble torre con 6 heatpipes que rivaliza con líquidas de 240 mm.', 899, 999, 22, 5, N'Almacen', N'/img/productos/trm-pa120se.webp', N'Tipo: Disipador por aire (doble torre)
Sockets compatibles: AM4, AM5, LGA1700, LGA1851
TDP soportado: 245 W
Altura: 155 mm', 1, 1, 1, DATEADD(MINUTE, 44, DATEADD(DAY, -30, SYSUTCDATETIME())), 6, 3);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (46, N'NOC-NHD15-CHR', N'Noctua NH-D15 chromax.black', N'Noctua', N'Referencia en enfriamiento por aire: silencioso y extremadamente eficiente.', 2799, NULL, 6, 5, N'Almacen', N'/img/productos/noc-nhd15-chr.webp', N'Tipo: Disipador por aire (doble torre)
Sockets compatibles: AM4, AM5, LGA1700, LGA1851
TDP soportado: 250 W
Altura: 165 mm', 1, 1, 0, DATEADD(MINUTE, 45, DATEADD(DAY, -30, SYSUTCDATETIME())), 6, 4);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (47, N'NZX-KRAKEN-240', N'NZXT Kraken 240', N'NZXT', N'AIO de 240 mm con pantalla LCD personalizable en la bomba.', 2599, 2999, 10, 5, N'Almacen', N'/img/productos/nzx-kraken-240.webp', N'Tipo: Enfriamiento líquido AIO
Sockets compatibles: AM4, AM5, LGA1700, LGA1851
TDP soportado: 250 W
Radiador: 240 mm', 1, 1, 0, DATEADD(MINUTE, 46, DATEADD(DAY, -30, SYSUTCDATETIME())), 6, 1);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (48, N'ARC-LF3-360', N'ARCTIC Liquid Freezer III 360', N'ARCTIC', N'Radiador grueso de 360 mm y VRM fan para procesadores de alto consumo.', 2299, 2599, 9, 5, N'Almacen', N'/img/productos/arc-lf3-360.webp', N'Tipo: Enfriamiento líquido AIO
Sockets compatibles: AM4, AM5, LGA1700, LGA1851
TDP soportado: 300 W
Radiador: 360 mm', 1, 1, 1, DATEADD(MINUTE, 47, DATEADD(DAY, -30, SYSUTCDATETIME())), 6, 2);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (49, N'NZX-H5-FLOW', N'NZXT H5 Flow (2024) Negro', N'NZXT', N'Panel frontal perforado para máximo flujo de aire y gestión de cables sencilla.', 1899, 2199, 14, 5, N'Almacen', N'/img/productos/nzx-h5-flow.webp', N'Formatos soportados: ATX, mATX, Mini-ITX
GPU máxima: 365 mm
Disipador máximo: 165 mm
Radiador máximo: 360 mm', 1, 1, 1, DATEADD(MINUTE, 48, DATEADD(DAY, -30, SYSUTCDATETIME())), 7, 1);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (50, N'COR-4000D-AF', N'Corsair 4000D Airflow', N'Corsair', N'Diseño de alto flujo con espacio para radiadores de 360 mm.', 1999, 2299, 12, 5, N'Almacen', N'/img/productos/cor-4000d-af.webp', N'Formatos soportados: ATX, mATX, Mini-ITX
GPU máxima: 360 mm
Disipador máximo: 170 mm
Radiador máximo: 360 mm', 1, 1, 0, DATEADD(MINUTE, 49, DATEADD(DAY, -30, SYSUTCDATETIME())), 7, 2);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (51, N'LIA-LANCOOL-216', N'Lian Li LANCOOL 216 RGB', N'Lian Li', N'Dos ventiladores frontales de 160 mm e increíble compatibilidad de componentes.', 2299, NULL, 10, 5, N'Almacen', N'/img/productos/lia-lancool-216.webp', N'Formatos soportados: ATX, mATX, Mini-ITX
GPU máxima: 392 mm
Disipador máximo: 180 mm
Radiador máximo: 360 mm', 1, 1, 0, DATEADD(MINUTE, 50, DATEADD(DAY, -30, SYSUTCDATETIME())), 7, 3);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (52, N'FRA-NORTH', N'Fractal Design North Charcoal', N'Fractal Design', N'Frente de madera natural y malla para un estilo elegante sin sacrificar temperatura.', 2999, 3299, 6, 5, N'Almacen', N'/img/productos/fra-north.webp', N'Formatos soportados: ATX, mATX, Mini-ITX
GPU máxima: 355 mm
Disipador máximo: 170 mm
Radiador máximo: 360 mm', 1, 1, 0, DATEADD(MINUTE, 51, DATEADD(DAY, -30, SYSUTCDATETIME())), 7, 4);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (53, N'CM-Q300L', N'Cooler Master MasterBox Q300L', N'Cooler Master', N'Gabinete Micro-ATX compacto con paneles magnéticos y filtros de polvo.', 999, 1199, 20, 5, N'Almacen', N'/img/productos/cm-q300l.webp', N'Formatos soportados: mATX, Mini-ITX
GPU máxima: 360 mm
Disipador máximo: 159 mm
Radiador máximo: 240 mm', 1, 1, 0, DATEADD(MINUTE, 52, DATEADD(DAY, -30, SYSUTCDATETIME())), 7, 1);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (54, N'MON-AIR100', N'Montech AIR 100 ARGB', N'Montech', N'Micro-ATX con 4 ventiladores ARGB incluidos y excelente ventilación.', 1199, NULL, 16, 5, N'Almacen', N'/img/productos/mon-air100.webp', N'Formatos soportados: mATX, Mini-ITX
GPU máxima: 330 mm
Disipador máximo: 161 mm
Radiador máximo: 240 mm', 1, 1, 0, DATEADD(MINUTE, 53, DATEADD(DAY, -30, SYSUTCDATETIME())), 7, 2);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (55, N'CM-NR200P', N'Cooler Master MasterBox NR200P', N'Cooler Master', N'Mini-ITX de 18 litros que admite tarjetas de video de tres ventiladores.', 2099, 2399, 7, 5, N'Almacen', N'/img/productos/cm-nr200p.webp', N'Formatos soportados: Mini-ITX
GPU máxima: 330 mm
Disipador máximo: 155 mm
Radiador máximo: 280 mm', 1, 1, 0, DATEADD(MINUTE, 54, DATEADD(DAY, -30, SYSUTCDATETIME())), 7, 3);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (56, N'XPG-PYLON-550', N'XPG Pylon 550W 80 PLUS Bronze', N'XPG', N'Fuente confiable para equipos de oficina y gaming de entrada.', 899, 1049, 25, 5, N'Almacen', N'/img/productos/xpg-pylon-550.webp', N'Potencia: 550 W
Certificación: 80 PLUS Bronze
Modular: No
Formato: ATX', 1, 1, 0, DATEADD(MINUTE, 55, DATEADD(DAY, -30, SYSUTCDATETIME())), 8, 4);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (57, N'CM-MWE-650', N'Cooler Master MWE 650 Bronze V2', N'Cooler Master', N'650 W con protecciones completas y ventilador HDB silencioso.', 1099, 1299, 22, 5, N'Almacen', N'/img/productos/cm-mwe-650.webp', N'Potencia: 650 W
Certificación: 80 PLUS Bronze
Modular: No
Formato: ATX', 1, 1, 0, DATEADD(MINUTE, 56, DATEADD(DAY, -30, SYSUTCDATETIME())), 8, 1);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (58, N'COR-RM750E', N'Corsair RM750e 750W 80 PLUS Gold', N'Corsair', N'Totalmente modular, ATX 3.1 con conector 12V-2x6 nativo.', 1899, 2199, 18, 5, N'Almacen', N'/img/productos/cor-rm750e.webp', N'Potencia: 750 W
Certificación: 80 PLUS Gold
Modular: Totalmente modular
Formato: ATX', 1, 1, 1, DATEADD(MINUTE, 57, DATEADD(DAY, -30, SYSUTCDATETIME())), 8, 2);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (59, N'SEA-FOCUS-850', N'Seasonic FOCUS GX-850 80 PLUS Gold', N'Seasonic', N'Calidad Seasonic con 10 años de garantía para equipos de alto rendimiento.', 2599, 2899, 12, 5, N'Almacen', N'/img/productos/sea-focus-850.webp', N'Potencia: 850 W
Certificación: 80 PLUS Gold
Modular: Totalmente modular
Formato: ATX', 1, 1, 0, DATEADD(MINUTE, 58, DATEADD(DAY, -30, SYSUTCDATETIME())), 8, 3);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (60, N'COR-RM1000X', N'Corsair RM1000x 1000W 80 PLUS Gold', N'Corsair', N'1000 W para tarjetas de gama alta y overclocking con ventilador de levitación magnética.', 3499, 3899, 8, 5, N'Almacen', N'/img/productos/cor-rm1000x.webp', N'Potencia: 1000 W
Certificación: 80 PLUS Gold
Modular: Totalmente modular
Formato: ATX', 1, 1, 0, DATEADD(MINUTE, 59, DATEADD(DAY, -30, SYSUTCDATETIME())), 8, 2);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (61, N'EVG-600-BR', N'EVGA 600 BR 600W 80 PLUS Bronze', N'EVGA', N'Fuente de 600 W eficiente con 3 años de garantía.', 999, NULL, 15, 5, N'Almacen', N'/img/productos/evg-600-br.webp', N'Potencia: 600 W
Certificación: 80 PLUS Bronze
Modular: No
Formato: ATX', 1, 1, 0, DATEADD(MINUTE, 60, DATEADD(DAY, -30, SYSUTCDATETIME())), 8, 4);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (62, N'ASU-VG249Q3A', N'ASUS TUF Gaming VG249Q3A 24" 180Hz', N'ASUS', N'Monitor gamer Full HD de 180 Hz con 1 ms y Adaptive Sync.', 2799, 3299, 15, 5, N'Almacen', N'/img/productos/asu-vg249q3a.webp', N'Tamaño: 23.8 pulgadas
Resolución: 1920 x 1080
Tasa de refresco: 180 Hz
Panel: IPS Fast', 1, 1, 0, DATEADD(MINUTE, 61, DATEADD(DAY, -30, SYSUTCDATETIME())), 9, 1);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (63, N'SAM-ODY-G5-27', N'Samsung Odyssey G5 27" QHD 165Hz', N'Samsung', N'Curvatura 1000R para mayor inmersión en resolución QHD.', 4499, 5299, 10, 5, N'Almacen', N'/img/productos/sam-ody-g5-27.webp', N'Tamaño: 27 pulgadas
Resolución: 2560 x 1440
Tasa de refresco: 165 Hz
Panel: VA curvo 1000R', 1, 1, 1, DATEADD(MINUTE, 62, DATEADD(DAY, -30, SYSUTCDATETIME())), 9, 2);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (64, N'LG-27GS75Q', N'LG UltraGear 27GS75Q 27" QHD 180Hz', N'LG', N'QHD IPS de 180 Hz con HDR10 y colores precisos.', 4799, NULL, 9, 5, N'Almacen', N'/img/productos/lg-27gs75q.webp', N'Tamaño: 27 pulgadas
Resolución: 2560 x 1440
Tasa de refresco: 180 Hz
Panel: IPS', 1, 1, 0, DATEADD(MINUTE, 63, DATEADD(DAY, -30, SYSUTCDATETIME())), 9, 3);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (65, N'GIG-M27Q', N'GIGABYTE M27Q 27" QHD 170Hz KVM', N'GIGABYTE', N'Monitor con función KVM integrada para controlar dos equipos.', 4299, 4799, 8, 5, N'Almacen', N'/img/productos/gig-m27q.webp', N'Tamaño: 27 pulgadas
Resolución: 2560 x 1440
Tasa de refresco: 170 Hz
Panel: SS IPS', 1, 1, 0, DATEADD(MINUTE, 64, DATEADD(DAY, -30, SYSUTCDATETIME())), 9, 4);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (66, N'HYX-ALLOY-CORE', N'HyperX Alloy Origins Core TKL', N'HyperX', N'Teclado mecánico TKL con cuerpo de aluminio y switches lineales.', 1599, 1899, 18, 5, N'Almacen', N'/img/productos/hyx-alloy-core.webp', N'Formato: TKL
Switches: HyperX Red lineales
Iluminación: RGB
Conexión: USB-C desmontable', 1, 1, 0, DATEADD(MINUTE, 65, DATEADD(DAY, -30, SYSUTCDATETIME())), 10, 1);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (67, N'RED-K552-KUMARA', N'Redragon Kumara K552 RGB', N'Redragon', N'Teclado mecánico compacto y resistente, favorito para iniciar en el gaming.', 699, 799, 30, 5, N'Almacen', N'/img/productos/red-k552-kumara.webp', N'Formato: TKL
Switches: Outemu Blue
Iluminación: RGB
Conexión: USB', 1, 1, 0, DATEADD(MINUTE, 66, DATEADD(DAY, -30, SYSUTCDATETIME())), 10, 4);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (68, N'LOG-G515-TKL', N'Logitech G515 LIGHTSPEED TKL', N'Logitech', N'Teclado inalámbrico de perfil bajo con switches mecánicos y batería de larga duración.', 2799, NULL, 10, 5, N'Almacen', N'/img/productos/log-g515-tkl.webp', N'Formato: TKL bajo perfil
Switches: GL Tactile
Iluminación: RGB LIGHTSYNC
Conexión: Inalámbrico / Bluetooth', 1, 1, 0, DATEADD(MINUTE, 67, DATEADD(DAY, -30, SYSUTCDATETIME())), 10, 2);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (69, N'LOG-G502-HERO', N'Logitech G502 HERO', N'Logitech', N'El mouse gamer más vendido con pesas ajustables y sensor HERO.', 899, 1099, 35, 5, N'Almacen', N'/img/productos/log-g502-hero.webp', N'Sensor: HERO 25K
DPI: 100 - 25,600
Botones: 11 programables
Conexión: USB', 1, 1, 1, DATEADD(MINUTE, 68, DATEADD(DAY, -30, SYSUTCDATETIME())), 11, 1);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (70, N'RAZ-DA-V3', N'Razer DeathAdder V3', N'Razer', N'Ergonomía icónica ultraligera para esports.', 1499, 1799, 14, 5, N'Almacen', N'/img/productos/raz-da-v3.webp', N'Sensor: Focus Pro 30K
DPI: 30,000
Peso: 59 g
Conexión: USB', 1, 1, 0, DATEADD(MINUTE, 69, DATEADD(DAY, -30, SYSUTCDATETIME())), 11, 3);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (71, N'HYX-CLOUD-II', N'HyperX Cloud II 7.1', N'HyperX', N'Audífonos legendarios por su comodidad y sonido envolvente 7.1.', 1399, 1699, 20, 5, N'Almacen', N'/img/productos/hyx-cloud-ii.webp', N'Tipo: Over-ear
Sonido: 7.1 virtual
Micrófono: Desmontable con cancelación
Conexión: USB / 3.5 mm', 1, 1, 0, DATEADD(MINUTE, 70, DATEADD(DAY, -30, SYSUTCDATETIME())), 12, 1);
INSERT INTO Productos (Id, Sku, Nombre, Marca, Descripcion, Precio, PrecioAnterior, Stock, StockMinimo, TipoInventario, ImagenUrl, EspecificacionesTecnicas, Envio, Activo, Destacado, FechaAlta, CategoriaId, ProveedorId) VALUES (72, N'LOG-G435', N'Logitech G435 LIGHTSPEED', N'Logitech', N'Audífonos inalámbricos ultraligeros para jugar con comodidad por horas.', 1199, NULL, 16, 5, N'Almacen', N'/img/productos/log-g435.webp', N'Tipo: Over-ear ligero
Peso: 165 g
Batería: 18 horas
Conexión: LIGHTSPEED / Bluetooth', 1, 1, 0, DATEADD(MINUTE, 71, DATEADD(DAY, -30, SYSUTCDATETIME())), 12, 2);
SET IDENTITY_INSERT Productos OFF;
GO

INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (1, N'AM4', NULL, NULL, NULL, NULL, NULL, 65, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 1);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (2, N'AM4', NULL, NULL, NULL, NULL, NULL, 105, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 0);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (3, N'AM5', NULL, NULL, NULL, NULL, NULL, 105, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, 0);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (4, N'AM5', NULL, NULL, NULL, NULL, NULL, 120, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, 0);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (5, N'AM5', NULL, NULL, NULL, NULL, NULL, 65, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, 0);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (6, N'AM5', NULL, NULL, NULL, NULL, NULL, 120, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, 0);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (7, N'LGA1700', NULL, NULL, NULL, NULL, NULL, 65, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, 1);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (8, N'LGA1700', NULL, NULL, NULL, NULL, NULL, 125, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, 0);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (9, N'LGA1700', NULL, NULL, NULL, NULL, NULL, 125, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, 0);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (10, N'LGA1851', NULL, NULL, NULL, NULL, NULL, 125, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, 0);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (11, N'LGA1851', NULL, NULL, NULL, NULL, NULL, 125, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, 0);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (12, N'AM4', NULL, N'DDR4', N'mATX', NULL, NULL, 40, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 4, NULL, NULL, 2, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (13, N'AM4', NULL, N'DDR4', N'ATX', NULL, NULL, 40, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 4, NULL, NULL, 2, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (14, N'AM5', NULL, N'DDR5', N'mATX', NULL, NULL, 40, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 4, NULL, NULL, 2, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (15, N'AM5', NULL, N'DDR5', N'ATX', NULL, NULL, 40, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 4, NULL, NULL, 3, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (16, N'AM5', NULL, N'DDR5', N'Mini-ITX', NULL, NULL, 40, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 2, NULL, NULL, 2, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (17, N'LGA1700', NULL, N'DDR4', N'mATX', NULL, NULL, 40, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 4, NULL, NULL, 2, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (18, N'LGA1700', NULL, N'DDR5', N'ATX', NULL, NULL, 40, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 4, NULL, NULL, 4, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (19, N'LGA1851', NULL, N'DDR5', N'ATX', NULL, NULL, 40, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 4, NULL, NULL, 4, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (20, N'LGA1851', NULL, N'DDR5', N'mATX', NULL, NULL, 40, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 4, NULL, NULL, 2, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (21, NULL, NULL, N'DDR4', NULL, NULL, NULL, 10, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 2, 16, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (22, NULL, NULL, N'DDR4', NULL, NULL, NULL, 10, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 2, 32, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (23, NULL, NULL, N'DDR5', NULL, NULL, NULL, 10, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 2, 32, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (24, NULL, NULL, N'DDR5', NULL, NULL, NULL, 10, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 2, 32, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (25, NULL, NULL, N'DDR5', NULL, NULL, NULL, 10, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 2, 64, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (26, NULL, NULL, N'DDR5', NULL, NULL, NULL, 10, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 2, 16, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (27, NULL, NULL, NULL, NULL, NULL, N'M.2 NVMe', 8, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1000, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (28, NULL, NULL, NULL, NULL, NULL, N'M.2 NVMe', 8, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 2000, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (29, NULL, NULL, NULL, NULL, NULL, N'M.2 NVMe', 8, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1000, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (30, NULL, NULL, NULL, NULL, NULL, N'M.2 NVMe', 8, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 2000, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (31, NULL, NULL, NULL, NULL, NULL, N'SATA', 8, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 480, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (32, NULL, NULL, NULL, NULL, NULL, N'SATA', 8, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 2000, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (33, NULL, NULL, NULL, NULL, NULL, NULL, 115, NULL, 550, NULL, 199, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (34, NULL, NULL, NULL, NULL, NULL, NULL, 160, NULL, 550, NULL, 227, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (35, NULL, NULL, NULL, NULL, NULL, NULL, 220, NULL, 650, NULL, 267, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (36, NULL, NULL, NULL, NULL, NULL, NULL, 285, NULL, 750, NULL, 336, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (37, NULL, NULL, NULL, NULL, NULL, NULL, 250, NULL, 650, NULL, 230, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (38, NULL, NULL, NULL, NULL, NULL, NULL, 360, NULL, 850, NULL, 348, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (39, NULL, NULL, NULL, NULL, NULL, NULL, 165, NULL, 550, NULL, 240, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (40, NULL, NULL, NULL, NULL, NULL, NULL, 263, NULL, 700, NULL, 322, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (41, NULL, NULL, NULL, NULL, NULL, NULL, 304, NULL, 750, NULL, 320, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (42, NULL, NULL, NULL, NULL, NULL, NULL, 190, NULL, 600, NULL, 272, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (43, NULL, N'AM4,AM5,LGA1700,LGA1851', NULL, NULL, NULL, NULL, 5, NULL, NULL, 150, NULL, NULL, 159, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (44, NULL, N'AM4,AM5,LGA1700,LGA1851', NULL, NULL, NULL, NULL, 5, NULL, NULL, 220, NULL, NULL, 155, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (45, NULL, N'AM4,AM5,LGA1700,LGA1851', NULL, NULL, NULL, NULL, 5, NULL, NULL, 245, NULL, NULL, 155, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (46, NULL, N'AM4,AM5,LGA1700,LGA1851', NULL, NULL, NULL, NULL, 5, NULL, NULL, 250, NULL, NULL, 165, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (47, NULL, N'AM4,AM5,LGA1700,LGA1851', NULL, NULL, NULL, NULL, 15, NULL, NULL, 250, NULL, NULL, NULL, NULL, 240, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (48, NULL, N'AM4,AM5,LGA1700,LGA1851', NULL, NULL, NULL, NULL, 15, NULL, NULL, 300, NULL, NULL, NULL, NULL, 360, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (49, NULL, NULL, NULL, NULL, N'ATX,mATX,Mini-ITX', NULL, NULL, NULL, NULL, NULL, NULL, 365, NULL, 165, NULL, 360, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (50, NULL, NULL, NULL, NULL, N'ATX,mATX,Mini-ITX', NULL, NULL, NULL, NULL, NULL, NULL, 360, NULL, 170, NULL, 360, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (51, NULL, NULL, NULL, NULL, N'ATX,mATX,Mini-ITX', NULL, NULL, NULL, NULL, NULL, NULL, 392, NULL, 180, NULL, 360, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (52, NULL, NULL, NULL, NULL, N'ATX,mATX,Mini-ITX', NULL, NULL, NULL, NULL, NULL, NULL, 355, NULL, 170, NULL, 360, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (53, NULL, NULL, NULL, NULL, N'mATX,Mini-ITX', NULL, NULL, NULL, NULL, NULL, NULL, 360, NULL, 159, NULL, 240, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (54, NULL, NULL, NULL, NULL, N'mATX,Mini-ITX', NULL, NULL, NULL, NULL, NULL, NULL, 330, NULL, 161, NULL, 240, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (55, NULL, NULL, NULL, NULL, N'Mini-ITX', NULL, NULL, NULL, NULL, NULL, NULL, 330, NULL, 155, NULL, 280, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (56, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 550, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (57, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 650, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (58, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 750, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (59, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 850, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (60, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1000, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO EspecificacionesComponente (ProductoId, Socket, SocketsSoportados, TipoMemoria, FormFactor, FormFactorsSoportados, Interfaz, ConsumoW, CapacidadW, FuenteRecomendadaW, TdpMaxW, LongitudMm, LongitudMaxGpuMm, AlturaMm, AlturaMaxCoolerMm, RadiadorMm, RadiadorMaxMm, SlotsRam, ModulosRam, CapacidadGb, RanurasM2, GraficosIntegrados, IncluyeDisipador) VALUES (61, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 600, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
GO

INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (1, N'Entrada', 25, 25, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (2, N'Entrada', 14, 14, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (3, N'Entrada', 18, 18, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (4, N'Entrada', 9, 9, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (5, N'Entrada', 12, 12, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (6, N'Entrada', 7, 7, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (7, N'Entrada', 30, 30, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (8, N'Entrada', 15, 15, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (9, N'Entrada', 10, 10, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (10, N'Entrada', 11, 11, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (11, N'Entrada', 8, 8, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (12, N'Entrada', 16, 16, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (13, N'Entrada', 10, 10, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (14, N'Entrada', 14, 14, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (15, N'Entrada', 12, 12, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (16, N'Entrada', 6, 6, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (17, N'Entrada', 15, 15, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (18, N'Entrada', 8, 8, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (19, N'Entrada', 6, 6, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (20, N'Entrada', 10, 10, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (21, N'Entrada', 40, 40, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (22, N'Entrada', 22, 22, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (23, N'Entrada', 28, 28, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (24, N'Entrada', 20, 20, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (25, N'Entrada', 8, 8, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (26, N'Entrada', 25, 25, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (27, N'Entrada', 45, 45, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (28, N'Entrada', 15, 15, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (29, N'Entrada', 20, 20, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (30, N'Entrada', 18, 18, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (31, N'Entrada', 50, 50, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (32, N'Entrada', 24, 24, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (33, N'Entrada', 20, 20, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (34, N'Entrada', 14, 14, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (35, N'Entrada', 8, 8, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (36, N'Entrada', 6, 6, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (37, N'Entrada', 12, 12, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (38, N'Entrada', 4, 4, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (39, N'Entrada', 16, 16, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (40, N'Entrada', 9, 9, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (41, N'Entrada', 7, 7, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (42, N'Entrada', 10, 10, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (43, N'Entrada', 30, 30, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (44, N'Entrada', 28, 28, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (45, N'Entrada', 22, 22, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (46, N'Entrada', 6, 6, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (47, N'Entrada', 10, 10, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (48, N'Entrada', 9, 9, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (49, N'Entrada', 14, 14, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (50, N'Entrada', 12, 12, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (51, N'Entrada', 10, 10, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (52, N'Entrada', 6, 6, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (53, N'Entrada', 20, 20, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (54, N'Entrada', 16, 16, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (55, N'Entrada', 7, 7, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (56, N'Entrada', 25, 25, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (57, N'Entrada', 22, 22, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (58, N'Entrada', 18, 18, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (59, N'Entrada', 12, 12, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (60, N'Entrada', 8, 8, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (61, N'Entrada', 15, 15, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (62, N'Entrada', 15, 15, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (63, N'Entrada', 10, 10, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (64, N'Entrada', 9, 9, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (65, N'Entrada', 8, 8, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (66, N'Entrada', 18, 18, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (67, N'Entrada', 30, 30, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (68, N'Entrada', 10, 10, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (69, N'Entrada', 35, 35, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (70, N'Entrada', 14, 14, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (71, N'Entrada', 20, 20, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
INSERT INTO MovimientosKardex (ProductoId, TipoMovimiento, Cantidad, StockResultante, Fecha, Motivo, PedidoId, AdministradorResponsable) VALUES (72, N'Entrada', 16, 16, DATEADD(DAY, -30, SYSUTCDATETIME()), N'Inventario inicial', NULL, N'sistema');
GO

INSERT INTO Sucursales (Nombre, Direccion, Ciudad, Telefono, Horario, Latitud, Longitud, Activa) VALUES
 (N'Reach León Centro', N'Blvd. Adolfo López Mateos 1502, Col. Los Gavilanes', N'León, Gto.', N'4777123400', N'Lun a Sáb 10:00 a 20:00 · Dom 11:00 a 17:00', 21.124300, -101.671900, 1),
 (N'Reach Plaza Mayor', N'Blvd. Juan Alonso de Torres 2002, Valle del Campestre', N'León, Gto.', N'4777123401', N'Lun a Dom 11:00 a 21:00', 21.159300, -101.694800, 1),
 (N'Reach Guanajuato Capital', N'Av. Juárez 120, Zona Centro', N'Guanajuato, Gto.', N'4737321500', N'Lun a Sáb 10:00 a 19:00', 21.017000, -101.256500, 1);
GO

INSERT INTO MetodosEnvio (Nombre, Descripcion, Paqueteria, Costo, DiasMin, DiasMax, AplicaEnvioGratis, RequiereDireccion, Activo) VALUES
 (N'Envío estándar', N'Entrega a domicilio en 3 a 5 días hábiles.', N'Paquetexpress', 149, 3, 5, 1, 1, 1),
 (N'Envío express', N'Entrega a domicilio en 1 a 2 días hábiles.', N'Estafeta', 299, 1, 2, 0, 1, 1),
 (N'Recoger en tienda', N'Elige la sucursal más cercana. Sin costo.', N'Sucursal Reach', 0, 0, 1, 0, 0, 1);
GO

INSERT INTO Usuarios (Nombre, Email, PasswordHash, Telefono, Rol, Activo, IntentosFallidos, BloqueadoHasta, FechaRegistro) VALUES
 (N'Alex Moreno', N'admin@reach.mx', N'$2a$11$gyomE3oIsSGK4m0gaY0jUOJiQBNX9Zn1OZc/dnR6S7GplbZMPbMAi', N'4771234567', N'Administrador', 1, 0, NULL, SYSUTCDATETIME()),
 (N'Sofía Ramírez', N'empleado@reach.mx', N'$2a$11$2Nl1xFdwCvfzFamPnHCgFuw7QSjRy5RA6rKJC.B58le31LOOxwGJK', N'4777654321', N'Empleado', 1, 0, NULL, SYSUTCDATETIME()),
 (N'Carlos Sánchez', N'cliente@reach.mx', N'$2a$11$HX.CHVKmIH1EGBQJ3E8FaeHGjVsYtCTXI8a0XPzBuqIyNLU8W/mPi', N'4779876543', N'Comprador', 1, 0, NULL, SYSUTCDATETIME());
GO

INSERT INTO Direcciones (UsuarioId, Alias, NombreRecibe, Telefono, Calle, NumeroExterior, NumeroInterior, Colonia, Ciudad, Estado, CodigoPostal, Referencias, EsPredeterminada, Latitud, Longitud)
SELECT Id, N'Casa', N'Carlos Sánchez', N'4779876543', N'Blvd. Torres Landa', N'1701', NULL, N'Jardines del Moral', N'León', N'Guanajuato', N'37160', N'Portón negro, frente al parque', 1, 21.149800, -101.701400
FROM Usuarios WHERE Email = N'cliente@reach.mx';
GO

INSERT INTO Notificaciones (UsuarioId, Titulo, Mensaje, Tipo, PedidoId, Leida, Fecha)
SELECT Id, N'¡Bienvenido a Reach!', N'Arma tu PC con piezas 100% compatibles y recibe aquí cada avance de tus pedidos.', N'Sistema', NULL, 0, SYSUTCDATETIME()
FROM Usuarios WHERE Email = N'cliente@reach.mx';
GO

SET IDENTITY_INSERT FichasTecnicas ON;
INSERT INTO FichasTecnicas (Id, NombreEquipoPrearmado, CategoriaUso, Descripcion, CostoManoObra, PrecioSugerido, Activo) VALUES (1, N'Reach Office Pro', N'Ofimática', N'Equipo silencioso y eficiente para oficina, escuela y multitarea diaria.', 799, 15192, 1);
SET IDENTITY_INSERT FichasTecnicas OFF;
INSERT INTO FichaTecnicaDetalles (FichaTecnicaId, ProductoId, Cantidad) VALUES (1, 7, 1);
INSERT INTO FichaTecnicaDetalles (FichaTecnicaId, ProductoId, Cantidad) VALUES (1, 17, 1);
INSERT INTO FichaTecnicaDetalles (FichaTecnicaId, ProductoId, Cantidad) VALUES (1, 21, 1);
INSERT INTO FichaTecnicaDetalles (FichaTecnicaId, ProductoId, Cantidad) VALUES (1, 27, 1);
INSERT INTO FichaTecnicaDetalles (FichaTecnicaId, ProductoId, Cantidad) VALUES (1, 39, 1);
INSERT INTO FichaTecnicaDetalles (FichaTecnicaId, ProductoId, Cantidad) VALUES (1, 53, 1);
INSERT INTO FichaTecnicaDetalles (FichaTecnicaId, ProductoId, Cantidad) VALUES (1, 56, 1);
SET IDENTITY_INSERT FichasTecnicas ON;
INSERT INTO FichasTecnicas (Id, NombreEquipoPrearmado, CategoriaUso, Descripcion, CostoManoObra, PrecioSugerido, Activo) VALUES (2, N'Reach Gamer 1440p', N'Gaming', N'Gaming fluido en 1440p con Ryzen 7 7800X3D y RTX 4070 SUPER.', 799, 37491, 1);
SET IDENTITY_INSERT FichasTecnicas OFF;
INSERT INTO FichaTecnicaDetalles (FichaTecnicaId, ProductoId, Cantidad) VALUES (2, 4, 1);
INSERT INTO FichaTecnicaDetalles (FichaTecnicaId, ProductoId, Cantidad) VALUES (2, 14, 1);
INSERT INTO FichaTecnicaDetalles (FichaTecnicaId, ProductoId, Cantidad) VALUES (2, 23, 1);
INSERT INTO FichaTecnicaDetalles (FichaTecnicaId, ProductoId, Cantidad) VALUES (2, 28, 1);
INSERT INTO FichaTecnicaDetalles (FichaTecnicaId, ProductoId, Cantidad) VALUES (2, 35, 1);
INSERT INTO FichaTecnicaDetalles (FichaTecnicaId, ProductoId, Cantidad) VALUES (2, 45, 1);
INSERT INTO FichaTecnicaDetalles (FichaTecnicaId, ProductoId, Cantidad) VALUES (2, 49, 1);
INSERT INTO FichaTecnicaDetalles (FichaTecnicaId, ProductoId, Cantidad) VALUES (2, 58, 1);
SET IDENTITY_INSERT FichasTecnicas ON;
INSERT INTO FichasTecnicas (Id, NombreEquipoPrearmado, CategoriaUso, Descripcion, CostoManoObra, PrecioSugerido, Activo) VALUES (3, N'Reach Creator Studio', N'Diseño y edición', N'Potencia para edición de video, arquitectura y renderizado 3D.', 799, 49991, 1);
SET IDENTITY_INSERT FichasTecnicas OFF;
INSERT INTO FichaTecnicaDetalles (FichaTecnicaId, ProductoId, Cantidad) VALUES (3, 11, 1);
INSERT INTO FichaTecnicaDetalles (FichaTecnicaId, ProductoId, Cantidad) VALUES (3, 19, 1);
INSERT INTO FichaTecnicaDetalles (FichaTecnicaId, ProductoId, Cantidad) VALUES (3, 25, 1);
INSERT INTO FichaTecnicaDetalles (FichaTecnicaId, ProductoId, Cantidad) VALUES (3, 28, 1);
INSERT INTO FichaTecnicaDetalles (FichaTecnicaId, ProductoId, Cantidad) VALUES (3, 36, 1);
INSERT INTO FichaTecnicaDetalles (FichaTecnicaId, ProductoId, Cantidad) VALUES (3, 48, 1);
INSERT INTO FichaTecnicaDetalles (FichaTecnicaId, ProductoId, Cantidad) VALUES (3, 51, 1);
INSERT INTO FichaTecnicaDetalles (FichaTecnicaId, ProductoId, Cantidad) VALUES (3, 59, 1);
GO

SELECT COUNT(*) AS TotalProductos FROM Productos;
GO