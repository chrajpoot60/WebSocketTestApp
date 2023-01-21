using System.Net.WebSockets;
using System.Text;

namespace WebSocketTestApp.Middlewares
{
    public class HttpContextMiddleware
    {
        public RequestDelegate _next;
        public HttpContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            //check request path
            //websocket request can come any path url but here only accepting path with /ws
            if (httpContext.Request.Path.StartsWithSegments("/ws"))
            {
                if (httpContext.WebSockets.IsWebSocketRequest)
                {
                    //to send websocket request from client to server and viceversa, we have to call AcceptWebSocketAsync function 
                    //it will upgrade TCP connection to websocket connection and return object of WebSocket
                    //websocket's request and response message must happen in running request pipeline
                    //if u try to send and receive websocket message after request pipeline it will throw exception  
                    WebSocket webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
                    await Echo(httpContext, webSocket);
                }
                else
                {
                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                }
            }
        }

        private async Task Echo(HttpContext httpContext, WebSocket webSocket)
        {
            var buffer = new byte[1024*4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            //reading received message
            Console.WriteLine(Encoding.UTF8.GetString(buffer, 0, result.Count));
            //to keep the connection alive, perform websocket request and response in loop
            while (!result.CloseStatus.HasValue)
            {
                //when webscocket is accepted or there is requset and response in loop then 
                //middleware request pipeline stop moving forward till the loop is not finished
                string messageFromServer = "this message from server";
                await webSocket.SendAsync(Encoding.UTF8.GetBytes(messageFromServer) ,
                    result.MessageType, result.EndOfMessage, CancellationToken.None);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                Console.WriteLine("message from Client: " + Encoding.UTF8.GetString(buffer));
            }

            //once loop finished and websocket is closed, the request proceeds back up the pipeline 
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}
