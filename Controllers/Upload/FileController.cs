using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;

namespace DotNetCoreWebApi.Controllers.Upload
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class FileController : ControllerBase
    {
        //初始化 
        public static IWebHostEnvironment _environment;
        CoreDbContext _Context;
        public FileController(IWebHostEnvironment environment, CoreDbContext context)
        {
            _environment = environment;
            _Context = context;
        }

        public class FileUploadAPI
        {
            public IFormFile files { get; set; }
        }

        [Authorize]
        [HttpPost]
        public IActionResult Save([FromForm] FileUploadAPI objFile, string UserAcc)
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
                    string path = _environment.WebRootPath + "Upload/" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day;
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
                        //保存文件信息到数据库
                        Attachments att = new Attachments()
                        {
                            FileName = FileName,
                            FileID = FileID,
                            Size = objFile.files.Length,
                            FilePath = path,
                            CreateUser = UserAcc,
                            CreateDate = DateTime.Now,
                            Ext = Ext
                        };
                        _Context.Attachments.Add(att);
                        _Context.SaveChanges();
                        filesStream.Flush();
                        item["FilePath"] = path + "/" + FileID + Ext;
                        item["FileName"] = FileName;
                        item["FileID"] = FileID;
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

        [HttpGet]
        public IActionResult Download(string FileID)
        {
            JObject rv = new JObject();
            if (!string.IsNullOrEmpty(FileID))
            {
                var data = _Context.Attachments.Where(b => b.FileID == FileID).ToList();
                if (data.Count() > 0)
                {
                    string filePath = AppContext.BaseDirectory + data[0].FilePath + "/" + data[0].FileID + data[0].Ext;
                    if (System.IO.File.Exists(filePath))
                    {
                        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);
                        return File(fileStream, "application/octet-stream", data[0].FileName + data[0].Ext);
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
                    rv["msg"] = "FIleID不存在";
                    return Ok(rv.ToString());
                }
            }
            else
            {
                rv["success"] = false;
                rv["msg"] = "FIleID不能为空";
                return Ok(rv.ToString());
            }
        }
    }
}