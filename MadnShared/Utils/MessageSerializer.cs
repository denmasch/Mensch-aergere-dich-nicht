using System.Text.Json;
using MadnShared.Messages.Base;
using MadnShared.Messages.ClientToServer;
using MadnShared.Messages.ServerToClient;

namespace MadnShared.Utils;

public static class MessageSerializer
{
    public static string Serialize(IMessage message)
    {
        return JsonSerializer.Serialize(message, message.GetType());
    }

    public static IMessage? Deserialize(string json)
    {
        var doc = JsonDocument.Parse(json);
        var type = doc.RootElement.GetProperty("Type").GetString();
        IMessage? message;
        
        switch (type)
        {
            case MessageType.DiceResult:
                message = JsonSerializer.Deserialize<DiceResultMessage>(json);
                break;
            case MessageType.RollDice:
                message = JsonSerializer.Deserialize<RollDiceMessage>(json);
                break;
            case MessageType.StartGame:
                message = JsonSerializer.Deserialize<StartGameMessage>(json);
                break;
            case MessageType.GameCreated:
                message = JsonSerializer.Deserialize<GameCreatedMessage>(json);
                break;
            case MessageType.JoinGame:
                message = JsonSerializer.Deserialize<JoinGameMessage>(json);
                break;
            case MessageType.GameJoined:
                message = JsonSerializer.Deserialize<GameJoinedMessage>(json);
                break;
            case MessageType.NextPlayer:
                message = JsonSerializer.Deserialize<NextPlayerMessage>(json);
                break;
            case MessageType.MoveFigure:
                message = JsonSerializer.Deserialize<MoveFigureMessage>(json);
                break;
            case MessageType.GameboardUpdated:
                message = JsonSerializer.Deserialize<GameboardUpdatedMessage>(json);
                break;
            case MessageType.LeaveGame:
                message = JsonSerializer.Deserialize<LeaveGameMessage>(json);
                break;
            case MessageType.GameLeft:
                message = JsonSerializer.Deserialize<GameLeftMessage>(json);
                break;
            case MessageType.CreateGame:
                message = JsonSerializer.Deserialize<CreateGameMessage>(json);
                break;
            default:
                message = null;
                break;
        }

        return message;
    }
}