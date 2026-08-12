using assetlen.Shared.Models.Models;
using assetlen.Shared.Models.Models.ViewModels;
using assetlen.Shared.Services;
using Blazored.LocalStorage;
using Microsoft.FluentUI.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static assetlen.Shared.Models.statics.statics;

namespace assetlen.Shared.statics
{
    public class SD : ISD
    {
        private readonly ILocalStorageService _localStorage;
        
        public bool activeMenu { get; set; } = true;
        public bool SelectMultiple { get; set; } = false;
        public decimal? screenSize { get; set; } = null;
        public string currentColor { get; private set; } = "#03C9D7";
        public string currentMode { get; private set; } = "Light";
        public initialState isClicked { get; set; } = new initialState();
        public bool themeSettings { get; set; } = false;
        public Module CurrentModule { get; set; }
        public UserClaimsDto? CurrentUser { get; private set; }

        // Event to notify components when user is updated
        public event Action? OnUserUpdated;

        public void SetUser(UserClaimsDto user)
        {
            CurrentUser = user;
            OnUserUpdated?.Invoke();
        }

        public void RemoveUser()
        {
            CurrentUser = null;
            OnUserUpdated?.Invoke();
        }

        public async void setColor(string colorHarshCode)
        {
            currentColor = colorHarshCode;
            await _localStorage.SetItemAsync("colorMode", colorHarshCode);
        }

        public async void setMode(string themeMode)
        {
            currentMode = themeMode;
            await _localStorage.SetItemAsync("themeMode", themeMode);
        }
    }
    
    public class initialState
    {
        public bool chat { get; set; }
        public bool cart { get; set; }
        public bool userProfile { get; set; }
        public bool notification { get; set; }
    }
}
