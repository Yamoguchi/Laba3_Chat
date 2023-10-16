using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SimpleChatClient
{
    class Program
    {
        private static TcpClient? client;
        private static NetworkStream? stream;
        private static string? nickname;

        static void Main(string[] args)
        {
            try
            {
                client = new TcpClient("127.0.0.1", 8888); // Подключение к серверу
                stream = client.GetStream();

                Console.WriteLine("Input your Nick Name");
                nickname = Console.ReadLine();
                byte[] nicknameBytes = Encoding.UTF8.GetBytes(nickname);
                stream.Write(nicknameBytes, 0, nicknameBytes.Length);

                // Поток для чтения сообщений от сервера
                Thread receiveThread = new Thread(ReceiveMessage);
                receiveThread.Start();

                // Отправка сообщений на сервер
                while (true)
                {
                    string? message = Console.ReadLine();
                    //message = nickname + ": " + message;
                    if (!string.IsNullOrEmpty(message))
                    {
                        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                        stream.Write(messageBytes, 0, messageBytes.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        // Функция для чтения сообщений от сервера
        private static void ReceiveMessage()
        {
            while (true)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                if (!message.StartsWith(nickname + ":"))
                {
                    Console.WriteLine(message);
                }
            }
        }
    }
}
