using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using VotingSystem.Models; // Ensure this 'using' directive points to your models

namespace VotingSystem.Controllers
{
    public class EmailNotificationController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DbAb85acVotacionesdbContext _context; // To fetch user details
        private const string BrevoApiKey = "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"; // Consider moving to appsettings.json
        private const string SenderEmail = "juliancam24708@gmail.com"; // Configure sender email
        private const string SenderName = "Sistema de Votaciones";

        public EmailNotificationController(IHttpClientFactory httpClientFactory, DbAb85acVotacionesdbContext context)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
        }

        // Private method to handle the actual email sending via Brevo API
        private async Task SendEmailAsync(string recipientEmail, string recipientName, string subject, string htmlContent)
        {
            var client = _httpClientFactory.CreateClient("BrevoApiClient"); // Use a named HttpClient
            client.DefaultRequestHeaders.Clear(); // Clear any default headers if necessary
            client.DefaultRequestHeaders.Add("api-key", BrevoApiKey);
            client.DefaultRequestHeaders.Add("accept", "application/json");

            var emailData = new
            {
                sender = new { email = SenderEmail, name = SenderName },
                to = new[] { new { email = recipientEmail, name = recipientName } },
                subject = subject,
                htmlContent = htmlContent
            };

            var jsonPayload = JsonSerializer.Serialize(emailData, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.brevo.com/v3/smtp/email", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error enviando correo a {recipientEmail}. Status: {response.StatusCode}. Error: {errorContent}");
                // Consider a more robust error logging mechanism or throwing a custom exception
                throw new Exception($"Failed to send email to {recipientEmail}. Status: {response.StatusCode}. Details: {errorContent}");
            }
            else
            {
                Console.WriteLine($"Correo enviado exitosamente a {recipientEmail}. Asunto: {subject}");
            }
        }

        // Helper method to generate HTML for a new votation email
        private string GenerateNuevaVotacionEmailHtml(Votacion votacion, Asamblea asamblea, Usuario usuario)
        {
            // HTML content from your original file
            return $@"
            <!DOCTYPE html>
            <html lang='es'>
            <head>
                <meta charset='UTF-8'>
                <style>
                    body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f5f7fa; margin: 0; padding: 0; }}
                    .container {{ max-width: 600px; margin: 30px auto; background-color: #ffffff; padding: 30px; border-radius: 10px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }}
                    .header img {{ width: 100%; max-height: 200px; object-fit: cover; border-radius: 10px 10px 0 0; }}
                    h1 {{ color: #2c3e50; margin-top:0; }}
                    h2 {{ color: #34495e; }}
                    p {{ color: #555; font-size: 15px; line-height: 1.6; }}
                    .button {{ display: inline-block; margin-top: 20px; padding: 12px 24px; background-color: #3498db; color: #ffffff !important; text-decoration: none; font-weight: bold; border-radius: 5px; transition: background-color 0.3s ease; }}
                    .button:hover {{ background-color: #2980b9; }}
                    .footer {{ margin-top: 30px; font-size: 12px; color: #aaa; text-align: center; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <img src='https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcR96ZrZnc6TWTJAgRU20uzS1k4a9LwrOWM-9g&s' alt='Imagen Votación' />
                    </div>
                    <h1>Nueva Votación Creada</h1>
                    <p>Estimado/a {usuario.Nombre} {usuario.Apellido},</p>
                    <p>Se ha creado una nueva votación en el sistema:</p>
                    <h2>{votacion.Titulo}</h2>
                    <p><strong>Asamblea:</strong> {asamblea?.Titulo ?? "N/A"}</p>
                    <p><strong>Descripción:</strong> {votacion.Descripcion}</p>
                    <p><strong>Tipo:</strong> {votacion.TipoVotacion}</p>
                    <p><strong>Fecha de inicio:</strong> {votacion.FechaInicio:dd/MM/yyyy HH:mm}</p>
                    <p><strong>Fecha de fin:</strong> {votacion.FechaFin:dd/MM/yyyy HH:mm}</p>
                    <a href='https://daniel190-001-site1.jtempurl.com/' class='button'>Ir al sistema y votar</a>
                    <div class='footer'>
                        © {DateTime.Now.Year} Sistema de Votaciones - Propiedad Horizontal
                    </div>
                </div>
            </body>
            </html>";
        }

        // Method to notify multiple users about a new votation (specific template)
        [NonAction] // Indicates this is not a routable controller action
        public async Task NotificarNuevaVotacionAsync(Votacion votacion, IEnumerable<Usuario> usuarios, Asamblea asamblea)
        {
            foreach (var usuario in usuarios)
            {
                string htmlContent = GenerateNuevaVotacionEmailHtml(votacion, asamblea, usuario);
                string subject = $"📢 Nueva Votación Disponible: {votacion.Titulo}";
                try
                {
                    await SendEmailAsync(usuario.Email, $"{usuario.Nombre} {usuario.Apellido}", subject, htmlContent);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fallo al enviar correo de nueva votación a {usuario.Email}: {ex.Message}");
                    // Log this error, but continue notifying other users
                }
                await Task.Delay(100); // Brief pause to avoid overwhelming the email API
            }
        }

        // 1. Notificar a todos los usuarios activos (generic message)
        [NonAction]
        public async Task NotificarATodosLosUsuariosActivosAsync(string subject, string messageBody)
        {
            var usuariosActivos = await _context.Usuarios.Where(u => u.Estado == "activo").ToListAsync();
            string htmlMessageBody = $"<p>{messageBody.Replace(Environment.NewLine, "<br>")}</p>"; // Basic HTML formatting

            foreach (var usuario in usuariosActivos)
            {
                try
                {
                    await SendEmailAsync(usuario.Email, $"{usuario.Nombre} {usuario.Apellido}", subject, htmlMessageBody);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fallo al enviar notificación genérica a {usuario.Email}: {ex.Message}");
                }
                await Task.Delay(100);
            }
        }

        // 2. Notificar a todos los administradores activos (generic message)
        [NonAction]
        public async Task NotificarAAdministradoresActivosAsync(string subject, string messageBody)
        {
            var adminUsuarios = await _context.Usuarios.Where(u => u.Rol == "admin" && u.Estado == "activo").ToListAsync();
            string htmlMessageBody = $"<p>{messageBody.Replace(Environment.NewLine, "<br>")}</p>";

            foreach (var admin in adminUsuarios)
            {
                try
                {
                    await SendEmailAsync(admin.Email, $"{admin.Nombre} {admin.Apellido}", subject, htmlMessageBody);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fallo al enviar notificación a administrador {admin.Email}: {ex.Message}");
                }
                await Task.Delay(100);
            }
        }

        // 3. Notificar a un usuario específico (generic message)
        [NonAction]
        public async Task NotificarAUsuarioAsync(Usuario usuario, string subject, string messageBody)
        {
            if (usuario == null)
            {
                Console.WriteLine("Intento de notificar a un usuario nulo.");
                throw new ArgumentNullException(nameof(usuario));
            }
            string htmlMessageBody = $"<p>{messageBody.Replace(Environment.NewLine, "<br>")}</p>";
            try
            {
                await SendEmailAsync(usuario.Email, $"{usuario.Nombre} {usuario.Apellido}", subject, htmlMessageBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fallo al enviar notificación a usuario específico {usuario.Email}: {ex.Message}");
                throw; // Rethrow to let the caller handle it
            }
        }
    }
}