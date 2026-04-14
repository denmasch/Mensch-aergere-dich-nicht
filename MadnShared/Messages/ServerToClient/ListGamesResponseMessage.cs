using MadnShared.Messages.Base;

namespace MadnShared.Messages.ServerToClient;

public class ListGamesResponseMessage : ILobbyMessage
{
    public string Type => MessageType.ListGamesResponse;
    
    public Dictionary<Guid, int> Games { get; set; }
}