using Confluent.Kafka;

Console.WriteLine("Consumer started");

var config = new ConsumerConfig
{
    BootstrapServers = "localhost:9092",
    GroupId = "test-group",
    AutoOffsetReset = AutoOffsetReset.Earliest,
    EnableAutoCommit = false
};

using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
consumer.Subscribe("test-topic");

try
{
    for (var i = 1; i <= 3; i++)
    {
        var result = consumer.Consume();
        var receivedTime = DateTime.Now;
        var messageTime = DateTime.Parse(result.Message.Value.Split('-')[1].Trim());
        var age = (receivedTime - messageTime).TotalSeconds;
        
        Console.WriteLine($"Received: {result.Message.Value} | Age: {age:F1}s");
        
        // delay
        Thread.Sleep(4000);
        
        // offset commit
        consumer.Commit(result);
    }
}
catch (ConsumeException e)
{
    Console.WriteLine($"Error: {e.Error.Reason}");
}

consumer.Close();