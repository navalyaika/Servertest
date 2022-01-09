using System;
using System.Net;
using System.IO;
using System.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Net.Mail;

namespace Servertest
{
    class Program
    {
        private const string QueueName = "queue1";

        private const string FromEmail = "yourmail@gmail.com";
        private const string FromPassword = "yourpass";


        static void Main(string[] args)
        {
            
                var facrory = new ConnectionFactory
                {
                    Uri = new Uri("amqp://user:user@localhost:5672")
                };
                using var connection = facrory.CreateConnection();
                using var channel = connection.CreateModel();
                channel.QueueDeclare(QueueName, true, false, false, null);
                
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, e) =>
                {
                    var body = e.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var tmp = message.Split("\"");
                    string email = tmp[3];
                    string msg = tmp[7];
                    //send email
                    SendMessage(email, msg);

                    Console.WriteLine($"{tmp[7]} send to {tmp[3]}");
                };

                channel.BasicConsume(QueueName, true, consumer);
            Console.ReadLine();
        }

        private static void SendMessage(string email, string message)
        {
            try
            {

                // отправитель - устанавливаем адрес и отображаемое в письме имя
                MailAddress from = new MailAddress(FromEmail);
                // кому отправляем
                MailAddress to = new MailAddress(email);
                // создаем объект сообщения
                MailMessage m = new MailMessage(from, to)
                {
                    // тема письма
                    Subject = "",
                    // текст письма
                    Body = message,
                    // письмо представляет код html
                    IsBodyHtml = false
                };
                // адрес smtp-сервера и порт, с которого будем отправлять письмо
                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    // логин и пароль
                    Credentials = new NetworkCredential(FromEmail, FromPassword),
                    EnableSsl = true,
                };
                smtp.Send(m);
            }
            catch(FormatException)
            {
                Console.WriteLine("Неверный формат электронной почты.Почта должна иметь окончания - @gmail / yandex / mail / bk / list и другие");
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Строка с адресом не должна быть пуста");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.HelpLink);
            }
        }
    }
}
