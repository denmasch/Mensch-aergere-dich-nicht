using System.Text.Json;
using MadnShared.Messages.Base;
using MadnShared.Messages.ClientToServer;
using MadnShared.Messages.ServerToClient;

namespace MadnShared.Utils;

public static class MessageSerializer
{
    public static string Serialize(GameMessage message)
    {
        return JsonSerializer.Serialize(message);
    }

    public static GameMessage? Deserialize(string json)
    {
        var doc = JsonDocument.Parse(json);
        var type = doc.RootElement.GetProperty("Type").GetString();
        GameMessage? message;
        
        switch (type)
        {
            case MessageType.DiceResult:
                message = JsonSerializer.Deserialize<DiceResultMessage>(json);
                break;
            case MessageType.RollDice:
                message = JsonSerializer.Deserialize<RollDiceMessage>(json);
                break;
            default:
                message = null;
                break;
        }

        return message;
    }
}