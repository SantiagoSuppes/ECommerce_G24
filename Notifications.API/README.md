# Notifications API

API REST para la gestión de notificaciones en el e-commerce.

## Descripción

La Notifications API permite registrar, enviar y recuperar notificaciones para los usuarios del sistema. Implementa un sistema completo de manejo de errores con códigos específicos, logging con Serilog y Correlation IDs para trazabilidad.

## Endpoints

### POST `/api/notifications/send`

Registrar y simular envío de notificación.

**Request Body:**
```json
{
  "usuarioId": "a1b2c3d4-0000-0000-0000-111122223333",
  "mensaje": "Su orden #f1e2d3c4 fue confirmada.",
  "tipo": "Email"
}
```

**Success Response (201 Created):**
```json
{
  "id": "11112222-3333-4444-5555-666677778888",
  "usuarioId": "a1b2c3d4-0000-0000-0000-111122223333",
  "mensaje": "Su orden #f1e2d3c4 fue confirmada.",
  "tipo": "Email",
  "estado": "Enviada",
  "fechaEnvio": "2024-03-10T12:01:00Z"
}
```

**Error Responses:**
- `404 NTF-001`: Usuario no encontrado
- `400 NTF-002`: Los datos de la notificación son inválidos
- `500 NTF-004`: Error interno al procesar la notificación

---

### GET `/api/notifications/{userId}`

Listar notificaciones de un usuario específico.

**Success Response (200 OK):**
```json
[
  {
    "id": "11112222-3333-4444-5555-666677778888",
    "usuarioId": "a1b2c3d4-0000-0000-0000-111122223333",
    "mensaje": "Su orden #f1e2d3c4 fue confirmada.",
    "tipo": "Email",
    "estado": "Enviada",
    "fechaEnvio": "2024-03-10T12:01:00Z"
  }
]
```

**Error Responses:**
- `404 NTF-001`: Usuario no encontrado
- `404 NTF-003`: No se encontraron notificaciones para el usuario
- `500 NTF-004`: Error interno

---

### GET `/health`

Verificar que el servicio está disponible.

**Response:**
```json
{
  "status": "Healthy"
}
```

---

### GET `/health/detailed`

Verificar salud detallada del servicio.

**Response:**
```json
{
  "status": "Healthy",
  "environment": "Development",
  "timestamp": "2024-03-10T12:01:00Z"
}
```

## Catálogo de Errores

| errorCode | HTTP | Mensaje | Cuándo se devuelve |
|-----------|------|---------|-------------------|
| NTF-001 | 404 | Usuario no encontrado. | POST cuando el UsuarioId no existe en Users API. GET cuando el userId no existe. |
| NTF-002 | 400 | Los datos de la notificación son inválidos. | POST con campos faltantes o tipo no reconocido. |
| NTF-003 | 404 | No se encontraron notificaciones para el usuario. | GET cuando el userId no tiene notificaciones registradas. |
| NTF-004 | 500 | Error interno al procesar la notificación. | Error inesperado en servicio o persistencia. |

## Características Implementadas

### 1. Documentación con Swagger/OpenAPI
- ✅ Swagger UI disponible en `/swagger/ui/`
- ✅ Documentación XML con ejemplos de respuesta
- ✅ Códigos de error documentados

### 2. Manejo de Errores con ExceptionHandler
- ✅ UserNotFoundException (404)
- ✅ ValidationException (400)
- ✅ NotFoundException (404)
- ✅ InternalServerException (500)
- ✅ GlobalExceptionHandler para excepciones no manejadas
- ✅ Respuestas en formato RFC 7231

### 3. Logging con Serilog
- ✅ Registro JSON estructurado
- ✅ Incluye: Timestamp, Nivel, Servicio, Endpoint, Correlation ID, errorCode, errorMessage
- ✅ Niveles de log: Debug, Information, Warning, Error

### 4. Health Checks
- ✅ GET /health/liveness
- ✅ GET /health/readiness  
- ✅ Respuesta JSON con estado

### 5. Correlation ID
- ✅ Header X-Correlation-Id generado automáticamente
- ✅ Incluido en todos los logs y respuestas de error
- ✅ Trazabilidad completa de requests

### 6. Validación de Datos
- ✅ Validación de campos requeridos
- ✅ Validación de tipos de datos
- ✅ Mensajes de error descriptivos

## Configuración

### appsettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Notifications.API": "Information"
    }
  }
}
```

## Estructura del Proyecto

```
Notifications.API/
├── Controllers/
│   └── NotificationsController.cs
├── Dtos/
│   ├── CreateNotificationRequestDto.cs
│   ├── NotificationResponseDto.cs
│   └── ErrorResponseDto.cs
├── Exceptions/
│   ├── UserNotFoundException.cs
│   ├── ValidationException.cs
│   ├── NotFoundException.cs
│   └── InternalServerException.cs
├── ExceptionHandlers/
│   ├── UserNotFoundExceptionHandler.cs
│   ├── ValidationExceptionHandler.cs
│   ├── NotFoundExceptionHandler.cs
│   ├── InternalServerExceptionHandler.cs
│   └── GlobalExceptionHandler.cs
├── Middleware/
│   └── CorrelationIdMiddleware.cs
├── Models/
│   └── Notification.cs
├── Services/
│   ├── INotificationService.cs
│   └── NotificationService.cs
├── appsettings.json
├── appsettings.Development.json
└── Program.cs
```

## Cómo Ejecutar

1. **Desarrollo:**
   ```bash
   dotnet run --project ECommerce_G24.csproj
   ```

2. **Acceder a Swagger:**
   ```
   http://localhost:5000/swagger
   ```

3. **Ejecutar pruebas:**
   - Usar el archivo `Notifications.API.http` en Visual Studio REST Client
   - O importar en Postman

## Próximos Pasos

- Integración real con Users API para verificar existencia de usuarios
- Persistencia en base de datos (Entity Framework Core)
- Sistema de notificaciones en cola (RabbitMQ)
- Integración con servicios de email/SMS
- Autenticación y autorización (JWT)
- Rate limiting
- Paginación en GET `/api/notifications/{userId}`
