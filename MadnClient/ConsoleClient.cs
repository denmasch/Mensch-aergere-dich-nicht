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
    private readonly Guid _playerId = Guid.NewGuid();
    private TaskCompletionSource<ListGamesResponseMessage> _listGamesTcs;
    private readonly IWebSocketClient _wsClient;

    public ConsoleClient(IWebSocketClient wsClient)
    {
        _wsClient = wsClient;
        _wsClient.MessageReceived += OnWsMessageReceived;
    }

    public async Task RunAsync(string serverUri)
    {
        await _wsClient.ConnectAsync(serverUri);
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

    private void OnWsMessageReceived(IMessage message)
    {
        Logger.LogInfo($"Received message: {message}");
        switch (message)
        {
            case CreateGameMessage createMsg:
                break;
            case ListGamesResponseMessage listResponse:
                _listGamesTcs?.TrySetResult(listResponse);
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