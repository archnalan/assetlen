using mowt.Shared.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Syncfusion.Licensing;

namespace mowt.Web.Services
{
    public class StorageServiceWeb : IStorageService
    {
        private readonly ILocalStorageService localStorage;
        private readonly ISyncLocalStorageService syncLocalStorage;
        private readonly IFormFactor formFactor;
        private readonly IJSRuntime jsRuntime;
        public StorageServiceWeb(ILocalStorageService localStorage, ISyncLocalStorageService syncLocalStorage, IFormFactor formFactor, IJSRuntime jsRuntime)
        {
            this.localStorage = localStorage;
            this.syncLocalStorage = syncLocalStorage;
            this.formFactor = formFactor;
            this.jsRuntime = jsRuntime;
        }

        public event Action? OnTokenRemoved;

        public async Task<T> GetItemAsync<T>(string objectKey)
        {
            if (formFactor.GetFormFactor() == "webClient")
            {
                return await localStorage.GetItemAsync<T>(objectKey);
            }
            else
            {
                //return  syncLocalStorage.GetItem<T>(objectKey);

                // Check if IJSInProcessRuntime is available before calling sync method
                if (jsRuntime is IJSInProcessRuntime)
                {
                    return syncLocalStorage.GetItem<T>(objectKey);
                }
                else
                {
                    // Fall back to async if sync is not supported
                    return default(T);
                }
            }
        }

        public async Task RemoveItemAsync(string objectKey)
        {
            if (formFactor.GetFormFactor() == "webClient")
            {
                await localStorage.RemoveItemAsync(objectKey);
            }
            else
            {
                if (jsRuntime is IJSInProcessRuntime) syncLocalStorage.RemoveItem(objectKey);
            }
            if (jsRuntime is IJSInProcessRuntime) OnTokenRemoved?.Invoke();
        }

        public async Task SetItemAsync<T>(string objectKey, T objectValue)
        {
            if (formFactor.GetFormFactor() == "WebClient")
            {
                await localStorage.SetItemAsync(objectKey, objectValue);
            }
            else
            {
                // Check if IJSInProcessRuntime is available before calling sync method
                if (jsRuntime is IJSInProcessRuntime)
                {
                    syncLocalStorage.SetItem(objectKey, objectValue);
                }
                else
                {
                    // Fall back to async if sync is not supported

                }

            }


        }
    }
    public class AppCloser : IAppCloser
    {
        public void CloseApp()
        {

        }


    }
    public class AppLifecycleHandler : IAppLifecycleHandler
    {
        public string CurrentRoute { get; set; } = string.Empty;

        private readonly AuthenticationStateProvider _authStateProvider;

        public AppLifecycleHandler(AuthenticationStateProvider authStateProvider)
        {
            _authStateProvider = authStateProvider;
        }

        public async Task OnAppClosing()
        {
            //await ((CustomAuthStateProvider)_authStateProvider).MarkUserAsLoggedOut();
        }

    }
    public class ConnectivityService : IConnectivityService
    {
        public bool HasInternet => throw new NotImplementedException();

        public string? ConnectionProfile => throw new NotImplementedException();
    }
}
