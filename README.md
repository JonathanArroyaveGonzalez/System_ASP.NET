# Sistema de Votaciones para Asambleas de Propiedad Horizontal 

## Tabla de Contenido
1. [Descripción del Proyecto](#descripción-del-proyecto)
2. [Diagrama Entidad-Relación](#diagrama-entidad-relación)
3. [Diagrama de Casos de Uso](#diagrama-de-casos-de-uso)
4. [Especificación de Requisitos](#especificación-de-requisitos)
   - [Requisitos Funcionales](#requisitos-funcionales)
   - [Requisitos No Funcionales](#requisitos-no-funcionales)
5. [Especificación de Casos de Uso](#especificación-de-casos-de-uso)
6. [Arquitectura del Sistema](#arquitectura-del-sistema)
7. [Metodología de Desarrollo](#metodología-de-desarrollo)

## Descripción del Proyecto

El Sistema de Votaciones para Asambleas de Propiedad Horizontal es una aplicación web desarrollada en .NET que facilita los procesos democráticos en comunidades de copropietarios. El sistema permite la creación y gestión de asambleas, votaciones ponderadas según el coeficiente de propiedad, y generación de reportes detallados de resultados.

## Diagrama Entidad-Relación

![Diagrama Entidad-Relación](https://raw.githubusercontent.com/JonathanArroyaveGonzalez/System_ASP.NET/refs/heads/main/wwwroot/assets/ER_diagram.png)

## Diagrama de Casos de Uso

![Diagrama de Casos de Uso](https://github.com/JonathanArroyaveGonzalez/System_ASP.NET/blob/main/wwwroot/assets/useCase_Diagram.png?raw=true)

## Especificación de Requisitos

### Requisitos Funcionales

| ID | Categoría | Requisito | Descripción |
|----|-----------|-----------|-------------|
| RF01 | Gestión de Usuarios | Registro de propietarios | Permitir el registro de propietarios con información básica (nombre, email, número de unidad, coeficiente de propiedad) |
| RF02 | Gestión de Usuarios | Administración de restricciones | Gestionar restricciones de votación para usuarios específicos |
| RF03 | Gestión de Asambleas | Creación de asambleas | Permitir la creación, actualización y eliminación de asambleas |
| RF04 | Gestión de Asambleas | Programación de asambleas | Establecer fecha y hora para la realización de asambleas |
| RF05 | Gestión de Asambleas | Definición de agenda | Crear y gestionar la agenda y puntos a votar en cada asamblea |
| RF06 | Sistema de Votaciones | Creación de votaciones | Permitir la creación de votaciones con múltiples opciones de respuesta |
| RF07 | Sistema de Votaciones | Configuración de duración | Establecer la duración de las votaciones |
| RF08 | Sistema de Votaciones | Registro de votos ponderados | Permitir el registro de votos con ponderación según coeficiente de propiedad |
| RF09 | Sistema de Votaciones | Cierre automático | Cerrar automáticamente las votaciones según el tiempo establecido |
| RF10 | Notificaciones | Envío de correos | Enviar automáticamente correos electrónicos para informar sobre nuevas votaciones |
| RF11 | Notificaciones | Alertas de resultados | Notificar sobre los resultados de las votaciones |
| RF12 | Reportes y Visualización | Generación de reportes | Crear reportes detallados de las votaciones realizadas |
| RF13 | Reportes y Visualización | Visualización gráfica | Presentar resultados mediante gráficos (barras, circulares) |
| RF14 | Reportes y Visualización | Exportación de reportes | Permitir la exportación de reportes en formatos PDF y Excel |
| RF15 | Reportes y Visualización | Histórico de votaciones | Mantener un registro histórico de votaciones por asamblea y por usuario |

### Requisitos No Funcionales

| ID | Categoría | Requisito | Descripción |
|----|-----------|-----------|-------------|
| RNF01 | Escalabilidad | Arquitectura escalable | Diseño de arquitectura que permita el crecimiento futuro del sistema |
| RNF02 | Escalabilidad | Diseño modular | Implementación modular para facilitar extensiones del sistema |
| RNF03 | Usabilidad | Interfaz intuitiva | Desarrollo de una interfaz de usuario intuitiva y responsiva |
| RNF04 | Calidad | Código documentado | Documentación del código siguiendo estándares de .NET |
| RNF05 | Calidad | Patrones de diseño | Implementación de patrones de diseño adecuados |
| RNF06 | Calidad | Pruebas unitarias | Desarrollo de pruebas unitarias con cobertura mínima del 60% |

## Especificación de Casos de Uso

### UC1: Iniciar sesión

| Aspecto | Descripción |
|---------|-------------|
| **Actores** | Administrador, Propietario |
| **Descripción** | Permite a los usuarios acceder al sistema mediante credenciales |
| **Precondiciones** | El usuario debe estar registrado en el sistema |
| **Flujo principal** | 1. El usuario accede a la página de inicio de sesión<br>2. Ingresa su nombre de usuario y contraseña<br>3. El sistema valida las credenciales<br>4. El sistema permite el acceso |
| **Flujos alternativos** | - Si las credenciales son incorrectas, se muestra un mensaje de error<br>- Si el usuario olvidó su contraseña, puede solicitar su recuperación |
| **Postcondiciones** | El usuario accede al sistema con sus respectivos permisos |

### UC2: Gestionar usuarios

| Aspecto | Descripción |
|---------|-------------|
| **Actores** | Administrador |
| **Descripción** | Permite registrar, modificar, desactivar y asignar roles a los usuarios del sistema |
| **Precondiciones** | El administrador debe haber iniciado sesión |
| **Flujo principal** | 1. El administrador accede al módulo de gestión de usuarios<br>2. Puede crear nuevos usuarios<br>3. Puede editar información de usuarios existentes<br>4. Puede desactivar usuarios<br>5. Puede asignar roles y permisos |
| **Flujos alternativos** | - Búsqueda de usuarios por diferentes criterios<br>- Importación masiva de usuarios |
| **Postcondiciones** | Los datos de usuarios son actualizados en el sistema |

### UC3: Crear asamblea

| Aspecto | Descripción |
|---------|-------------|
| **Actores** | Administrador |
| **Descripción** | Permite la creación de una nueva asamblea en el sistema |
| **Precondiciones** | El administrador debe haber iniciado sesión |
| **Flujo principal** | 1. El administrador accede al módulo de asambleas<br>2. Crea una nueva asamblea con nombre y descripción<br>3. Establece fecha y hora<br>4. Define los puntos de agenda<br>5. Guarda la información |
| **Flujos alternativos** | - Edición de asambleas creadas<br>- Cancelación de asambleas |
| **Postcondiciones** | La asamblea queda registrada en el sistema |

### UC4: Programar votación

| Aspecto | Descripción |
|---------|-------------|
| **Actores** | Administrador |
| **Descripción** | Permite configurar una votación dentro de una asamblea |
| **Precondiciones** | Debe existir una asamblea creada |
| **Flujo principal** | 1. El administrador selecciona una asamblea<br>2. Crea una nueva votación<br>3. Define las opciones de respuesta<br>4. Establece la duración y el quórum requerido<br>5. Guarda la configuración |
| **Flujos alternativos** | - Modificación de parámetros de votación<br>- Cancelación de votación |
| **Postcondiciones** | La votación queda programada y se envían notificaciones |

### UC5: Establecer restricciones

| Aspecto | Descripción |
|---------|-------------|
| **Actores** | Administrador |
| **Descripción** | Permite configurar restricciones para la participación en votaciones |
| **Precondiciones** | Deben existir usuarios y votaciones en el sistema |
| **Flujo principal** | 1. El administrador accede a la configuración de restricciones<br>2. Selecciona la votación<br>3. Define qué usuarios pueden participar<br>4. Establece condiciones de participación<br>5. Guarda la configuración |
| **Flujos alternativos** | - Restricciones por unidad inmobiliaria<br>- Restricciones por estado de morosidad |
| **Postcondiciones** | Las restricciones quedan aplicadas en el sistema |

### UC6: Participar en votación

| Aspecto | Descripción |
|---------|-------------|
| **Actores** | Propietario |
| **Descripción** | Permite a un propietario emitir su voto en una votación activa |
| **Precondiciones** | La votación debe estar activa y el propietario debe tener permiso para votar |
| **Flujo principal** | 1. El propietario accede a la sección de votaciones activas<br>2. Selecciona la votación<br>3. Visualiza las opciones<br>4. Emite su voto<br>5. Confirma la elección |
| **Flujos alternativos** | - Cambio de voto dentro del tiempo permitido<br>- Abstención de voto |
| **Postcondiciones** | El voto queda registrado en el sistema con el coeficiente correspondiente |

### UC7: Ver resultados

| Aspecto | Descripción |
|---------|-------------|
| **Actores** | Administrador, Propietario |
| **Descripción** | Permite visualizar los resultados de una votación finalizada |
| **Precondiciones** | La votación debe haber concluido |
| **Flujo principal** | 1. El usuario accede a la sección de resultados<br>2. Selecciona la votación<br>3. Visualiza los resultados detallados<br>4. Puede ver diferentes representaciones gráficas |
| **Flujos alternativos** | - Filtrado de resultados por diferentes criterios |
| **Postcondiciones** | El usuario obtiene la información solicitada |

### UC8: Generar reportes

| Aspecto | Descripción |
|---------|-------------|
| **Actores** | Administrador |
| **Descripción** | Permite crear reportes detallados sobre las votaciones |
| **Precondiciones** | Deben existir votaciones finalizadas en el sistema |
| **Flujo principal** | 1. El administrador accede al módulo de reportes<br>2. Selecciona el tipo de reporte<br>3. Configura los parámetros<br>4. Genera el reporte<br>5. Visualiza los resultados con gráficos |
| **Flujos alternativos** | - Exportación en diferentes formatos<br>- Programación de reportes periódicos |
| **Postcondiciones** | El reporte queda generado y disponible |

### UC9: Visualizar gráficos

| Aspecto | Descripción |
|---------|-------------|
| **Actores** | Administrador, Propietario |
| **Descripción** | Permite ver representaciones gráficas de resultados |
| **Precondiciones** | Deben existir datos de votaciones |
| **Flujo principal** | 1. El usuario selecciona una votación<br>2. Elige el tipo de gráfico (barras, circular, etc.)<br>3. El sistema genera y muestra el gráfico<br>4. El usuario puede interactuar con la visualización |
| **Flujos alternativos** | - Configuración de parámetros de visualización |
| **Postcondiciones** | El usuario visualiza los datos de forma gráfica |

### UC10: Enviar notificaciones

| Aspecto | Descripción |
|---------|-------------|
| **Actores** | Sistema |
| **Descripción** | Envío automático de notificaciones por correo para eventos del sistema |
| **Precondiciones** | Debe ocurrir un evento que requiera notificación |
| **Flujo principal** | 1. Se produce un evento (nueva votación, cierre, etc.)<br>2. El sistema identifica los destinatarios<br>3. Genera el contenido del mensaje<br>4. Envía las notificaciones por correo |
| **Flujos alternativos** | - Reenvío en caso de fallo<br>- Notificaciones programadas |
| **Postcondiciones** | Los usuarios reciben las notificaciones correspondientes |

### UC11: Exportar resultados

| Aspecto | Descripción |
|---------|-------------|
| **Actores** | Administrador |
| **Descripción** | Permite exportar los resultados de votaciones en diferentes formatos |
| **Precondiciones** | Deben existir resultados de votaciones |
| **Flujo principal** | 1. El administrador accede a los resultados<br>2. Selecciona la opción de exportar<br>3. Elige el formato (PDF, Excel)<br>4. Confirma la exportación<br>5. Descarga el archivo |
| **Flujos alternativos** | - Configuración de parámetros de exportación |
| **Postcondiciones** | El archivo con los resultados queda disponible para descarga |

### UC12: Gestionar perfil

| Aspecto | Descripción |
|---------|-------------|
| **Actores** | Propietario |
| **Descripción** | Permite a los propietarios gestionar su información personal |
| **Precondiciones** | El propietario debe haber iniciado sesión |
| **Flujo principal** | 1. El propietario accede a su perfil<br>2. Visualiza su información personal<br>3. Edita los campos permitidos<br>4. Guarda los cambios |
| **Flujos alternativos** | - Cambio de contraseña<br>- Configuración de preferencias de notificación |
| **Postcondiciones** | La información del perfil queda actualizada |

### UC13: Cerrar votación

| Aspecto | Descripción |
|---------|-------------|
| **Actores** | Sistema |
| **Descripción** | Cierre automático de votaciones según tiempo establecido |
| **Precondiciones** | La votación debe tener configurado un tiempo de cierre |
| **Flujo principal** | 1. El sistema monitorea el tiempo de las votaciones<br>2. Al llegar al tiempo límite, cierra la votación<br>3. Procesa los resultados<br>4. Envía notificaciones con los resultados |
| **Flujos alternativos** | - Cierre manual por un administrador |
| **Postcondiciones** | La votación queda cerrada y los resultados disponibles |

### UC14: Consultar histórico

| Aspecto | Descripción |
|---------|-------------|
| **Actores** | Administrador, Propietario |
| **Descripción** | Permite consultar el histórico de votaciones |
| **Precondiciones** | Deben existir votaciones previas en el sistema |
| **Flujo principal** | 1. El usuario accede al histórico de votaciones<br>2. Aplica filtros (fecha, tipo, asamblea)<br>3. Visualiza el listado de votaciones<br>4. Puede acceder al detalle de cada una |
| **Flujos alternativos** | - Búsqueda por diferentes criterios |
| **Postcondiciones** | El usuario obtiene la información histórica solicitada |

## Arquitectura del Sistema

El sistema sigue una arquitectura MVC (Modelo-Vista-Controlador) con Entity Framework como ORM, estructurado en los siguientes proyectos:

```
SistemaVotaciones/
├── SistemaVotaciones.Web/            # Proyecto principal MVC
│   ├── Controllers/                  # Controladores MVC
│   ├── Views/                        # Vistas por entidad
│   ├── Models/                       # ViewModels
│   ├── wwwroot/                      # Assets estáticos (CSS, JS)
│   └── Areas/                        # Módulos separados (Admin, Reports)
├── SistemaVotaciones.Core/           # Proyecto de lógica de negocio
│   ├── Entities/                     # Entidades de dominio
│   ├── Interfaces/                   # Interfaces de servicios
│   └── Services/                     # Implementación de servicios
├── SistemaVotaciones.Infrastructure/ # Proyecto de infraestructura
│   ├── Data/                         # Contexto y configuración EF
│   ├── Repositories/                 # Implementación de repositorios
│   └── Services/                     # Servicios externos (Email, etc.)
└── SistemaVotaciones.Tests/          # Proyecto de pruebas
```

La aplicación se ha desarrollado en .NET utilizando SQL Server o PostgreSQL como base de datos, con la posibilidad de utilizar ADO.NET o un ORM como Entity Framework para el acceso a datos.

## Metodología de Desarrollo

El proyecto utiliza metodología ágil Scrum con las siguientes características:

### Roles Scrum

| Rol | Responsable | Descripción |
|-----|-------------|-------------|
| Scrum Master / QA / Facilitador | Jonathan Arroyave | Responsable de asegurar que el equipo siga los principios de Scrum y eliminar impedimentos |
| Product Owner (PO) | Sebastián Gálvez Yepes | Responsable de maximizar el valor del producto y representar los intereses de los stakeholders |
| Equipo de desarrollo | Ambos | Responsables de entregar incrementos de producto al final de cada sprint |

### Implementación de Scrum

#### Sprints
- Duración: 1 semana
- Inicia los lunes y termina los domingos
- Entrega de una nueva funcionalidad, prototipo o mejora visible en cada sprint

#### Reuniones Scrum

| Ceremonia | Frecuencia | Duración | Detalles |
|-----------|------------|----------|----------|
| Sprint Planning | Inicio de cada sprint | 30-60 min | Revisar el backlog, estimar, planificar tareas |
| Daily Scrum | Sábados y domingos | 10-15 min | Repaso breve: ¿Qué hice? ¿Qué haré? ¿Hay bloqueos? |
| Sprint Review | Fin de sprint | 30 min | Demo de lo realizado, discusión de lo siguiente |
| Sprint Retrospective | Fin de sprint | 30 min | ¿Qué funcionó? ¿Qué mejorar? ¿Qué cambiar? |

### Herramientas

- Control de versiones: Git + GitHub
- CI/CD: GitHub Actions
- Documentación: GitHub

### Flujo de Trabajo

- Branching: Uso de Git Flow o ramas por funcionalidad (feature/login, bugfix/votacion-error, etc.)
- Revisión de código cruzada entre ambos desarrolladores
- Deploy por sprint en entorno de producción CI/CD
