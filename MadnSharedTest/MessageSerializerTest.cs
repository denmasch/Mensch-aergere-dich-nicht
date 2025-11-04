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
        CreateGameMessage msg = new CreateGameMessage();
        msg.PlayerId = "1";

        string expected = "{\"PlayerId\":\"1\",\"Type\":\"create_game\"}";
        
        string result = MessageSerializer.Serialize(msg);
        
        Assert.AreEqual(expected, result);
    }
}