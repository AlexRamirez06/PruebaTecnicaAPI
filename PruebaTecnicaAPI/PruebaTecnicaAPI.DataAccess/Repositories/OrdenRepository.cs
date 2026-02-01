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
    public class OrdenRepository : IRepository<Orden>
    {
        public RequestStatus Delete(Orden item)
        {
            throw new NotImplementedException();
        }

        public Orden Find(int? id)
        {
            throw new NotImplementedException();
        }

        IEnumerable<Orden> IRepository<Orden>.Find(Orden item)
        {
            throw new NotImplementedException();
        }

        RequestStatus IRepository<Orden>.Insert(Orden item)
        {
            throw new NotImplementedException();
        }

        RequestStatus IRepository<Orden>.Update(Orden item)
        {
            throw new NotImplementedException();
        }

        IEnumerable<Orden> List()
        {
            throw new NotImplementedException();
        }

        // Método específico para crear orden con detalles
        // Retorna dynamic porque necesita dos result sets y no puede usar ViewModels aquí
        public Dictionary<string, IEnumerable<dynamic>> CrearOrden(long clienteId, string detallesJson)
        {
            var parameter = new DynamicParameters();
            parameter.Add("@ClienteId", clienteId, System.Data.DbType.Int64, System.Data.ParameterDirection.Input);
            parameter.Add("@Detalles", detallesJson, System.Data.DbType.String, System.Data.ParameterDirection.Input);

            using var db = new SqlConnection(PruebaTecnicaAPIContext.ConnectionString);
            using var multi = db.QueryMultiple(ScriptDatabase.Ordenes_Crear, parameter, commandType: System.Data.CommandType.StoredProcedure);

            // Primer result set
            var errorCheck = multi.Read<dynamic>().FirstOrDefault();
            if (errorCheck != null && errorCheck.code_Status != null)
            {
                throw new Exception(errorCheck.message_Status);
            }

            // Segundo result set: detalles
            var detalles = multi.Read<dynamic>().ToList();

            var resultado = new Dictionary<string, IEnumerable<dynamic>>();
            resultado["Orden"] = new List<dynamic> { errorCheck };
            resultado["Detalles"] = detalles;

            return resultado;
        }

        IEnumerable<Orden> IRepository<Orden>.List()
        {
            throw new NotImplementedException();
        }
    }
}