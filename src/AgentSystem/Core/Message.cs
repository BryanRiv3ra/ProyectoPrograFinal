using System;
using System.Collections.Generic;

namespace Proyecto_Final.AgentSystem.Core
{
    public enum MessageType
    {
        Request,
        Response,
        Notification,
        Command
    }

    public class Message
    {
        public Guid Id { get; private set; }
        public string SenderAgentId { get; set; }
        public string ReceiverAgentId { get; set; }
        public MessageType Type { get; set; }
        public string Subject { get; set; }
        public Dictionary<string, object> Content { get; set; }
        public DateTime CreatedAt { get; private set; }
        public bool IsProcessed { get; set; }
        public Guid? ReplyToMessageId { get; set; }

        public Message()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.Now;
            Content = new Dictionary<string, object>();
            IsProcessed = false;
        }

        public Message(string sender, string receiver, MessageType type, string subject)
            : this()
        {
            SenderAgentId = sender;
            ReceiverAgentId = receiver;
            Type = type;
            Subject = subject;
        }

        public void AddContent(string key, object value)
        {
            if (Content.ContainsKey(key))
            {
                Content[key] = value;
            }
            else
            {
                Content.Add(key, value);
            }
        }

        public T GetContent<T>(string key, T defaultValue = default)
        {
            if (Content.ContainsKey(key) && Content[key] is T)
            {
                return (T)Content[key];
            }
            return defaultValue;
        }

        public Message CreateResponse(string subject = null)
        {
            var response = new Message
            {
                SenderAgentId = this.ReceiverAgentId,
                ReceiverAgentId = this.SenderAgentId,
                Type = MessageType.Response,
                Subject = subject ?? $"Re: {this.Subject}",
                ReplyToMessageId = this.Id
            };
            return response;
        }
    }
}