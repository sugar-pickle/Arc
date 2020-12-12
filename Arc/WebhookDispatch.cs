using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

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
            => await SendToWebhook(accountsHandler.GetUsername(alarm.AccountNumber),SiaAlarm.GetWebhookAlarmString(alarm));

        public async Task SendToWebhook(string message) => await SendToWebhook(SystemUsername, message);

        private async Task SendToWebhook(string username, string content)
        {
            if (configured)
            {
                var context = new WebhookContext
                {
                    username = username,
                    content = content
                };
                var jsonContent = JsonConvert.SerializeObject(context);
                var request = new RestRequest(Method.POST);
                request.AddHeader("content-type", "application/json");
                request.AddJsonBody(jsonContent);
                var response = await restClient.ExecuteAsync(request);

                if (!response.IsSuccessful)
                    arcLog.Log($"Webhook request failed - {response.Content}");
            }
        }
    }

    public class WebhookContext
    {
        public string username { get; set; }
        public string content { get; set; }
    }
}
