using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PruebaTecnicaAPI.DataAccess
{
    public class ScriptDatabase
    {

        #region Clientes
        public static string Clientes_Listar = "SP_Clientes_Listar";
        public static string Cliente_Buscar = "SP_Cliente_Buscar";
        public static string Cliente_Insertar = "[SP_Cliente_Insertar]";
        public static string Cliente_Actualizar = "SP_Cliente_Actualizar";
        #endregion

        #region Productos
        public static string Productos_Listar = "SP_Productos_Listar";
        public static string Producto_Buscar = "SP_Productos_Buscar";
        public static string Producto_Insertar = "SP_Producto_Insertar";
        public static string Producto_Actualizar = "SP_Producto_Actualizar";
        #endregion

        #region Ordenes
        public static string Ordenes_Crear = "SP_Ordenes_Crear";
        #endregion
    }
}

