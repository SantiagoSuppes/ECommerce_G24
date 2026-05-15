# Cart.API - E-Commerce Microservices

## Descripción

Este microservicio es responsable de:

* Obtener el carrito de un usuario.
* Agregar productos al carrito.
* Actualizar cantidades de productos.
* Eliminar productos del carrito.
* Vaciar el carrito completo.
* Validar productos y stock contra `Products.API`.

Además, implementa:

* Swagger/OpenAPI
* Manejo global de errores con `IExceptionHandler`
* Logging estructurado con Serilog
* Correlation ID
* Health Checks
* Comunicación HTTP entre microservicios usando `HttpClient`

---

# Tecnologías utilizadas

* .NET 8
* ASP.NET Core Web API
* Swagger / OpenAPI
* Serilog
* Health Checks
* IHttpClientFactory
* IExceptionHandler

---

# Estructura del proyecto

```text
Cart.API/
│
├── Controllers
│   └── CartController.cs
│
├── DTOs
│   ├── AddCartItemRequestDto.cs
│   ├── UpdateCartItemRequestDto.cs
│   ├── CartResponseDto.cs
│   ├── CartItemResponseDto.cs
│   ├── ProductResponseDto.cs
│   └── ErrorResponseDto.cs
│
├── ExceptionHandlers
│   ├── NotFoundExceptionHandler.cs
│   ├── ValidationExceptionHandler.cs
│   ├── BusinessRuleExceptionHandler.cs
│   └── GlobalExceptionHandler.cs
│
├── Exceptions
│   ├── NotFoundException.cs
│   ├── ValidationException.cs
│   └── BusinessRuleException.cs
│
├── Middleware
│   └── CorrelationIdMiddleware.cs
│
├── Models
│   ├── Cart.cs
│   └── CartItem.cs
│
├── Services
│   ├── ICartService.cs
│   ├── CartService.cs
│   └── CorrelationIdDelegatingHandler.cs
│
├── logs
│
├── appsettings.json
├── Program.cs
└── Cart.API.csproj
```

---

# Instalación de paquetes NuGet

Ejecutar:

```bash
dotnet add package Swashbuckle.AspNetCore --version 6.6.2

dotnet add package Microsoft.OpenApi --version 1.6.14

dotnet add package Serilog.AspNetCore --version 8.0.1

dotnet add package Serilog.Sinks.Console --version 5.0.1

dotnet add package Serilog.Sinks.File --version 5.0.0

dotnet add package Serilog.Enrichers.Environment --version 2.3.0

dotnet add package Serilog.Enrichers.Process --version 2.0.2

dotnet add package Serilog.Enrichers.Thread --version 3.1.0
```

---

# Configuración del proyecto

## Cart.API.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>

    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
  </PropertyGroup>

  <ItemGroup>

    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.6.2" />
    <PackageReference Include="Microsoft.OpenApi" Version="1.6.14" />

    <PackageReference Include="Serilog.AspNetCore" Version="8.0.1" />
    <PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
    <PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
    <PackageReference Include="Serilog.Enrichers.Environment" Version="2.3.0" />
    <PackageReference Include="Serilog.Enrichers.Process" Version="2.0.2" />
    <PackageReference Include="Serilog.Enrichers.Thread" Version="3.1.0" />

  </ItemGroup>

</Project>
```

---

# Endpoints implementados

## Obtener carrito

```http
GET /api/cart/{userId}
```

## Agregar producto al carrito

```http
POST /api/cart/{userId}/items
```

## Actualizar cantidad de producto

```http
PUT /api/cart/{userId}/items/{productId}
```

## Eliminar producto del carrito

```http
DELETE /api/cart/{userId}/items/{productId}
```

## Vaciar carrito

```http
DELETE /api/cart/{userId}
```

---

# Catálogo de errores

| Código  | HTTP | Descripción            |
| ------- | ---- | ---------------------- |
| CRT-001 | 404  | Carrito no encontrado  |
| CRT-002 | 404  | Producto no encontrado |
| CRT-003 | 422  | Stock insuficiente     |
| CRT-004 | 400  | Cantidad inválida      |
| CRT-005 | 500  | Error interno          |

---

# Swagger

Swagger se encuentra disponible en:

```text
/swagger
```

Ejemplo:

```text
http://localhost:5134/swagger
```

---

# Manejo global de errores

Se implementó `IExceptionHandler` para:

* NotFoundException
* ValidationException
* BusinessRuleException
* GlobalException

Formato de error:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "El recurso solicitado no fue encontrado.",
  "instance": "/api/cart/123",
  "errorCode": "CRT-001",
  "errorMessage": "Carrito no encontrado.",
  "correlationId": "abc-123"
}
```

---

# Logging con Serilog

Se implementaron logs:

* En consola
* En archivo JSON estructurado

Los archivos se generan automáticamente dentro de:

```text
logs/
```

Ejemplo:

```text
logs/cart-api-20260515.json
```

Información incluida:

* Timestamp
* Nivel
* Servicio
* Endpoint
* Correlation ID
* ErrorCode
* Duración del request

---

# Correlation ID

Cada request genera automáticamente:

```text
X-Correlation-Id
```

El Correlation ID:

* Se agrega a todos los logs
* Se devuelve en respuestas de error
* Se propaga automáticamente a Products.API

---

# Health Checks

Endpoints implementados:

```http
GET /health
GET /health/ready
GET /health/live
```

Respuesta:

```json
{
  "status": "Healthy"
}
```

---

# Comunicación con Products.API

Cart.API utiliza `IHttpClientFactory` para comunicarse con `Products.API`.

Validaciones realizadas:

* Verificar que el producto exista
* Verificar stock disponible

Configuración en `appsettings.json`:

```json
{
  "Services": {
    "ProductsApi": "https://localhost:7001"
  }
}
```

---

# Cómo ejecutar el proyecto

## Ejecutar Products.API

```bash
dotnet run --project src/Products.API
```

## Ejecutar Cart.API

```bash
dotnet run --project src/Cart.API
```

---

# Ejemplos de pruebas

## Crear producto en Products.API

```http
POST /api/products
```

Body:

```json
{
  "nombre": "Notebook Dell",
  "descripcion": "Laptop gamer",
  "precio": 1500,
  "stock": 10,
  "categoria": "Electrónica"
}
```

---

## Agregar producto al carrito

```http
POST /api/cart/{userId}/items
```

Body:

```json
{
  "productoId": "11111111-1111-1111-1111-111111111111",
  "cantidad": 2
}
```

---

## Obtener carrito

```http
GET /api/cart/{userId}
```

---

## Actualizar cantidad

```http
PUT /api/cart/{userId}/items/{productId}
```

Body:

```json
{
  "cantidad": 4
}
```

---

## Eliminar producto

```http
DELETE /api/cart/{userId}/items/{productId}
```

---

## Vaciar carrito

```http
DELETE /api/cart/{userId}
```

---

# Problemas encontrados y solución

## Error Swagger

Error:

```text
Method not found:
Microsoft.OpenApi.IOpenApiRequestBody.get_Content()
```

Solución:

* Utilizar:

```xml
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.6.2" />
<PackageReference Include="Microsoft.OpenApi" Version="1.6.14" />
```

* Limpiar paquetes:

```bash
dotnet clean
dotnet restore
```

---

# Objetivos del TP cubiertos

* Arquitectura de microservicios
* REST API
* Swagger/OpenAPI
* Manejo global de errores
* Logging estructurado
* Correlation ID
* Health Checks
* Comunicación HTTP entre microservicios
* Buenas prácticas ASP.NET Core 8

---
