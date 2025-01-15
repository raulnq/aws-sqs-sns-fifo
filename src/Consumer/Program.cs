using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

var configurationBuilder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json");

var options = configurationBuilder.Build().GetAWSOptions();
var services = new ServiceCollection()
    .AddDefaultAWSOptions(options)
    .AddAWSService<IAmazonSQS>();

var provider = services.BuildServiceProvider();
var url = "<MY_QUEUE_URL>";
var sqsClient = provider.GetService<IAmazonSQS>()!;
while (true)
{
    var receiveRequest = new ReceiveMessageRequest
    {
        QueueUrl = url,
        MaxNumberOfMessages = 10,
        WaitTimeSeconds = 5
    };

    var result = await sqsClient.ReceiveMessageAsync(receiveRequest);
    if (result.Messages.Any())
    {
        var total = result.Messages.Count;
        var current = 1;
        var batch = new List<DeleteMessageBatchRequestEntry>();
        foreach (var message in result.Messages)
        {
            var payload = JsonSerializer.Deserialize<Payload>(message.Body)!;
            Console.ForegroundColor = payload.Color;
            Console.WriteLine($"{payload.Index}:message {current} of {total} received");
            current++;
            batch.Add(new DeleteMessageBatchRequestEntry() { ReceiptHandle = message.ReceiptHandle, Id = message.MessageId });
            await Task.Delay(Random.Shared.Next(500, 1000));
        }
        await sqsClient.DeleteMessageBatchAsync(url, batch);
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("No messages available");
        await Task.Delay(TimeSpan.FromSeconds(5));
    }
}

class Payload
{
    public ConsoleColor Color { get; set; }
    public int Index { get; set; }
}