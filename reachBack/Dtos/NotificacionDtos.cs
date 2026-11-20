namespace reachBack.Dtos;

public record NotificacionDto(int Id, string Titulo, string Mensaje, string Tipo, int? PedidoId, bool Leida, DateTime Fecha);

public record ConteoNotificacionesDto(int NoLeidas, int UltimoId);
