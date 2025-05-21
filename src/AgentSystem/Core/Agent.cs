using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Proyecto_Final.AgentSystem.Core
{
    public abstract class Agent
    {
        public string Id { get; private set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsRunning { get; private set; }
        
        protected Queue<Message> IncomingMessages { get; private set; }
        protected List<Message> ProcessedMessages { get; private set; }
        protected A2ACommunication CommunicationSystem { get; private set; }
        
        private CancellationTokenSource _cancellationTokenSource;
        private Task _processingTask;
        
        public Agent(string id, string name, string description = "")
        {
            Id = id;
            Name = name;
            Description = description;
            IncomingMessages = new Queue<Message>();
            ProcessedMessages = new List<Message>();
            IsRunning = false;
        }
        
        public void SetCommunicationSystem(A2ACommunication communicationSystem)
        {
            CommunicationSystem = communicationSystem;
        }
        
        public void Start()
        {
            if (IsRunning)
                return;
                
            if (CommunicationSystem == null)
                throw new InvalidOperationException("No se ha establecido un sistema de comunicación para el agente.");
                
            IsRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            _processingTask = Task.Run(() => ProcessingLoop(_cancellationTokenSource.Token));
            
            OnStart();
        }
        
        public void Stop()
        {
            if (!IsRunning)
                return;
                
            IsRunning = false;
            _cancellationTokenSource.Cancel();
            _processingTask.Wait();
            _cancellationTokenSource.Dispose();
            
            OnStop();
        }
        
        public void ReceiveMessage(Message message)
        {
            if (message.ReceiverAgentId != Id && message.ReceiverAgentId != "*")
                return;
                
            lock (IncomingMessages)
            {
                IncomingMessages.Enqueue(message);
            }
        }
        
        public void SendMessage(Message message)
        {
            if (CommunicationSystem == null)
                throw new InvalidOperationException("No se ha establecido un sistema de comunicación para el agente.");
                
            message.SenderAgentId = Id;
            CommunicationSystem.SendMessage(message);
        }
        
        private void ProcessingLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Message message = null;
                
                lock (IncomingMessages)
                {
                    if (IncomingMessages.Count > 0)
                    {
                        message = IncomingMessages.Dequeue();
                    }
                }
                
                if (message != null)
                {
                    try
                    {
                        ProcessMessage(message);
                        message.IsProcessed = true;
                        ProcessedMessages.Add(message);
                    }
                    catch (Exception ex)
                    {
                        // Aquí se podría implementar un sistema de logging
                        Console.WriteLine($"Error al procesar mensaje en agente {Id}: {ex.Message}");
                    }
                }
                
                // Ejecutar comportamiento periódico del agente
                PeriodicBehavior();
                
                // Pequeña pausa para no consumir demasiados recursos
                Thread.Sleep(100);
            }
        }
        
        // Métodos abstractos que deben implementar las clases derivadas
        protected abstract void ProcessMessage(Message message);
        protected abstract void PeriodicBehavior();
        protected virtual void OnStart() { }
        protected virtual void OnStop() { }
    }
}