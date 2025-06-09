using hw3.TwoPhaseCommit.Shared;
using Microsoft.AspNetCore.SignalR.Client;

namespace hw3.TwoPhaseCommit.Client2;

public class Worker(ILogger<Worker> logger) : BackgroundService
{
    private static readonly Random Rnd = new();
    private readonly Dictionary<string, int> _persons = [];
    private Person? _dataToProcess;

    private HubConnection? _connection; 

    protected override async Task ExecuteAsync(CancellationToken ctx)
    {
        try
        {
            if (_connection is null)
            {
                _connection ??= new HubConnectionBuilder()
                    .WithUrl("http://localhost:5087/person")
                    .WithAutomaticReconnect()
                    .Build();

                _connection.On<Person, bool>("Prepare", Prepare);
                _connection.On("Commit", Commit);
                _connection.On("Rollback", Rollback);

                await _connection.StartAsync(ctx);
            }
                
            await Task.Delay(120000, ctx);
        }
        catch (Exception ex)
        {
            logger.LogInformation("{ex}", ex);
        }
    }

    private async Task<bool> Prepare(Person person)
    {
        var val = Rnd.Next(1, 100);
        if (val > 70)
        {
            logger.LogInformation("Unluck: {value}", val);
            return false;
        }

        if (_dataToProcess is not null)
        {
            logger.LogWarning("I already was prepared. Dropping received data...");
            _dataToProcess = null;
        }
        
        logger.LogInformation("Working: {value}", val);
        _dataToProcess = person;
        await Task.Delay(Rnd.Next(1000, 3000));
        return true;
    }

    private Task Commit()
    {
        if (_dataToProcess is null)
        {
            logger.LogWarning("Call 'Prepare' first. No data to commit.");
            return Task.CompletedTask;
        }

        if (!_persons.TryAdd(_dataToProcess.Name, _dataToProcess.Age))
            _persons[_dataToProcess.Name] = _dataToProcess.Age;
        
        logger.LogInformation("Updated: {@person}", _dataToProcess);
        _dataToProcess = null;
        return Task.CompletedTask;
    }

    private Task Rollback()
    {
        logger.LogWarning("Rollback request received. Dropping received data...");
        _dataToProcess = null;
        return Task.CompletedTask;
    }
}