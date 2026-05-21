using Microsoft.FluentUI.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Pages.Components
{
    public class DialogMessageDto
    {
        public string DialogTitle { get; set; }
        public string DialogMessage { get; set; }
        public bool IsWarning { get; set; } = true;
        public string DialogButtonPrimary { get; set; }
        public string DialogButtonCancel { get; set; }
    }
}
