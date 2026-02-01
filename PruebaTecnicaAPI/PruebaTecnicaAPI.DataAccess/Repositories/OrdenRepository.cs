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
    /// Repositorio para gestionar las operaciones de acceso a datos de Órdenes.
    /// Implementa la interfaz IRepository para mantener la consistencia con otros repositorios.
    /// </summary>
    public class OrdenRepository : IRepository<Orden>
    {
        #region Métodos No Implementados de IRepository

        /// <summary>
        /// Elimina una orden (No implementado).
        /// </summary>
        /// <param name="item">La orden a eliminar</param>
        /// <returns>Estado de la solicitud</returns>
        /// <exception cref="NotImplementedException">Este método no está implementado</exception>
        public RequestStatus Delete(Orden item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Busca una orden por ID (No implementado).
        /// </summary>
        /// <param name="id">ID de la orden a buscar</param>
        /// <returns>La orden encontrada</returns>
        /// <exception cref="NotImplementedException">Este método no está implementado</exception>
        public Orden Find(int? id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Busca órdenes que coincidan con los criterios (No implementado).
        /// </summary>
        /// <param name="item">Criterios de búsqueda</param>
        /// <returns>Colección de órdenes encontradas</returns>
        /// <exception cref="NotImplementedException">Este método no está implementado</exception>
        IEnumerable<Orden> IRepository<Orden>.Find(Orden item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Inserta una nueva orden (No implementado).
        /// Usar el método CrearOrden para insertar órdenes con detalles.
        /// </summary>
        /// <param name="item">La orden a insertar</param>
        /// <returns>Estado de la solicitud</returns>
        /// <exception cref="NotImplementedException">Este método no está implementado</exception>
        RequestStatus IRepository<Orden>.Insert(Orden item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Actualiza una orden existente (No implementado).
        /// </summary>
        /// <param name="item">La orden a actualizar</param>
        /// <returns>Estado de la solicitud</returns>
        /// <exception cref="NotImplementedException">Este método no está implementado</exception>
        RequestStatus IRepository<Orden>.Update(Orden item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Lista todas las órdenes (No implementado - método privado).
        /// </summary>
        /// <returns>Colección de órdenes</returns>
        /// <exception cref="NotImplementedException">Este método no está implementado</exception>
        IEnumerable<Orden> List()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Lista todas las órdenes (No implementado - implementación de interfaz).
        /// </summary>
        /// <returns>Colección de órdenes</returns>
        /// <exception cref="NotImplementedException">Este método no está implementado</exception>
        IEnumerable<Orden> IRepository<Orden>.List()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Métodos Implementados

        /// <summary>
        /// Crea una nueva orden con sus detalles asociados mediante un procedimiento almacenado.
        /// Este método maneja la transacción completa de creación de orden y sus líneas de detalle.
        /// </summary>
        /// <param name="clienteId">ID del cliente que realiza la orden</param>
        /// <param name="detallesJson">
        /// String JSON que contiene los detalles de la orden.
        /// Formato esperado: [{"ProductoId": 1, "Cantidad": 2}, {"ProductoId": 3, "Cantidad": 1}]
        /// </param>
        /// <returns>
        /// Diccionario con dos llaves:
        /// - "Orden": Colección con un solo elemento que contiene los datos de la orden creada
        /// - "Detalles": Colección con los detalles de la orden (productos, cantidades, precios)
        /// </returns>
        /// <exception cref="Exception">
        /// Se lanza cuando:
        /// - El cliente no existe
        /// - Algún producto no existe
        /// - No hay suficiente existencia de algún producto
        /// - El JSON de detalles es inválido
        /// - Ocurre un error en la base de datos
        /// </exception>
        /// <remarks>
        /// El método utiliza QueryMultiple de Dapper para procesar dos result sets:
        /// 1. Primer result set: Información de la orden creada o mensaje de error
        /// 2. Segundo result set: Lista de detalles de la orden con información de productos
        /// 
        /// Retorna dynamic porque necesita manejar múltiples estructuras de datos
        /// y no puede usar ViewModels en la capa de repositorio.
        /// </remarks>
        /// <example>
        /// Uso del método:
        /// <code>
        /// var detallesJson = "[{\"ProductoId\": 1, \"Cantidad\": 2}, {\"ProductoId\": 3, \"Cantidad\": 1}]";
        /// var resultado = repository.CrearOrden(clienteId: 5, detallesJson);
        /// var orden = resultado["Orden"].First();
        /// var detalles = resultado["Detalles"];
        /// </code>
        /// </example>
        public Dictionary<string, IEnumerable<dynamic>> CrearOrden(long clienteId, string detallesJson)
        {
            // Configurar parámetros para el procedimiento almacenado
            var parameter = new DynamicParameters();
            parameter.Add("@ClienteId", clienteId, System.Data.DbType.Int64, System.Data.ParameterDirection.Input);
            parameter.Add("@Detalles", detallesJson, System.Data.DbType.String, System.Data.ParameterDirection.Input);

            // Establecer conexión a la base de datos
            using var db = new SqlConnection(PruebaTecnicaAPIContext.ConnectionString);

            // Ejecutar el procedimiento almacenado que retorna múltiples result sets
            using var multi = db.QueryMultiple(
                ScriptDatabase.Ordenes_Crear,
                parameter,
                commandType: System.Data.CommandType.StoredProcedure
            );

            // Leer el primer result set: orden creada o mensaje de error
            var errorCheck = multi.Read<dynamic>().FirstOrDefault();

            // Verificar si el procedimiento almacenado retornó un error
            if (errorCheck != null && errorCheck.code_Status != null)
            {
                // Lanzar excepción con el mensaje de error del SP
                throw new Exception(errorCheck.message_Status);
            }

            // Leer el segundo result set: detalles de la orden
            var detalles = multi.Read<dynamic>().ToList();

            // Construir el diccionario de respuesta
            var resultado = new Dictionary<string, IEnumerable<dynamic>>();
            resultado["Orden"] = new List<dynamic> { errorCheck };
            resultado["Detalles"] = detalles;

            return resultado;
        }

        #endregion
    }
}