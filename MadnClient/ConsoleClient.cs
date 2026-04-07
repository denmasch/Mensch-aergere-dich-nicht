using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MadnShared.Logger;
using MadnShared.Messages.Base;
using MadnShared.Messages.ClientToServer;
using MadnShared.Messages.ServerToClient;
using MadnShared.Utils;
namespace MadnClient;

public class ConsoleClient
{
    private readonly Guid _playerId = new();
    private ClientWebSocket? _socket;
    private CancellationTokenSource _cts = new();
    private TaskCompletionSource<ListGamesResponseMessage> _listGamesTcs;

    public async Task RunAsync(string serverUri)
    {
        await EnsureConnectedAsync(serverUri);
        ShowWelcome();
        Console.ReadKey(true);
        
        while (true)
        {
            var choice = ShowMenu();
            if (choice == "1")
            {
                Console.WriteLine("Creating game...");
                Logger.LogInfo("Requested to create game");
                await SendCreateGameAsync();
            }
            else if (choice == "2")
            {
                var response = await ListGamesAsync();
                if (response.Games == null)
                {
                    Logger.LogError("Failed to get game list from server.");
                    continue;
                }
                var game = ShowGameList(response);
                if (game == "b" || game == "B")
                {
                    continue;
                }
                else if (game != null && int.TryParse(game, out int id))
                {
                    var gameId = response.Games.Keys.ElementAtOrDefault(id - 1);
                    await SendJoinGameAsync(gameId);
                    
                    Logger.LogInfo($"Requested to join game");
                }
                else
                {
                    Console.WriteLine("Invalid choice");
                }
            }
            else if (choice == "q" || choice == "Q")
            {
                await CloseAsync();
                Logger.LogInfo($"Client closed");
                break;
            }
            else
            {
                Console.WriteLine("Invalid choice");
            }
        }
    }

    private void ShowWelcome()
    {
        Console.Clear();
        Console.WriteLine("Willkommen zu Mensch ärgere dich nicht");
        Console.WriteLine();
        Console.WriteLine("Beliebige Taste drücken...");
    }

    private string ShowMenu()
    {
        Console.Clear();
        Console.WriteLine("Menü:");
        Console.WriteLine("1) Spiel erstellen");
        Console.WriteLine("2) Spiel beitreten");
        Console.WriteLine("Q) Beenden");
        Console.Write("Auswahl: ");
        var key = Console.ReadKey(true);
        Console.WriteLine(key.KeyChar);
        return key.KeyChar.ToString();
    }
    
    private string ShowGameList(ListGamesResponseMessage response)
    {
        Console.Clear();
        Console.WriteLine("Verfügbare Spiele:");
        const string headerFormat = "| {0,-3} | {1,-36} | {2,-3} |";
        const string divider = "+-----+--------------------------------------+-----+";

        Console.WriteLine(divider);
        Console.WriteLine(headerFormat, "Nr.", "GameId", "Spieler");
        Console.WriteLine(divider);

        int i = 1;
        foreach (var game in response.Games)
        {
            Console.WriteLine(headerFormat, i, game.Key, $"{game.Value}/4");
            i++;
        }
        Console.WriteLine(divider);
        Console.WriteLine("Geben Sie eine Nummer ein, um einem Spiel beizutreten, oder 'b' um zurück zum Menü zu gehen:");
        var input = Console.ReadLine();
        return input ?? string.Empty;
    }

    private async Task EnsureConnectedAsync(string serverUri)
    {
        if (_socket != null && _socket.State == WebSocketState.Open)
            return;

        _socket = new ClientWebSocket();
        try
        {
            Logger.LogInfo("Connecting to " + serverUri);
            await _socket.ConnectAsync(new Uri(serverUri), CancellationToken.None);
            Logger.LogInfo("Connected to server at " + serverUri);
            _cts = new CancellationTokenSource();
            _ = Task.Run(() => ReceiveLoopAsync(_socket, _cts.Token));
        }
        catch (Exception ex)
        {
            Logger.LogError("WebSocket connect failed: " + ex.Message);
            _socket?.Dispose();
            _socket = null;
        }
    }

    private async Task SendMessageAsync(IMessage message)
    {
        if (_socket == null || _socket.State != WebSocketState.Open)
        {
            Logger.LogError("Cannot send message, not connected to server.");
            return;
        }
        var json = MessageSerializer.Serialize(message);
        var buffer = Encoding.UTF8.GetBytes(json);
        await _socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private async Task SendCreateGameAsync()
    {
        try
        {
            var createMsg = new CreateGameMessage
            {
            };

            await SendMessageAsync(createMsg);
            Logger.LogInfo($"Sent CreateGame message");
        }
        catch (Exception ex)
        {
            Logger.LogError("Exception when sending CreateGame message: " + ex.Message);
        }
    }
    
    private async Task SendJoinGameAsync(Guid gameId)
    {
        try
        {
            var createMsg = new JoinGameMessage
            {
                GameId = gameId,
                PlayerId = _playerId
            };

            await SendMessageAsync(createMsg);
            Logger.LogInfo($"Sent JoinGame message with GameId {createMsg.GameId}");
        }
        catch (Exception ex)
        {
            Logger.LogError("Exception when sending JoinGame message: " + ex.Message);
        }
    }
    
    private async Task<ListGamesResponseMessage> ListGamesAsync()
    {
        try
        {
            _listGamesTcs = new TaskCompletionSource<ListGamesResponseMessage>();
            var listMsg = new ListGamesMessage();

            await SendMessageAsync(listMsg);
            Logger.LogInfo($"Sent ListGames message");
            return await _listGamesTcs.Task;
        }
        catch (Exception ex)
        {
            Logger.LogError("Exception when sending JoinGame message: " + ex.Message);
            return null;
        }
    }

    private async Task ReceiveLoopAsync(ClientWebSocket socket, CancellationToken ct)
    {
        var buffer = new byte[4096];
        try
        {
            while (!ct.IsCancellationRequested && socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Logger.LogInfo("Server closed");
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", CancellationToken.None);
                    break;
                }

                var msgJson = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var gameMsg = MessageSerializer.Deserialize(msgJson);
                
                OnMessageReceived(gameMsg);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("ReceiveLoop error: " + ex.Message);
        }
    }

    private void OnMessageReceived(IMessage message)
    {
        Logger.LogInfo($"Received message: {message}");
        switch (message)
        {
            case CreateGameMessage createMsg:
                break;
            case ListGamesResponseMessage listResponse:
                _listGamesTcs?.SetResult(listResponse);
                break;
        }
    }

    private async Task CloseAsync()
    {
        try
        {
            _cts.Cancel();
            if (_socket != null)
            {
                if (_socket.State == WebSocketState.Open)
                {
                    await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", CancellationToken.None);
                }
                _socket.Dispose();
                _socket = null;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("Error closing WebSocket: " + ex.Message);
        }
    }
}