using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using TaskOrganizrBackEnd.DataObjects;
using System.Net.Http.Headers;
using System;
using System.Security.Claims;

namespace TaskOrganizrBackEnd.Controllers
{
    [MobileAppController]
    public class UserInfoController : ApiController
    {
        /// <summary>
        /// Returns the caller's info from the correct provider. The user who invokes it must be authenticated.
        /// </summary>
        /// <returns>The users info</returns>
        public async Task<UserInfo> GetUserInfo()
        {

            string provider = "";
            string secret;
            string accessToken = GetAccessToken(out provider, out secret);

            UserInfo info = new UserInfo();
            switch (provider)
            {
                case "facebook":
                    using (HttpClient client = new HttpClient())
                    {
                        using (
                            HttpResponseMessage response =
                                await
                                    client.GetAsync("https://graph.facebook.com/me" 
                                    + "?fields=email,name,gender&access_token=" +
                                                    accessToken))
                        {
                            var o = JObject.Parse(await response.Content.ReadAsStringAsync());
                            info.Id = o["id"].ToString();
                            info.Name = o["name"].ToString();
                            info.Gender = o["gender"].ToString();
                            info.EmailAddress = o["email"].ToString();
                            
                        }
                        using (
                            HttpResponseMessage response =
                                await
                                    client.GetAsync("https://graph.facebook.com/me" +
                                                    "/picture?redirect=false&access_token=" + accessToken))
                        {
                            var x = JObject.Parse(await response.Content.ReadAsStringAsync());
                            info.ImageUri = (x["data"]["url"].ToString());
                        }
                    }
                    break;
            }

            return info;
        }

        /// <summary>
        /// Returns the access token and the provider the current user is using.
        /// </summary>
        /// <param name="provider">The provider e.g. facebook</param>
        /// <param name="secret">The user's secret when using Twitter</param>
        /// <returns>The Access Token</returns>
        private string GetAccessToken(out string provider, out string secret)
        {
            var serviceUser = User as ClaimsPrincipal;
            var ident = serviceUser.FindFirst("http://schemas.microsoft.com/identity/claims/identityprovider").Value;
            string token = "";
            secret = "";
            provider = ident;
            switch (ident)
            {
                case "facebook":
                    token = Request.Headers.GetValues("X-MS-TOKEN-FACEBOOK-ACCESS-TOKEN").FirstOrDefault();
                    break;
            }
            return token;
        }

        /// <summary>
        /// Encodes to HMAC-SHA1 used by Twitter OAuth 1.1 Authentication
        /// </summary>
        /// <param name="input">The input string</param>
        /// <param name="key">The input key</param>
        /// <returns>The Base64 HMAC-SHA1 encoded string</returns>
        public static string Encode(string input, byte[] key)
        {
            HMACSHA1 myhmacsha1 = new HMACSHA1(key);
            byte[] byteArray = Encoding.ASCII.GetBytes(input);
            MemoryStream stream = new MemoryStream(byteArray);
            return Convert.ToBase64String(myhmacsha1.ComputeHash(stream));
        }
        /// <summary>
        /// Returns the Unix Timestamp of the given DateTime
        /// </summary>
        /// <param name="dateTime">The DateTime to convert</param>
        /// <returns>The Unix Timestamp</returns>
        public static long DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (long)(TimeZoneInfo.ConvertTimeToUtc(dateTime) -
                           new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
        }
        /// <summary>
        /// Generates a random number from 123400 to int.MaxValue
        /// </summary>
        /// <returns>A random number as string</returns>
        public static string GenerateNonce()
        {
            return new Random()
                .Next(123400, int.MaxValue)
                .ToString("X");
        }

    }
}
