using SmtpServer;
using SmtpServer.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmtpConnector.Services;

public class MyUserAuthenticatorFactory : IUserAuthenticatorFactory
{
    private readonly IUserAuthenticator _userAuthenticator;

    public MyUserAuthenticatorFactory( IUserAuthenticator userAuthenticator )
    {
        _userAuthenticator = userAuthenticator;
    }
    public IUserAuthenticator CreateInstance( ISessionContext context )
    {
        return _userAuthenticator;
    }
}
