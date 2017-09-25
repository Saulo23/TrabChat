using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerData;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Threading;



namespace client
{
    class Program
    {
        public static Socket clientS;
        public static string nome;
        public static int status; // 1 = online, 2 = offline
        public static string id;

        static void Main(string[] args)
        {
            Console.WriteLine("Digite seu nome: ");
            nome = Console.ReadLine();
            
            string ip = Packet.getIp4Address();

            clientS = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(ip), 4242 );

            try
            {
                clientS.Connect(ipe);
                Console.WriteLine(nome + " parabens, vc conseguiu se conectar com o servidor :)");
            }
            catch
            {
                Console.WriteLine("Nao foi possivel conectar com o servidor :(");
                Thread.Sleep(1000);
            }

            Thread t = new Thread(data_IN);
            t.Start();

            for(;;)
            {
                Console.Write("::>");
                string input = Console.ReadLine();

                Packet p = new Packet(PacketType.chat, id);
                p.Gdata.Add(nome);
                p.Gdata.Add(input);
                clientS.Send(p.toBytes());
            }
        }

        static void data_IN()
        {
            byte[] buffer;
            int readBytes;

            for(;;)
            {
                try
                {
                    buffer = new byte[clientS.SendBufferSize];
                    readBytes = clientS.Receive(buffer);

                    if (readBytes > 0)
                    {
                        DataManager(new Packet(buffer));
                    }
                }
                catch (SocketException ex)
                {
                    Console.WriteLine("Servidor perdeu conexao!!");
                    Console.ReadLine();
                    Environment.Exit(0);
                }
            }

        }

        static void DataManager(Packet p)
        {
            switch (p.packetType)
            {
                case PacketType.Registration:
                    Console.WriteLine("Conectado ao servidor!");
                    id = p.Gdata[0];
                    break;
                case PacketType.chat:
                    Console.WriteLine(p.Gdata[0] + ": " + p.Gdata[1]);
                    break;
            }
        }
    }
}
