using Newtonsoft.Json;
using ServicioBackground.Enum;

namespace ServicioBackground.DTO
{

    public partial class TicketsPendientes
    {
        [JsonProperty("tickets")]
        public List<Ticket> Tickets { get; set; }
        public TicketsPendientes()
        {
            Tickets = new List<Ticket>();
        }
    }

    public partial class Ticket
    {
        [JsonIgnore]
        public int Id { get; set; }

        [JsonProperty("no_ticket")]
        public string NoTicket { get; set; }

        [JsonProperty("total")]
        public double Total { get; set; }

        [JsonProperty("subtotal")]
        public double Subtotal { get; set; }

        [JsonProperty("impuesto")]
        public double Impuesto { get; set; }

        [JsonProperty("forma_pago")]
        public string FormaPago { get; set; }

        [JsonProperty("fecha")]
        public DateTime Fecha { get; set; }

        [JsonProperty("entidad")]
        public string Entidad { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonIgnore]
        public EnumEstado EstadoEnvio { get; set; }

        [JsonIgnore]
        public string RespuestaServicio { get; set; }

        [JsonIgnore]
        public DateTime FechaCreacion { get; set; }

        [JsonProperty("productos")]
        public List<Producto> Productos { get; set; }

        [JsonProperty("cp")]
        public string CodigoPostal { get; set; }


        [JsonIgnore]
        public int Reintento { get; set; }

        public Ticket()
        {
            Productos = new List<Producto>();
            FechaCreacion = DateTime.Now;
            Reintento = 0;
        }
    }

    public partial class Producto
    {
        [JsonIgnore]
        public int Id { get; set; }

        [JsonIgnore]
        public int TicketId { get; set; }

        [JsonProperty("descripcion")]
        public string Descripcion { get; set; }

        [JsonProperty("valor_unitario")]
        public double ValorUnitario { get; set; }

        [JsonProperty("importe")]
        public long Importe { get; set; }

        [JsonProperty("cantidad")]
        public long Cantidad { get; set; }

        [JsonProperty("clave_unidad")]
        public string ClaveUnidad { get; set; }

        [JsonProperty("unidad_medida")]
        public string UnidadMedida { get; set; }

        [JsonProperty("clave_producto")]
        public string ClaveProducto { get; set; }

        [JsonProperty("iva_factor")]
        public double IvaFactor { get; set; }

        [JsonProperty("ieps_factor")]
        public long IepsFactor { get; set; }

        [JsonProperty("iva")]
        public double Iva { get; set; }

        [JsonProperty("ieps")]
        public long Ieps { get; set; }

        [JsonProperty("descuento")]
        public long Descuento { get; set; }

        [JsonProperty("objeto_imp")]
        public string ObjetoImp { get; set; }

        [JsonProperty("ieps_exento")]
        public bool IepsExento { get; set; }

        [JsonProperty("iva_exento")]
        public bool IvaExento { get; set; }
    }

}
