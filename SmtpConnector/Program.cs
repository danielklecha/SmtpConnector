using SmtpConnector.Models;
using SmtpConnector.Services;
using SmtpServer.Authentication;
using SmtpServer.Storage;

var builder = WebApplication.CreateBuilder( args );
builder.Services.Configure<SmtpOptions>( builder.Configuration.GetSection( "SmtpServer" ) );
builder.Services.Configure<AzureOptions>( builder.Configuration.GetSection( "AzureProvider" ) );
builder.Services.AddSingleton<IMessageStore, AzureMessageStore>();
builder.Services.AddSingleton<IMessageStoreFactory, MyMessageStoreFactory>();
builder.Services.AddSingleton<IUserAuthenticator, MyUserAuthenticator>();
builder.Services.AddSingleton<IUserAuthenticatorFactory, MyUserAuthenticatorFactory>();
builder.Services.AddSingleton<SmtpService>();
builder.Services.AddHostedService( provider => provider.GetRequiredService<SmtpService>() );
builder.Services.AddHealthChecks()
    .AddCheck<SmtpService>("SMTP server");
using var app = builder.Build();
app.MapHealthChecks( "/hc" );
app.MapGet( "/", () => "SMTP connector" );
await app.RunAsync();