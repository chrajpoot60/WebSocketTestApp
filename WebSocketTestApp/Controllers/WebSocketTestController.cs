using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace WebSocketTestApp.Controllers
{
    [Route("websocket")]
    [ApiController]
    public class WebSocketTestController : ControllerBase
    {
        private readonly IServiceProvider _serviceProvider;
        public WebSocketTestController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        [HttpPost]
        [Route("user")]
        public async Task<IActionResult> AddUserWithMessage([FromBody] JsonElement userInput)
        {
            var userString = userInput.ToString();
            if(string.IsNullOrEmpty(userString))
                return new BadRequestResult();
            var userJObject = JObject.Parse(userString);
            if (userJObject == null)
                return new BadRequestResult();
            if (userJObject.ContainsKey("user") && userJObject.ContainsKey("message"))
            {
                if (userJObject.GetValue("user") != null && userJObject.GetValue("message") != null)
                {
                    var user_id = userJObject.GetValue("user").ToString();
                    var message = JsonConvert.SerializeObject(userJObject.GetValue("message"));
                    var webSocketObjectCollection = _serviceProvider.GetRequiredService<WebSocketObjectCollection>();
                    webSocketObjectCollection.webSocketCollection.TryGetValue(user_id, out WebSocket webSocket);
                    if(webSocket != null)
                    {
                        await webSocket.SendAsync(Encoding.UTF8.GetBytes(message), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    
                }
            }
            return new OkResult();
        }
    }
}
