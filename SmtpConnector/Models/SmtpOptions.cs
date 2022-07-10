using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmtpConnector.Models;

public class SmtpOptions
{
    public string? User { get; set; }
    public string? Password { get; set; }
    public int Port { get; set; }
    public bool IsPortSecure { get; set; }
}