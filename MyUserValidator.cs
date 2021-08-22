using IdentityServer4.Models;
using IdentityServer4.Validation;
 
namespace DotNetCoreWebApi
{
    public class MyUserValidator : IResourceOwnerPasswordValidator
    {
        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            if (context.UserName == "admin" && context.Password == "123")
            {
                //验证成功
                //使用subject可用于在资源服务器区分用户身份等等
                //获取：资源服务器通过User.Claims.Where(l => l.Type == "sub").FirstOrDefault();获取
                context.Result = new GrantValidationResult(subject: "admin", authenticationMethod: "custom");
            }
            else
            {
                //验证失败
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "invalid custom credential");
            }
            return Task.FromResult(0);
        }
    }
}