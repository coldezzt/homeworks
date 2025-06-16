using Confluent.Kafka;

Console.WriteLine("Producer started");

var config = new ProducerConfig
{
    BootstrapServers = "localhost:9092"
};

using var producer = new ProducerBuilder<Null, string>(config).Build();

for (var i = 1; i <= 3; i++)
{
    var timestamp = DateTime.Now;
    var message = new Message<Null, string> 
    { 
        Value = $"Message {i} - {timestamp:HH:mm:ss.fff}" 
    };
    
    producer.Produce("test-topic", message);
    Console.WriteLine($"Sent: {message.Value}");
    
    // delay
    Thread.Sleep(100); 
}

producer.Flush();
Console.WriteLine("3 messages sent");