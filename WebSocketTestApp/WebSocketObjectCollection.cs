using System.Net.WebSockets;

namespace WebSocketTestApp
{
    public class WebSocketObjectCollection
    {
        public Dictionary<string, WebSocket> webSocketCollection { get; set; }
        public WebSocketObjectCollection()
        {
            webSocketCollection = new Dictionary<string, WebSocket>();
        }
    }
}
