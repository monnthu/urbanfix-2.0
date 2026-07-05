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
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Message))
                return BadRequest("Mensaje vacío");

            var apiKey = _configuration["OpenAIApiKey"];
            
            // Fallback si no hay API Key configurada
            if (string.IsNullOrEmpty(apiKey))
            {
                return Json(new { reply = "Hola. Soy el asistente de Urbanfix. Actualmente mi inteligencia artificial avanzada está desactivada porque falta configurar la clave de OpenAI (OpenAIApiKey). Por favor, pide a un administrador que la agregue en las variables de entorno para que pueda responder tus preguntas de forma inteligente." });
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                var payload = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                        new { role = "system", content = "Eres el asistente virtual de Urbanfix El Salvador. Tu objetivo es ayudar a los ciudadanos a entender la plataforma y reportar problemas de infraestructura (baches, agua, electricidad, basura). Sé amable, conciso y útil. Si preguntan por autoridades: ANDA (agua), FOVIAL o Alcaldías (calles), AES o DELSUR (electricidad)." },
                        new { role = "user", content = request.Message }
                    },
                    max_tokens = 250,
                    temperature = 0.7
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(responseString);
                    var reply = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
                    return Json(new { reply });
                }
                else
                {
                    return Json(new { reply = "Hubo un error al comunicarse con la IA. Intenta de nuevo más tarde." });
                }
            }
            catch (Exception)
            {
                return Json(new { reply = "Error interno del servidor al procesar la IA." });
            }
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }
}
