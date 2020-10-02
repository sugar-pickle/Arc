using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace Atomic.ArcÍ
{
    public interface IWebhookDispatch
    {
        Task SendToWebhook(SiaAlarm alarm);
    }

    public class GenericWebhookDispatch : IWebhookDispatch
    {
        private readonly RestClient restClient;
        private readonly IAccountsHandler accountsHandler;
        private readonly IArcLog arcLog;

        public GenericWebhookDispatch(ArcConfig config, IAccountsHandler accountsHandler, IArcLog arcLog)
        {
            restClient = new RestClient(config.WebhookUrl);
            this.accountsHandler = accountsHandler;
            this.arcLog = arcLog;
        }

        public async Task SendToWebhook(SiaAlarm alarm)
        {
            var request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/json");

            var context = new WebhookContext
            {
                Username = accountsHandler.GetUsername(alarm.AccountNumber),
                Content = JsonConvert.SerializeObject(alarm)
            };
            request.AddJsonBody(JsonConvert.SerializeObject(context));
            var response = await restClient.ExecuteAsync(request);

            if (!response.IsSuccessful)
                arcLog.Log($"Webhook request failed - {response}");
        }
    }

    internal class WebhookContext
    {
        internal string Username { get; set; }
        internal string Content { get; set; }
    }
}
