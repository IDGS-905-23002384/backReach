namespace reachBack.Models;

public class FichaTecnica
{
    public int Id { get; set; }
    public string NombreEquipoPrearmado { get; set; } = string.Empty;
    public string CategoriaUso { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal CostoManoObra { get; set; }
    public decimal PrecioSugerido { get; set; }
    public bool Activo { get; set; } = true;

    public List<FichaTecnicaDetalle> ComponentesRequeridos { get; set; } = new();
}
