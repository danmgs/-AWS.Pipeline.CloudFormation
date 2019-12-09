using Microsoft.AspNetCore.Http;

namespace app.Web.Models
{
    public class HomeViewModel
    {
        public HomeViewModel(IHeaderDictionary headers)
        {
            Headers = headers;
        }

        public IHeaderDictionary Headers { get; set; }
        public string ServerHostname => WebHelper.DisplayLocalHostName();
        public string ClientHostname => Headers["Host"];
        public string ClientSourceIp => Headers["X-Forwarded-For"];
        public string ClientIp => !string.IsNullOrEmpty(ClientHostname) ? ClientHostname : ClientSourceIp;
    }
}
