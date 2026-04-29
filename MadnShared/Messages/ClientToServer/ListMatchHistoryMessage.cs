using MadnShared.Messages.Base;

namespace MadnShared.Messages.ClientToServer;

public class ListMatchHistoryMessage : ILobbyMessage
{
    public string Type => MessageType.ListMatchHistory;
    // optional: paging/filtering could be added later
}

