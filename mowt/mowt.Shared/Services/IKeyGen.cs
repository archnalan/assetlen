using mowt.Shared.Models.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Services
{
    public interface IKeyGen
    {
        Task<string> GetDeviceID();
        string createRegistrationFile(PublicKeyDto pubKeyData);
        string GetCreateHarshfromString(string stringforHashing);
        PublicKeyDto deCipherRegistraionFile(string cipherText);
        void DeleteRegistrationFile();
        string ReadRegistraionFile();
        void saveRegistraionFile(string cipherText);
    }
}
