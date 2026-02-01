using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PruebaTecnicaAPI.BusinessLogic.Services;
using PruebaTecnicaAPI.Entities.Entities;
using PruebaTecnicaAPI.Models;

namespace PruebaTecnicaAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClienteController : Controller
    {
        public readonly GeneralService _generalServices;
        public readonly IMapper _mapper;

        public ClienteController(GeneralService generalServices, IMapper mapper)
        {
            _generalServices = generalServices;
            _mapper = mapper;
        }

        [HttpGet("Listar")]
        public IActionResult ListarClientes()
        {
            var list = _generalServices.ListClientes();
            var listViewModel = _mapper.Map<IEnumerable<ClienteViewModel>>(list);

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
            var cliente = new Cliente { ClienteId = id };
            var result = _generalServices.BuscarCliente(cliente);
            var clienteViewModel = _mapper.Map<ClienteViewModel>(result.Data);

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
                data = clienteViewModel
            });
        }

        [HttpPost("Insertar")]
        public IActionResult Create([FromBody] ClienteViewModel item)
        {
            // Mapear ViewModel a Entity
            var cliente = _mapper.Map<Cliente>(item);

            // Llamar al service
            var result = _generalServices.InsertarCliente(cliente);

            // Mapear el resultado a ViewModel
            var clienteViewModel = result.Data != null ? _mapper.Map<ClienteViewModel>(result.Data) : null;

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
                data = clienteViewModel
            });
        }

        [HttpPut("Actualizar/{id}")]
        public IActionResult Update(int id, [FromBody] ClienteViewModel item)
        {
            // Asegurar que el ID del body coincida con el ID de la ruta
            item.ClienteId = id;

            // Mapear ViewModel a Entity
            var cliente = _mapper.Map<Cliente>(item);

            // Llamar al service
            var result = _generalServices.ActualizarCliente(cliente);

            // Mapear el resultado a ViewModel
            var clienteViewModel = result.Data != null ? _mapper.Map<ClienteViewModel>(result.Data) : null;

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
                data = clienteViewModel
            });
        }
    }
}