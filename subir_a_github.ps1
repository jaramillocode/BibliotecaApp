# Crea el repositorio Git local con commits descriptivos por etapa y lo sube a GitHub.
# Uso (PowerShell, dentro de la carpeta del proyecto):
#   1. Cree un repositorio VACÍO en github.com (sin README) y copie su URL.
#   2. .\subir_a_github.ps1 -Url https://github.com/SU_USUARIO/BibliotecaApp.git
param([Parameter(Mandatory = $true)][string]$Url)

$ErrorActionPreference = "Stop"
git init -b main | Out-Null

function Confirmar($mensaje, $rutas) {
    foreach ($r in $rutas) { if (Test-Path $r) { git add $r } }
    git diff --cached --quiet
    if ($LASTEXITCODE -ne 0) { git commit -m $mensaje | Out-Null; Write-Host "OK: $mensaje" }
}

Confirmar "Crear estructura inicial del proyecto y .gitignore" @(".gitignore", "BibliotecaApp.sln")
Confirmar "Agregar script SQL con tablas, restricciones, vistas y datos de prueba" @("database")
Confirmar "Crear proyecto de entidades con herencia EntidadBase y Persona" @("src/Biblioteca.Entidades")
Confirmar "Agregar capa de acceso a datos con consultas parametrizadas y transacciones" @("src/Biblioteca.Datos")
Confirmar "Agregar capa de negocio con validaciones y manejo de excepciones" @("src/Biblioteca.Negocio")
Confirmar "Crear formulario principal, estilos y plantilla base de los modulos" @("src/Biblioteca.Presentacion/Program.cs", "src/Biblioteca.Presentacion/Ui.cs", "src/Biblioteca.Presentacion/FrmPrincipal.cs", "src/Biblioteca.Presentacion/App.config", "src/Biblioteca.Presentacion/Biblioteca.Presentacion.csproj", "src/Biblioteca.Presentacion/Formularios/FormEmbebido.cs", "src/Biblioteca.Presentacion/Formularios/FormModulo.cs", "src/Biblioteca.Presentacion/Formularios/FormInicio.cs")
Confirmar "Implementar CRUD de autores, editoriales y usuarios" @("src/Biblioteca.Presentacion/Formularios/FormAutores.cs", "src/Biblioteca.Presentacion/Formularios/FormEditoriales.cs", "src/Biblioteca.Presentacion/Formularios/FormUsuarios.cs")
Confirmar "Implementar gestion de libros con busqueda por ISBN, titulo y autor" @("src/Biblioteca.Presentacion/Formularios/FormLibros.cs")
Confirmar "Implementar prestamos y devoluciones con control de disponibilidad" @("src/Biblioteca.Presentacion/Formularios/FormPrestamos.cs")
Confirmar "Implementar modulo de reportes con exportacion a CSV" @("src/Biblioteca.Presentacion/Formularios/FormReportes.cs")
Confirmar "Agregar diagramas de casos de uso, clases, entidad-relacion y arquitectura" @("docs/diagramas")
Confirmar "Agregar documento tecnico del proyecto" @("docs/Documento_Tecnico.md")
Confirmar "Agregar capturas de pantalla del sistema" @("capturas")
Confirmar "Agregar README con instrucciones de ejecucion" @("README.md")
Confirmar "Agregar guia de entrega" @("GUIA_ENTREGA.md", "subir_a_github.ps1")
git add -A
git diff --cached --quiet
if ($LASTEXITCODE -ne 0) { git commit -m "Ajustes finales del proyecto" | Out-Null }

git remote add origin $Url
git push -u origin main
Write-Host "Listo. Revise el repositorio en GitHub."
