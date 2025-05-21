using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Proyecto_Final.AgentSystem.Core
{
    public class A2ACommunication
    {
        private readonly Dictionary<string, Agent> _registeredAgents;
        private readonly Queue<Message> _messageQueue;
        private readonly object _lockObject = new object();
        private readonly Thread _dispatchThread;
        private bool _isRunning;
        
        public A2ACommunication()
        {
            _registeredAgents = new Dictionary<string, Agent>();
            _messageQueue = new Queue<Message>();
            _dispatchThread = new Thread(DispatchMessages);
            _isRunning = false;
        }
        
        public void Start()
        {
            if (_isRunning)
                return;
                
            _isRunning = true;
            _dispatchThread.Start();
        }
        
        public void Stop()
        {
            if (!_isRunning)
                return;
                
            _isRunning = false;
            _dispatchThread.Join();
        }
        
        public void RegisterAgent(Agent agent)
        {
            if (agent == null)
                throw new ArgumentNullException(nameof(agent));
                
            lock (_lockObject)
            {
                if (_registeredAgents.ContainsKey(agent.Id))
                    throw new InvalidOperationException($"Ya existe un agente registrado con el ID {agent.Id}");
                    
                _registeredAgents.Add(agent.Id, agent);
                agent.SetCommunicationSystem(this);
            }
        }
        
        public void UnregisterAgent(string agentId)
        {
            lock (_lockObject)
            {
                if (_registeredAgents.ContainsKey(agentId))
                {
                    _registeredAgents.Remove(agentId);
                }
            }
        }
        
        public void SendMessage(Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
                
            lock (_lockObject)
            {
                _messageQueue.Enqueue(message);
            }
        }
        
        private void DispatchMessages()
        {
            while (_isRunning)
            {
                Message message = null;
                
                lock (_lockObject)
                {
                    if (_messageQueue.Count > 0)
                    {
                        message = _messageQueue.Dequeue();
                    }
                }
                
                if (message != null)
                {
                    DeliverMessage(message);
                }
                
                // Pequeña pausa para no consumir demasiados recursos
                Thread.Sleep(50);
            }
        }
        
        private void DeliverMessage(Message message)
        {
            if (message.ReceiverAgentId == "*")
            {
                // Mensaje de difusión (broadcast) para todos los agentes
                lock (_lockObject)
                {
                    foreach (var agent in _registeredAgents.Values)
                    {
                        if (agent.Id != message.SenderAgentId) // No enviar a sí mismo
                        {
                            agent.ReceiveMessage(message);
                        }
                    }
                }
            }
            else
            {
                // Mensaje dirigido a un agente específico
                lock (_lockObject)
                {
                    if (_registeredAgents.TryGetValue(message.ReceiverAgentId, out Agent receiver))
                    {
                        receiver.ReceiveMessage(message);
                    }
                    else
                    {
                        // El agente destinatario no está registrado
                        // Aquí se podría implementar un sistema de logging
                        Console.WriteLine($"No se pudo entregar el mensaje al agente {message.ReceiverAgentId} porque no está registrado.");
                    }
                }
            }
        }
        
        public List<Agent> GetAllAgents()
        {
            lock (_lockObject)
            {
                return _registeredAgents.Values.ToList();
            }
        }
        
        public Agent GetAgent(string agentId)
        {
            lock (_lockObject)
            {
                if (_registeredAgents.TryGetValue(agentId, out Agent agent))
                {
                    return agent;
                }
                return null;
            }
        }
    }
}