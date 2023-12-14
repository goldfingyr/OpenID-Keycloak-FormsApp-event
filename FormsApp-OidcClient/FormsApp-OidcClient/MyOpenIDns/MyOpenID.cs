using FormsApp_OidcClient.Browser;
using IdentityModel.OidcClient;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormsApp_OidcClient.MyOpenIDns
{
    public class MyOpenID
    {
        static OidcClient   _oidcClient = null;         //There can be only one !!!
        static LoginResult  _oidcLoginResult = null;    //There can be only one !!!
        static Form1 _responseForm = null;
        
        public MyOpenID(Form1 responseForm)
        {
            if (_oidcClient != null) return;            //There can be only one !!!
            _responseForm = responseForm;
            var options = new OidcClientOptions
            {
                Authority = Environment.GetEnvironmentVariable("OpenIDRealmURI") + "/",
                ClientId = Environment.GetEnvironmentVariable("OpenIDClient"),
                ClientSecret = Environment.GetEnvironmentVariable("OpenIDSecret"),
                Scope = Environment.GetEnvironmentVariable("Scope"),
                RedirectUri = Environment.GetEnvironmentVariable("OpenIDRedirectURI"),
                Browser = new WinFormsWebView()
            };
            // IdentityModel.OidcClient will not accept self signed certificates
            options.Policy.Discovery.RequireHttps = false;
            _oidcClient = new OidcClient(options);
        }

        public async void Login()
        {
            if (_oidcLoginResult != null) return;       //There can be only one !!!
            _oidcLoginResult = await _oidcClient.LoginAsync();
            // Send event to forms
            _responseForm.DoRefreshAll();
        }

        public bool IsLoggedIn()
        {
            if (_oidcLoginResult == null) return false; 
            return !_oidcLoginResult.IsError;
        }

        public List<System.Security.Claims.Claim> GetClaims()
        {
            return (List<System.Security.Claims.Claim>)_oidcLoginResult.User.Claims;
        }

        public LoginResult GetResult()
        {
            return _oidcLoginResult;
        }

        public string GetAccessToken()
        {
            return _oidcLoginResult.AccessToken;
        }

        public string GetBearerToken()
        {
            return "Baerer: " + _oidcLoginResult.AccessToken;
        }
    }
}
