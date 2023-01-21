using System.Net.WebSockets;
using System.Text;

namespace WebSocketTestApp.Extensions
{
    public static class AppBuilderExtensions
    {
        public static void AddWebSocketMiddleware(this IApplicationBuilder appBuilder)
        {
            appBuilder.Use(async (context, next) =>
            {
                //check request path
                //websocket request can come any path url but here only accepting path with /ws
                if (context.WebSockets.IsWebSocketRequest)
                {
                    //to send websocket request from client to server and viceversa, we have to call AcceptWebSocketAsync function 
                    //it will upgrade TCP connection to websocket connection and return object of WebSocket
                    //websocket's request and response message must happen in running request pipeline
                    //if u try to send and receive websocket message after request pipeline it will throw exception  
                    var user_id = context.Request.Query.FirstOrDefault();

                    var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    var webSocketObjectCollection = context.RequestServices.GetRequiredService<WebSocketObjectCollection>();
                    if (user_id.Value.Count() > 0)
                        webSocketObjectCollection.webSocketCollection.Add(user_id.Value, webSocket);

                    await Echo(webSocket);
                    if(user_id.Value.Count() > 0)
                        webSocketObjectCollection.webSocketCollection.Remove(user_id.Value);
                }
                else
                {
                    //context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await next(context);
                }

            });
        }
        private static async Task Echo(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
            //reading received message
            Console.WriteLine(Encoding.UTF8.GetString(buffer, 0, receiveResult.Count));
            var current_time = DateTime.Now;
            //to keep the websocket connection alive, perform websocket request and response in loop
            while (!receiveResult.CloseStatus.HasValue)
            {
                //when webscocket is accepted or there is requset and response in loop then 
                //middleware request pipeline stop moving forward till the loop is not finished

                string messageFromServer = "this message from server";
                await webSocket.SendAsync(
                       Encoding.UTF8.GetBytes(messageFromServer),
                       receiveResult.MessageType,
                       receiveResult.EndOfMessage,
                       CancellationToken.None);

                //if (current_time.AddMinutes(1) == DateTime.Now)
                //{
                //    await webSocket.SendAsync(
                //       Encoding.UTF8.GetBytes(messageFromServer),
                //       receiveResult.MessageType,
                //       receiveResult.EndOfMessage,
                //       CancellationToken.None);
                //    current_time = DateTime.Now;
                //}
                if (webSocket.CloseStatus.HasValue)
                    break;
                receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
                Console.WriteLine("message from Client: " + Encoding.UTF8.GetString(buffer));
            }

            //once loop finished and websocket is closed, the request proceeds back up the pipeline 
            await webSocket.CloseAsync(
                receiveResult.CloseStatus.Value,
                receiveResult.CloseStatusDescription,
                CancellationToken.None);
        }
    }
}
