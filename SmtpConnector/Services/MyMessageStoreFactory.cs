using SmtpServer;
using SmtpServer.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmtpConnector.Services;

public class MyMessageStoreFactory : IMessageStoreFactory
{
    private readonly IMessageStore _messageStore;

    public MyMessageStoreFactory( IMessageStore messageStore )
    {
        _messageStore = messageStore;
    }
    public IMessageStore CreateInstance( ISessionContext context )
    {
        return _messageStore;
    }
}