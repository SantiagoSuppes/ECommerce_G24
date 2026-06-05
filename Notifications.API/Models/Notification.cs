namespace ECommerce_G24.Notifications.API.Models;

public class Notification
{
    public string Id { get; set; }
    public string UsuarioId { get; set; }
    public string Mensaje { get; set; }
    public string Tipo { get; set; }
    public string Estado { get; set; }
    public DateTime FechaEnvio { get; set; }
}
