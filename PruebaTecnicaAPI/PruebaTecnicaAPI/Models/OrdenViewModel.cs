namespace PruebaTecnicaAPI.Models
{
    public class DetalleOrdenViewModel
    {
        public long DetalleOrdenId { get; set; }
        public long OrdenId { get; set; }
        public long ProductoId { get; set; }
        public string ProductoNombre { get; set; }
        public int Cantidad { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Total { get; set; }
    }

    public class OrdenViewModel
    {
        public long OrdenId { get; set; }
        public long ClienteId { get; set; }
        public string ClienteNombre { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Total { get; set; }
        public DateTime FechaCreacion { get; set; }
        public List<DetalleOrdenViewModel> Detalles { get; set; }
    }

    public class DetalleOrdenRequest
    {
        public long ProductoId { get; set; }
        public int Cantidad { get; set; }
    }

    public class OrdenRequest
    {
        public long OrdenId { get; set; }
        public long ClienteId { get; set; }
        public List<DetalleOrdenRequest> Detalle { get; set; }
    }
}