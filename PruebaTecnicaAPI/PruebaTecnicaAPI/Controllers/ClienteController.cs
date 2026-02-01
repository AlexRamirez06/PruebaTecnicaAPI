using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PruebaTecnicaAPI.BusinessLogic.Services;
using PruebaTecnicaAPI.Entities.Entities;
using PruebaTecnicaAPI.Models;

namespace PruebaTecnicaAPI.Controllers
{
    /// <summary>
    /// Controlador API para gestionar operaciones CRUD de clientes.
    /// Proporciona endpoints para listar, buscar, crear y actualizar clientes.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ClienteController : Controller
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
        /// Inicializa una nueva instancia del controlador de clientes.
        /// </summary>
        /// <param name="generalServices">Servicio general inyectado por dependencia</param>
        /// <param name="mapper">AutoMapper inyectado por dependencia</param>
        public ClienteController(GeneralService generalServices, IMapper mapper)
        {
            _generalServices = generalServices;
            _mapper = mapper;
        }

        #endregion

        #region Endpoints

        /// <summary>
        /// Lista todos los clientes registrados en el sistema.
        /// </summary>
        /// <returns>
        /// Respuesta JSON estandarizada con:
        /// - success: true (siempre exitoso, retorna lista vacía si no hay clientes)
        /// - message: Cadena vacía
        /// - errors: Lista vacía
        /// - data: Lista de ClienteViewModel con todos los clientes
        /// </returns>
        /// <response code="200">
        /// Retorna la lista de todos los clientes.
        /// 
        /// Ejemplo de respuesta:
        /// {
        ///   "success": true,
        ///   "message": "",
        ///   "errors": [],
        ///   "data": [
        ///     {
        ///       "clienteId": 1,
        ///       "nombre": "Juan Pérez",
        ///       "identidad": "0801-1990-12345"
        ///     },
        ///     {
        ///       "clienteId": 2,
        ///       "nombre": "María González",
        ///       "identidad": "0801-1985-67890"
        ///     }
        ///   ]
        /// }
        /// </response>
        /// <remarks>
        /// Este endpoint no requiere parámetros y siempre retorna una respuesta exitosa.
        /// Si no hay clientes registrados, retorna un array vacío en el campo data.
        /// </remarks>
        [HttpGet("Listar")]
        public IActionResult ListarClientes()
        {
            // Obtener la lista de clientes del servicio
            var list = _generalServices.ListClientes();

            // Mapear las entidades a ViewModels
            var listViewModel = _mapper.Map<IEnumerable<ClienteViewModel>>(list);

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
        /// Busca un cliente específico por su ID.
        /// </summary>
        /// <param name="id">ID del cliente a buscar</param>
        /// <returns>
        /// Respuesta JSON estandarizada con:
        /// - success: true si se encuentra, false si no existe
        /// - message: Mensaje descriptivo del resultado
        /// - errors: Lista de errores (vacía si success = true)
        /// - data: ClienteViewModel con los datos del cliente (null si no se encuentra)
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
        ///     "clienteId": 1,
        ///     "nombre": "Juan Pérez",
        ///     "identidad": "0801-1990-12345"
        ///   }
        /// }
        /// 
        /// Ejemplo de error:
        /// {
        ///   "success": false,
        ///   "message": "Cliente no encontrado",
        ///   "errors": ["No existe un cliente con el ID especificado"],
        ///   "data": null
        /// }
        /// </response>
        /// <remarks>
        /// El ID debe ser un número entero positivo.
        /// Si el cliente no existe, retorna success = false con un mensaje descriptivo.
        /// </remarks>
        [HttpGet("Buscar/{id}")]
        public IActionResult Find(int id)
        {
            // Crear objeto cliente con el ID a buscar
            var cliente = new Cliente { ClienteId = id };

            // Llamar al servicio para buscar el cliente
            var result = _generalServices.BuscarCliente(cliente);

            // Mapear el resultado a ViewModel (puede ser null si no se encuentra)
            var clienteViewModel = _mapper.Map<ClienteViewModel>(result.Data);

            // Si el cliente no fue encontrado
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

            // Retornar cliente encontrado
            return Ok(new
            {
                success = true,
                message = "",
                errors = new List<string>(),
                data = clienteViewModel
            });
        }

        /// <summary>
        /// Crea un nuevo cliente en el sistema.
        /// </summary>
        /// <param name="item">
        /// Objeto ClienteViewModel con los datos del nuevo cliente:
        /// - clienteId: Debe ser 0 (se genera automáticamente)
        /// - nombre: Nombre del cliente (3-100 caracteres)
        /// - identidad: Número de identidad único (formato: ####-####-#####)
        /// </param>
        /// <returns>
        /// Respuesta JSON estandarizada con:
        /// - success: true si se crea exitosamente, false si hay error
        /// - message: Mensaje descriptivo del resultado
        /// - errors: Lista de errores de validación (vacía si success = true)
        /// - data: ClienteViewModel con los datos del cliente creado incluyendo el ID generado (null si hay error)
        /// </returns>
        /// <response code="200">
        /// Retorna siempre código 200 con la estructura JSON que indica éxito o error.
        /// 
        /// Ejemplo de éxito:
        /// {
        ///   "success": true,
        ///   "message": "Cliente creado exitosamente",
        ///   "errors": [],
        ///   "data": {
        ///     "clienteId": 4,
        ///     "nombre": "Pedro Martínez",
        ///     "identidad": "0801-1988-55555"
        ///   }
        /// }
        /// 
        /// Ejemplo de error (validación):
        /// {
        ///   "success": false,
        ///   "message": "Error al crear el cliente",
        ///   "errors": ["El clienteId debe ser 0 para un nuevo cliente."],
        ///   "data": null
        /// }
        /// 
        /// Ejemplo de error (identidad duplicada):
        /// {
        ///   "success": false,
        ///   "message": "Error al crear el cliente",
        ///   "errors": ["Ya existe un cliente con la identidad 0801-1988-55555."],
        ///   "data": null
        /// }
        /// </response>
        /// <remarks>
        /// Validaciones aplicadas:
        /// - El clienteId debe ser 0 (se genera automáticamente en la base de datos)
        /// - El nombre debe tener entre 3 y 100 caracteres
        /// - La identidad es requerida y debe tener formato válido (####-####-#####)
        /// - La identidad debe ser única en el sistema
        /// 
        /// Todas las validaciones se realizan en el procedimiento almacenado.
        /// </remarks>
        /// <example>
        /// Request:
        /// POST /Cliente/Insertar
        /// {
        ///   "clienteId": 0,
        ///   "nombre": "Pedro Martínez",
        ///   "identidad": "0801-1988-55555"
        /// }
        /// </example>
        [HttpPost("Insertar")]
        public IActionResult Create([FromBody] ClienteViewModel item)
        {
            // Mapear ViewModel a Entity
            var cliente = _mapper.Map<Cliente>(item);

            // Llamar al service para insertar el cliente
            var result = _generalServices.InsertarCliente(cliente);

            // Mapear el resultado a ViewModel (null si hay error)
            var clienteViewModel = result.Data != null ? _mapper.Map<ClienteViewModel>(result.Data) : null;

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

            // Retornar cliente creado exitosamente
            return Ok(new
            {
                success = true,
                message = result.Message,
                errors = new List<string>(),
                data = clienteViewModel
            });
        }

        /// <summary>
        /// Actualiza los datos de un cliente existente.
        /// </summary>
        /// <param name="id">ID del cliente a actualizar (debe coincidir con el clienteId en el body)</param>
        /// <param name="item">
        /// Objeto ClienteViewModel con los datos actualizados:
        /// - nombre: Nuevo nombre del cliente (3-100 caracteres)
        /// - identidad: Nueva identidad del cliente (formato: ####-####-#####)
        /// </param>
        /// <returns>
        /// Respuesta JSON estandarizada con:
        /// - success: true si se actualiza exitosamente, false si hay error
        /// - message: Mensaje descriptivo del resultado
        /// - errors: Lista de errores de validación (vacía si success = true)
        /// - data: ClienteViewModel con los datos del cliente actualizado (null si hay error)
        /// </returns>
        /// <response code="200">
        /// Retorna siempre código 200 con la estructura JSON que indica éxito o error.
        /// 
        /// Ejemplo de éxito:
        /// {
        ///   "success": true,
        ///   "message": "Cliente actualizado exitosamente",
        ///   "errors": [],
        ///   "data": {
        ///     "clienteId": 1,
        ///     "nombre": "Juan Carlos Pérez",
        ///     "identidad": "0801-1990-12345"
        ///   }
        /// }
        /// 
        /// Ejemplo de error (cliente no existe):
        /// {
        ///   "success": false,
        ///   "message": "Error al actualizar el cliente",
        ///   "errors": ["Cliente no encontrado."],
        ///   "data": null
        /// }
        /// 
        /// Ejemplo de error (identidad duplicada):
        /// {
        ///   "success": false,
        ///   "message": "Error al actualizar el cliente",
        ///   "errors": ["Ya existe un cliente con la identidad 0801-1985-67890."],
        ///   "data": null
        /// }
        /// </response>
        /// <remarks>
        /// Validaciones aplicadas:
        /// - El cliente debe existir en el sistema
        /// - El nombre debe tener entre 3 y 100 caracteres
        /// - La identidad es requerida y debe tener formato válido (####-####-#####)
        /// - La identidad debe ser única (excepto para el mismo cliente)
        /// 
        /// El ID del parámetro de ruta prevalece sobre el clienteId del body para evitar inconsistencias.
        /// Todas las validaciones se realizan en el procedimiento almacenado.
        /// </remarks>
        /// <example>
        /// Request:
        /// PUT /Cliente/Actualizar/1
        /// {
        ///   "nombre": "Juan Carlos Pérez",
        ///   "identidad": "0801-1990-12345"
        /// }
        /// </example>
        [HttpPut("Actualizar/{id}")]
        public IActionResult Update(int id, [FromBody] ClienteViewModel item)
        {
            // Asegurar que el ID del body coincida con el ID de la ruta
            // El ID de la ruta prevalece para evitar inconsistencias
            item.ClienteId = id;

            // Mapear ViewModel a Entity
            var cliente = _mapper.Map<Cliente>(item);

            // Llamar al service para actualizar el cliente
            var result = _generalServices.ActualizarCliente(cliente);

            // Mapear el resultado a ViewModel (null si hay error)
            var clienteViewModel = result.Data != null ? _mapper.Map<ClienteViewModel>(result.Data) : null;

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

            // Retornar cliente actualizado exitosamente
            return Ok(new
            {
                success = true,
                message = result.Message,
                errors = new List<string>(),
                data = clienteViewModel
            });
        }

        #endregion
    }
}