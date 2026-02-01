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
    public class ClienteRepository : IRepository<Cliente>
    {
        public RequestStatus Delete(Cliente item)
        {
            throw new NotImplementedException();
        }

        public Cliente Find(int? id)
        {
            throw new NotImplementedException();
        }

        public Cliente Find(Cliente item)
        {
            var parameter = new DynamicParameters();
            parameter.Add("@ClienteId", item.ClienteId, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);

            using var db = new SqlConnection(PruebaTecnicaAPIContext.ConnectionString);
            var result = db.Query<dynamic>(ScriptDatabase.Cliente_Buscar, parameter, commandType: System.Data.CommandType.StoredProcedure).FirstOrDefault();

            // Verificar si es un mensaje de error del SP
            if (result != null && result.code_Status != null)
            {
                // Es un error del SP
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

        public Cliente Insert(Cliente item)
        {
            var parameter = new DynamicParameters();
            parameter.Add("@Nombre", item.Nombre, System.Data.DbType.String, System.Data.ParameterDirection.Input);
            parameter.Add("@Identidad", item.Identidad, System.Data.DbType.String, System.Data.ParameterDirection.Input);

            using var db = new SqlConnection(PruebaTecnicaAPIContext.ConnectionString);
            var result = db.Query<dynamic>(ScriptDatabase.Cliente_Insertar, parameter, commandType: System.Data.CommandType.StoredProcedure).FirstOrDefault();

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

        public Cliente Update(Cliente item)
        {
            var parameter = new DynamicParameters();
            parameter.Add("@ClienteId", item.ClienteId, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
            parameter.Add("@Nombre", item.Nombre, System.Data.DbType.String, System.Data.ParameterDirection.Input);
            parameter.Add("@Identidad", item.Identidad, System.Data.DbType.String, System.Data.ParameterDirection.Input);

            using var db = new SqlConnection(PruebaTecnicaAPIContext.ConnectionString);
            var result = db.Query<dynamic>(ScriptDatabase.Cliente_Actualizar, parameter, commandType: System.Data.CommandType.StoredProcedure).FirstOrDefault();

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

        public IEnumerable<Cliente> List()
        {
            var parameter = new DynamicParameters();

            using var db = new SqlConnection(PruebaTecnicaAPIContext.ConnectionString);
            var result = db.Query<Cliente>(ScriptDatabase.Clientes_Listar, parameter, commandType: System.Data.CommandType.StoredProcedure).ToList();
            return result;

        }


        IEnumerable<Cliente> IRepository<Cliente>.Find(Cliente item)
        {
            throw new NotImplementedException();
        }

        RequestStatus IRepository<Cliente>.Insert(Cliente item)
        {
            throw new NotImplementedException();
        }

        RequestStatus IRepository<Cliente>.Update(Cliente item)
        {
            throw new NotImplementedException();
        }
    }
}
