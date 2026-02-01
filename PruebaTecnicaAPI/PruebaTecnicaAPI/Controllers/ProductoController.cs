using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PruebaTecnicaAPI.BusinessLogic.Services;
using PruebaTecnicaAPI.Entities.Entities;
using PruebaTecnicaAPI.Models;

namespace PruebaTecnicaAPI.Controllers
{
    /// <summary>
    /// Controlador API para gestionar operaciones CRUD de productos.
    /// Proporciona endpoints para listar, buscar, crear y actualizar productos.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ProductoController : Controller
    {
        #region Propiedades

        /// <summary>
        /// Servicio general que contiene la lógica de negocio.
        /// </summary>
        public readonly GeneralService _generalServices;

        /// <summary>
        /// AutoMapper para conversión entre entidades y ViewModels.
        /// </summary>
        public readonly IMapper _mapper;

        #endregion

        #region Constructor

        /// <summary>
        /// Inicializa una nueva instancia del controlador de productos.
        /// </summary>
        /// <param name="generalServices">Servicio general inyectado por dependencia</param>
        /// <param name="mapper">AutoMapper inyectado por dependencia</param>
        public ProductoController(GeneralService generalServices, IMapper mapper)
        {
            _generalServices = generalServices;
            _mapper = mapper;
        }

        #endregion

        #region Endpoints

        /// <summary>
        /// Lista todos los productos disponibles en el sistema.
        /// </summary>
        /// <returns>
        /// Respuesta JSON estandarizada con:
        /// - success: true (siempre exitoso, retorna lista vacía si no hay productos)
        /// - message: Cadena vacía
        /// - errors: Lista vacía
        /// - data: Lista de ProductoViewModel con todos los productos
        /// </returns>
        /// <response code="200">
        /// Retorna la lista de todos los productos.
        /// 
        /// Ejemplo de respuesta:
        /// {
        ///   "success": true,
        ///   "message": "",
        ///   "errors": [],
        ///   "data": [
        ///     {
        ///       "productoId": 1,
        ///       "nombre": "Laptop Dell",
        ///       "descripcion": "Laptop gaming 16GB RAM",
        ///       "precio": 25000.00,
        ///       "existencia": 10
        ///     },
        ///     {
        ///       "productoId": 2,
        ///       "nombre": "Mouse Gamer",
        ///       "descripcion": "Mouse RGB 16000 DPI",
        ///       "precio": 850.50,
        ///       "existencia": 25
        ///     }
        ///   ]
        /// }
        /// </response>
        /// <remarks>
        /// Este endpoint no requiere parámetros y siempre retorna una respuesta exitosa.
        /// Si no hay productos registrados, retorna un array vacío en el campo data.
        /// Los productos incluyen información completa: nombre, descripción, precio y existencia.
        /// </remarks>
        [HttpGet("Listar")]
        public IActionResult ListarProductos()
        {
            // Obtener la lista de productos del servicio
            var list = _generalServices.ListProductos();

            // Mapear las entidades a ViewModels
            var listViewModel = _mapper.Map<IEnumerable<ProductoViewModel>>(list);

            // Retornar respuesta estandarizada
            return Ok(new
            {
                success = true,
                message = "",
                errors = new List<string>(),
                data = listViewModel
            });
        }

        /// <summary>
        /// Busca un producto específico por su ID.
        /// </summary>
        /// <param name="id">ID del producto a buscar</param>
        /// <returns>
        /// Respuesta JSON estandarizada con:
        /// - success: true si se encuentra, false si no existe
        /// - message: Mensaje descriptivo del resultado
        /// - errors: Lista de errores (vacía si success = true)
        /// - data: ProductoViewModel con los datos del producto (null si no se encuentra)
        /// </returns>
        /// <response code="200">
        /// Retorna siempre código 200 con la estructura JSON que indica éxito o error.
        /// 
        /// Ejemplo de éxito:
        /// {
        ///   "success": true,
        ///   "message": "",
        ///   "errors": [],
        ///   "data": {
        ///     "productoId": 1,
        ///     "nombre": "Laptop Dell",
        ///     "descripcion": "Laptop gaming 16GB RAM",
        ///     "precio": 25000.00,
        ///     "existencia": 10
        ///   }
        /// }
        /// 
        /// Ejemplo de error:
        /// {
        ///   "success": false,
        ///   "message": "Producto no encontrado",
        ///   "errors": ["No existe un producto con el ID especificado"],
        ///   "data": null
        /// }
        /// </response>
        /// <remarks>
        /// El ID debe ser un número entero positivo.
        /// Si el producto no existe, retorna success = false con un mensaje descriptivo.
        /// Útil para consultar detalles completos del producto antes de agregarlo a una orden.
        /// </remarks>
        [HttpGet("Buscar/{id}")]
        public IActionResult Find(int id)
        {
            // Crear objeto producto con el ID a buscar
            var producto = new Producto { ProductoId = id };

            // Llamar al servicio para buscar el producto
            var result = _generalServices.BuscarProducto(producto);

            // Mapear el resultado a ViewModel (puede ser null si no se encuentra)
            var productoViewModel = _mapper.Map<ProductoViewModel>(result.Data);

            // Si el producto no fue encontrado
            if (!result.Success)
            {
                return Ok(new
                {
                    success = false,
                    message = result.Message,
                    errors = result.Errors,
                    data = (object)null
                });
            }

            // Retornar producto encontrado
            return Ok(new
            {
                success = true,
                message = "",
                errors = new List<string>(),
                data = productoViewModel
            });
        }

        /// <summary>
        /// Crea un nuevo producto en el sistema.
        /// </summary>
        /// <param name="item">
        /// Objeto ProductoViewModel con los datos del nuevo producto:
        /// - productoId: Debe ser 0 (se genera automáticamente)
        /// - nombre: Nombre del producto (3-100 caracteres)
        /// - descripcion: Descripción del producto (requerida)
        /// - precio: Precio unitario (debe ser mayor a 0)
        /// - existencia: Cantidad en stock (debe ser mayor o igual a 0)
        /// </param>
        /// <returns>
        /// Respuesta JSON estandarizada con:
        /// - success: true si se crea exitosamente, false si hay error
        /// - message: Mensaje descriptivo del resultado
        /// - errors: Lista de errores de validación (vacía si success = true)
        /// - data: ProductoViewModel con los datos del producto creado incluyendo el ID generado (null si hay error)
        /// </returns>
        /// <response code="200">
        /// Retorna siempre código 200 con la estructura JSON que indica éxito o error.
        /// 
        /// Ejemplo de éxito:
        /// {
        ///   "success": true,
        ///   "message": "Producto creado exitosamente",
        ///   "errors": [],
        ///   "data": {
        ///     "productoId": 5,
        ///     "nombre": "Mouse Gamer",
        ///     "descripcion": "Mouse RGB 16000 DPI",
        ///     "precio": 850.50,
        ///     "existencia": 25
        ///   }
        /// }
        /// 
        /// Ejemplo de error (validación):
        /// {
        ///   "success": false,
        ///   "message": "Error al crear el producto",
        ///   "errors": ["El productoId debe ser 0 para un nuevo producto."],
        ///   "data": null
        /// }
        /// 
        /// Ejemplo de error (precio inválido):
        /// {
        ///   "success": false,
        ///   "message": "Error al crear el producto",
        ///   "errors": ["El precio debe ser mayor a 0."],
        ///   "data": null
        /// }
        /// </response>
        /// <remarks>
        /// Validaciones aplicadas:
        /// - El productoId debe ser 0 (se genera automáticamente en la base de datos)
        /// - El nombre debe tener entre 3 y 100 caracteres
        /// - La descripción es requerida y no puede estar vacía
        /// - El precio debe ser mayor a 0
        /// - La existencia debe ser mayor o igual a 0 (permite productos sin stock)
        /// 
        /// Todas las validaciones se realizan en el procedimiento almacenado.
        /// El precio se almacena con 2 decimales de precisión.
        /// </remarks>
        /// <example>
        /// Request:
        /// POST /Producto/Insertar
        /// {
        ///   "productoId": 0,
        ///   "nombre": "Mouse Gamer",
        ///   "descripcion": "Mouse RGB 16000 DPI",
        ///   "precio": 850.50,
        ///   "existencia": 25
        /// }
        /// </example>
        [HttpPost("Insertar")]
        public IActionResult Create([FromBody] ProductoViewModel item)
        {
            // Mapear ViewModel a Entity
            var producto = _mapper.Map<Producto>(item);

            // Llamar al service para insertar el producto
            var result = _generalServices.InsertarProducto(producto);

            // Mapear el resultado a ViewModel (null si hay error)
            var productoViewModel = result.Data != null ? _mapper.Map<ProductoViewModel>(result.Data) : null;

            // Si hay error en la validación o inserción
            if (!result.Success)
            {
                return Ok(new
                {
                    success = false,
                    message = result.Message,
                    errors = result.Errors,
                    data = (object)null
                });
            }

            // Retornar producto creado exitosamente
            return Ok(new
            {
                success = true,
                message = result.Message,
                errors = new List<string>(),
                data = productoViewModel
            });
        }

        /// <summary>
        /// Actualiza los datos de un producto existente.
        /// </summary>
        /// <param name="id">ID del producto a actualizar (debe coincidir con el productoId en el body)</param>
        /// <param name="item">
        /// Objeto ProductoViewModel con los datos actualizados:
        /// - nombre: Nuevo nombre del producto (3-100 caracteres)
        /// - descripcion: Nueva descripción del producto (requerida)
        /// - precio: Nuevo precio unitario (debe ser mayor a 0)
        /// - existencia: Nueva cantidad en stock (debe ser mayor o igual a 0)
        /// </param>
        /// <returns>
        /// Respuesta JSON estandarizada con:
        /// - success: true si se actualiza exitosamente, false si hay error
        /// - message: Mensaje descriptivo del resultado
        /// - errors: Lista de errores de validación (vacía si success = true)
        /// - data: ProductoViewModel con los datos del producto actualizado (null si hay error)
        /// </returns>
        /// <response code="200">
        /// Retorna siempre código 200 con la estructura JSON que indica éxito o error.
        /// 
        /// Ejemplo de éxito:
        /// {
        ///   "success": true,
        ///   "message": "Producto actualizado exitosamente",
        ///   "errors": [],
        ///   "data": {
        ///     "productoId": 1,
        ///     "nombre": "Laptop Dell XPS 15",
        ///     "descripcion": "Laptop profesional 32GB RAM, 1TB SSD",
        ///     "precio": 35000.00,
        ///     "existencia": 5
        ///   }
        /// }
        /// 
        /// Ejemplo de error (producto no existe):
        /// {
        ///   "success": false,
        ///   "message": "Error al actualizar el producto",
        ///   "errors": ["Producto no encontrado."],
        ///   "data": null
        /// }
        /// 
        /// Ejemplo de error (existencia inválida):
        /// {
        ///   "success": false,
        ///   "message": "Error al actualizar el producto",
        ///   "errors": ["La existencia debe ser mayor o igual a 0."],
        ///   "data": null
        /// }
        /// </response>
        /// <remarks>
        /// Validaciones aplicadas:
        /// - El producto debe existir en el sistema
        /// - El nombre debe tener entre 3 y 100 caracteres
        /// - La descripción es requerida y no puede estar vacía
        /// - El precio debe ser mayor a 0
        /// - La existencia debe ser mayor o igual a 0
        /// 
        /// El ID del parámetro de ruta prevalece sobre el productoId del body para evitar inconsistencias.
        /// Todas las validaciones se realizan en el procedimiento almacenado.
        /// Útil para actualizar precios, ajustar inventario o modificar descripciones.
        /// </remarks>
        /// <example>
        /// Request:
        /// PUT /Producto/Actualizar/1
        /// {
        ///   "nombre": "Laptop Dell XPS 15",
        ///   "descripcion": "Laptop profesional 32GB RAM, 1TB SSD",
        ///   "precio": 35000.00,
        ///   "existencia": 5
        /// }
        /// </example>
        [HttpPut("Actualizar/{id}")]
        public IActionResult Update(int id, [FromBody] ProductoViewModel item)
        {
            // Asegurar que el ID del body coincida con el ID de la ruta
            // El ID de la ruta prevalece para evitar inconsistencias
            item.ProductoId = id;

            // Mapear ViewModel a Entity
            var producto = _mapper.Map<Producto>(item);

            // Llamar al service para actualizar el producto
            var result = _generalServices.ActualizarProducto(producto);

            // Mapear el resultado a ViewModel (null si hay error)
            var productoViewModel = result.Data != null ? _mapper.Map<ProductoViewModel>(result.Data) : null;

            // Si hay error en la validación o actualización
            if (!result.Success)
            {
                return Ok(new
                {
                    success = false,
                    message = result.Message,
                    errors = result.Errors,
                    data = (object)null
                });
            }

            // Retornar producto actualizado exitosamente
            return Ok(new
            {
                success = true,
                message = result.Message,
                errors = new List<string>(),
                data = productoViewModel
            });
        }

        #endregion
    }
}