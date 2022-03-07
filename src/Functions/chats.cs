using Newtonsoft.Json;

namespace WPPConnect
{
    public static class chats
    {
        public async static Task<ChatStore> getAllChats(this Models.Client client)
        {
            var request = await client.Connection.BrowserPage.EvaluateFunctionAsync("async => WPP.whatsapp.ChatStore.toJSON()");
            var jsonnew = new {chat = request};
            string json = JsonConvert.SerializeObject(jsonnew);
            ChatStore chatList = JsonConvert.DeserializeObject<ChatStore>(json);
            return chatList;
        }
    }
}
