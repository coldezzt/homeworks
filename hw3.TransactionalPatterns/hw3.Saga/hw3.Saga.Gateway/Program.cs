using MassTransit;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        Path.Combine("./", "logs", "diagnostics.txt"),
        rollingInterval: RollingInterval.Day,
        fileSizeLimitBytes: 10 * 1024 * 1024,
        retainedFileCountLimit: 2,
        rollOnFileSizeLimit: true,
        shared: true,
        flushToDiskInterval: TimeSpan.FromSeconds(1))
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMassTransit(cfg =>
    {
        cfg.SetKebabCaseEndpointNameFormatter();
        cfg.AddDelayedMessageScheduler();
        cfg.UsingRabbitMq((brc, rbfc) =>
        {
            rbfc.UseMessageRetry(r =>
            {
                r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            });
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

app.UseAuthorization();
app.MapControllers();
app.UseSwagger().UseSwaggerUI();

app.Run();