using mowt.Shared.Services;

namespace mowt.Web.Client.Services
{
    public class FormFactor : IFormFactor
    {
        public string GetFormFactor()
        {
            return $"WebClient";
        }

        public string GetPlatform()
        {
            return Environment.OSVersion.ToString();
        }
    }
}
