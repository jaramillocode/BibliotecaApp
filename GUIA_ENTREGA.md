# Guía de entrega – paso a paso

**No hay que hacer video.** El PDF del ACA pide: documento técnico, código fuente, script SQL y repositorio en GitHub con README, **capturas de pantalla** y commits. Nada de video.

## Lo que ya está hecho
Código completo (4 capas), script SQL, README, documento técnico con diagramas, script para subir a GitHub.

## Lo que le falta a usted (≈ 45 min)

### 1. Instalar (si no lo tiene)
- Visual Studio 2022 con "Desarrollo de escritorio de .NET"
- SQL Server Express + SQL Server Management Studio (SSMS)
- Git para Windows y una cuenta en github.com

### 2. Crear la base de datos
Abra `database/biblioteca_script.sql` en SSMS → F5. Debe terminar sin errores y mostrar los conteos (10 autores, 6 editoriales, 10 usuarios, 14 libros, 12 préstamos).

### 3. Abrir y ejecutar el proyecto
1. Doble clic en `BibliotecaApp.sln`.
2. Revise `src/Biblioteca.Presentacion/App.config`: si su servidor no es `localhost\SQLEXPRESS`, cámbielo.
3. Proyecto de inicio: **Biblioteca.Presentacion** → F5.
4. Si sale error de compilación, copie el mensaje completo y envíemelo.

### 4. Tomar las capturas (Windows + Mayús + S)
Siga la lista de `capturas/LEEME.md` (17 capturas, con el nombre exacto de cada archivo). Resumen de lo que hay que provocar:
- Un error de ISBN repetido (use `978-84-204-8165-2`), uno de correo inválido (`juan@`), eliminar un autor con libros, y prestar un libro hasta que no queden ejemplares.
- Un préstamo nuevo y su devolución.
- SSMS con las tablas desplegadas, y GitHub (repositorio y commits; estas dos se toman en el paso 6).

### 5. Completar sus datos
Reemplace en `docs/Documento_Tecnico.md` y `README.md` todo lo que está entre corchetes: `[NOMBRE COMPLETO]`, `[CÓDIGO]`, `[PROGRAMA ACADÉMICO]`, `[NOMBRE DEL DOCENTE]`, `[URL DEL REPOSITORIO EN GITHUB]`. (Use Buscar y reemplazar.) Si su universidad no es CUN, cambie también el nombre en la portada.

### 6. Subir a GitHub
1. En github.com cree un repositorio **vacío** llamado `BibliotecaApp` (sin README).
2. En PowerShell, dentro de la carpeta del proyecto: `.\subir_a_github.ps1 -Url https://github.com/SU_USUARIO/BibliotecaApp.git`
3. Tome las capturas 16 y 17, guárdelas en `capturas/` y haga: `git add . ; git commit -m "Agregar capturas de GitHub" ; git push`.

(Antes de ejecutar el script, configure su identidad una sola vez: `git config --global user.name "Su Nombre"` y `git config --global user.email "su@correo"`.)

### 7. Pruebas del documento
En la sección 13 hay 27 pruebas. Ejecútelas en la aplicación y deje ✔ solo en las que de verdad salgan como dice la columna "Resultado esperado". Si alguna falla, avíseme.

### 8. Exportar el documento (con numeración de páginas)
- Opción fácil: abra `Documento_Tecnico.md` en Visual Studio Code, instale la extensión **Markdown PDF** y use "Export (pdf)". En sus ajustes active `markdown-pdf.displayHeaderFooter` para ver el número de página.
- Alternativa: con Pandoc, `pandoc docs/Documento_Tecnico.md -o Documento_Tecnico.docx --toc`, abrirlo en Word e insertar número de página.

### 9. Entregar
Segun pida su docente: el enlace del repositorio, el `.md` (o su PDF/Word), la carpeta del proyecto (zip) y el script SQL.
