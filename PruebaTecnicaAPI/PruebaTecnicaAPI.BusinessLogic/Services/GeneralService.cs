using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PruebaTecnicaAPI.DataAccess.Repositories;
using PruebaTecnicaAPI.Entities.Entities;


namespace PruebaTecnicaAPI.BusinessLogic.Services
{
    public class GeneralService
    {
        public ClienteRepository _clienteRepository { get; }
        public ProductoRepository _productoRepository { get; }
        public OrdenRepository _ordenRepository { get; }

        public GeneralService(ClienteRepository clienteRepository, ProductoRepository productoRepository, OrdenRepository ordenRepository)
        {
            _clienteRepository = clienteRepository;
            _productoRepository = productoRepository;
            _ordenRepository = ordenRepository;
        }


        #region Clientes

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

        public ServiceResult CrearOrden(long clienteId, string detallesJson)
        {
            var result = new ServiceResult();
            try
            {
                // Llamar al repositorio
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
                result.Message = ex.Message.Contains("cliente") ? "Error al crear la orden" : "Error al procesar la orden";
                result.Errors = new List<string> { ex.Message };
                result.Data = null;
                return result;
            }
        }

        #endregion
    }
}
