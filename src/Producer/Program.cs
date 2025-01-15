using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

var configurationBuilder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json");

var options = configurationBuilder.Build().GetAWSOptions();
var services = new ServiceCollection()
    .AddDefaultAWSOptions(options)
    .AddAWSService<IAmazonSimpleNotificationService>();

var provider = services.BuildServiceProvider();
var topicArn = "<MY_TOPIC_ARN>";
var snsClient = provider.GetService<IAmazonSimpleNotificationService>()!;

for (int i = 0; i < 500; i++)
{
    var messageGroupId = (ConsoleColor)Random.Shared.Next(1, 15);
    var payload = new Payload() { Color = messageGroupId, Index = i };
    var message = JsonSerializer.Serialize(payload);

    var request = new PublishRequest
    {
        TopicArn = topicArn,
        Message = message,
        MessageGroupId = messageGroupId.ToString(),
        MessageDeduplicationId = Guid.NewGuid().ToString()
    };
    var response = await snsClient.PublishAsync(request);
    Console.ForegroundColor = messageGroupId;
    Console.WriteLine($"{response.MessageId} sent");
}

class Payload
{
    public ConsoleColor Color { get; set; }
    public int Index { get; set; }
}