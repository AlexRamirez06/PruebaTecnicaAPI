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
    public class ProductoRepository : IRepository<Producto>
    {
        public RequestStatus Delete(Producto item)
        {
            throw new NotImplementedException();
        }

        public Producto Find(int? id)
        {
            throw new NotImplementedException();
        }

        public Producto Find(Producto item)
        {
            var parameter = new DynamicParameters();
            parameter.Add("@ProductoId", item.ProductoId, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);

            using var db = new SqlConnection(PruebaTecnicaAPIContext.ConnectionString);
            var result = db.Query<dynamic>(ScriptDatabase.Producto_Buscar, parameter, commandType: System.Data.CommandType.StoredProcedure).FirstOrDefault();

            // Verificar si es un mensaje de error del SP
            if (result != null && result.code_Status != null)
            {
                // Es un error del SP
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

        public Producto Insert(Producto item)
        {
            var parameter = new DynamicParameters();
            parameter.Add("@Nombre", item.Nombre, System.Data.DbType.String, System.Data.ParameterDirection.Input);
            parameter.Add("@Descripcion", item.Descripcion, System.Data.DbType.String, System.Data.ParameterDirection.Input);
            parameter.Add("@Precio", item.Precio, System.Data.DbType.Decimal, System.Data.ParameterDirection.Input);
            parameter.Add("@Existencia", item.Existencia, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);

            using var db = new SqlConnection(PruebaTecnicaAPIContext.ConnectionString);
            var result = db.Query<dynamic>(ScriptDatabase.Producto_Insertar, parameter, commandType: System.Data.CommandType.StoredProcedure).FirstOrDefault();

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

        public Producto Update(Producto item)
        {
            var parameter = new DynamicParameters();
            parameter.Add("@ProductoId", item.ProductoId, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);
            parameter.Add("@Nombre", item.Nombre, System.Data.DbType.String, System.Data.ParameterDirection.Input);
            parameter.Add("@Descripcion", item.Descripcion, System.Data.DbType.String, System.Data.ParameterDirection.Input);
            parameter.Add("@Precio", item.Precio, System.Data.DbType.Decimal, System.Data.ParameterDirection.Input);
            parameter.Add("@Existencia", item.Existencia, System.Data.DbType.Int32, System.Data.ParameterDirection.Input);

            using var db = new SqlConnection(PruebaTecnicaAPIContext.ConnectionString);
            var result = db.Query<dynamic>(ScriptDatabase.Producto_Actualizar, parameter, commandType: System.Data.CommandType.StoredProcedure).FirstOrDefault();

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

        public IEnumerable<Producto> List()
        {
            var parameter = new DynamicParameters();

            using var db = new SqlConnection(PruebaTecnicaAPIContext.ConnectionString);
            var result = db.Query<Producto>(ScriptDatabase.Productos_Listar, parameter, commandType: System.Data.CommandType.StoredProcedure).ToList();
            return result;
        }

        IEnumerable<Producto> IRepository<Producto>.Find(Producto item)
        {
            throw new NotImplementedException();
        }

        RequestStatus IRepository<Producto>.Insert(Producto item)
        {
            throw new NotImplementedException();
        }

        RequestStatus IRepository<Producto>.Update(Producto item)
        {
            throw new NotImplementedException();
        }
    }
}