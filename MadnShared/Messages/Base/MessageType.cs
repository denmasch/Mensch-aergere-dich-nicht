namespace MadnShared.Messages.Base;

public static class MessageType
{
    public const string RollDice = "roll_dice";
    public const string DiceResult = "dice_result";
    public const string NextPlayer = "next_player";
    public const string MoveFigure = "move_figure";
    public const string CreateGame = "create_game";
    public const string JoinGame = "join_game";
    public const string GameCreated = "game_created";
    public const string GameJoined = "game_joined";
    
    // Error messages
    public const string UnknownMessageType = "unknown_message_type";
}