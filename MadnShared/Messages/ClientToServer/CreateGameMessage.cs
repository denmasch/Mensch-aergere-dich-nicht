using System;
using MadnShared.Messages.Base;

namespace MadnShared.Messages.ClientToServer;

public class CreateGameMessage : ILobbyMessage
{
    public string Type => MessageType.CreateGame;
}

