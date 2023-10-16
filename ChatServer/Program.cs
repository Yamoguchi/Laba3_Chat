using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SimpleChatServer
{
    class Program
    {
        private static readonly List<TcpClient> clients = new List<TcpClient>(); // список клиентов
        private static readonly List<string> nicknames = new List<string>(); // список никнеймов клиентов

        static void Main(string[] args)
        {
            TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 8888); // создание сервера
            server.Start(); // запуск сервера
            Console.WriteLine("Сервер запущен.");

            while (true) // бесконечный цикл для прослушивания новых подключений
            {
                TcpClient client = server.AcceptTcpClient(); // принятие нового клиента
                clients.Add(client); // добавление клиента в список

                // запуск обработчика клиента в отдельном потоке
                Thread clientThread = new Thread(HandleClient);
                clientThread.Start(client);
            }
        }

        // Обработчик клиента
        private static void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;
            NetworkStream stream = client.GetStream();
            string nickname;

            byte[] nicknameRequest = Encoding.UTF8.GetBytes("NICK"); // запрос на никнейм
            stream.Write(nicknameRequest, 0, nicknameRequest.Length); // отправка запроса на никнейм

            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            nickname = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            nicknames.Add(nickname); // добавление никнейма в список

            BroadcastMessage($"{nickname} присоединился к чату!\n"); // отправка сообщения о присоединении нового клиента

            while (client.Connected)
            {
                try
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    BroadcastMessage($"{nickname}: {message}");
                }
                catch
                {
                    client.Close();
                    nicknames.Remove(nickname);
                    clients.Remove(client);
                    BroadcastMessage($"{nickname} покинул чат.\n");
                    break;
                }
            }
        }

        // Отправка сообщения всем клиентам
        private static void BroadcastMessage(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            foreach (TcpClient client in clients) // отправка сообщения каждому клиенту
            {
                NetworkStream stream = client.GetStream();
                stream.Write(data, 0, data.Length);
            }
        }
    }
}