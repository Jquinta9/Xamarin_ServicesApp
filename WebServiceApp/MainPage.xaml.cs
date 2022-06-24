using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace WebServiceApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            _cache = new MemoryCache(new MemoryCacheOptions() { });
        }

        private readonly IMemoryCache _cache;

        public void Set<T>(string key, T value, DateTimeOffset absoluteExpiry)
        {
            _cache.Set(key, value, absoluteExpiry);
        }

        public T Get<T>(string key)
        {
            if (_cache.TryGetValue(key, out T value))
                return value;
            else
                return default(T);
        }

        private async void EncryptButton_Clicked(object sender, EventArgs e)
        {
            HttpClient client = new HttpClient();
            string result;
            try
            {
                var response = await client.GetAsync(
                "https://venus.sod.asu.edu/WSRepository/Services/EncryptionRest/Service.svc/Encrypt?text="
                + EncryptEntry.Text);
                response.EnsureSuccessStatusCode();
                result = (await response.Content.ReadAsStringAsync()).Replace(@"""", "");
            }
            catch (HttpRequestException ex)
            {
                result = ex.ToString();
            }
            EncryptLabel.Text = result;
            DecryptEntry.Text = result;// Auto fill the text for decryption
            _cache.Set("results", result, DateTime.Now.AddSeconds(10));
            CacheLabel.Text = (string)_cache.Get("results");
        }

        private async void DecryptButton_Clicked(object sender, EventArgs e)
        {
            HttpClient client = new HttpClient();
            string result;
            try
            {
                var response = await client.GetAsync(
                "https://venus.sod.asu.edu/WSRepository/Services/EncryptionRest/Service.svc/Decrypt?text="
                + DecryptEntry.Text);
                response.EnsureSuccessStatusCode();
                result = (await response.Content.ReadAsStringAsync()).Replace(@"""", "");
            }
            catch (HttpRequestException ex)
            {
                result = ex.ToString();
            }
            DecryptLabel.Text = result;
            _cache.Set("results", result, DateTime.Now.AddSeconds(10));
            CacheLabel.Text = (string)_cache.Get("results");
        }

        private async void SendButton_Clicked(object sender, EventArgs e)
        {
            string result;
            try
            {
                await Sms.ComposeAsync(new SmsMessage(DecryptEntry.Text, PhoneNumberEntry.Text));
                if(DecryptEntry.Text.Equals("")) { result = "Invalid Entry"; }
                else { result = "message sent"; }
                
            }
            catch (Exception)
            {
                result = "Invalid Entry";
            }
            SendLabel.Text = result;
        }

        private async void LocationButton_Clicked(object sender, EventArgs e)
        {
            string result;
            try
            {
                var location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best));
                result = $"lat: {location.Latitude}, lng: {location.Longitude}";
            }
            catch (Exception)
            {
                result = "Location Not Availiable";
            }
            EncryptEntry.Text = result;
            EncryptLabel.Text = result;

        }

        private void ResetButton_Clicked(object sender, EventArgs e)
        {
            PhoneNumberEntry.Text = "";
            DecryptLabel.Text = "";
            EncryptLabel.Text = "";
            DecryptEntry.Text = "";
            EncryptEntry.Text = "";
            CacheLabel.Text = "";


        }
    }
}


