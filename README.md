# Sistema de Gestión de Biblioteca

Aplicación de escritorio para administrar una biblioteca: libros, autores, editoriales, usuarios, préstamos, devoluciones y reportes.
Proyecto de **Aca – Programación Avanzada**. Desarrollado con **C# (.NET 8), Windows Forms y SQL Server**.

## Funcionalidades

- CRUD de libros, autores, editoriales y usuarios
- Búsqueda de libros por código/ISBN, título o autor
- Préstamos y devoluciones con control automático de ejemplares disponibles
- 10 reportes consultables y exportables a CSV
- Validación de datos, manejo de excepciones y mensajes claros
- Consultas parametrizadas y transacciones para mantener la integridad

## Arquitectura

| Proyecto | Capa |
|---|---|
| `Biblioteca.Presentacion` | Windows Forms (interfaz) |
| `Biblioteca.Negocio` | Reglas y validaciones |
| `Biblioteca.Datos` | Acceso a SQL Server (consultas parametrizadas) |
| `Biblioteca.Entidades` | Clases del dominio (herencia: `EntidadBase → Persona → Autor/Usuario`) |

```
BibliotecaApp/
├── BibliotecaApp.sln
├── database/biblioteca_script.sql   Script de la base de datos y datos de prueba
├── docs/                            Documento técnico y diagramas
├── capturas/                        Capturas de pantalla del sistema
└── src/                             Código fuente (4 proyectos)
```

## Requisitos

- Windows 10/11
- Visual Studio 2022 (17.8 o superior) con la carga de trabajo **Desarrollo de escritorio de .NET**
- SQL Server Express (o LocalDB) y, opcionalmente, SQL Server Management Studio

## Cómo ejecutarlo

1. **Clonar** el repositorio y abrir `BibliotecaApp.sln` en Visual Studio.
2. **Crear la base de datos:** abrir `database/biblioteca_script.sql` en SQL Server Management Studio y ejecutarlo (F5).
   También por consola: `sqlcmd -S localhost\SQLEXPRESS -E -I -i database\biblioteca_script.sql`
   > El script elimina y vuelve a crear `BibliotecaDB`.
3. **Revisar la cadena de conexión** en `src/Biblioteca.Presentacion/App.config` y cambiar `Server=` según su equipo:
   - `localhost\SQLEXPRESS` (SQL Server Express)
   - `(localdb)\MSSQLLocalDB` (LocalDB de Visual Studio)
   - `localhost` (instancia por defecto)
4. En Visual Studio, establecer **Biblioteca.Presentacion** como proyecto de inicio y pulsar **F5**. Los paquetes NuGet se restauran solos.

Si la conexión falla, la aplicación muestra un mensaje indicando qué revisar.

## Documentación

- [Documento técnico](docs/Documento_Tecnico.md): requerimientos, casos de uso, diagramas, diccionario de datos, arquitectura, pruebas y conclusiones.
- Diagramas en [`docs/diagramas`](docs/diagramas) (imagen y código Mermaid).
- Capturas en [`capturas`](capturas).

## Autor

Juan Andres Jaramillo — 54465 — Corporación Unificada Nacional de Educación Superior (CUN), 2026.
