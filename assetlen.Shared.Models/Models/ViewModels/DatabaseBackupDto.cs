using assetlen.Shared.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels
{
    public class DatabaseBackupDto
    {
        public ConfigurationDto BackupLocation { get; set; }
        public ConfigurationDto NumberOfFileToKeep { get; set; }
        public ConfigurationDto AutoBackupDb { get; set; }
        public bool SaveAndBackupNow { get; set; }
    }
}
