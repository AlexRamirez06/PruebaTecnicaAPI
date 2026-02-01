using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PruebaTecnicaAPI.BusinessLogic.Services;
using PruebaTecnicaAPI.Entities.Entities;
using PruebaTecnicaAPI.Models;

namespace PruebaTecnicaAPI.Controllers
{
    /// <summary>
    /// Controlador API para gestionar operaciones relacionadas con órdenes de compra.
    /// Proporciona endpoints para crear órdenes con sus detalles asociados.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class OrdenController : Controller
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
        /// Inicializa una nueva instancia del controlador de órdenes.
        /// </summary>
        /// <param name="generalServices">Servicio general inyectado por dependencia</param>
        /// <param name="mapper">AutoMapper inyectado por dependencia</param>
        public OrdenController(GeneralService generalServices, IMapper mapper)
        {
            _generalServices = generalServices;
            _mapper = mapper;
        }

        #endregion

        #region Endpoints

        /// <summary>
        /// Crea una nueva orden de compra con sus detalles asociados.
        /// </summary>
        /// <param name="item">
        /// Objeto OrdenRequest que contiene:
        /// - OrdenId: Debe ser 0 (se genera automáticamente)
        /// - ClienteId: ID del cliente que realiza la orden
        /// - Detalle: Lista de productos con cantidades
        /// </param>
        /// <returns>
        /// Respuesta JSON estandarizada con:
        /// - success: Indica si la operación fue exitosa
        /// - message: Mensaje descriptivo del resultado
        /// - errors: Lista de errores (vacía si success = true)
        /// - data: Objeto OrdenViewModel con la orden creada y sus detalles (null si hay error)
        /// </returns>
        /// <response code="200">
        /// Retorna siempre código 200 con la estructura JSON que indica éxito o error.
        /// 
        /// Ejemplo de éxito:
        /// {
        ///   "success": true,
        ///   "message": "Orden creada exitosamente",
        ///   "errors": [],
        ///   "data": {
        ///     "ordenId": 1,
        ///     "clienteId": 5,
        ///     "clienteNombre": "Juan Pérez",
        ///     "subtotal": 1000.00,
        ///     "impuesto": 150.00,
        ///     "total": 1150.00,
        ///     "fechaCreacion": "2026-02-01T10:30:00",
        ///     "detalles": [...]
        ///   }
        /// }
        /// 
        /// Ejemplo de error:
        /// {
        ///   "success": false,
        ///   "message": "Error al crear la orden",
        ///   "errors": ["El cliente no existe"],
        ///   "data": null
        /// }
        /// </response>
        /// <remarks>
        /// Validaciones aplicadas en el controlador:
        /// - El OrdenId debe ser 0 (nueva orden)
        /// - Debe incluir al menos un detalle
        /// 
        /// Validaciones aplicadas en el procedimiento almacenado:
        /// - El cliente debe existir
        /// - Todos los productos deben existir
        /// - Debe haber suficiente existencia de cada producto
        /// - Las cantidades deben ser mayores a 0
        /// 
        /// El endpoint realiza una transacción completa que incluye:
        /// 1. Crear el registro de la orden
        /// 2. Crear los detalles de la orden
        /// 3. Actualizar las existencias de los productos
        /// 4. Calcular subtotales, impuestos y totales
        /// 
        /// El campo "Detalle" en el request debe tener el formato:
        /// [
        ///   { "productoId": 1, "cantidad": 2 },
        ///   { "productoId": 3, "cantidad": 1 }
        /// ]
        /// 
        /// Los detalles se serializan a JSON en formato camelCase antes de enviarlos
        /// al procedimiento almacenado.
        /// </remarks>
        /// <example>
        /// Request:
        /// POST /Orden/Insertar
        /// {
        ///   "ordenId": 0,
        ///   "clienteId": 5,
        ///   "detalle": [
        ///     { "productoId": 1, "cantidad": 2 },
        ///     { "productoId": 3, "cantidad": 1 }
        ///   ]
        /// }
        /// </example>
        [HttpPost("Insertar")]
        public IActionResult Create([FromBody] OrdenRequest item)
        {
            // Validación: ordenId debe ser 0
            if (item.OrdenId != 0)
            {
                return Ok(new
                {
                    success = false,
                    message = "Error al crear la orden",
                    errors = new List<string> { "El ordenId debe ser 0 para una nueva orden." },
                    data = (object)null
                });
            }

            // Validación: debe tener al menos un detalle
            if (item.Detalle == null || item.Detalle.Count == 0)
            {
                return Ok(new
                {
                    success = false,
                    message = "Error al crear la orden",
                    errors = new List<string> { "Debe tener al menos un detalle." },
                    data = (object)null
                });
            }

            // Serializar detalles a JSON en camelCase para el procedimiento almacenado
            // El SP espera el JSON en formato camelCase
            var opciones = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            };
            var detallesJson = System.Text.Json.JsonSerializer.Serialize(item.Detalle, opciones);

            // Llamar al service para crear la orden
            var result = _generalServices.CrearOrden(item.ClienteId, detallesJson);

            // Si hubo error en el service
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

            // Mapear dynamic a OrdenViewModel
            // El resultado del SP viene como Dictionary con dos llaves: "Orden" y "Detalles"
            var data = (Dictionary<string, IEnumerable<dynamic>>)result.Data;
            var ordenData = data["Orden"].First();
            var detallesData = data["Detalles"];

            // Construir el ViewModel de la orden
            var orden = new OrdenViewModel
            {
                OrdenId = ordenData.OrdenId,
                ClienteId = ordenData.ClienteId,
                ClienteNombre = ordenData.ClienteNombre,
                Subtotal = ordenData.Subtotal,
                Impuesto = ordenData.Impuesto,
                Total = ordenData.Total,
                FechaCreacion = ordenData.FechaCreacion,
                Detalles = new List<DetalleOrdenViewModel>()
            };

            // Mapear cada detalle de la orden
            foreach (var d in detallesData)
            {
                orden.Detalles.Add(new DetalleOrdenViewModel
                {
                    DetalleOrdenId = d.DetalleOrdenId,
                    OrdenId = d.OrdenId,
                    ProductoId = d.ProductoId,
                    ProductoNombre = d.ProductoNombre,
                    Cantidad = d.Cantidad,
                    Subtotal = d.Subtotal,
                    Impuesto = d.Impuesto,
                    Total = d.Total
                });
            }

            // Retornar respuesta exitosa con la orden creada
            return Ok(new
            {
                success = true,
                message = result.Message,
                errors = new List<string>(),
                data = orden
            });
        }

        #endregion
    }
}