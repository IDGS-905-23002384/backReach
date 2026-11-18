namespace reachBack.Models;

public class MetodoEnvio
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Paqueteria { get; set; } = string.Empty;
    public decimal Costo { get; set; }
    public int DiasMin { get; set; }
    public int DiasMax { get; set; }
    public bool AplicaEnvioGratis { get; set; }
    public bool RequiereDireccion { get; set; } = true;
    public bool Activo { get; set; } = true;
}
