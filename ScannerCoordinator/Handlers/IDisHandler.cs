namespace ScannerCoordinator.Handlers
{
    using Microsoft.AspNetCore.Http;

    using System.Net.WebSockets;
    using System.Threading.Tasks;

    public interface IDisHandler
    {
        Task PushAsync(HttpContext context, WebSocket webSocket);
        Task DisconnectAsync(HttpContext context, WebSocket webSocket);
    }
}
