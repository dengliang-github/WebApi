using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore.SqlServer;

namespace DotNetCoreWebApi.Controllers.User
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class GetDataController : ControllerBase
    {
        [HttpGet]
        public IActionResult UserInfo()
        {
            JObject rv = new JObject();
            JArray children = new JArray();
            try
            {
                rv["success"] = true;
                JObject item = new JObject();
                children.Add(item);
                item["Account"] = "A0001";
                item["DisplayName"] = "lisi";
                item["Sex"] = "Male";
                item["Plant"]="SH";
                rv["data"] = children;
            }
            catch (Exception ex)
            {
                rv["success"] = false;
                rv["msg"] = ex.Message;
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