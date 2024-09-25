using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

public class MessageService : IHostedService {

    // Två variabler för att spara kopplingen till RabbitMQ
    private IConnection connection;
    private IModel channel;

    // Anslut till RabbitMQ
    public void Connect() {
        var factory = new ConnectionFactory { HostName = "localhost" };
        connection = factory.CreateConnection();
        channel = connection.CreateModel();

        // Skapa en exchange för att kunna skicka meddelanden
        // till andra microservices
        channel.ExchangeDeclare("create-user", ExchangeType.Fanout);
        //Skapa exchange för token
        channel.ExchangeDeclare("get-token", ExchangeType.Fanout);
    }

    // Skicka ett meddelande till andra microservices om att en användare har skapats
    public void NotifyUserCreation(SendUserDto user) {
        var json = JsonSerializer.Serialize(user);
        var message = Encoding.UTF8.GetBytes(json);

        channel.BasicPublish("create-user", string.Empty, null, message);
    }

    //Metod för att skicka ut token till andra microservices vid inloggning
    public void ExportToken(string token)
    {
        var message = Encoding.UTF8.GetBytes(token);
        channel.BasicPublish("get-token", string.Empty, null, message);
    }

    //Logging
    public void SendLoggingActions(string action)
    {
        var message = Encoding.UTF8.GetBytes(action);
        channel.BasicPublish("logging", string.Empty, null, message);
    }

    // Anropas när programmet startas
    public Task StartAsync(CancellationToken cancellationToken)
    {
        Connect();
        return Task.CompletedTask;
    }

    // Anropas när programmet stoppas, och då kopplar vi bort från
    // RabbitMQ
    public Task StopAsync(CancellationToken cancellationToken)
    {
        channel.Close();
        connection.Close();
        return Task.CompletedTask;
    }
}