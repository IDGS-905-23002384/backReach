namespace reachBack.Models;

public static class Roles
{
    public const string Comprador = "Comprador";
    public const string Empleado = "Empleado";
    public const string Administrador = "Administrador";
    public const string Personal = Empleado + "," + Administrador;
}
