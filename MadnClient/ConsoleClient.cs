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
using System.IO;
using System.Text.Json;
using MadnShared.Stats;
using System.Linq;
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
    private TaskCompletionSource<MatchHistoryResponseMessage> _matchHistoryTcs;

    public ConsoleClient(IWebSocketClient wsClient)
    {
        _wsClient = wsClient;
        _wsClient.MessageReceived += OnWsMessageReceived;
        _welcomeTcs = new TaskCompletionSource<Guid>();
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
                    _frontend = new GameFrontend(_wsClient, _playerId, response.Color, response.Gameboard);
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
                    if (id < 1 || id > response.Games.Count)
                    {
                        Console.WriteLine("Invalid game number");
                        continue;
                    }

                    var gameId = response.Games.Keys.ElementAtOrDefault(id - 1);

                    var res = await SendJoinGameAsync(gameId);

                    if (res == null)
                    {
                        Console.WriteLine("Beitritt nicht möglich");
                        Logger.LogInfo("Failed to join game");
                    }
                    else
                    {
                        _frontend = new GameFrontend(_wsClient, _playerId, res.Color, res.Gameboard);
                        await _frontend.EnterGameAsync(res.GameId);
                    }
                }
                else
                {
                    Console.WriteLine("Invalid choice");
                }
            }
            else if (choice == "3")
            {
                // Matchhistory
                await ShowMatchHistoryAsync();
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
        Console.WriteLine("┌────────────────────────────────────────┐");
        Console.WriteLine("│ Willkommen zu Mensch ärgere dich nicht │");
        Console.WriteLine("└────────────────────────────────────────┘");
        Console.WriteLine();
        Console.WriteLine("Beliebige Taste drücken...");
    }

    private string ShowMenu()
    {
        Console.Clear();
        Console.WriteLine("┌────────────────────────────────────────┐");
        Console.WriteLine("│ Menü                                   │");
        Console.WriteLine("├────────────────────────────────────────┤");
        Console.WriteLine("│ 1) Spiel erstellen                     │");
        Console.WriteLine("│ 2) Spiel beitreten                     │");
        Console.WriteLine("│ 3) Spielhistorie                       │");
        Console.WriteLine("│ Q) Beenden                             │");
        Console.WriteLine("└────────────────────────────────────────┘");
        Console.WriteLine();
        Console.Write("Auswahl: ");
        var key = Console.ReadKey(true);
        Console.WriteLine(key.KeyChar);
        return key.KeyChar.ToString();
    }

    private async Task ShowMatchHistoryAsync()
    {
        try
        {
            _matchHistoryTcs = new TaskCompletionSource<MatchHistoryResponseMessage>();
            var req = new ListMatchHistoryMessage();
            await SendMessageAsync(req);
            Logger.LogInfo("Sent ListMatchHistory request");

            var resp = await _matchHistoryTcs.Task;
            var matches = resp?.Matches ?? new System.Collections.Generic.List<MatchStats>();

            if (matches.Count == 0)
            {
                Console.Clear();
                Console.WriteLine("Keine Matches gefunden.");
                Console.WriteLine("Drücke eine Taste, um zurückzugehen...");
                Console.ReadKey(true);
                return;
            }

            while (true)
            {
                Console.Clear();
                Console.WriteLine("┌──────────────────────────────────────────────────────────────────────────┐");
                Console.WriteLine("│ Spielhistorie                                                            │");
                Console.WriteLine("├─────┬──────────────────────────────────────┬──────────────────┬──────────┤");
                Console.WriteLine("│ Num │ GameId                               │ Startzeit        │ Gewinner │");
                Console.WriteLine("├─────┼──────────────────────────────────────┼──────────────────┼──────────┤");
                for (int i = 0; i < matches.Count; i++)
                {
                    var ms = matches[i];
                    var winner = ms?.WinnerColor.HasValue == true ? ms.WinnerColor.Value.ToString() : "-";
                    var start = ClientTime.FormatLocal(ms?.StartTime);
                    Console.WriteLine($"│ {i + 1,3} │ {ms?.GameId,-36} │ {start,-16} │ {winner,-8} │");
                }
                Console.WriteLine("└─────┴──────────────────────────────────────┴──────────────────┴──────────┘");
    
                Console.WriteLine();
                Console.WriteLine("Gib die Nummer eines Spiels ein, um Details anzusehen, oder 'b' um zurückzugehen:");
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) continue;
                if (input.Equals("b", StringComparison.OrdinalIgnoreCase)) return;
                if (int.TryParse(input, out int sel))
                {
                    if (sel < 1 || sel > matches.Count)
                    {
                        Console.WriteLine("Ungültige Auswahl, drücke eine Taste...");
                        Console.ReadKey(true);
                        continue;
                    }

                    var ms = matches[sel - 1];
                    ShowMatchDetailsFromStats(ms);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("Error while requesting match history: " + ex.Message);
        }
    }

    private void ShowMatchDetailsFromStats(MatchStats ms)
    {
        Console.Clear();
        
        Console.WriteLine($"┌───────────────────────────────────────────────────────────────┐");
        Console.WriteLine($"│ Spieldetails                                                  │");
        Console.WriteLine($"├──────────────┬────────────────────────────────────────────────┤");
        Console.WriteLine($"│ GameId:      │ {ms.GameId,-46} │");
        Console.WriteLine($"│ Startzeit:   │ {ClientTime.FormatLocal(ms.StartTime),-46} │");
        Console.WriteLine($"│ Endzeit:     │ {ClientTime.FormatLocal(ms.EndTime),-46} │");
        Console.WriteLine($"│ Anzahl Züge: │ {ms.TotalTurns,-46} │");
        Console.WriteLine($"│ Gewinner:    │ {(ms.WinnerColor.HasValue ? ColorHelper.ColorToString(ms.WinnerColor.Value) : "-"),-46} │");
        Console.WriteLine($"│ Spieler:     ├───────┬──────────────────┬─────────────────────┤");
        Console.WriteLine($"│              │ Farbe │ Gelaufene Felder │ Geschlagene Figuren │");
        Console.WriteLine($"│              ├───────┼──────────────────┼─────────────────────┤");
        foreach (var p in ms.Players)
        {
            Console.WriteLine($"│              │ {ColorHelper.ColorToString(p.Color),-5} │ {p.MovementCount,-16} │ {p.Captures,-19} │");
        }
        Console.WriteLine($"└──────────────┴───────┴──────────────────┴─────────────────────┘");

        Console.WriteLine();
        Console.WriteLine("Drücke eine Taste um zurückzugehen...");
        Console.ReadKey(true);
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
            case MatchHistoryResponseMessage mh:
                _matchHistoryTcs?.TrySetResult(mh);
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

    private string ShowGameList(ListGamesResponseMessage response)
    {
        const string headerFormat = "│ {0,-3} │ {1,-36} │ {2,7} │";
        Console.Clear();
        Console.WriteLine("┌──────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Verfügbare Spiele                                    │");
        Console.WriteLine("├─────┬──────────────────────────────────────┬─────────┤");
        Console.WriteLine(headerFormat, "Nr.", "GameId", "Spieler");
        Console.WriteLine("├─────┼──────────────────────────────────────┼─────────┤");
        
        if (response.Games == null || response.Games.Count == 0)
        {
            Console.WriteLine(headerFormat, "", "", "");
        }
        else
        {
            int i = 1;
            foreach (var game in response.Games)
            {
                Console.WriteLine(headerFormat, i, game.Key, $"{game.Value}/4");
                i++;
            }
        }
        
        Console.WriteLine("└─────┴──────────────────────────────────────┴─────────┘");
        Console.WriteLine();
        Console.WriteLine("Geben Sie eine Nummer ein, um einem Spiel beizutreten, oder 'b' um zurück zum Menü zu gehen:");
        var input = Console.ReadLine();
        return input ?? string.Empty;
    }

}

