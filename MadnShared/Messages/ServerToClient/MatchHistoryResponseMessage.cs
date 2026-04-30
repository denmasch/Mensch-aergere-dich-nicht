using System.Collections.Generic;
using MadnShared.Messages.Base;
using MadnShared.Stats;

namespace MadnShared.Messages.ServerToClient;

public class MatchHistoryResponseMessage : ILobbyMessage
{
    public string Type => MessageType.MatchHistoryResponse;

    // Full match stats list (can be large) - client will present a summary and allow viewing details
    public List<MatchStats> Matches { get; set; } = new List<MatchStats>();
}

