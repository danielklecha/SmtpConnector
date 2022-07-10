using Microsoft.Extensions.Options;
using SmtpConnector.Models;
using SmtpServer;
using SmtpServer.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmtpConnector.Services;

public class MyUserAuthenticator : IUserAuthenticator
{
    private readonly IOptions<SmtpOptions> _smtpOptions;

    public MyUserAuthenticator( IOptions<SmtpOptions> smtpOptions )
    {
        _smtpOptions = smtpOptions;
    }

    public Task<bool> AuthenticateAsync( ISessionContext context, string user, string password, CancellationToken cancellationToken )
    {
        var options = _smtpOptions.Value;
        if ( string.IsNullOrEmpty( options.User ) )
            return Task.FromResult( true );
        if ( options.User.Equals( user ) && (options.Password?.Equals( password ) ?? true ) )
            return Task.FromResult( true );
        return Task.FromResult( false );
    }
}
