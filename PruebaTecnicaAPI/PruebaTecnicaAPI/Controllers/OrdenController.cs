using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PruebaTecnicaAPI.BusinessLogic.Services;
using PruebaTecnicaAPI.Entities.Entities;
using PruebaTecnicaAPI.Models;

namespace PruebaTecnicaAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdenController : Controller
    {
        public readonly GeneralService _generalServices;
        public readonly IMapper _mapper;

        public OrdenController(GeneralService generalServices, IMapper mapper)
        {
            _generalServices = generalServices;
            _mapper = mapper;
        }

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

            // Serializar detalles a JSON en camelCase para el SP
            var opciones = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            };
            var detallesJson = System.Text.Json.JsonSerializer.Serialize(item.Detalle, opciones);

            // Llamar al service
            var result = _generalServices.CrearOrden(item.ClienteId, detallesJson);

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

            // Mapear dynamic a OrdenViewModel usando Dictionary
            var data = (Dictionary<string, IEnumerable<dynamic>>)result.Data;
            var ordenData = data["Orden"].First();
            var detallesData = data["Detalles"];

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

            return Ok(new
            {
                success = true,
                message = result.Message,
                errors = new List<string>(),
                data = orden
            });
        }
    }
}