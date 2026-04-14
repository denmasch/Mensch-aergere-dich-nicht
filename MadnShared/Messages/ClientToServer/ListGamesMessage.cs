using MadnShared.Messages.Base;

namespace MadnShared.Messages.ClientToServer;

public class ListGamesMessage : ILobbyMessage
{
    public string Type => MessageType.ListGames;
    
}