using FormsApp_OidcClient.Browser;
using FormsApp_OidcClient.Models;
using FormsApp_OidcClient.MyOpenIDns;
using IdentityModel.OidcClient;
using Newtonsoft.Json;
using System.Text;

namespace FormsApp_OidcClient
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
            MyOpenID myOpenID = new MyOpenID(this);
            //
            // Login could be called here, but in this demo not as
            // login procedure for Client Credential Flow does not need that.
            //myOpenID.Login();

        }

        public void DoRefreshAll()
        {
            MyOpenID myOpenID = new MyOpenID(this);
            if (myOpenID.IsLoggedIn())
            {
                LoginResult result = myOpenID.GetResult();
                RefreshTbResponse( result );

            }
        }

        private void RefreshTbResponse(LoginResult result)
        {
            var sb = new StringBuilder(128);
            foreach (var claim in result.User.Claims)
            {
                sb.AppendLine($"{claim.Type}: {claim.Value}");
            }

            if (!string.IsNullOrWhiteSpace(result.RefreshToken))
            {
                sb.AppendLine();
                sb.AppendLine($"refresh token: {result.RefreshToken}");
            }

            if (!string.IsNullOrWhiteSpace(result.IdentityToken))
            {
                sb.AppendLine();
                sb.AppendLine($"identity token: {result.IdentityToken}");
            }

            if (!string.IsNullOrWhiteSpace(result.AccessToken))
            {
                sb.AppendLine();
                sb.AppendLine($"access token: {result.AccessToken}");
            }

            TbResponse.Text = sb.ToString();
            TbApiResponse.Update();
        }

        private async void BtLogin_Click(object sender, EventArgs e)
        {
            MyOpenID myOpenID = new MyOpenID(this);
            myOpenID.Login();

            if (myOpenID.IsLoggedIn())
            //{
            //    MessageBox.Show(this, result.Error, "Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
            //else
            {

                // Call the API

                using (HttpClient client = new HttpClient())
                {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:7000/WeatherForecast");
                    request.Headers.Add("Authorization", myOpenID.GetBearerToken());
                    using (var response = client.SendAsync(request).Result)
                    {
                        try
                        {
                            response.EnsureSuccessStatusCode();
                            string apiResponse = response.Content.ReadAsStringAsync().Result;
                            TbApiResponse.Text = apiResponse;
                        }
                        catch (HttpRequestException ex)
                        {
                            TbApiResponse.Text = ex.Message;
                        }

                    }
                }

            }
        }

        private void getCccfToken_Click(object sender, EventArgs e)
        {
            OpenIDTokenResponse tokenResponse;
            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://auth.c.ucnit.eu/realms/xOIDCx/protocol/openid-connect/token");
                var collection = new List<KeyValuePair<string, string>>();
                collection.Add(new("response_type", "code"));
                collection.Add(new("client_id", Environment.GetEnvironmentVariable("OpenIDClient")));
                collection.Add(new("client_secret", Environment.GetEnvironmentVariable("OpenIDSecret")));
                collection.Add(new("grant_type", "client_credentials"));
                var content = new FormUrlEncodedContent(collection);
                request.Content = content;
                using (var response = client.SendAsync(request).Result)
                {
                    response.EnsureSuccessStatusCode();
                    // Get the response
                    string tokenJsonString = response.Content.ReadAsStringAsync().Result;
                    tokenResponse = JsonConvert.DeserializeObject<OpenIDTokenResponse>(tokenJsonString);
                    TbResponse.Text = tokenResponse.access_token;
                }
            }

            // Call the API

            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:7000/WeatherForecast");
                request.Headers.Add("Authorization", "Bearer " + tokenResponse.access_token);
                using (var response = client.SendAsync(request).Result)
                {
                    try
                    {
                        response.EnsureSuccessStatusCode();
                        string apiResponse = response.Content.ReadAsStringAsync().Result;
                        TbApiResponse.Text = apiResponse;
                    }
                    catch (HttpRequestException ex)
                    {
                        TbApiResponse.Text = ex.Message;
                    }

                }
            }
        }
    }
}