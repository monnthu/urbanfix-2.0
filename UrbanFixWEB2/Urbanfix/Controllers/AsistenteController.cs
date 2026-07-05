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

            var apiKey = _configuration["GeminiApiKey"];
            
            // Fallback si no hay API Key configurada
            if (string.IsNullOrEmpty(apiKey))
            {
                return Json(new { reply = "Hola. Soy el asistente de Urbanfix. Actualmente mi inteligencia artificial avanzada está desactivada porque falta configurar la clave de Google Gemini (GeminiApiKey). Por favor, pide a un administrador que la agregue en las variables de entorno." });
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                
                // Gemini usa la URL con la API key en el querystring
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";

                var systemInstruction = "Eres el asistente virtual de Urbanfix El Salvador. Tu objetivo es ayudar a los ciudadanos a entender la plataforma y reportar problemas de infraestructura (baches, agua, electricidad, basura). Sé amable, conciso y útil. Si preguntan por autoridades: ANDA (agua), FOVIAL o Alcaldías (calles), AES o DELSUR (electricidad). Responde de forma directa, sin usar formato markdown complejo.";

                var payload = new
                {
                    system_instruction = new {
                        parts = new[] { new { text = systemInstruction } }
                    },
                    contents = new[]
                    {
                        new {
                            role = "user",
                            parts = new[] { new { text = request.Message } }
                        }
                    },
                    generationConfig = new {
                        temperature = 0.7,
                        maxOutputTokens = 250
                    }
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(responseString);
                    
                    // Extraer la respuesta de la estructura JSON de Gemini
                    var reply = doc.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString();
                        
                    return Json(new { reply });
                }
                else
                {
                    var errorDetail = await response.Content.ReadAsStringAsync();
                    return Json(new { reply = $"Hubo un error al comunicarse con la IA. Código: {response.StatusCode}. Detalle: {errorDetail}" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { reply = $"Error interno del servidor al procesar la IA: {ex.Message}" });
            }
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }
}
