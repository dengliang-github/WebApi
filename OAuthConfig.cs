using IdentityServer4.Models;
using System.Collections.Generic;
 
namespace DotNetCoreWebApi
{
    public class OAuthConfig
    {
        /// <summary>
        /// 允许访问哪些Api（就像我允许我家里的哪些东西可以让顾客访问使用，如桌子，椅子等等）   CreateDate：2019-12-26 14:08:29；Author：Ling_bug
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new[]
            {
                new ApiResource("api1", "Lingbug Api1"),
                new ApiResource("api2", "Lingbug Api2")
            };
        }
 
        /// <summary>
        /// 允许哪些客户端访问
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    //对应请求参数的client_id（假设身高）
                    ClientId = "pwd_client",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    //对应请求参数的client_secret（假设口令）
                    ClientSecrets =
                    {
                        new Secret(Encrypt("pwd_secret")),
                    },
                    AllowedScopes = {"api1", "api2"},
                    AccessTokenLifetime = 3600, //AccessToken的过期时间
                }
            };
        }
 
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="valueString"></param>
        /// <returns></returns>
        private static string Encrypt(string valueString)
        {
            return string.IsNullOrWhiteSpace(valueString) ? null : valueString.Sha256();
        }
    }
}