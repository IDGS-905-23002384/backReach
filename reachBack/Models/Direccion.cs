namespace reachBack.Models;

public class Direccion
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public string Alias { get; set; } = "Casa";
    public string NombreRecibe { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Calle { get; set; } = string.Empty;
    public string NumeroExterior { get; set; } = string.Empty;
    public string? NumeroInterior { get; set; }
    public string Colonia { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string CodigoPostal { get; set; } = string.Empty;
    public string? Referencias { get; set; }
    public bool EsPredeterminada { get; set; }
}
