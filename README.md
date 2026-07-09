# SeminarioFinal

Sistema web desarrollado como prototipo tecnológico para la gestión de no conformidades y acciones correctivas dentro del área de calidad de la empresa Fumiscor.

El sistema permite registrar no conformidades, gestionar fichas técnicas, asignar equipos de trabajo, definir responsables, realizar el seguimiento de acciones correctivas y consultar indicadores relacionados con el estado de los procesos de calidad.

## Video demostrativo

El video demostrativo del uso del sistema no se incluye como archivo dentro del repositorio debido a su tamaño. Para evitar que el repositorio quede excesivamente pesado, se deja disponible mediante el siguiente enlace externo:

https://drive.google.com/drive/folders/1zOQJGakrCyV-mAd-3uacGEqF4dcBzxjA?usp=drive_link

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
* SQL Server Management Studio, para restaurar o importar la base de datos.
* Git, en caso de clonar el repositorio desde GitHub.

El sistema utiliza una base de datos SQL Server, la cual se incluye dentro del material del repositorio para facilitar la ejecución del prototipo con datos de prueba, usuarios, roles, no conformidades, fichas técnicas, equipos y demás información necesaria para verificar el funcionamiento general del sistema.

## Configuración inicial

1. Clonar o descargar el repositorio.
2. Abrir la solución del proyecto desde Visual Studio.
3. Abrir SQL Server Management Studio.
4. Restaurar o ejecutar el archivo de base de datos incluido en la carpeta `database`.
5. Verificar el nombre de la base de datos creada en SQL Server.
6. Configurar la cadena de conexión en el archivo `appsettings.json`, indicando el servidor local y el nombre de la base de datos restaurada.
7. Compilar la solución desde Visual Studio.
8. Ejecutar el proyecto.

Una vez realizada esta configuración, el sistema podrá ejecutarse utilizando la información precargada en la base de datos incluida en el repositorio.

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

## Base de datos

El repositorio incluye una base de datos de prueba con información precargada para facilitar la revisión del prototipo. Esta base contiene usuarios, roles, equipos de trabajo, no conformidades, fichas técnicas, tareas y estados necesarios para verificar las principales funcionalidades del sistema.

La base de datos debe restaurarse o importarse en SQL Server antes de ejecutar la aplicación. Luego, se debe actualizar la cadena de conexión del archivo `appsettings.json` para que apunte a la base restaurada.


## Roles del sistema

El sistema contempla usuarios con rol general de supervisor o empleado. Además, dentro de los equipos de trabajo se diferencian los roles de piloto y auditor.

El supervisor puede registrar no conformidades, asignar equipos, administrar fichas técnicas, controlar avances y cerrar procesos.

El empleado participa en las fichas técnicas asignadas según su rol dentro del equipo. El piloto valida avances y los auditores completan los pasos correspondientes.

## Demo

El repositorio forma parte de la entrega final del prototipado tecnológico. Se incluye el código fuente del sistema y el material necesario para su revisión.

Video demostrativo: //ENLACE

## Autor

Matías Ignacio Benza
Legajo: 02913
Carrera: Ingeniería en Software
Universidad Siglo 21
