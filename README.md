# SeminarioFinal

Sistema web desarrollado como prototipo tecnológico para la gestión de no conformidades y acciones correctivas dentro del área de calidad de la empresa Fumiscor.

El sistema permite registrar no conformidades, gestionar fichas técnicas, asignar equipos de trabajo, definir responsables, realizar el seguimiento de acciones correctivas y consultar indicadores relacionados con el estado de los procesos de calidad.

## Tecnologías utilizadas

* ASP.NET Core Razor Pages
* C#
* Entity Framework Core
* SQL Server
* HTML
* CSS
* Bootstrap
* JavaScript
* Visual Studio

## Requisitos para ejecutar el sistema

Para ejecutar el prototipo se requiere contar con los siguientes componentes instalados:

* Visual Studio 2022 o superior.
* .NET SDK compatible con la versión del proyecto.
* SQL Server o SQL Server Express.
* SQL Server Management Studio, para crear y administrar la base de datos.
* Git, en caso de clonar el repositorio desde GitHub.

Además, es necesario contar con una base de datos creada en SQL Server y configurar correctamente la cadena de conexión en el archivo `appsettings.json`.


## Configuración inicial

1. Clonar o descargar el repositorio.
2. Abrir la solución del proyecto desde Visual Studio.
3. Configurar la cadena de conexión a la base de datos en el archivo `appsettings.json`.
4. Crear la base de datos en SQL Server.
5. Ejecutar las migraciones de Entity Framework, si corresponde.
6. Compilar la solución.
7. Ejecutar el proyecto desde Visual Studio.

## Funcionalidades principales

* Inicio de sesión de usuarios.
* Gestión de usuarios según roles.
* Registro de no conformidades.
* Creación y seguimiento de fichas técnicas.
* Administración de equipos de trabajo.
* Asignación de roles de piloto y auditor.
* Carga y seguimiento de acciones correctivas.
* Control de estados del proceso.
* Visualización de indicadores y tablero de seguimiento.

## Roles del sistema

El sistema contempla usuarios con rol general de supervisor o empleado. Además, dentro de los equipos de trabajo se diferencian los roles de piloto y auditor.

El supervisor puede registrar no conformidades, asignar equipos, administrar fichas técnicas, controlar avances y cerrar procesos.

El empleado participa en las fichas técnicas asignadas según su rol dentro del equipo. El piloto valida avances y los auditores completan los pasos correspondientes.

## Demo

El repositorio forma parte de la entrega final del prototipado tecnológico. Se incluye el código fuente del sistema y el material necesario para su revisión.

Video demostrativo: [agregar enlace al video]

## Autor

Matías Ignacio Benza
Legajo: 02913
Carrera: Ingeniería en Software
Universidad Siglo 21
