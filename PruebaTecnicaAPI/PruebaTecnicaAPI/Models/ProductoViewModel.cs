namespace PruebaTecnicaAPI.Models
{
    public class ProductoViewModel
    {
        public long ProductoId { get; set; }

        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public decimal Precio { get; set; }

        public int Existencia { get; set; }
    }
}
