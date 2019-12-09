using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Text;

namespace app.Web.Models
{
    public class HomeViewModel
    {
        public HomeViewModel(IHeaderDictionary headers)
        {
            Headers = headers;
        }

        public IHeaderDictionary Headers { get; set; }
        public string ServerPrivateHostname => WebHelper.DisplayLocalHostName();
        public string ServerHostname => Headers["Host"];
        public string ClientSourceIp => Headers["X-Forwarded-For"];

        public string PrintDetails()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var h in Headers)
                sb.Append(h.Key).Append(": ").Append(h.Value).Append("\n");

            return sb.ToString();
        }
    }
}
