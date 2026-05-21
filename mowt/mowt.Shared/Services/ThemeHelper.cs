using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor;

namespace mowt.Shared.Services
{
    public static class ThemeHelper
    {
        public static Theme GetCurrentTheme(string uri)
        {
            if (uri.IndexOf("material") > -1)
            {
                if (uri.IndexOf("dark") > -1)
                {
                    return Theme.MaterialDark;
                }
                return Theme.Material;
            }
            else if (uri.IndexOf("bootstrap5") > -1)
            {
                if (uri.IndexOf("dark") > -1)
                {
                    return Theme.Bootstrap5Dark;
                }
                return Theme.Bootstrap5;
            }
            else if (uri.IndexOf("bootstrap4") > -1)
            {
                return Theme.Bootstrap4;
            }
            else if (uri.IndexOf("bootstrap") > -1)
            {
                if (uri.IndexOf("dark") > -1)
                {
                    return Theme.BootstrapDark;
                }
                return Theme.Bootstrap;
            }
            else if (uri.IndexOf("tailwind") > -1)
            {
                if (uri.IndexOf("dark") > -1)
                {
                    return Theme.TailwindDark;
                }
                return Theme.Tailwind;
            }
            else if (uri.IndexOf("highcontrast") > -1)
            {
                return Theme.HighContrast;
            }
            else
            {
                return Theme.Bootstrap5;
            }
        }
    }
}
