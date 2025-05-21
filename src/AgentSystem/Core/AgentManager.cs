using System;
using System.Collections.Generic;
using System.Linq;

namespace Proyecto_Final.AgentSystem.Core
{
    public class AgentManager
    {
        private readonly A2ACommunication _communicationSystem;
        private readonly Dictionary<string, Agent> _managedAgents;
        
        public AgentManager()
        {
            _communicationSystem = new A2ACommunication();
            _managedAgents = new Dictionary<string, Agent>();
        }
        
        public void Start()
        {
            _communicationSystem.Start();
            
            // Iniciar todos los agentes registrados
            foreach (var agent in _managedAgents.Values)
            {
                agent.Start();
            }
        }
        
        public void Stop()
        {
            // Detener todos los agentes registrados
            foreach (var agent in _managedAgents.Values)
            {
                agent.Stop();
            }
            
            _communicationSystem.Stop();
        }
        
        public void RegisterAgent(Agent agent)
        {
            if (agent == null)
                throw new ArgumentNullException(nameof(agent));
                
            if (_managedAgents.ContainsKey(agent.Id))
                throw new InvalidOperationException($"Ya existe un agente registrado con el ID {agent.Id}");
                
            _managedAgents.Add(agent.Id, agent);
            _communicationSystem.RegisterAgent(agent);
        }
        
        public void UnregisterAgent(string agentId)
        {
            if (_managedAgents.TryGetValue(agentId, out Agent agent))
            {
                agent.Stop();
                _communicationSystem.UnregisterAgent(agentId);
                _managedAgents.Remove(agentId);
            }
        }
        
        public Agent GetAgent(string agentId)
        {
            if (_managedAgents.TryGetValue(agentId, out Agent agent))
            {
                return agent;
            }
            return null;
        }
        
        public List<Agent> GetAllAgents()
        {
            return _managedAgents.Values.ToList();
        }
        
        public void SendBroadcastMessage(string senderAgentId, MessageType type, string subject, Dictionary<string, object> content = null)
        {
            var message = new Message
            {
                SenderAgentId = senderAgentId,
                ReceiverAgentId = "*",
                Type = type,
                Subject = subject,
                Content = content ?? new Dictionary<string, object>()
            };
            
            _communicationSystem.SendMessage(message);
        }
    }
}