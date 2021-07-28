using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace DotNetCoreWebApi.Controllers.Upload
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class FileController : ControllerBase
    {
        //初始化 
        public static IWebHostEnvironment _environment;
        public FileController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public class FileUploadAPI
        {
            public IFormFile files { get; set; }
        }

        [HttpPost]
        public IActionResult Save([FromForm] FileUploadAPI objFile)
        {
            JObject rv = new JObject();
            JArray children = new JArray();
            try
            {
                if (objFile.files.Length > 0)
                {
                    rv["success"] = true;
                    JObject item = new JObject();
                    children.Add(item);
                    string path = _environment.WebRootPath + "Upload";
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    string FileName = objFile.files.FileName;
                    string Ext = FileName.Substring(FileName.LastIndexOf('.'), FileName.Length - FileName.LastIndexOf('.'));
                    using (FileStream filesStream = System.IO.File.Create(path + "/" + objFile.files.FileName))
                    {
                        objFile.files.CopyTo(filesStream);
                        filesStream.Flush();
                        item["FilePath"] = path + "/" + objFile.files.FileName;
                        item["FileName"] = FileName;
                        item["Ext"] = Ext;
                        item["Size"] = objFile.files.Length;
                        rv["data"] = children;
                    }
                }
                else
                {
                    rv["success"] = false;
                    rv["msg"] = "";
                }
            }
            catch (Exception ex)
            {
                rv["success"] = false;
                rv["msg"] = ex.Message;
            }
            return Ok(rv.ToString());
        }

        public IActionResult Download([FromForm] string FileID)
        {
            return null;
        }
        
    }
}