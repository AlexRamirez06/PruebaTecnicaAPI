using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PruebaTecnicaAPI.BusinessLogic.Services;
using PruebaTecnicaAPI.Entities.Entities;
using PruebaTecnicaAPI.Models;
namespace PruebaTecnicaAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductoController: Controller
    {
        public readonly GeneralService _generalServices;
        public readonly IMapper _mapper;

        public ProductoController(GeneralService generalServices, IMapper mapper)
        {
            _generalServices = generalServices;
            _mapper = mapper;
        }

        [HttpGet("Listar")]
        public IActionResult ListarProductos()
        {
            var list = _generalServices.ListProductos();
            var listViewModel = _mapper.Map<IEnumerable<ProductoViewModel>>(list);

            return Ok(new
            {
                success = true,
                message = "",
                errors = new List<string>(),
                data = listViewModel
            });
        }

        [HttpGet("Buscar/{id}")]
        public IActionResult Find(int id)
        {
            var producto = new Producto { ProductoId = id };
            var result = _generalServices.BuscarProducto(producto);
            var productoViewModel = _mapper.Map<ProductoViewModel>(result.Data);

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

            return Ok(new
            {
                success = true,
                message = "",
                errors = new List<string>(),
                data = productoViewModel
            });
        }

        [HttpPost("Insertar")]
        public IActionResult Create([FromBody] ProductoViewModel item)
        {
            // Mapear ViewModel a Entity
            var producto = _mapper.Map<Producto>(item);

            // Llamar al service
            var result = _generalServices.InsertarProducto(producto);

            // Mapear el resultado a ViewModel
            var productoViewModel = result.Data != null ? _mapper.Map<ProductoViewModel>(result.Data) : null;

            // Si hay error
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

            // Si es exitoso
            return Ok(new
            {
                success = true,
                message = result.Message,
                errors = new List<string>(),
                data = productoViewModel
            });
        }

        [HttpPut("Actualizar/{id}")]
        public IActionResult Update(int id, [FromBody] ProductoViewModel item)
        {
            // Asegurar que el ID del body coincida con el ID de la ruta
            item.ProductoId = id;

            // Mapear ViewModel a Entity
            var producto = _mapper.Map<Producto>(item);

            // Llamar al service
            var result = _generalServices.ActualizarProducto(producto);

            // Mapear el resultado a ViewModel
            var productoViewModel = result.Data != null ? _mapper.Map<ProductoViewModel>(result.Data) : null;

            // Si hay error
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

            // Si es exitoso
            return Ok(new
            {
                success = true,
                message = result.Message,
                errors = new List<string>(),
                data = productoViewModel
            });
        }
    }
}
