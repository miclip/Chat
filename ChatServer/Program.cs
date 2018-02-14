using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Configuration;
using ChatMessages;

namespace ChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var instanceIp = Environment.GetEnvironmentVariable("CF_INSTANCE_IP");

            Console.WriteLine($"InstanceIp is {instanceIp}");

            //var port = Environment.GetEnvironmentVariable("PORT") ?? "0";

            //Console.WriteLine($"PORT is {port}");
            
            string configString = @"
akka {  
    actor {
        provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
    }
    remote {
        dot-netty.tcp {
            port = " + 1024 + @"
            hostname = 0.0.0.0
            public-hostname = localhost
        }
    }
}
";
            //configString.Replace("127.0.0.1", instanceIp)
            var config = ConfigurationFactory.ParseString(configString);

            using (var system = ActorSystem.Create("MyServer", config))
            {
                system.ActorOf<ChatServerActor>("ChatServer");

                Console.ReadLine();
            }
        }
    }

    class ChatServerActor : TypedActor,
        IHandle<SayRequest>,
        IHandle<ConnectRequest>,
        IHandle<NickRequest>,
        IHandle<Disconnect>,
        IHandle<ChannelsRequest>,
        ILogReceive

    {
        private readonly HashSet<IActorRef> _clients = new HashSet<IActorRef>();

        public void Handle(SayRequest message)
        {
            //  Console.WriteLine("User {0} said {1}",message.Username , message.Text);
            var response = new SayResponse
            {
                Username = message.Username,
                Text = message.Text,
            };
            foreach (var client in _clients) client.Tell(response, Self);
        }

        public void Handle(ConnectRequest message)
        {
            //   Console.WriteLine("User {0} has connected", message.Username);
            _clients.Add(this.Sender);
            Sender.Tell(new ConnectResponse
            {
                Message = "Hello and welcome to Akka .NET chat example",
            }, Self);
        }

        public void Handle(NickRequest message)
        {
            var response = new NickResponse
            {
                OldUsername = message.OldUsername,
                NewUsername = message.NewUsername,
            };

            foreach (var client in _clients) client.Tell(response, Self);
        }

        public void Handle(Disconnect message)
        {

        }

        public void Handle(ChannelsRequest message)
        {

        }
    }
}