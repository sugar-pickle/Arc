# Webhook ARC

Implements a basic Surgard alarm receiver and sends incoming events to a webhook.

The receiver runs as a server listening on the port specified in appsettings.json. It will accept a connection from any source so use of an external firewall to restrict source addresses is recommended if using externally.

Runs in .net core 3.1 and is supported on any OS. 

Docker support coming soon!

## Usage

Copy the appsettings.json.default file to appsettings.json

Configure the Webhook URL in the appsettings.json