using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using SmtpConnector.Models;
using SmtpServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmtpConnector.Services;

public class SmtpService : BackgroundService, IHealthCheck
{
    private readonly IServer _server;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<SmtpOptions> _smtpOptions;
    private readonly ILogger<SmtpService> _logger;
    private HealthCheckResult _healthCheckResult;

    public SmtpService(
        IServer server,
        IHostApplicationLifetime hostApplicationLifetime,
        IServiceProvider serviceProvider,
        IOptions<SmtpOptions> smtpOptions,
        ILogger<SmtpService> logger )
    {
        _server = server;
        _hostApplicationLifetime = hostApplicationLifetime;
        _serviceProvider = serviceProvider;
        _smtpOptions = smtpOptions;
        _logger = logger;
        _healthCheckResult = HealthCheckResult.Unhealthy( "SMTP server was not initiated" );
    }

    public Task<HealthCheckResult> CheckHealthAsync( HealthCheckContext context, CancellationToken cancellationToken = default )
    {
        
        return Task.FromResult( _healthCheckResult );
    }

    protected override async Task ExecuteAsync( CancellationToken stoppingToken )
    {
        await WaitForApplicationStarted();
        try
        {
            var options = new SmtpServerOptionsBuilder()
                .ServerName( GetHost() )
                .Port( _smtpOptions.Value.Port, isSecure: _smtpOptions.Value.IsPortSecure )
                .MaxAuthenticationAttempts( 3 )
                .Build();
            var smtpServer = new SmtpServer.SmtpServer( options, _serviceProvider );
            _healthCheckResult = HealthCheckResult.Healthy( $"SMTP server is running on {_smtpOptions.Value.Port}" );
            await smtpServer.StartAsync( stoppingToken );
        }
        catch ( Exception ex )
        {
            _logger.LogError( ex, "Critical issue with SMTP server" );
            _healthCheckResult = HealthCheckResult.Unhealthy( exception: ex );
        }
    }

    private string GetHost()
    {
        var addresses = _server.Features.Get<IServerAddressesFeature>()?.Addresses;
        if ( Uri.TryCreate( addresses?.FirstOrDefault(), UriKind.Absolute, out var uri ) )
            return uri.Host;
        throw new Exception( "Unable to get host" );
    }

    private Task WaitForApplicationStarted()
    {
        var completionSource = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        _hostApplicationLifetime.ApplicationStarted.Register( () => completionSource.TrySetResult() );
        return completionSource.Task;
    }
}