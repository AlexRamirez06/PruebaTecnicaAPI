using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using PruebaTecnicaAPI.Entities.Entities;
using Microsoft.Data.SqlClient;
using PruebaTecnicaAPI.DataAccess.Context;

namespace PruebaTecnicaAPI.DataAccess.Repositories
{
    /// <summary>
    /// Repositorio para gestionar las operaciones de acceso a datos de Productos.
    /// Implementa la interfaz IRepository para mantener la consistencia con otros repositorios.
    /// Utiliza Dapper para ejecutar procedimientos almacenados y mapear resultados.
    /// </summary>
    public class ProductoRepository : IRepository<Producto>
    {
        #region Métodos No Implementados de IRepository

        /// <summary>
        /// Elimina un producto (No implementado).
        /// </summary>
        /// <param name="item">El producto a eliminar</param>
        /// <returns>Estado de la solicitud</returns>
        /// <exception cref="NotImplementedException">Este método no está implementado</exception>
        public RequestStatus Delete(Producto item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Busca un producto por ID (No implementado - versión nullable).
        /// </summary>
        /// <param name="id">ID del producto a buscar</param>
        /// <returns>El producto encontrado</returns>
        /// <exception cref="NotImplementedException">Este método no está implementado</exception>
        public Producto Find(int? id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Busca productos que coincidan con los criterios (No implementado - implementación de interfaz).
        /// </summary>
        /// <param name="item">Criterios de búsqueda</param>
        /// <returns>Colección de productos encontrados</returns>
        /// <exception cref="NotImplementedException">Este método no está implementado</exception>
        IEnumerable<Producto> IRepository<Producto>.Find(Producto item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Inserta un nuevo producto (No implementado - implementación de interfaz).
        /// Usar el método Insert(Producto item) que retorna Producto.
        /// </summary>
        /// <param name="item">El producto a insertar</param>
        /// <returns>Estado de la solicitud</returns>
        /// <exception cref="NotImplementedException">Este método no está implementado</exception>
        RequestStatus IRepository<Producto>.Insert(Producto item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Actualiza un producto existente (No implementado - implementación de interfaz).
        /// Usar el método Update(Producto item) que retorna Producto.
        /// </summary>
        /// <param name="item">El producto a actualizar</param>
        /// <returns>Estado de la solicitud</returns>
        /// <exception cref="NotImplementedException">Este método no está implementado</exception>
        RequestStatus IRepository<Producto>.Update(Producto item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Métodos Implementados

        /// <summary>
        /// Busca un producto específico por su ID mediante procedimiento almacenado.
        /// </summary>
        /// <param name="item">Objeto Producto con el ProductoId a buscar</param>
        /// <returns>
        /// Producto encontrado con todos sus datos, o null si:
        /// - No existe un producto con ese ID
        /// - El procedimiento almacenado retorna un error
        /// </returns>
        /// <remarks>
        /// Ejecuta el procedimiento almacenado SP_Productos_Buscar.
        /// Si el SP retorna un objeto con code_Status (error), retorna null.
        /// Mapea dinámicamente los campos: ProductoId, Nombre, Descripcion, Precio, Existencia.
        /// </remarks>
        /// <example>
        /// <code>
        /// var producto = new Producto { ProductoId = 1 };
        /// var resultado = repository.Find(producto);
        /// if (resultado != null)
        /// {
        ///     Console.WriteLine($"{resultado.Nombre} - ${resultado.Precio}");
        /// }
        /// </code>
        /// </example>
        public Producto Find(Producto item)
        {
            // Configurar parámetros para el procedimiento almacenado
            var parameter = new DynamicParameters();
            parameter.Add("@ProductoId", item.ProductoId, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);

            // Establecer conexión a la base de datos
            using var db = new SqlConnection(PruebaTecnicaAPIContext.ConnectionString);

            // Ejecutar el procedimiento almacenado y obtener el primer resultado
            var result = db.Query<dynamic>(
                ScriptDatabase.Producto_Buscar,
                parameter,
                commandType: System.Data.CommandType.StoredProcedure
            ).FirstOrDefault();

            // Verificar si es un mensaje de error del SP
            if (result != null && result.code_Status != null)
            {
                // Es un error del SP (producto no encontrado o error de BD)
                return null;
            }

            // Mapear el resultado dinámico a Producto
            if (result != null)
            {
                return new Producto
                {
                    ProductoId = result.ProductoId,
                    Nombre = result.Nombre,
                    Descripcion = result.Descripcion,
                    Precio = result.Precio,
                    Existencia = result.Existencia
                };
            }

            return null;
        }

        /// <summary>
        /// Inserta un nuevo producto en la base de datos mediante procedimiento almacenado.
        /// </summary>
        /// <param name="item">
        /// Objeto Producto con los datos a insertar:
        /// - Nombre: Nombre del producto (3-100 caracteres)
        /// - Descripcion: Descripción del producto (requerida)
        /// - Precio: Precio unitario (debe ser mayor a 0, con 2 decimales)
        /// - Existencia: Cantidad en stock (debe ser mayor o igual a 0)
        /// </param>
        /// <returns>
        /// Producto insertado con el ProductoId generado automáticamente, o null si falla.
        /// </returns>
        /// <exception cref="Exception">
        /// Se lanza cuando:
        /// - El nombre no cumple con las validaciones (longitud)
        /// - La descripción está vacía
        /// - El precio es menor o igual a 0
        /// - La existencia es negativa
        /// - Ocurre un error en la base de datos
        /// La excepción contiene el message_Status del procedimiento almacenado.
        /// </exception>
        /// <remarks>
        /// Ejecuta el procedimiento almacenado SP_Productos_Insertar.
        /// El ProductoId se genera automáticamente en la BD usando SCOPE_IDENTITY().
        /// Si el SP retorna un objeto con code_Status, lanza una excepción con el mensaje de error.
        /// Mapea dinámicamente los campos retornados: ProductoId, Nombre, Descripcion, Precio, Existencia.
        /// El precio se almacena con precisión decimal de 2 dígitos.
        /// </remarks>
        /// <example>
        /// <code>
        /// var nuevoProducto = new Producto
        /// {
        ///     Nombre = "Laptop Dell",
        ///     Descripcion = "Laptop gaming 16GB RAM",
        ///     Precio = 25000.00m,
        ///     Existencia = 10
        /// };
        /// try
        /// {
        ///     var productoCreado = repository.Insert(nuevoProducto);
        ///     Console.WriteLine($"Producto creado con ID: {productoCreado.ProductoId}");
        /// }
        /// catch (Exception ex)
        /// {
        ///     Console.WriteLine($"Error: {ex.Message}");
        /// }
        /// </code>
        /// </example>
        public Producto Insert(Producto item)
        {
            // Configurar parámetros para el procedimiento almacenado
            var parameter = new DynamicParameters();
            parameter.Add("@Nombre", item.Nombre, System.Data.DbType.String, System.Data.ParameterDirection.Input);
            parameter.Add("@Descripcion", item.Descripcion, System.Data.DbType.String, System.Data.ParameterDirection.Input);
            parameter.Add("@Precio", item.Precio, System.Data.DbType.Decimal, System.Data.ParameterDirection.Input);
            parameter.Add("@Existencia", item.Existencia, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);

            // Establecer conexión a la base de datos
            using var db = new SqlConnection(PruebaTecnicaAPIContext.ConnectionString);

            // Ejecutar el procedimiento almacenado y obtener el primer resultado
            var result = db.Query<dynamic>(
                ScriptDatabase.Producto_Insertar,
                parameter,
                commandType: System.Data.CommandType.StoredProcedure
            ).FirstOrDefault();

            // Verificar si es un mensaje de error del SP
            if (result != null && result.code_Status != null)
            {
                // Es un error del SP, lanzar excepción con el mensaje
                throw new Exception(result.message_Status);
            }

            // Mapear el resultado dinámico a Producto
            if (result != null)
            {
                return new Producto
                {
                    ProductoId = result.ProductoId,
                    Nombre = result.Nombre,
                    Descripcion = result.Descripcion,
                    Precio = result.Precio,
                    Existencia = result.Existencia
                };
            }

            return null;
        }

        /// <summary>
        /// Actualiza los datos de un producto existente en la base de datos mediante procedimiento almacenado.
        /// </summary>
        /// <param name="item">
        /// Objeto Producto con los datos actualizados:
        /// - ProductoId: ID del producto a actualizar (debe existir)
        /// - Nombre: Nuevo nombre del producto (3-100 caracteres)
        /// - Descripcion: Nueva descripción del producto (requerida)
        /// - Precio: Nuevo precio unitario (debe ser mayor a 0, con 2 decimales)
        /// - Existencia: Nueva cantidad en stock (debe ser mayor o igual a 0)
        /// </param>
        /// <returns>
        /// Producto actualizado con los nuevos datos, o null si falla.
        /// </returns>
        /// <exception cref="Exception">
        /// Se lanza cuando:
        /// - El producto no existe en el sistema
        /// - El nombre no cumple con las validaciones (longitud)
        /// - La descripción está vacía
        /// - El precio es menor o igual a 0
        /// - La existencia es negativa
        /// - Ocurre un error en la base de datos
        /// La excepción contiene el message_Status del procedimiento almacenado.
        /// </exception>
        /// <remarks>
        /// Ejecuta el procedimiento almacenado SP_Productos_Actualizar.
        /// Si el SP retorna un objeto con code_Status, lanza una excepción con el mensaje de error.
        /// Mapea dinámicamente los campos retornados: ProductoId, Nombre, Descripcion, Precio, Existencia.
        /// Útil para actualizar precios, ajustar inventario o modificar descripciones de productos.
        /// El precio se almacena con precisión decimal de 2 dígitos.
        /// </remarks>
        /// <example>
        /// <code>
        /// var productoActualizado = new Producto
        /// {
        ///     ProductoId = 1,
        ///     Nombre = "Laptop Dell XPS 15",
        ///     Descripcion = "Laptop profesional 32GB RAM, 1TB SSD",
        ///     Precio = 35000.00m,
        ///     Existencia = 5
        /// };
        /// try
        /// {
        ///     var resultado = repository.Update(productoActualizado);
        ///     Console.WriteLine($"Producto actualizado: {resultado.Nombre}");
        /// }
        /// catch (Exception ex)
        /// {
        ///     Console.WriteLine($"Error: {ex.Message}");
        /// }
        /// </code>
        /// </example>
        public Producto Update(Producto item)
        {
            // Configurar parámetros para el procedimiento almacenado
            var parameter = new DynamicParameters();
            parameter.Add("@ProductoId", item.ProductoId, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
            parameter.Add("@Nombre", item.Nombre, System.Data.DbType.String, System.Data.ParameterDirection.Input);
            parameter.Add("@Descripcion", item.Descripcion, System.Data.DbType.String, System.Data.ParameterDirection.Input);
            parameter.Add("@Precio", item.Precio, System.Data.DbType.Decimal, System.Data.ParameterDirection.Input);
            parameter.Add("@Existencia", item.Existencia, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);

            // Establecer conexión a la base de datos
            using var db = new SqlConnection(PruebaTecnicaAPIContext.ConnectionString);

            // Ejecutar el procedimiento almacenado y obtener el primer resultado
            var result = db.Query<dynamic>(
                ScriptDatabase.Producto_Actualizar,
                parameter,
                commandType: System.Data.CommandType.StoredProcedure
            ).FirstOrDefault();

            // Verificar si es un mensaje de error del SP
            if (result != null && result.code_Status != null)
            {
                // Es un error del SP, lanzar excepción con el mensaje
                throw new Exception(result.message_Status);
            }

            // Mapear el resultado dinámico a Producto
            if (result != null)
            {
                return new Producto
                {
                    ProductoId = result.ProductoId,
                    Nombre = result.Nombre,
                    Descripcion = result.Descripcion,
                    Precio = result.Precio,
                    Existencia = result.Existencia
                };
            }

            return null;
        }

        /// <summary>
        /// Lista todos los productos registrados en el sistema mediante procedimiento almacenado.
        /// </summary>
        /// <returns>
        /// Colección de productos con todos sus datos (nombre, descripción, precio, existencia).
        /// Retorna una lista vacía si no hay productos registrados.
        /// </returns>
        /// <remarks>
        /// Ejecuta el procedimiento almacenado SP_Productos_Listar.
        /// No requiere parámetros.
        /// Mapea automáticamente los resultados a objetos Producto usando Dapper.
        /// Este método no lanza excepciones; si ocurre un error en el SP, retorna lista vacía.
        /// Útil para mostrar catálogos de productos o generar reportes de inventario.
        /// </remarks>
        /// <example>
        /// <code>
        /// var productos = repository.List();
        /// foreach (var producto in productos)
        /// {
        ///     Console.WriteLine($"{producto.ProductoId} - {producto.Nombre} - ${producto.Precio} - Stock: {producto.Existencia}");
        /// }
        /// </code>
        /// </example>
        public IEnumerable<Producto> List()
        {
            // Configurar parámetros vacíos (el SP no requiere parámetros)
            var parameter = new DynamicParameters();

            // Establecer conexión a la base de datos
            using var db = new SqlConnection(PruebaTecnicaAPIContext.ConnectionString);

            // Ejecutar el procedimiento almacenado y mapear a lista de Producto
            var result = db.Query<Producto>(
                ScriptDatabase.Productos_Listar,
                parameter,
                commandType: System.Data.CommandType.StoredProcedure
            ).ToList();

            return result;
        }

        #endregion
    }
}