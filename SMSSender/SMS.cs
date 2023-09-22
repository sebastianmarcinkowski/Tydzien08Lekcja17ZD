using SMSApi.Api;
using SMSApi.Api.Response;

namespace SMSSender
{
    public class SMS
    {
        private readonly string _token;
        private readonly IClient client;
        private readonly SMSFactory smsApi;

        public SMS(string token)
        {
            _token = token;
            client = new SMSApi.Api.ClientOAuth(token);
            smsApi = new SMSApi.Api.SMSFactory(client);
        }

        public Status Send(string senderName, string toNumber, string body)
        {
            return smsApi.ActionSend()
                .SetText(body)
                .SetTo(toNumber)
                .SetSender(senderName)
                .Execute();
        }
    }
}
