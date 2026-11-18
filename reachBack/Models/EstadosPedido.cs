namespace reachBack.Models;

public static class EstadosPedido
{
    public const string Pagado = "Pagado";
    public const string Completado = "Completado";
    public const string Cancelado = "Cancelado";

    public const string ArmadoNoAplica = "No aplica";
    public const string ArmadoAtendido = "Atendido";
    public const string ArmadoEnsamblando = "Ensamblando";
    public const string ArmadoTerminado = "Terminado";

    public const string EnvioPreparando = "Preparando paquete";
    public const string EnvioEnviado = "Enviado";
    public const string EnvioEntregado = "Entregado";

    public static readonly string[] Armado = { ArmadoAtendido, ArmadoEnsamblando, ArmadoTerminado };
    public static readonly string[] Envio = { EnvioPreparando, EnvioEnviado, EnvioEntregado };
}
