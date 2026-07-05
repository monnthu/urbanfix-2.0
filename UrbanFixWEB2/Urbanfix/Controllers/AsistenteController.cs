using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Urbanfix.Controllers
{
    public class AsistenteController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public AsistenteController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Chat([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Message))
                return BadRequest("Mensaje vacío");

            var input = request.Message.ToLower();
            string reply;

            // Motor de reglas basado en palabras clave (Funciona 100% offline, sin API Keys)
            if (input.Contains("hola") || input.Contains("buenas") || input.Contains("saludos"))
            {
                reply = "¡Hola! Soy el asistente virtual de Urbanfix. Estoy aquí para ayudarte a reportar problemas en tu comunidad o darte información de contacto de las autoridades. ¿En qué te puedo ayudar?";
            }
            else if (input.Contains("finalidad") || input.Contains("quienes somos") || input.Contains("objetivo") || input.Contains("para que sirve"))
            {
                reply = "Nuestra finalidad es dar voz a los ciudadanos salvadoreños. Urbanfix permite reportar problemas de infraestructura (como baches, fugas de agua o fallas eléctricas) para que las autoridades correspondientes puedan tomar acción rápida.";
            }
            else if (input.Contains("como usar") || input.Contains("como reportar") || input.Contains("ayuda") || input.Contains("pasos"))
            {
                reply = "Es muy fácil: 1) Regístrate en la plataforma. 2) Ve a la pestaña 'Reportar daños'. 3) Llena los datos del problema y ubícalo en el mapa. 4) ¡Listo! Tu reporte será público para que las autoridades lo vean.";
            }
            else if (input.Contains("agua") || input.Contains("pozo") || input.Contains("fuga") || input.Contains("inundacion") || input.Contains("tuberia"))
            {
                reply = "Los problemas relacionados con agua potable, fugas, tuberías rotas o alcantarillado corresponden a ANDA. Puedes contactarlos directamente a su Call Center marcando el 915 o a través de su WhatsApp oficial.";
            }
            else if (input.Contains("luz") || input.Contains("electric") || input.Contains("poste") || input.Contains("cable") || input.Contains("energia"))
            {
                reply = "Los problemas eléctricos (postes caídos, cables expuestos, cortes de energía) deben ser reportados a tu distribuidora local. Si estás en la zona central, contacta a AES El Salvador (2506-9000) o DELSUR (2233-5600).";
            }
            else if (input.Contains("calle") || input.Contains("bache") || input.Contains("via") || input.Contains("hoyo") || input.Contains("carretera"))
            {
                reply = "El mantenimiento de las vías depende de su tipo: si es una carretera principal, corresponde a FOVIAL (puedes marcar al 2228-8425). Si es una calle dentro de una colonia o pasaje, corresponde a la Alcaldía de tu municipio.";
            }
            else if (input.Contains("basura") || input.Contains("desecho") || input.Contains("tren de aseo") || input.Contains("limpieza"))
            {
                reply = "La recolección de basura y limpieza de espacios públicos es responsabilidad de la Alcaldía de tu municipio. Te recomendamos hacer el reporte aquí en Urbanfix y etiquetarlo en las redes sociales de tu alcaldía.";
            }
            else if (input.Contains("gracias") || input.Contains("excelente") || input.Contains("muy bien"))
            {
                reply = "¡De nada! Es un placer ayudarte. Recuerda que juntos podemos mejorar nuestras comunidades. ¿Tienes alguna otra duda?";
            }
            else
            {
                reply = "Entiendo. Para darte la mejor respuesta, ¿podrías ser un poco más específico? Puedes preguntarme sobre cómo reportar un problema, cuál es nuestra finalidad, o pedirme el contacto de autoridades para problemas de agua, calles, electricidad o basura.";
            }

            // Simulamos un pequeño retraso para que parezca que está "pensando"
            System.Threading.Thread.Sleep(800);

            return Json(new { reply });
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }
}
