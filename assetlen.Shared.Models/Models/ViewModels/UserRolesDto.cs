using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Models.Models.ViewModels
{
    public class UserRolesDto
    {
        // Admin Module
        public bool AdminModuleLogin { get; set; }
        public bool Modifysettings { get; set; }
        public bool SetUserAccount { get; set; }
        public bool SetModifyReceiptDesign { get; set; }
        public bool SetSystemConfig { get; set; }
        public bool ProductConfig { get; set; }
        public bool GenerateReports { get; set; }
        public bool SupplierMgt { get; set; }
        public bool AccountManagement { get; set; }
        // Requires separate assignment (not auto-enabled with AdminModuleLogin)
        public bool FeedbackApproval { get; set; }
        public bool EmployeeApproval { get; set; }
        public bool ViewSystemlog { get; set; }

        // Library Module
        public bool LibraryModuleLogin { get; set; } = true;
        public bool CreateCommentsAndFeedback { get; set; } = true;

        // System
        public bool mowtSuperAdmin { get; set; }
    }
}
