# PruebaTecnicaAPI

API REST desarrollada en **ASP.NET Core Web API** (.NET) para el manejo de órdenes con clientes y productos. Implementa una arquitectura en capas con uso de Stored Procedures y transacciones para garantizar la consistencia de datos.

---

## Estructura del Proyecto

```
PruebaTecnicaAPI.sln
├── PruebaTecnicaAPI                  # Proyecto principal (Entry Point)
│   ├── Controllers/                  # Controladores de la API
│   ├── Extensions/                   # Extensiones y configuraciones (AutoMapper)
│   ├── Models/                       # ViewModels y Request Models (DTOs)
│   ├── Program.cs                    # Configuración de la aplicación
│   └── appsettings.json              # Configuración de conexión a BD
├── PruebaTecnicaAPI.BusinessLogic    # Capa de lógica de negocio
│   └── Services/                     # Servicios (GeneralService)
├── PruebaTecnicaAPI.DataAccess       # Capa de acceso a datos
│   ├── Context/                      # Contexto de conexión a BD
│   ├── Repositories/                 # Repositorios (Cliente, Producto, Orden)
│   └── ScriptDatabase.cs            # Nombres de los Stored Procedures
└── PruebaTecnicaAPI.Entities         # Entidades de la base de datos
    └── Entities/                     # Modelos de datos
```

---

## Tecnologías Utilizadas

- **ASP.NET Core Web API** — Framework principal
- **Dapper** — Micro ORM para ejecutar Stored Procedures
- **AutoMapper** — Mapeo entre Entities y ViewModels
- **Microsoft.Data.SqlClient** — Conexión a SQL Server
- **SQL Server** — Base de datos relacional

---

## Prerequisitos

- .NET SDK instalado
- SQL Server (cualquier versión compatible)
- Visual Studio 2022 o VS Code

---

## Configuración

1. Clonar el repositorio.

2. No es necesario un script de la base de datos o crear una nueva, ya que esta subida a un servidor, el acceso a ella es:
    Server: db39761.public.databaseasp.net
    Login: db39761
    Password: HolaAlex123
  En la cual usted mismo puede verificar los datos insertados y listados, así como los procedimientos almacenados que se usan

3. Actualizar la cadena de conexión en `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR;Database=OrderManagementDB;Trusted_Connection=true;"
  }
}
```

4. Ejecutar los Stored Procedures en SSMS antes de iniciar la aplicación.

 (Pasos 3 y 4 no necesarios en caso se use la misma Base de datos, solo en caso de que se cambie a otra es necesario y no 
  olvidar los procedimientos)

---

## Endpoints

### Clientes

| Método | Endpoint                    | Descripción                  |
|--------|-----------------------------|------------------------------|
| GET    | `/Cliente/Listar`           | Lista todos los clientes     |
| GET    | `/Cliente/Buscar/{id}`      | Busca un cliente por ID      |
| POST   | `/Cliente/Insertar`         | Crea un nuevo cliente        |
| PUT    | `/Cliente/Actualizar/{id}`  | Actualiza un cliente         |

**Ejemplo de request (Insertar):**
```json
{
  "clienteId": 0,
  "nombre": "Juan Pérez",
  "identidad": "0801-1990-12345"
}
```

---

### Productos

| Método | Endpoint                     | Descripción                   |
|--------|------------------------------|-------------------------------|
| GET    | `/Producto/Listar`           | Lista todos los productos     |
| GET    | `/Producto/Buscar/{id}`      | Busca un producto por ID      |
| POST   | `/Producto/Insertar`         | Crea un nuevo producto        |
| PUT    | `/Producto/Actualizar/{id}`  | Actualiza un producto         |

**Ejemplo de request (Insertar):**
```json
{
  "productoId": 0,
  "nombre": "Laptop Dell XPS 15",
  "descripcion": "Laptop de alto rendimiento",
  "precio": 1299.99,
  "existencia": 50
}
```

---

### Órdenes

| Método | Endpoint              | Descripción                          |
|--------|-----------------------|--------------------------------------|
| POST   | `/Orden/Insertar`     | Crea una orden con sus detalles      |

**Ejemplo de request:**
```json
{
  "ordenId": 0,
  "clienteId": 1,
  "detalle": [
    {
      "productoId": 1,
      "cantidad": 2
    },
    {
      "productoId": 3,
      "cantidad": 1
    }
  ]
}
```

**Ejemplo de respuesta exitosa:**
```json
{
  "success": true,
  "message": "Orden creada exitosamente",
  "errors": [],
  "data": {
    "ordenId": 1,
    "clienteId": 1,
    "clienteNombre": "Juan Pérez",
    "subtotal": 2799.97,
    "impuesto": 419.99,
    "total": 3219.96,
    "fechaCreacion": "2025-01-31T15:30:00",
    "detalles": [
      {
        "detalleOrdenId": 1,
        "ordenId": 1,
        "productoId": 1,
        "productoNombre": "Laptop Dell XPS 15",
        "cantidad": 2,
        "subtotal": 2599.98,
        "impuesto": 389.99,
        "total": 2989.97
      },
      {
        "detalleOrdenId": 2,
        "ordenId": 1,
        "productoId": 3,
        "productoNombre": "Teclado Mecánico Keychron K2",
        "cantidad": 1,
        "subtotal": 89.99,
        "impuesto": 13.49,
        "total": 103.48
      }
    ]
  }
}
```

**Ejemplo de respuesta de error:**
```json
{
  "success": false,
  "message": "Error al procesar la orden",
  "errors": [
    "El producto 'Laptop Dell XPS 15' no tiene suficientes existencias. Disponible: 1, Solicitado: 2"
  ],
  "data": null
}
```

---

## Lógica de Negocio - Creación de Orden

El proceso de creación de una orden sigue estos pasos dentro de una transacción:

1. **Validar cliente** — El cliente debe existir en la base de datos.
2. **Inicializar orden** — Se crea la orden con impuesto, subtotal y total en 0.
3. **Procesar cada detalle:**
   - Validar que el producto exista.
   - Validar que la existencia sea suficiente (validación crítica).
   - Calcular: `Subtotal = Cantidad × Precio`, `Impuesto = Subtotal × 0.15`, `Total = Subtotal + Impuesto`.
   - Crear el registro de DetalleOrden.
   - Actualizar la existencia del producto (`Existencia -= Cantidad`).
4. **Actualizar orden** — Se calculan los totales como la suma de los detalles.
5. **Transacción** — Todo el proceso está dentro de una transacción. Si falla cualquier paso, se hace rollback completo.

---

## Validaciones

- Cliente debe existir en la base de datos.
- Debe tener al menos un detalle en la orden.
- Cada producto debe existir.
- La existencia del producto debe ser suficiente para la cantidad solicitada.
- La cantidad de cada detalle debe ser mayor a 0.
- No se permite el mismo producto más de una vez en los detalles de una orden.
- El ordenId debe ser 0 para crear una nueva orden.

---

## Formato de Respuesta

Todas las respuestas de la API siguen el mismo formato:

```json
{
  "success": true/false,
  "message": "Mensaje descriptivo",
  "errors": [],
  "data": {}
}
```

| Campo     | Descripción                                          |
|-----------|------------------------------------------------------|
| success   | `true` si la operación fue exitosa, `false` si no   |
| message   | Mensaje descriptivo de la operación                 |
| errors    | Lista de errores (vacía si fue exitoso)             |
| data      | Datos retornados (`null` si hay error)              |
