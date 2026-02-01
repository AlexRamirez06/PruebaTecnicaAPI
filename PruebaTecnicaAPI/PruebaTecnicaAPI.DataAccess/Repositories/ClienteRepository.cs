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
    /// Repositorio para gestionar las operaciones de acceso a datos de Clientes.
    /// Implementa la interfaz IRepository para mantener la consistencia con otros repositorios.
    /// Utiliza Dapper para ejecutar procedimientos almacenados y mapear resultados.
    /// </summary>
    public class ClienteRepository : IRepository<Cliente>
    {
        #region Métodos No Implementados de IRepository

        /// <summary>
        /// Elimina un cliente (No implementado).
        /// </summary>
        /// <param name="item">El cliente a eliminar</param>
        /// <returns>Estado de la solicitud</returns>
        /// <exception cref="NotImplementedException">Este método no está implementado</exception>
        public RequestStatus Delete(Cliente item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Busca un cliente por ID (No implementado - versión nullable).
        /// </summary>
        /// <param name="id">ID del cliente a buscar</param>
        /// <returns>El cliente encontrado</returns>
        /// <exception cref="NotImplementedException">Este método no está implementado</exception>
        public Cliente Find(int? id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Busca clientes que coincidan con los criterios (No implementado - implementación de interfaz).
        /// </summary>
        /// <param name="item">Criterios de búsqueda</param>
        /// <returns>Colección de clientes encontrados</returns>
        /// <exception cref="NotImplementedException">Este método no está implementado</exception>
        IEnumerable<Cliente> IRepository<Cliente>.Find(Cliente item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Inserta un nuevo cliente (No implementado - implementación de interfaz).
        /// Usar el método Insert(Cliente item) que retorna Cliente.
        /// </summary>
        /// <param name="item">El cliente a insertar</param>
        /// <returns>Estado de la solicitud</returns>
        /// <exception cref="NotImplementedException">Este método no está implementado</exception>
        RequestStatus IRepository<Cliente>.Insert(Cliente item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Actualiza un cliente existente (No implementado - implementación de interfaz).
        /// Usar el método Update(Cliente item) que retorna Cliente.
        /// </summary>
        /// <param name="item">El cliente a actualizar</param>
        /// <returns>Estado de la solicitud</returns>
        /// <exception cref="NotImplementedException">Este método no está implementado</exception>
        RequestStatus IRepository<Cliente>.Update(Cliente item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Métodos Implementados

        /// <summary>
        /// Busca un cliente específico por su ID mediante procedimiento almacenado.
        /// </summary>
        /// <param name="item">Objeto Cliente con el ClienteId a buscar</param>
        /// <returns>
        /// Cliente encontrado con todos sus datos, o null si:
        /// - No existe un cliente con ese ID
        /// - El procedimiento almacenado retorna un error
        /// </returns>
        /// <remarks>
        /// Ejecuta el procedimiento almacenado SP_Clientes_Buscar.
        /// Si el SP retorna un objeto con code_Status (error), retorna null.
        /// Mapea dinámicamente los campos: ClienteId, Nombre, Identidad.
        /// </remarks>
        /// <example>
        /// <code>
        /// var cliente = new Cliente { ClienteId = 1 };
        /// var resultado = repository.Find(cliente);
        /// if (resultado != null)
        /// {
        ///     Console.WriteLine(resultado.Nombre);
        /// }
        /// </code>
        /// </example>
        public Cliente Find(Cliente item)
        {
            // Configurar parámetros para el procedimiento almacenado
            var parameter = new DynamicParameters();
            parameter.Add("@ClienteId", item.ClienteId, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);

            // Establecer conexión a la base de datos
            using var db = new SqlConnection(PruebaTecnicaAPIContext.ConnectionString);

            // Ejecutar el procedimiento almacenado y obtener el primer resultado
            var result = db.Query<dynamic>(
                ScriptDatabase.Cliente_Buscar,
                parameter,
                commandType: System.Data.CommandType.StoredProcedure
            ).FirstOrDefault();

            // Verificar si es un mensaje de error del SP
            if (result != null && result.code_Status != null)
            {
                // Es un error del SP (cliente no encontrado o error de BD)
                return null;
            }

            // Mapear el resultado dinámico a Cliente
            if (result != null)
            {
                return new Cliente
                {
                    ClienteId = result.ClienteId,
                    Nombre = result.Nombre,
                    Identidad = result.Identidad
                };
            }

            return null;
        }

        /// <summary>
        /// Inserta un nuevo cliente en la base de datos mediante procedimiento almacenado.
        /// </summary>
        /// <param name="item">
        /// Objeto Cliente con los datos a insertar:
        /// - Nombre: Nombre del cliente (3-100 caracteres)
        /// - Identidad: Número de identidad único (formato ####-####-#####)
        /// </param>
        /// <returns>
        /// Cliente insertado con el ClienteId generado automáticamente, o null si falla.
        /// </returns>
        /// <exception cref="Exception">
        /// Se lanza cuando:
        /// - El nombre no cumple con las validaciones (longitud)
        /// - La identidad no tiene formato válido
        /// - La identidad ya existe en el sistema (duplicada)
        /// - Ocurre un error en la base de datos
        /// La excepción contiene el message_Status del procedimiento almacenado.
        /// </exception>
        /// <remarks>
        /// Ejecuta el procedimiento almacenado SP_Clientes_Insertar.
        /// El ClienteId se genera automáticamente en la BD usando SCOPE_IDENTITY().
        /// Si el SP retorna un objeto con code_Status, lanza una excepción con el mensaje de error.
        /// Mapea dinámicamente los campos retornados: ClienteId, Nombre, Identidad.
        /// </remarks>
        /// <example>
        /// <code>
        /// var nuevoCliente = new Cliente
        /// {
        ///     Nombre = "Juan Pérez",
        ///     Identidad = "0801-1990-12345"
        /// };
        /// try
        /// {
        ///     var clienteCreado = repository.Insert(nuevoCliente);
        ///     Console.WriteLine($"Cliente creado con ID: {clienteCreado.ClienteId}");
        /// }
        /// catch (Exception ex)
        /// {
        ///     Console.WriteLine($"Error: {ex.Message}");
        /// }
        /// </code>
        /// </example>
        public Cliente Insert(Cliente item)
        {
            // Configurar parámetros para el procedimiento almacenado
            var parameter = new DynamicParameters();
            parameter.Add("@Nombre", item.Nombre, System.Data.DbType.String, System.Data.ParameterDirection.Input);
            parameter.Add("@Identidad", item.Identidad, System.Data.DbType.String, System.Data.ParameterDirection.Input);

            // Establecer conexión a la base de datos
            using var db = new SqlConnection(PruebaTecnicaAPIContext.ConnectionString);

            // Ejecutar el procedimiento almacenado y obtener el primer resultado
            var result = db.Query<dynamic>(
                ScriptDatabase.Cliente_Insertar,
                parameter,
                commandType: System.Data.CommandType.StoredProcedure
            ).FirstOrDefault();

            // Verificar si es un mensaje de error del SP
            if (result != null && result.code_Status != null)
            {
                // Es un error del SP, lanzar excepción con el mensaje
                throw new Exception(result.message_Status);
            }

            // Mapear el resultado dinámico a Cliente
            if (result != null)
            {
                return new Cliente
                {
                    ClienteId = result.ClienteId,
                    Nombre = result.Nombre,
                    Identidad = result.Identidad
                };
            }

            return null;
        }

        /// <summary>
        /// Actualiza los datos de un cliente existente en la base de datos mediante procedimiento almacenado.
        /// </summary>
        /// <param name="item">
        /// Objeto Cliente con los datos actualizados:
        /// - ClienteId: ID del cliente a actualizar (debe existir)
        /// - Nombre: Nuevo nombre del cliente (3-100 caracteres)
        /// - Identidad: Nueva identidad del cliente (formato ####-####-#####)
        /// </param>
        /// <returns>
        /// Cliente actualizado con los nuevos datos, o null si falla.
        /// </returns>
        /// <exception cref="Exception">
        /// Se lanza cuando:
        /// - El cliente no existe en el sistema
        /// - El nombre no cumple con las validaciones (longitud)
        /// - La identidad no tiene formato válido
        /// - La identidad ya existe para otro cliente (duplicada)
        /// - Ocurre un error en la base de datos
        /// La excepción contiene el message_Status del procedimiento almacenado.
        /// </exception>
        /// <remarks>
        /// Ejecuta el procedimiento almacenado SP_Clientes_Actualizar.
        /// Si el SP retorna un objeto con code_Status, lanza una excepción con el mensaje de error.
        /// Mapea dinámicamente los campos retornados: ClienteId, Nombre, Identidad.
        /// La identidad puede ser la misma que tenía el cliente (no se marca como duplicada).
        /// </remarks>
        /// <example>
        /// <code>
        /// var clienteActualizado = new Cliente
        /// {
        ///     ClienteId = 1,
        ///     Nombre = "Juan Carlos Pérez",
        ///     Identidad = "0801-1990-12345"
        /// };
        /// try
        /// {
        ///     var resultado = repository.Update(clienteActualizado);
        ///     Console.WriteLine($"Cliente actualizado: {resultado.Nombre}");
        /// }
        /// catch (Exception ex)
        /// {
        ///     Console.WriteLine($"Error: {ex.Message}");
        /// }
        /// </code>
        /// </example>
        public Cliente Update(Cliente item)
        {
            // Configurar parámetros para el procedimiento almacenado
            var parameter = new DynamicParameters();
            parameter.Add("@ClienteId", item.ClienteId, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
            parameter.Add("@Nombre", item.Nombre, System.Data.DbType.String, System.Data.ParameterDirection.Input);
            parameter.Add("@Identidad", item.Identidad, System.Data.DbType.String, System.Data.ParameterDirection.Input);

            // Establecer conexión a la base de datos
            using var db = new SqlConnection(PruebaTecnicaAPIContext.ConnectionString);

            // Ejecutar el procedimiento almacenado y obtener el primer resultado
            var result = db.Query<dynamic>(
                ScriptDatabase.Cliente_Actualizar,
                parameter,
                commandType: System.Data.CommandType.StoredProcedure
            ).FirstOrDefault();

            // Verificar si es un mensaje de error del SP
            if (result != null && result.code_Status != null)
            {
                // Es un error del SP, lanzar excepción con el mensaje
                throw new Exception(result.message_Status);
            }

            // Mapear el resultado dinámico a Cliente
            if (result != null)
            {
                return new Cliente
                {
                    ClienteId = result.ClienteId,
                    Nombre = result.Nombre,
                    Identidad = result.Identidad
                };
            }

            return null;
        }

        /// <summary>
        /// Lista todos los clientes registrados en el sistema mediante procedimiento almacenado.
        /// </summary>
        /// <returns>
        /// Colección de clientes con todos sus datos.
        /// Retorna una lista vacía si no hay clientes registrados.
        /// </returns>
        /// <remarks>
        /// Ejecuta el procedimiento almacenado SP_Clientes_Listar.
        /// No requiere parámetros.
        /// Mapea automáticamente los resultados a objetos Cliente usando Dapper.
        /// Este método no lanza excepciones; si ocurre un error en el SP, retorna lista vacía.
        /// </remarks>
        /// <example>
        /// <code>
        /// var clientes = repository.List();
        /// foreach (var cliente in clientes)
        /// {
        ///     Console.WriteLine($"{cliente.ClienteId} - {cliente.Nombre}");
        /// }
        /// </code>
        /// </example>
        public IEnumerable<Cliente> List()
        {
            // Configurar parámetros vacíos (el SP no requiere parámetros)
            var parameter = new DynamicParameters();

            // Establecer conexión a la base de datos
            using var db = new SqlConnection(PruebaTecnicaAPIContext.ConnectionString);

            // Ejecutar el procedimiento almacenado y mapear a lista de Cliente
            var result = db.Query<Cliente>(
                ScriptDatabase.Clientes_Listar,
                parameter,
                commandType: System.Data.CommandType.StoredProcedure
            ).ToList();

            return result;
        }

        #endregion
    }
}