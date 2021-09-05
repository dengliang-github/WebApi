using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authorization;

namespace DotNetCoreWebApi.Controllers.User
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class UserController : ControllerBase
    {
        CoreDbContext _Context;
        public static IWebHostEnvironment _environment;
        public UserController(CoreDbContext context, IWebHostEnvironment environment)
        {
            _Context = context;
            _environment = environment;
        }

        public class FileUploadAPI
        {
            public IFormFile files { get; set; }
        }

        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public IActionResult GetUserList([FromForm] UserList obj)
        {
            JObject rv = new JObject();
            JArray children = new JArray();
            try
            {
                rv["success"] = true;
                rv["Total"] = _Context.SYS_USER.Count();
                var Data = new List<SYS_USER>();
                if (string.IsNullOrEmpty(obj.Account))
                {
                    Data = _Context.SYS_USER.Skip(obj.PageSize * (obj.PageNum - 1)).Take(obj.PageSize).ToList();
                }
                else
                {
                    rv["Total"] = 1;
                    Data = _Context.SYS_USER.Where(b => b.Account == obj.Account).Skip(obj.PageSize * (obj.PageNum - 1)).Take(obj.PageSize).ToList();
                }

                foreach (var rows in Data)
                {
                    JObject item = new JObject();
                    children.Add(item);
                    item["Account"] = rows.Account;
                    item["DisplayName"] = rows.DisplayName;
                    item["Sex"] = rows.Sex;// == "Male" ? "男" : "女";
                    item["Avatar"] = rows.Avatar;
                    item["Disable"] = rows.Disable;
                }
                rv["data"] = children;
            }
            catch (Exception ex)
            {
                rv["success"] = false;
                rv["msg"] = ex.Message;
            }
            return Ok(rv.ToString());
        }

        /// <summary>
        /// 创建用户
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public IActionResult CreateUser([FromForm] SYS_USER user)
        {
            JObject rv = new JObject();
            try
            {
                if (!string.IsNullOrEmpty(user.Account) && !string.IsNullOrEmpty(user.PassWord))
                {
                    var data = _Context.SYS_USER.Where(b => b.Account == user.Account).ToList();
                    if (data.Count() <= 0)
                    {
                        //密码加密处理
                        user.PassWord = MD5H.MD5(user.PassWord);
                        //默认激活账号
                        user.Disable = false;
                        _Context.SYS_USER.Add(user);
                        _Context.SaveChanges();
                        rv["success"] = true;
                        rv["Msg"] = "账号创建成功";
                    }
                    else
                    {
                        rv["success"] = false;
                        rv["Msg"] = "账号：" + user.Account + "已存在";
                    }
                }
                else
                {
                    rv["success"] = false;
                    rv["Msg"] = "账号和密码不能为空";
                }
            }
            catch (Exception ex)
            {
                rv["success"] = false;
                rv["Msg"] = ex.Message;
            }
            return Ok(rv.ToString());
        }

        /// <summary>
        /// 修改用户信息
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public IActionResult ModifyUserInfo([FromForm] SYS_USER user)
        {
            JObject rv = new JObject();
            try
            {
                if (!string.IsNullOrEmpty(user.DisplayName))
                {
                    //查询出历史头像，删除历史设置的头像
                    var Data = _Context.SYS_USER.Where(p => p.Account == user.Account).ToList();
                    string Path = AppContext.BaseDirectory + "/UploadData/UserData/Avatar/" + Data[0].Avatar;
                    if (System.IO.File.Exists(Path) && !Data[0].Avatar.Equals(user.Avatar))
                    {
                        System.IO.File.Delete(Path);
                    }
                    Data[0].DisplayName = user.DisplayName;
                    Data[0].Sex = user.Sex;
                    Data[0].Avatar = user.Avatar;
                    _Context.SYS_USER.Attach(Data[0]);
                    //不需要查询到话可以执行下面的代码
                    //标记修改的字段
                    // _Context.Entry(user).Property(p => p.DisplayName).IsModified = true;
                    // _Context.Entry(user).Property(p => p.Sex).IsModified = true;
                    // _Context.Entry(user).Property(p => p.Avatar).IsModified = true;
                }
                else
                {
                    //user.Disable = user.Disable;
                    _Context.Entry(user).Property(p => p.Disable).IsModified = true;
                }
                _Context.SaveChanges();
                rv["success"] = true;
                rv["Msg"] = "修改成功";
            }
            catch (Exception ex)
            {
                rv["success"] = false;
                rv["Msg"] = ex.Message;
            }
            return Ok(rv.ToString());
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public IActionResult DelUser([FromForm] SYS_USER user)
        {
            JObject rv = new JObject();
            try
            {
                //删除用户的同时删除用户数据
                var Data = _Context.SYS_USER.Where(p => p.Account == user.Account).ToList();
                string Path = AppContext.BaseDirectory + "/UploadData/UserData/Avatar/" + Data[0].Avatar;
                if (System.IO.File.Exists(Path))
                {
                    System.IO.File.Delete(Path);
                }
                _Context.SYS_USER.Remove(Data[0]);
                _Context.SaveChanges();
                rv["success"] = true;
                rv["Msg"] = "删除成功";
            }
            catch (Exception ex)
            {
                rv["success"] = false;
                rv["Msg"] = ex.Message;
            }
            return Ok(rv.ToString());
        }

        [HttpGet]
        public IActionResult GetConfig()
        {
            JObject rv = new JObject();
            JArray children = new JArray();
            try
            {
                rv["success"] = true;
                JObject item = new JObject();
                children.Add(item);
                item["DBCON"] = ConfigHelper.GetSectionValue("DBCON");
                rv["data"] = children;
            }
            catch (Exception ex)
            {
                rv["success"] = false;
                rv["Msg"] = ex.Message;
            }
            return Ok(rv.ToString());
        }

        /// <summary>
        /// 上传用户头像
        /// </summary>
        /// <param name="objFile"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public IActionResult UploadAvatar([FromForm] FileUploadAPI objFile)
        {
            JObject rv = new JObject();
            try
            {
                if (objFile.files.Length > 0)
                {
                    rv["success"] = true;
                    string path = AppContext.BaseDirectory + "/UploadData/UserData/Avatar";
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    string FileName = objFile.files.FileName;
                    string Ext = FileName.Substring(FileName.LastIndexOf('.'), FileName.Length - FileName.LastIndexOf('.'));
                    //文件名称使用guid生成
                    string FileID = Guid.NewGuid().ToString();
                    using (FileStream filesStream = System.IO.File.Create(path + "/" + FileID + Ext))
                    {
                        objFile.files.CopyTo(filesStream);
                        rv["AvatarID"] = FileID + Ext;
                    }
                }
            }
            catch (Exception ex)
            {
                rv["success"] = false;
                rv["msg"] = ex.Message;
            }
            return Ok(rv.ToString());
        }

        /// <summary>
        /// 获取用户头像
        /// </summary>
        /// <param name="Account"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetUserAvatar(string Account)
        {
            JObject rv = new JObject();
            try
            {
                var Data = _Context.SYS_USER.Where(b => b.Account == Account).ToList();
                if (Data.Count() > 0)
                {
                    string filePath = AppContext.BaseDirectory + "/UploadData/UserData/Avatar/" + Data[0].Avatar;
                    if (System.IO.File.Exists(filePath))
                    {
                        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);
                        return File(fileStream, "application/octet-stream", Data[0].Avatar);
                    }
                    else
                    {
                        rv["success"] = false;
                        rv["msg"] = "文件不存在或已被删除";
                        return Ok(rv.ToString());
                    }
                }
                else
                {
                    rv["success"] = false;
                    rv["msg"] = "账号错误";
                    return Ok(rv.ToString());
                }

            }
            catch (Exception ex)
            {
                rv["success"] = false;
                rv["msg"] = ex.Message;
                return Ok(rv.ToString());
            }
        }
    }
}