using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PruebaTecnicaAPI.DataAccess.Repositories;
using PruebaTecnicaAPI.Entities.Entities;

namespace PruebaTecnicaAPI.BusinessLogic.Services
{
    /// <summary>
    /// Servicio general que contiene la lógica de negocio para gestionar Clientes, Productos y Órdenes.
    /// Actúa como intermediario entre los controladores y los repositorios de datos.
    /// </summary>
    public class GeneralService
    {
        #region Propiedades

        /// <summary>
        /// Repositorio para operaciones de datos de clientes.
        /// </summary>
        public ClienteRepository _clienteRepository { get; }

        /// <summary>
        /// Repositorio para operaciones de datos de productos.
        /// </summary>
        public ProductoRepository _productoRepository { get; }

        /// <summary>
        /// Repositorio para operaciones de datos de órdenes.
        /// </summary>
        public OrdenRepository _ordenRepository { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Inicializa una nueva instancia del servicio general con los repositorios necesarios.
        /// </summary>
        /// <param name="clienteRepository">Repositorio de clientes inyectado por dependencia</param>
        /// <param name="productoRepository">Repositorio de productos inyectado por dependencia</param>
        /// <param name="ordenRepository">Repositorio de órdenes inyectado por dependencia</param>
        public GeneralService(
            ClienteRepository clienteRepository,
            ProductoRepository productoRepository,
            OrdenRepository ordenRepository)
        {
            _clienteRepository = clienteRepository;
            _productoRepository = productoRepository;
            _ordenRepository = ordenRepository;
        }

        #endregion

        #region Clientes

        /// <summary>
        /// Lista todos los clientes registrados en el sistema.
        /// </summary>
        /// <returns>
        /// Colección de clientes. Retorna una lista vacía si ocurre un error.
        /// </returns>
        /// <remarks>
        /// Este método captura las excepciones y retorna una lista vacía en lugar de propagarlas.
        /// </remarks>
        /// <example>
        /// <code>
        /// var clientes = generalService.ListClientes();
        /// foreach (var cliente in clientes)
        /// {
        ///     Console.WriteLine(cliente.Nombre);
        /// }
        /// </code>
        /// </example>
        public IEnumerable<Cliente> ListClientes()
        {
            try
            {
                var list = _clienteRepository.List();
                return list;
            }
            catch (Exception ex)
            {
                return new List<Cliente>();
            }
        }

        /// <summary>
        /// Busca un cliente específico por su información.
        /// </summary>
        /// <param name="item">Objeto cliente con el ClienteId a buscar</param>
        /// <returns>
        /// ServiceResult con:
        /// - Success = true y Data = Cliente si se encuentra
        /// - Success = false y Errors con mensaje si no se encuentra o hay error
        /// </returns>
        /// <example>
        /// <code>
        /// var cliente = new Cliente { ClienteId = 1 };
        /// var resultado = generalService.BuscarCliente(cliente);
        /// if (resultado.Success)
        /// {
        ///     var clienteEncontrado = (Cliente)resultado.Data;
        /// }
        /// </code>
        /// </example>
        public ServiceResult BuscarCliente(Cliente item)
        {
            var result = new ServiceResult();
            try
            {
                var cliente = _clienteRepository.Find(item);

                if (cliente == null)
                {
                    result.Success = false;
                    result.Message = "Cliente no encontrado";
                    result.Errors = new List<string> { "No existe un cliente con el ID especificado" };
                    result.Data = null;
                    return result;
                }

                return result.Ok(cliente);
            }
            catch (Exception ex)
            {
                return result.Error(ex.Message);
            }
        }

        /// <summary>
        /// Inserta un nuevo cliente en el sistema.
        /// </summary>
        /// <param name="item">Objeto cliente con los datos a insertar. El ClienteId debe ser 0.</param>
        /// <returns>
        /// ServiceResult con:
        /// - Success = true, Message = "Cliente creado exitosamente" y Data = Cliente creado si es exitoso
        /// - Success = false y Errors con mensaje de validación o error si falla
        /// </returns>
        /// <remarks>
        /// Validaciones aplicadas:
        /// - El ClienteId debe ser 0 (se genera automáticamente en la BD)
        /// - El nombre debe tener entre 3 y 100 caracteres
        /// - La identidad es requerida y debe tener formato válido (####-####-#####)
        /// - La identidad debe ser única en el sistema
        /// </remarks>
        /// <example>
        /// <code>
        /// var nuevoCliente = new Cliente 
        /// { 
        ///     ClienteId = 0,
        ///     Nombre = "Juan Pérez",
        ///     Identidad = "0801-1990-12345"
        /// };
        /// var resultado = generalService.InsertarCliente(nuevoCliente);
        /// </code>
        /// </example>
        public ServiceResult InsertarCliente(Cliente item)
        {
            var result = new ServiceResult();
            try
            {
                // Validación: clienteId debe ser 0
                if (item.ClienteId != 0)
                {
                    result.Success = false;
                    result.Message = "Error al crear el cliente";
                    result.Errors = new List<string> { "El clienteId debe ser 0 para un nuevo cliente." };
                    result.Data = null;
                    return result;
                }

                var cliente = _clienteRepository.Insert(item);

                if (cliente == null)
                {
                    result.Success = false;
                    result.Message = "Error al crear el cliente";
                    result.Errors = new List<string> { "No se pudo insertar el cliente." };
                    result.Data = null;
                    return result;
                }

                result.Success = true;
                result.Message = "Cliente creado exitosamente";
                result.Data = cliente;
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error al crear el cliente";
                result.Errors = new List<string> { ex.Message };
                result.Data = null;
                return result;
            }
        }

        /// <summary>
        /// Actualiza la información de un cliente existente.
        /// </summary>
        /// <param name="item">Objeto cliente con los datos actualizados. Debe incluir el ClienteId.</param>
        /// <returns>
        /// ServiceResult con:
        /// - Success = true, Message = "Cliente actualizado exitosamente" y Data = Cliente actualizado si es exitoso
        /// - Success = false y Errors con mensaje de error si falla
        /// </returns>
        /// <remarks>
        /// Validaciones aplicadas:
        /// - El cliente debe existir en el sistema
        /// - El nombre debe tener entre 3 y 100 caracteres
        /// - La identidad es requerida y debe tener formato válido (####-####-#####)
        /// - La identidad debe ser única (excepto para el mismo cliente)
        /// </remarks>
        /// <example>
        /// <code>
        /// var clienteActualizado = new Cliente 
        /// { 
        ///     ClienteId = 1,
        ///     Nombre = "Juan Carlos Pérez",
        ///     Identidad = "0801-1990-12345"
        /// };
        /// var resultado = generalService.ActualizarCliente(clienteActualizado);
        /// </code>
        /// </example>
        public ServiceResult ActualizarCliente(Cliente item)
        {
            var result = new ServiceResult();
            try
            {
                var cliente = _clienteRepository.Update(item);

                if (cliente == null)
                {
                    result.Success = false;
                    result.Message = "Error al actualizar el cliente";
                    result.Errors = new List<string> { "No se pudo actualizar el cliente." };
                    result.Data = null;
                    return result;
                }

                result.Success = true;
                result.Message = "Cliente actualizado exitosamente";
                result.Data = cliente;
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error al actualizar el cliente";
                result.Errors = new List<string> { ex.Message };
                result.Data = null;
                return result;
            }
        }

        #endregion

        #region Productos

        /// <summary>
        /// Lista todos los productos disponibles en el sistema.
        /// </summary>
        /// <returns>
        /// Colección de productos. Retorna una lista vacía si ocurre un error.
        /// </returns>
        /// <remarks>
        /// Este método captura las excepciones y retorna una lista vacía en lugar de propagarlas.
        /// </remarks>
        /// <example>
        /// <code>
        /// var productos = generalService.ListProductos();
        /// foreach (var producto in productos)
        /// {
        ///     Console.WriteLine($"{producto.Nombre} - ${producto.Precio}");
        /// }
        /// </code>
        /// </example>
        public IEnumerable<Producto> ListProductos()
        {
            try
            {
                var list = _productoRepository.List();
                return list;
            }
            catch (Exception ex)
            {
                return new List<Producto>();
            }
        }

        /// <summary>
        /// Busca un producto específico por su información.
        /// </summary>
        /// <param name="item">Objeto producto con el ProductoId a buscar</param>
        /// <returns>
        /// ServiceResult con:
        /// - Success = true y Data = Producto si se encuentra
        /// - Success = false y Errors con mensaje si no se encuentra o hay error
        /// </returns>
        /// <example>
        /// <code>
        /// var producto = new Producto { ProductoId = 1 };
        /// var resultado = generalService.BuscarProducto(producto);
        /// if (resultado.Success)
        /// {
        ///     var productoEncontrado = (Producto)resultado.Data;
        /// }
        /// </code>
        /// </example>
        public ServiceResult BuscarProducto(Producto item)
        {
            var result = new ServiceResult();
            try
            {
                var producto = _productoRepository.Find(item);

                if (producto == null)
                {
                    result.Success = false;
                    result.Message = "Producto no encontrado";
                    result.Errors = new List<string> { "No existe un producto con el ID especificado" };
                    result.Data = null;
                    return result;
                }

                return result.Ok(producto);
            }
            catch (Exception ex)
            {
                return result.Error(ex.Message);
            }
        }

        /// <summary>
        /// Inserta un nuevo producto en el sistema.
        /// </summary>
        /// <param name="item">Objeto producto con los datos a insertar. El ProductoId debe ser 0.</param>
        /// <returns>
        /// ServiceResult con:
        /// - Success = true, Message = "Producto creado exitosamente" y Data = Producto creado si es exitoso
        /// - Success = false y Errors con mensaje de validación o error si falla
        /// </returns>
        /// <remarks>
        /// Validaciones aplicadas:
        /// - El ProductoId debe ser 0 (se genera automáticamente en la BD)
        /// - El nombre debe tener entre 3 y 100 caracteres
        /// - La descripción es requerida
        /// - El precio debe ser mayor a 0
        /// - La existencia debe ser mayor o igual a 0
        /// </remarks>
        /// <example>
        /// <code>
        /// var nuevoProducto = new Producto 
        /// { 
        ///     ProductoId = 0,
        ///     Nombre = "Laptop Dell",
        ///     Descripcion = "Laptop gaming 16GB RAM",
        ///     Precio = 25000.00m,
        ///     Existencia = 10
        /// };
        /// var resultado = generalService.InsertarProducto(nuevoProducto);
        /// </code>
        /// </example>
        public ServiceResult InsertarProducto(Producto item)
        {
            var result = new ServiceResult();
            try
            {
                // Validación: productoId debe ser 0
                if (item.ProductoId != 0)
                {
                    result.Success = false;
                    result.Message = "Error al crear el producto";
                    result.Errors = new List<string> { "El productoId debe ser 0 para un nuevo producto." };
                    result.Data = null;
                    return result;
                }

                var producto = _productoRepository.Insert(item);

                if (producto == null)
                {
                    result.Success = false;
                    result.Message = "Error al crear el producto";
                    result.Errors = new List<string> { "No se pudo insertar el producto." };
                    result.Data = null;
                    return result;
                }

                result.Success = true;
                result.Message = "Producto creado exitosamente";
                result.Data = producto;
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error al crear el producto";
                result.Errors = new List<string> { ex.Message };
                result.Data = null;
                return result;
            }
        }

        /// <summary>
        /// Actualiza la información de un producto existente.
        /// </summary>
        /// <param name="item">Objeto producto con los datos actualizados. Debe incluir el ProductoId.</param>
        /// <returns>
        /// ServiceResult con:
        /// - Success = true, Message = "Producto actualizado exitosamente" y Data = Producto actualizado si es exitoso
        /// - Success = false y Errors con mensaje de error si falla
        /// </returns>
        /// <remarks>
        /// Validaciones aplicadas:
        /// - El producto debe existir en el sistema
        /// - El nombre debe tener entre 3 y 100 caracteres
        /// - La descripción es requerida
        /// - El precio debe ser mayor a 0
        /// - La existencia debe ser mayor o igual a 0
        /// </remarks>
        /// <example>
        /// <code>
        /// var productoActualizado = new Producto 
        /// { 
        ///     ProductoId = 1,
        ///     Nombre = "Laptop Dell XPS",
        ///     Descripcion = "Laptop profesional 32GB RAM",
        ///     Precio = 35000.00m,
        ///     Existencia = 5
        /// };
        /// var resultado = generalService.ActualizarProducto(productoActualizado);
        /// </code>
        /// </example>
        public ServiceResult ActualizarProducto(Producto item)
        {
            var result = new ServiceResult();
            try
            {
                var producto = _productoRepository.Update(item);

                if (producto == null)
                {
                    result.Success = false;
                    result.Message = "Error al actualizar el producto";
                    result.Errors = new List<string> { "No se pudo actualizar el producto." };
                    result.Data = null;
                    return result;
                }

                result.Success = true;
                result.Message = "Producto actualizado exitosamente";
                result.Data = producto;
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error al actualizar el producto";
                result.Errors = new List<string> { ex.Message };
                result.Data = null;
                return result;
            }
        }

        #endregion

        #region Ordenes

        /// <summary>
        /// Crea una nueva orden de compra con sus detalles asociados.
        /// </summary>
        /// <param name="clienteId">ID del cliente que realiza la orden</param>
        /// <param name="detallesJson">
        /// String JSON que contiene los detalles de la orden.
        /// Formato: [{"ProductoId": 1, "Cantidad": 2}, {"ProductoId": 3, "Cantidad": 1}]
        /// </param>
        /// <returns>
        /// ServiceResult con:
        /// - Success = true, Message = "Orden creada exitosamente" y Data = Dictionary con "Orden" y "Detalles" si es exitoso
        /// - Success = false y Errors con mensaje de error si falla
        /// </returns>
        /// <remarks>
        /// Este método realiza una transacción completa que incluye:
        /// 1. Validar que el cliente existe
        /// 2. Validar que todos los productos existen
        /// 3. Validar que hay suficiente existencia
        /// 4. Crear la orden
        /// 5. Crear los detalles de la orden
        /// 6. Actualizar las existencias de los productos
        /// 
        /// El Data del ServiceResult contiene un Dictionary con:
        /// - "Orden": Información de la orden creada (OrdenId, ClienteId, Fecha, Total)
        /// - "Detalles": Lista de detalles con información de productos
        /// </remarks>
        /// <exception cref="Exception">
        /// Se captura y se incluye en Errors cuando:
        /// - El cliente no existe
        /// - Algún producto no existe
        /// - No hay suficiente existencia
        /// - El JSON es inválido
        /// - Ocurre un error en la base de datos
        /// </exception>
        /// <example>
        /// <code>
        /// var detallesJson = "[{\"ProductoId\": 1, \"Cantidad\": 2}, {\"ProductoId\": 3, \"Cantidad\": 1}]";
        /// var resultado = generalService.CrearOrden(clienteId: 5, detallesJson);
        /// if (resultado.Success)
        /// {
        ///     var response = (Dictionary&lt;string, IEnumerable&lt;dynamic&gt;&gt;)resultado.Data;
        ///     var orden = response["Orden"].First();
        ///     var detalles = response["Detalles"];
        /// }
        /// </code>
        /// </example>
        public ServiceResult CrearOrden(long clienteId, string detallesJson)
        {
            var result = new ServiceResult();
            try
            {
                // Llamar al repositorio para crear la orden
                var response = _ordenRepository.CrearOrden(clienteId, detallesJson);

                if (response == null)
                {
                    result.Success = false;
                    result.Message = "Error al crear la orden";
                    result.Errors = new List<string> { "No se pudo crear la orden." };
                    result.Data = null;
                    return result;
                }

                result.Success = true;
                result.Message = "Orden creada exitosamente";
                result.Data = response;
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                // Personalizar el mensaje según el tipo de error
                result.Message = ex.Message.Contains("cliente")
                    ? "Error al crear la orden"
                    : "Error al procesar la orden";
                result.Errors = new List<string> { ex.Message };
                result.Data = null;
                return result;
            }
        }

        #endregion
    }
}