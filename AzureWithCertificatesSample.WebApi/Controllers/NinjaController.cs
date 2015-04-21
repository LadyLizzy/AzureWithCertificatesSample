using System.Web.Http;

namespace AzureWithCertificatesSample.WebApi.Controllers
{
    public class NinjaController : ApiController
    {
        public IHttpActionResult Get()
        {
            return Ok("HAAAAAIIYA!!! I'm in ^_^");
        }
    }
}