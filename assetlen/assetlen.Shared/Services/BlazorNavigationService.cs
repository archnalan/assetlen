using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.Services
{
    public class BlazorNavigationService
    {
        private readonly List<string> _navigationQueue = new();
        public event Action<string>? OnNavigate;

        public void RequestNavigation(string route)
        {
            if (OnNavigate != null)
            {
                // If there is a listener, invoke immediately
                OnNavigate(route);
            }
            else
            {
                // Queue requests if no listener is active
                _navigationQueue.Add(route);
            }
        }

        public void RegisterListener(Action<string> handler)
        {
            OnNavigate += handler;

            // Process any queued navigation requests
            foreach (var route in _navigationQueue)
            {
                handler(route);
            }
            _navigationQueue.Clear();
        }

        public void UnregisterListener(Action<string> handler)
        {
            OnNavigate -= handler;
        }
    }
}
