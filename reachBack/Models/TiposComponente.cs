namespace reachBack.Models;

public static class TiposComponente
{
    public const string Cpu = "CPU";
    public const string TarjetaMadre = "MOTHERBOARD";
    public const string Ram = "RAM";
    public const string Almacenamiento = "STORAGE";
    public const string Gpu = "GPU";
    public const string Enfriamiento = "COOLER";
    public const string Gabinete = "CASE";
    public const string Fuente = "PSU";

    public static readonly string[] Orden = { Cpu, TarjetaMadre, Ram, Almacenamiento, Gpu, Enfriamiento, Gabinete, Fuente };
}
