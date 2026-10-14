using PharmacyMiddleware.Clients;
using PharmacyMiddleware.Configuration;
using PharmacyMiddleware.Handlers;
using PharmacyMiddleware.Services;
using PharmacyMiddleware.State;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) =>
{
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .Enrich.FromLogContext()
       .WriteTo.Console()
       .WriteTo.File("logs/bale-middleware-.log", rollingInterval: RollingInterval.Day);
});

builder.Services
    .AddOptions<CoreApiOptions>()
    .Bind(builder.Configuration.GetSection(CoreApiOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<BaleOptions>()
    .Bind(builder.Configuration.GetSection(BaleOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddHttpClient<ICoreApiClient, CoreApiClient>()
    .AddStandardResilienceHandler(options =>
    {
        options.Retry.MaxRetryAttempts = 2;
        options.Retry.UseJitter = true;
        options.Retry.BackoffType = Polly.DelayBackoffType.Exponential;
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(15);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(45);
    });

builder.Services
    .AddHttpClient<IBaleBotClient, BaleBotClient>()
    .AddStandardResilienceHandler(options =>
    {
        options.Retry.MaxRetryAttempts = 2;
        options.Retry.UseJitter = true;
        options.Retry.BackoffType = Polly.DelayBackoffType.Exponential;
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(15);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(45);
    });

builder.Services.AddSingleton<IUserStateStore, InMemoryUserStateStore>();
builder.Services.AddScoped<IOrderTrackingService, OrderTrackingService>();
builder.Services.AddScoped<IMessageFormatter, MessageFormatter>();
builder.Services.AddScoped<IBaleUpdateHandler, BaleUpdateHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.MapControllers();

app.Run();