using MadnShared.GameAssets;
using MadnShared.Messages.Base;

namespace MadnShared.Messages.ServerToClient;

public class GameboardUpdatedMessage : IGameMessage
{
    public string Type => MessageType.GameboardUpdated;
    
    public GameboardDTO Gameboard { get; set; }
}