using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels
{
    public class PublicKeyDto
    {

        public string DeviceKey { get; set; }
        public long ActivationID1 { get; set; }
        public long ActivationID2 { get; set; }
        public long ActivationID3 { get; set; }
        public long ActivationID4 { get; set; }
        public long ActivationID5 { get; set; }
        public string userName { get; set; }
        public int ModuleID { get; set; }

        public PublicKeyDto()
        {
            long obfuscatorPI = long.Parse(Math.PI.ToString().Substring(2, 9) + Math.PI.ToString().Substring(2, 9));
            ActivationID1 = DateTime.Today.AddDays(7.56455).Ticks + obfuscatorPI;
            ActivationID2 = DateTime.Now.Ticks + obfuscatorPI;
            ActivationID3 = (DateTime.Now.AddMinutes(78).Ticks) + obfuscatorPI;
            ActivationID4 = (DateTime.Today.Ticks + obfuscatorPI);
            ActivationID5 = (DateTime.Today.AddDays(1).Ticks) + obfuscatorPI;
        }

    }
}
