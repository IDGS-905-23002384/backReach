namespace reachBack.Dtos;

public record SucursalDto(int Id, string Nombre, string Direccion, string Ciudad, string Telefono, string Horario, decimal Latitud, decimal Longitud);

public record PuntoDto(double Latitud, double Longitud);

public record RastreoDto(
    string Estado,
    double Progreso,
    string OrigenNombre,
    PuntoDto Origen,
    string DestinoNombre,
    PuntoDto Destino,
    bool DestinoAproximado,
    PuntoDto Repartidor,
    List<PuntoDto> Ruta);
