/* =====================================================================
   SISTEMA DE GESTIÓN DE BIBLIOTECA - Script de base de datos (SQL Server)
   Aca - Programación Avanzada

   Contenido:
     1. Creación de la base de datos BibliotecaDB
     2. Creación de tablas (claves primarias, foráneas, unicidad y restricciones)
     3. Índices
     4. Vistas utilizadas por el módulo de reportes
     5. Datos de prueba
     6. Consultas de verificación

   Cómo ejecutarlo:
     - SQL Server Management Studio: abrir este archivo y presionar F5.
     - Línea de comandos:  sqlcmd -S localhost\SQLEXPRESS -E -I -i biblioteca_script.sql

   ATENCIÓN: si la base BibliotecaDB ya existe, este script la ELIMINA y la
   vuelve a crear desde cero (se pierden los datos que tenga).
   ===================================================================== */

USE master;
GO

IF DB_ID(N'BibliotecaDB') IS NOT NULL
BEGIN
    ALTER DATABASE BibliotecaDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE BibliotecaDB;
END
GO

-- Intercalación sin distinción de mayúsculas ni tildes: buscar "garcia" encuentra "García".
CREATE DATABASE BibliotecaDB COLLATE Latin1_General_CI_AI;
GO

USE BibliotecaDB;
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* =====================================================================
   2. TABLAS
   ===================================================================== */

-- ---------------------------------------------------------------------
-- Autores
-- ---------------------------------------------------------------------
CREATE TABLE dbo.Autores
(
    IdAutor      INT IDENTITY(1, 1) NOT NULL,
    Nombre       NVARCHAR(80)       NOT NULL,
    Apellido     NVARCHAR(80)       NOT NULL,
    Nacionalidad NVARCHAR(60)       NULL,
    CONSTRAINT PK_Autores PRIMARY KEY (IdAutor),
    CONSTRAINT UQ_Autores_NombreApellido UNIQUE (Nombre, Apellido),
    CONSTRAINT CK_Autores_Nombre CHECK (LEN(LTRIM(RTRIM(Nombre))) > 0),
    CONSTRAINT CK_Autores_Apellido CHECK (LEN(LTRIM(RTRIM(Apellido))) > 0)
);
GO

-- ---------------------------------------------------------------------
-- Editoriales
-- ---------------------------------------------------------------------
CREATE TABLE dbo.Editoriales
(
    IdEditorial INT IDENTITY(1, 1) NOT NULL,
    Nombre      NVARCHAR(100)      NOT NULL,
    Pais        NVARCHAR(60)       NULL,
    Telefono    NVARCHAR(20)       NULL,
    CONSTRAINT PK_Editoriales PRIMARY KEY (IdEditorial),
    CONSTRAINT UQ_Editoriales_Nombre UNIQUE (Nombre),
    CONSTRAINT CK_Editoriales_Nombre CHECK (LEN(LTRIM(RTRIM(Nombre))) > 0)
);
GO

-- ---------------------------------------------------------------------
-- Usuarios
-- ---------------------------------------------------------------------
CREATE TABLE dbo.Usuarios
(
    IdUsuario INT IDENTITY(1, 1) NOT NULL,
    Nombre    NVARCHAR(80)       NOT NULL,
    Apellido  NVARCHAR(80)       NOT NULL,
    Documento NVARCHAR(20)       NOT NULL,
    Telefono  NVARCHAR(20)       NULL,
    Correo    NVARCHAR(120)      NULL,
    CONSTRAINT PK_Usuarios PRIMARY KEY (IdUsuario),
    CONSTRAINT UQ_Usuarios_Documento UNIQUE (Documento),
    CONSTRAINT CK_Usuarios_Nombre CHECK (LEN(LTRIM(RTRIM(Nombre))) > 0),
    CONSTRAINT CK_Usuarios_Apellido CHECK (LEN(LTRIM(RTRIM(Apellido))) > 0),
    CONSTRAINT CK_Usuarios_Correo CHECK (Correo IS NULL OR Correo LIKE N'%_@_%._%')
);
GO

-- ---------------------------------------------------------------------
-- Libros
--   Existencias = total de ejemplares que tiene la biblioteca.
--   Disponibles = ejemplares que están en el estante (no prestados).
-- ---------------------------------------------------------------------
CREATE TABLE dbo.Libros
(
    IdLibro         INT IDENTITY(1, 1) NOT NULL,
    Isbn            NVARCHAR(17)       NOT NULL,
    Titulo          NVARCHAR(200)      NOT NULL,
    IdAutor         INT                NOT NULL,
    IdEditorial     INT                NOT NULL,
    AnioPublicacion SMALLINT           NOT NULL,
    Existencias     INT                NOT NULL CONSTRAINT DF_Libros_Existencias DEFAULT (1),
    Disponibles     INT                NOT NULL CONSTRAINT DF_Libros_Disponibles DEFAULT (1),
    CONSTRAINT PK_Libros PRIMARY KEY (IdLibro),
    CONSTRAINT UQ_Libros_Isbn UNIQUE (Isbn),
    CONSTRAINT FK_Libros_Autores FOREIGN KEY (IdAutor) REFERENCES dbo.Autores (IdAutor),
    CONSTRAINT FK_Libros_Editoriales FOREIGN KEY (IdEditorial) REFERENCES dbo.Editoriales (IdEditorial),
    CONSTRAINT CK_Libros_Titulo CHECK (LEN(LTRIM(RTRIM(Titulo))) > 0),
    CONSTRAINT CK_Libros_Anio CHECK (AnioPublicacion BETWEEN 1000 AND 2100),
    CONSTRAINT CK_Libros_Existencias CHECK (Existencias >= 0),
    CONSTRAINT CK_Libros_Disponibles CHECK (Disponibles >= 0 AND Disponibles <= Existencias)
);
GO

-- ---------------------------------------------------------------------
-- Préstamos
--   Un préstamo asocia un usuario con un libro. Mientras el libro no se
--   devuelve, Estado = 'Prestado' y FechaDevolucion es NULL.
--   No se necesita una tabla DetallePrestamos: cada préstamo es de un
--   solo libro (relación uno a uno entre préstamo y libro).
-- ---------------------------------------------------------------------
CREATE TABLE dbo.Prestamos
(
    IdPrestamo      INT IDENTITY(1, 1) NOT NULL,
    IdUsuario       INT                NOT NULL,
    IdLibro         INT                NOT NULL,
    FechaPrestamo   DATE               NOT NULL CONSTRAINT DF_Prestamos_Fecha DEFAULT (CAST(GETDATE() AS DATE)),
    FechaDevolucion DATE               NULL,
    Estado          NVARCHAR(10)       NOT NULL CONSTRAINT DF_Prestamos_Estado DEFAULT (N'Prestado'),
    CONSTRAINT PK_Prestamos PRIMARY KEY (IdPrestamo),
    CONSTRAINT FK_Prestamos_Usuarios FOREIGN KEY (IdUsuario) REFERENCES dbo.Usuarios (IdUsuario),
    CONSTRAINT FK_Prestamos_Libros FOREIGN KEY (IdLibro) REFERENCES dbo.Libros (IdLibro),
    CONSTRAINT CK_Prestamos_Estado CHECK (Estado IN (N'Prestado', N'Devuelto')),
    -- Coherencia entre el estado y la fecha de devolución
    CONSTRAINT CK_Prestamos_Coherencia CHECK
    (
        (Estado = N'Prestado' AND FechaDevolucion IS NULL)
        OR
        (Estado = N'Devuelto' AND FechaDevolucion IS NOT NULL AND FechaDevolucion >= FechaPrestamo)
    )
);
GO

/* =====================================================================
   3. ÍNDICES
   ===================================================================== */

-- Un usuario no puede tener el mismo libro prestado dos veces a la vez
-- (sí puede volver a pedirlo después de devolverlo).
CREATE UNIQUE INDEX UX_Prestamos_Activo
    ON dbo.Prestamos (IdUsuario, IdLibro)
    WHERE Estado = N'Prestado';
GO

-- Aceleran los JOIN y las búsquedas más frecuentes de la aplicación.
CREATE INDEX IX_Libros_IdAutor ON dbo.Libros (IdAutor);
CREATE INDEX IX_Libros_IdEditorial ON dbo.Libros (IdEditorial);
CREATE INDEX IX_Libros_Titulo ON dbo.Libros (Titulo);
CREATE INDEX IX_Prestamos_IdUsuario ON dbo.Prestamos (IdUsuario);
CREATE INDEX IX_Prestamos_IdLibro ON dbo.Prestamos (IdLibro);
GO

/* =====================================================================
   4. VISTAS (las usa el módulo de reportes de la aplicación)
   ===================================================================== */

CREATE VIEW dbo.vw_Libros
AS
SELECT  l.IdLibro,
        l.Isbn,
        l.Titulo,
        a.Nombre + N' ' + a.Apellido AS Autor,
        e.Nombre                     AS Editorial,
        l.AnioPublicacion,
        l.Existencias,
        l.Disponibles
FROM    dbo.Libros l
        INNER JOIN dbo.Autores a ON a.IdAutor = l.IdAutor
        INNER JOIN dbo.Editoriales e ON e.IdEditorial = l.IdEditorial;
GO

CREATE VIEW dbo.vw_Prestamos
AS
SELECT  p.IdPrestamo,
        u.Nombre + N' ' + u.Apellido AS Usuario,
        u.Documento,
        l.Titulo                     AS Libro,
        l.Isbn,
        p.FechaPrestamo,
        p.FechaDevolucion,
        p.Estado,
        -- Días que duró el préstamo (si ya se devolvió) o que lleva activo
        DATEDIFF(DAY, p.FechaPrestamo, ISNULL(p.FechaDevolucion, CAST(GETDATE() AS DATE))) AS DiasTranscurridos
FROM    dbo.Prestamos p
        INNER JOIN dbo.Usuarios u ON u.IdUsuario = p.IdUsuario
        INNER JOIN dbo.Libros l ON l.IdLibro = p.IdLibro;
GO

/* =====================================================================
   5. DATOS DE PRUEBA
   (Los ISBN y los datos de los usuarios son de ejemplo, no corresponden a personas reales.)
   ===================================================================== */

SET NOCOUNT ON;

INSERT INTO dbo.Autores (Nombre, Apellido, Nacionalidad) VALUES
    (N'Gabriel',  N'García Márquez', N'Colombiana'),   -- 1
    (N'Jorge',    N'Isaacs',         N'Colombiana'),   -- 2
    (N'Isabel',   N'Allende',        N'Chilena'),      -- 3
    (N'Mario',    N'Vargas Llosa',   N'Peruana'),      -- 4
    (N'Julio',    N'Cortázar',       N'Argentina'),    -- 5
    (N'Laura',    N'Restrepo',       N'Colombiana'),   -- 6
    (N'Miguel',   N'de Cervantes',   N'Española'),     -- 7
    (N'Jorge Luis', N'Borges',       N'Argentina'),    -- 8
    (N'Robert C.', N'Martin',        N'Estadounidense'), -- 9
    (N'Andrew',   N'Hunt',           N'Estadounidense'); -- 10

INSERT INTO dbo.Editoriales (Nombre, Pais, Telefono) VALUES
    (N'Alfaguara',            N'España',         N'6012345601'),  -- 1
    (N'Penguin Random House', N'Estados Unidos', N'6012345602'),  -- 2
    (N'Editorial Norma',      N'Colombia',       N'6012345603'),  -- 3
    (N'Editorial Planeta',    N'España',         N'6012345604'),  -- 4
    (N'Prentice Hall',        N'Estados Unidos', N'6012345605'),  -- 5
    (N'Anaya Multimedia',     N'España',         N'6012345606');  -- 6

INSERT INTO dbo.Usuarios (Nombre, Apellido, Documento, Telefono, Correo) VALUES
    (N'Laura',        N'Gómez',    N'1001234501', N'3001234501', N'laura.gomez@ejemplo.edu.co'),       -- 1
    (N'Andrés',       N'Martínez', N'1001234502', N'3001234502', N'andres.martinez@ejemplo.edu.co'),    -- 2
    (N'Valentina',    N'Rojas',    N'1001234503', N'3001234503', N'valentina.rojas@ejemplo.edu.co'),    -- 3
    (N'Santiago',     N'Herrera',  N'1001234504', N'3001234504', N'santiago.herrera@ejemplo.edu.co'),   -- 4
    (N'Daniela',      N'Ortiz',    N'1001234505', N'3001234505', N'daniela.ortiz@ejemplo.edu.co'),      -- 5
    (N'Sebastián',    N'Vélez',    N'1001234506', N'3001234506', N'sebastian.velez@ejemplo.edu.co'),    -- 6
    (N'Mariana',      N'Cardona',  N'1001234507', N'3001234507', N'mariana.cardona@ejemplo.edu.co'),   -- 7
    (N'Carlos',       N'Mejía',    N'1001234508', N'3001234508', N'carlos.mejia@ejemplo.edu.co'),      -- 8
    (N'Paula',        N'Zapata',   N'1001234509', N'3001234509', NULL),                                 -- 9
    (N'Felipe',       N'Arango',   N'1001234510', NULL,          N'felipe.arango@ejemplo.edu.co');      -- 10

-- Disponibles se deja igual a Existencias y se recalcula más abajo con los préstamos activos.
INSERT INTO dbo.Libros (Isbn, Titulo, IdAutor, IdEditorial, AnioPublicacion, Existencias, Disponibles) VALUES
    (N'978-84-204-8165-2', N'Cien años de soledad',                    1, 1, 1967, 5, 5),   -- 1
    (N'978-84-973-9701-1', N'El amor en los tiempos del cólera',        1, 2, 1985, 3, 3),   -- 2
    (N'978-84-973-9702-8', N'Crónica de una muerte anunciada',          1, 2, 1981, 3, 3),   -- 3
    (N'978-95-804-5001-3', N'María',                                    2, 3, 1867, 2, 2),   -- 4
    (N'978-84-973-9703-5', N'La casa de los espíritus',                 3, 2, 1982, 4, 4),   -- 5
    (N'978-84-204-8166-9', N'La ciudad y los perros',                   4, 1, 1963, 3, 3),   -- 6
    (N'978-84-204-8167-6', N'Rayuela',                                  5, 1, 1963, 2, 2),   -- 7
    (N'978-84-204-8168-3', N'Delirio',                                  6, 1, 2004, 3, 3),   -- 8
    (N'978-84-084-1234-5', N'Don Quijote de la Mancha',                 7, 4, 1605, 4, 4),   -- 9
    (N'978-84-973-9704-2', N'El Aleph',                                 8, 2, 1949, 3, 3),   -- 10
    (N'978-84-084-1235-2', N'Ficciones',                                8, 4, 1944, 2, 2),   -- 11
    (N'978-01-323-5088-4', N'Clean Code',                               9, 5, 2008, 3, 3),   -- 12
    (N'978-84-415-1234-9', N'El programador pragmático',               10, 6, 2000, 2, 2),   -- 13
    (N'978-84-415-3456-3', N'Arquitectura limpia',                      9, 6, 2018, 2, 2);   -- 14

-- Préstamos activos (Estado = Prestado, sin fecha de devolución)
INSERT INTO dbo.Prestamos (IdUsuario, IdLibro, FechaPrestamo, FechaDevolucion, Estado) VALUES
    (1, 1,  '2026-10-05', NULL, N'Prestado'),
    (2, 12, '2026-10-12', NULL, N'Prestado'),
    (3, 5,  '2026-10-20', NULL, N'Prestado'),
    (4, 1,  '2026-10-25', NULL, N'Prestado'),
    (5, 9,  '2026-10-28', NULL, N'Prestado'),
    (6, 13, '2026-10-30', NULL, N'Prestado');

-- Préstamos ya devueltos (historial)
INSERT INTO dbo.Prestamos (IdUsuario, IdLibro, FechaPrestamo, FechaDevolucion, Estado) VALUES
    (1, 7,  '2026-09-01', '2026-09-10', N'Devuelto'),
    (2, 2,  '2026-09-03', '2026-09-15', N'Devuelto'),
    (3, 12, '2026-09-08', '2026-09-20', N'Devuelto'),
    (7, 10, '2026-09-14', '2026-09-28', N'Devuelto'),
    (8, 6,  '2026-09-18', '2026-10-02', N'Devuelto'),
    (9, 3,  '2026-09-22', '2026-10-06', N'Devuelto');

-- Coherencia del inventario: disponibles = existencias - préstamos activos
UPDATE l
SET    l.Disponibles = l.Existencias - (SELECT COUNT(*)
                                        FROM   dbo.Prestamos p
                                        WHERE  p.IdLibro = l.IdLibro
                                          AND  p.Estado = N'Prestado')
FROM   dbo.Libros l;
GO

SET NOCOUNT OFF;

/* =====================================================================
   6. CONSULTAS DE VERIFICACIÓN (opcional: ejecútelas para comprobar la carga)
   ===================================================================== */

SELECT 'Autores'      AS Tabla, COUNT(*) AS Registros FROM dbo.Autores
UNION ALL SELECT 'Editoriales', COUNT(*) FROM dbo.Editoriales
UNION ALL SELECT 'Usuarios',    COUNT(*) FROM dbo.Usuarios
UNION ALL SELECT 'Libros',      COUNT(*) FROM dbo.Libros
UNION ALL SELECT 'Prestamos',   COUNT(*) FROM dbo.Prestamos;
GO

-- Libros con su autor, editorial y disponibilidad
SELECT * FROM dbo.vw_Libros ORDER BY Titulo;
GO

-- Préstamos activos
SELECT * FROM dbo.vw_Prestamos WHERE Estado = N'Prestado' ORDER BY FechaPrestamo;
GO
