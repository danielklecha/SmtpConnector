# SmtpConnector

SMPT connector allow to forward email using different provider.

## Configuration

You must set basic options in appsettings.json file: Port, IsPortSecured.
You can secure SMTP server using login and password.

## Health checks

You can check health state in /hc route.

## Azure

Provider allow to send emails using Azure app registration.
You must set ClientId, ClientSecret and TenantId.
If you set Email then this mailbox will be used. Otherwise, mailbox will be choose using email sender.
