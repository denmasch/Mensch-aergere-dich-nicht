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
    private Guid _playerId = Guid.Empty;
    private TaskCompletionSource<ListGamesResponseMessage> _listGamesTcs;
    private TaskCompletionSource<GameCreatedMessage> _createGameTcs;
    private TaskCompletionSource<GameJoinedMessage> _joinGameTcs;
    private readonly IWebSocketClient _wsClient;
    private GameFrontend _frontend;
    private TaskCompletionSource<Guid> _welcomeTcs;

    public ConsoleClient(IWebSocketClient wsClient)
    {
        _wsClient = wsClient;
        _wsClient.MessageReceived += OnWsMessageReceived;
        _welcomeTcs = new TaskCompletionSource<Guid>();
    }

    public async Task RunAsync(string serverUri)
    {
        await _wsClient.ConnectAsync(serverUri);
        await EnsureFrontendInitializedAsync();
        ShowWelcome();
        Console.ReadKey(true);
        
        while (true)
        {
            var choice = ShowMenu();
            if (choice == "1")
            {
                Console.Clear();
                Console.WriteLine("Erstelle Spiel ...");
                Logger.LogInfo("Requested to create game");
                var response = await SendCreateGameAsync();
                if (response == null)
                {
                    Console.WriteLine("Spiel konnte nicht erstellt werden.");
                    Logger.LogError("Failed to create game");
                }
                else
                {
                    await _frontend.EnterGameAsync(response.GameId);
                }
                
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
                    
                    var res = await SendJoinGameAsync(gameId);

                    if (res == null)
                    {
                        Console.WriteLine("Beitritt nicht möglich");
                        Logger.LogInfo("Failed to join game");
                    }
                    else
                    {
                        await _frontend.EnterGameAsync(res.GameId);
                    }
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

    private async Task EnsureFrontendInitializedAsync()
    {
        if (_frontend != null) return;
        await _welcomeTcs.Task;
        if (_frontend == null)
        {
            _frontend = new GameFrontend(_wsClient, _playerId);
        }
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
        const string headerFormat = "| {0,-3} | {1,-36} | {2,7} |";
        const string divider = "+-----+--------------------------------------+---------+";

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

    private async Task SendMessageAsync(IMessage message)
    {
        try
        {
            await _wsClient.SendAsync(message);
        }
        catch (Exception ex)
        {
            Logger.LogError("Cannot send message: " + ex.Message);
        }
    }

    private async Task<GameCreatedMessage> SendCreateGameAsync()
    {
        try
        {
            _createGameTcs = new TaskCompletionSource<GameCreatedMessage>();
            var createMsg = new CreateGameMessage();

            await SendMessageAsync(createMsg);
            Logger.LogInfo($"Sent CreateGame message");
            return await _createGameTcs.Task;
        }
        catch (Exception ex)
        {
            Logger.LogError("Exception when sending CreateGame message: " + ex.Message);
            return null;
        }
    }
    
    private async Task<GameJoinedMessage> SendJoinGameAsync(Guid gameId)
    {
        try
        {
            _joinGameTcs = new TaskCompletionSource<GameJoinedMessage>();
            var createMsg = new JoinGameMessage
            {
                GameId = gameId,
                PlayerId = _playerId
            };

            await SendMessageAsync(createMsg);
            Logger.LogInfo($"Sent JoinGame message with GameId {createMsg.GameId}");
            return await _joinGameTcs.Task;
        }
        catch (Exception ex)
        {
            Logger.LogError("Exception when sending JoinGame message: " + ex.Message);
            return null;
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

    private void OnWsMessageReceived(IMessage message)
    {
        Logger.LogInfo($"Received message: {message}");
        switch (message)
        {
            case WelcomeMessage welcome:
                _playerId = welcome.ClientId;
                Logger.LogInfo($"Received WelcomeMessage. Assigned client id {_playerId}");
                if (_frontend == null)
                {
                    _frontend = new GameFrontend(_wsClient, _playerId);
                }
                _welcomeTcs?.TrySetResult(_playerId);
                break;
            case GameCreatedMessage createdMsg:
                _createGameTcs?.TrySetResult(createdMsg);
                break;
            case ListGamesResponseMessage listResponse:
                _listGamesTcs?.TrySetResult(listResponse);
                break;
            case GameJoinedMessage joinResponse:
                _joinGameTcs?.TrySetResult(joinResponse);
                break;
        }
    }

    private async Task CloseAsync()
    {
        try
        {
            await _wsClient.CloseAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError("Error closing WebSocket: " + ex.Message);
        }
    }
}