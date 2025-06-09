using hw3.Saga.Microservice1.Consumers;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(cfg =>
    {
        cfg.SetKebabCaseEndpointNameFormatter();
        cfg.AddDelayedMessageScheduler();
        cfg.AddConsumer<GetMoneyConsumer>();
        cfg.UsingRabbitMq((brc, rbfc) =>
        {
            rbfc.UseMessageRetry(r => { r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1)); });
            rbfc.UseDelayedMessageScheduler();
            rbfc.Host("localhost", h =>
            {
                h.Username("guest");
                h.Password("guest");
            }); 
            rbfc.ConfigureEndpoints(brc);
        });
    });

var app = builder.Build();

app.UseHttpsRedirection();

app.Run();