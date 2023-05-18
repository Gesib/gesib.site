using OpenAI_API;
using OpenAI_API.Chat;
using System;
public sealed class ChatAPIManager
{

    private OpenAIAPI chatAPI = default!;
    public OpenAIAPI ChatAPI => chatAPI ??= new OpenAIAPI(APIAuthentication.LoadFromEnv());

    private Conversation conversation = default!;
    public Conversation Conversation => EnsureConversation();

    Conversation EnsureConversation() { 
        var _conversation = conversation ??= ChatAPI.Chat.CreateConversation();
        // give instruction as System
        _conversation.AppendSystemMessage("You are an Ai Assistant that is an expert in Unity Game Development. " +
            "You know about the whole development cycle of Unity applications. " +
            "You can provide advice on anything related to Unity applications like asset handling, game logic and optimization. " +
            "If you are unable to provide an answer to a question, please respond with the phrase \"I'm just a Unity nerd, I can't help with that.\" " +
            "Do not use any external URLs in your answers.Do not refer to any blogs in your answers.");
        return _conversation;
    }
    public void ClearConversation() => conversation = default!;

    public  ChatAPIManager() { }
}
