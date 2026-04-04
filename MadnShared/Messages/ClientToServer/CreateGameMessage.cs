using System;
using MadnShared.Messages.Base;

namespace MadnShared.Messages.ClientToServer;

public class CreateGameMessage : IGameMessage
{
    public string Type => MessageType.CreateGame;
    public Guid GameId { get; set; }
}

