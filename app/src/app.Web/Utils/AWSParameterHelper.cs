using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using log4net;
using System;
using System.Threading.Tasks;

namespace app.Web.Utils
{
    public static class AWSParameterHelper
    {
        static readonly ILog _log = LogManager.GetLogger(typeof(AWSParameterHelper));

        public static async Task<string> GetConfiguration(string keyname)
        {
            // NOTE: set the region here to match the region used when you created the parameter
            //var region = Amazon.RegionEndpoint.EUWest3;
            var request = new GetParameterRequest()
            {
                Name = keyname
            };

            using (var client = new AmazonSimpleSystemsManagementClient())
            {
                try
                {
                    var response = await client.GetParameterAsync(request);
                    _log.Debug($"Parameter {request.Name} value is: {response.Parameter.Value}");
                    return response.Parameter.Value;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            return null;
        }
    }
}
