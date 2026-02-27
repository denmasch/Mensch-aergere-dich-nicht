using MadnShared.Messages.Base;
using MadnShared.Messages.ClientToServer;
using MadnShared.Utils;

namespace MadnSharedTest;

[TestClass]
public sealed class MessageSerializerTest
{
    [TestMethod]
    public void SerializeTest()
    {
        Guid gameId = Guid.NewGuid();
        Guid playerId = Guid.NewGuid();
        StartGameMessage msg = new StartGameMessage();
        msg.PlayerId = playerId;
        msg.GameId = gameId;

        string expected = "{\"Type\":\"start_game\",\"GameId\":\""+ gameId.ToString() +"\",\"PlayerId\":\""+ playerId +"\"}";
        
        string result = MessageSerializer.Serialize(msg);
        
        Assert.AreEqual(expected, result);
    }
}