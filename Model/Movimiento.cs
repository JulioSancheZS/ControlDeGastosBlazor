namespace BlazorWallet.Model
{
    public class Movimiento
    {
        public Guid IdMovimiento { get; set; }
        public string NombreMovimiento { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime? FechaMovimiento { get; set; } = DateTime.Now;
        public Guid IdTipoMovimiento { get; set; }
        public Guid IdFrecuencia { get; set; }

        public bool EsFijo { get; set; } = false;
        public DateTime? FechaInicio { get; set; }
        public DateTime? UltimaAplicacion { get; set; }
    }
}
