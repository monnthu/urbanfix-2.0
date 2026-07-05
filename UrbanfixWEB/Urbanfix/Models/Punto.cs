namespace Urbanfix.Models;

public class Punto
{
    public int Id { get; set; }
    public double Latitud { get; set; }
    public double Longitud { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}
