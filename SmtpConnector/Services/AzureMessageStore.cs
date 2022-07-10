using Azure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using SmtpConnector.Models;
using SmtpServer;
using SmtpServer.Protocol;
using System.Buffers;
using System.Text;

namespace SmtpConnector.Services;

public class AzureMessageStore : SmtpServer.Storage.MessageStore
{
    private readonly GraphServiceClient _graphServiceClient;
    private readonly IOptions<AzureOptions> _azureOptions;
    private readonly ILogger<AzureMessageStore> _logger;

    public AzureMessageStore(
        IOptions<AzureOptions> azureOptions,
        ILogger<AzureMessageStore> logger)
    {
        _azureOptions = azureOptions;
        _logger = logger;
        var credentials = new ClientSecretCredential(
                azureOptions.Value.TenantId,
                azureOptions.Value.ClientId,
                azureOptions.Value.ClientSecret,
                new TokenCredentialOptions { AuthorityHost = AzureAuthorityHosts.AzurePublicCloud } );
        _graphServiceClient = new( credentials, new[] { "Mail.Send", "User.Read.All" } );
    }
    public override async Task<SmtpResponse> SaveAsync( ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken )
    {
        try
        {
            await using var stream = new MemoryStream();
            var position = buffer.GetPosition( 0 );
            while ( buffer.TryGet( ref position, out var memory ) )
                await stream.WriteAsync( memory, cancellationToken );
            stream.Position = 0;
            StringContent stringContent = new( Convert.ToBase64String( stream.ToArray() ), Encoding.UTF8, "text/plain" );
            var email = _azureOptions.Value.Email;
            if ( string.IsNullOrEmpty( email ) )
            {
                stream.Position = 0;
                var mimeMessage = await MimeKit.MimeMessage.LoadAsync( stream, cancellationToken );
                email = mimeMessage.From.FirstOrDefault()?.Name;
                if( string.IsNullOrEmpty( email ) )
                    return SmtpResponse.MailboxUnavailable;
            }
            var user = ( await _graphServiceClient.Users.Request().Filter( $"mail eq '{email}'" ).GetAsync( cancellationToken ) ).FirstOrDefault();
            if ( user == null )
                return SmtpResponse.MailboxUnavailable;
            var sendMailRequest = _graphServiceClient.Users[ user.Id ].SendMail( new Message(), _azureOptions.Value.SaveMessgeToSendItems ).Request().GetHttpRequestMessage();
            sendMailRequest.Content = stringContent;
            sendMailRequest.Method = HttpMethod.Post;
            var httpResponseMessage = await _graphServiceClient.HttpProvider.SendAsync( sendMailRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken );
            return httpResponseMessage.StatusCode switch
            {
                System.Net.HttpStatusCode.OK or System.Net.HttpStatusCode.Accepted or System.Net.HttpStatusCode.Created => SmtpResponse.Ok,
                _ => SmtpResponse.TransactionFailed,
            };
        }
        catch ( Exception ex )
        {
            _logger.LogError( ex, "Unable to send email" );
            return SmtpResponse.TransactionFailed;
        }
    }
}