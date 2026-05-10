namespace Users.API.Models;

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string Nombre { get; set; }
    public required string Apellido { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public bool Activo { get; set; } = true;
    public int IntentosFallidos { get; set; } = 0;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public DateTime? FechaUltimoLogin { get; set; }
}
