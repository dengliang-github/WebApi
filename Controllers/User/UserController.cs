using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authorization;

namespace DotNetCoreWebApi.Controllers.User
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class UserController : ControllerBase
    {
        CoreDbContext _Context;
        public UserController(CoreDbContext context)
        {
            _Context = context;
        }

        //[Authorize]
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
                    Data = _Context.SYS_USER.Skip(obj.PageSize*(obj.PageNum-1)).Take(obj.PageSize).ToList();
                }
                else
                {
                    rv["Total"] = 1;
                    Data = _Context.SYS_USER.Where(b => b.Account == obj.Account).Skip(obj.PageSize * (obj.PageNum-1)).Take(obj.PageSize).ToList();
                }

                foreach (var rows in Data)
                {
                    JObject item = new JObject();
                    children.Add(item);
                    item["Account"] = rows.Account;
                    item["DisplayName"] = rows.DisplayName;
                    item["Sex"] = rows.Sex;// == "Male" ? "男" : "女";
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

        [HttpPost]
        public IActionResult ModifyUserInfo([FromForm] SYS_USER user)
        {
            JObject rv = new JObject();
            try
            {
                var Data = _Context.SYS_USER.First(b => b.Account == user.Account);
                if (!string.IsNullOrEmpty(user.DisplayName))
                {
                    Data.DisplayName = user.DisplayName;
                    Data.Sex = user.Sex;
                }
                else
                {
                    Data.Disable = user.Disable;
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

        [HttpPost]
        public IActionResult DelUser([FromForm] SYS_USER user)
        {
            JObject rv = new JObject();
            try
            {
                _Context.SYS_USER.Remove(user);
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
    }
}