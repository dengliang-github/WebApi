using System.ComponentModel.DataAnnotations;

namespace DotNetCoreWebApi
{
    public class SYS_USER
    {
        [Key]
        public string Account { get; set; }
        //密码
        public string PassWord { get; set; }
        //用户名
        public string DisplayName { get; set; }
        //性别
        public string Sex { get; set; }
        //是否禁用
        public bool Disable { get; set; }
        //头像
        public string Avatar { get; set; }
    }
}