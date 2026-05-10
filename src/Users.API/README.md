# Users API

## 📁 Estructura del Proyecto

```
ECommerce_G24/
├── Users.API/
│   ├── Controllers/              # Futuro: Controladores
│   ├── Dtos/                     # Data Transfer Objects
│   │   ├── LoginUserRequestDto.cs
│   │   ├── RegisterUserRequestDto.cs
│   │   └── UserResponseDto.cs
│   ├── ExceptionHandlers/        # Manejadores de excepciones HTTP
│   │   ├── DuplicateEmailExceptionHandler.cs
│   │   ├── InternalServerExceptionHandler.cs
│   │   ├── InvalidCredentialsExceptionHandler.cs
│   │   ├── NotFoundExceptionHandler.cs
│   │   ├── UserBlockedExceptionHandler.cs
│   │   ├── UserFraudBlockedExceptionHandler.cs
│   │   └── ValidationExceptionHandler.cs
│   ├── Exceptions/               # Excepciones personalizadas
│   │   ├── BussinessRuleException.cs
│   │   ├── DuplicateEmailException.cs
│   │   ├── InternalServerException.cs
│   │   ├── InvalidCredentialsException.cs
│   │   ├── NotFoundException.cs
│   │   ├── UserBlockedException.cs
│   │   ├── UserFraudBlockedException.cs
│   │   └── ValidationException.cs
│   ├── Models/                   # Entidades del dominio
│   │   └── User.cs
│   ├── Services/                 # Lógica de negocio
│   │   ├── IUserService.cs
│   │   └── UserService.cs
│   ├── Utilities/                # Funciones auxiliares
│   │   └── PasswordHelper.cs
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── Program.cs                # Punto de entrada
│   └── Users.API.http            # Pruebas de endpoints
├── ECommerce_G24.csproj
├── appsettings.json
├── appsettings.Development.json
└── ECommerce_G24.http
```

## 🚀 Endpoints

### POST `/api/users/register`
Registra un nuevo usuario.

**Request:**
```json
{
  "nombre": "María",
  "apellido": "González",
  "email": "maria@email.com",
  "password": "MiPassword123!"
}
```

**Response (201 Created):**
```json
{
  "id": "a1b2c344-0000-0000-0000-111122223333",
  "nombre": "María",
  "apellido": "González",
  "email": "maria@email.com",
  "activo": true,
  "fechaRegistro": "2024-03-01T00:00:00Z",
  "fechaUltimoLogin": null
}
```

### POST `/api/users/login`
Autentica un usuario.

**Request:**
```json
{
  "email": "maria@email.com",
  "password": "MiPassword123!"
}
```

**Response (200 OK):**
```json
{
  "id": "a1b2c344-0000-0000-0000-111122223333",
  "nombre": "María",
  "apellido": "González",
  "email": "maria@email.com",
  "activo": true,
  "fechaRegistro": "2024-03-01T00:00:00Z",
  "fechaUltimoLogin": "2024-03-01T10:30:00Z"
}
```

## 🔐 Códigos de Error

| Código | HTTP | Mensaje | Escenario |
|--------|------|---------|-----------|
| USR-001 | 409 | El email ya está registrado | Email duplicado en registro |
| USR-002 | 400 | Los datos del usuario son inválidos | Campos faltantes o inválidos |
| USR-003 | 401 | Las credenciales no son válidas | Email/contraseña incorrectos |
| USR-004 | 403 | Usuario bloqueado por demasiados intentos | 3+ intentos fallidos |
| USR-005 | 403 | Usuario bloqueado por razones de seguridad | Bloqueado manualmente |
| USR-006 | 500 | Error interno del servidor | Excepción no controlada |

## 🛡️ Seguridad

- ✅ Contraseñas hasheadas con SHA256
- ✅ Bloqueo automático tras 3 intentos fallidos
- ✅ Validación de email (formato)
- ✅ Contraseña mínimo 6 caracteres
- ✅ Prevención de duplicados de email
- ✅ Tracking de último login

## 🧪 Testing

Utiliza el archivo `Users.API/Users.API.http` para probar los endpoints en Visual Studio o con extensiones como REST Client.

## 📝 Lectura de Archivos (Flujo End-to-End)

1. **Program.cs** - Punto de entrada y configuración
2. **DTOs** - Contratos de entrada/salida
3. **IUserService & UserService** - Lógica de negocio
4. **User Model** - Estructura de datos
5. **PasswordHelper** - Utilidades
6. **Exceptions & ExceptionHandlers** - Manejo de errores
