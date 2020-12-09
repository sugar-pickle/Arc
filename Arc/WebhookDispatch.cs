using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace Atomic.Arc
{
    public interface IWebhookDispatch
    {
        Task SendToWebhook(SiaAlarm alarm);
        Task SendToWebhook(string message);
    }

    public class GenericWebhookDispatch : IWebhookDispatch
    {
        private readonly RestClient restClient;
        private readonly IAccountsHandler accountsHandler;
        private readonly IArcLog arcLog;
        private readonly bool configured;
        private const string SystemUsername = "System";

        public GenericWebhookDispatch(ArcConfig config, IAccountsHandler accountsHandler, IArcLog arcLog)
        {
            if (config.WebhookEnabled)
            {
                configured = true;
                restClient = new RestClient(config.WebhookUrl);
            }
            this.accountsHandler = accountsHandler;
            this.arcLog = arcLog;
        }

        public async Task SendToWebhook(SiaAlarm alarm)
            => await SendToWebhook(accountsHandler.GetUsername(alarm.AccountNumber),JsonConvert.SerializeObject(alarm));

        public async Task SendToWebhook(string message) => await SendToWebhook(SystemUsername, message);

        private async Task SendToWebhook(string username, string content)
        {
            if (configured)
            {
                var context = new WebhookContext
                {
                    Username = username,
                    Content = content
                };
                var request = new RestRequest(Method.POST);
                request.AddHeader("content-type", "application/json");
                request.AddJsonBody(JsonConvert.SerializeObject(context));
                var response = await restClient.ExecuteAsync(request);

                if (!response.IsSuccessful)
                    arcLog.Log($"Webhook request failed - {response}");
            }
        }
    }

    internal class WebhookContext
    {
        internal string Username { get; set; }
        internal string Content { get; set; }
    }
}
