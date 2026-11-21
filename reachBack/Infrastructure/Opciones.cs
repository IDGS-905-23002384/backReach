namespace reachBack.Infrastructure;

public class JwtOptions
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "reachBack";
    public string Audience { get; set; } = "reachApp";
    public int ExpiracionHoras { get; set; } = 8;
}

public class TiendaOptions
{
    public decimal CostoEnsamble { get; set; } = 799m;
    public decimal EnvioGratisDesde { get; set; } = 1999m;
    public decimal TasaIva { get; set; } = 0.16m;
    public int MaxIntentosLogin { get; set; } = 5;
    public int MinutosBloqueo { get; set; } = 15;
    public string AlmacenNombre { get; set; } = "CEDIS Reach · Puerto Interior, Silao";
    public double AlmacenLatitud { get; set; } = 20.9725;
    public double AlmacenLongitud { get; set; } = -101.4320;
}
