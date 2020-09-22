# Webhook ARC

Implements a basic Surgard alarm receiver and sends incoming events to a webhook.

The receiver runs as a server listening on the port specified in appsettings.json. It will accept a connection from any source so use of an external firewall to restrict source addresses is recommended if using externally.

Runs in .net core 3.1 and is supported on any OS. 

## Usage

Copy the appsettings.json.default file to appsettings.json

Configure the Webhook URL in the appsettings.json

## Usage (Docker)

Follow the instructions above to set up the appsettings.json

From the project root, run the following to build the docker image:

```
docker build -t webhook-arc:latest .
```

Then run this to start the container:

```
docker run -d -p 50001:50001 --rm --name webhook-arc webhook-arc:latest
```