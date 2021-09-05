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

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="objFile">文件对象</param>
        /// <param name="UserAcc">上传用户-必须是系统中存在的账号</param>
        /// <returns>附件信息-FileID可用于下载</returns>
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
                    var QData = _Context.SYS_USER.Where(b => b.Account == UserAcc).ToList();
                    if (QData.Count() > 0)
                    {
                        rv["success"] = true;
                        JObject item = new JObject();
                        children.Add(item);
                        string path = _environment.WebRootPath + "UploadData/Upload/" + DateTime.Now.Year + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00");
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
                                CreateDate = DateTime.Now.ToCstTime(),
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
                        rv["msg"] = "用户不存在";
                    }
                }
                else
                {
                    rv["success"] = false;
                    rv["msg"] = "未读取到文件";
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
        /// 文件下载
        /// </summary>
        /// <param name="FileID">文件编号</param>
        /// <returns></returns>
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

        /// <summary>
        /// 获取文件列表
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public IActionResult GetFileList([FromForm] FileParameter obj)
        {
            JObject rv = new JObject();
            JArray children = new JArray();
            try
            {
                rv["success"] = true;
                rv["Total"] = _Context.Attachments.Count();
                //var Data = new List<Attachments>();
                IEnumerable<Attachments> Data = _Context.Attachments.Join(_Context.SYS_USER, A => A.CreateUser, B => B.Account, (A, B) => new Attachments
                {
                    id = A.id,
                    FileName = A.FileName,
                    FileID = A.FileID,
                    Size = A.Size,
                    FilePath = A.FilePath,
                    CreateUser = B.DisplayName,
                    CreateDate = A.CreateDate,
                    Ext = A.Ext
                }).Skip(obj.PageSize * (obj.PageNum - 1)).Take(obj.PageSize);
                //筛选条件
                if (!string.IsNullOrEmpty(obj.FileID))
                {
                    Data = Data.Where(b => b.FileID == obj.FileID).ToList();
                    rv["Total"] = 1;
                }
                foreach (var rows in Data)
                {
                    JObject item = new JObject();
                    children.Add(item);
                    item["FileID"] = rows.FileID;
                    item["FileName"] = rows.FileName;
                    item["Size"] = Math.Round(((Convert.ToDecimal(rows.Size) / 1000) / 1000), 2, MidpointRounding.AwayFromZero);
                    item["FilePath"] = rows.FilePath;
                    item["CreateUser"] = rows.CreateUser;
                    item["CreateDate"] = rows.CreateDate.ToString("yyyy-MM-dd HH:mm:ss");
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
        /// 删除文件
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public IActionResult RemoveFile([FromForm] Attachments obj)
        {
            JObject rv = new JObject();
            try
            {
                var Data = _Context.Attachments.Where(b => b.FileID == obj.FileID).ToList();
                if (Data.Count() > 0)
                {
                    string Path = _environment.ContentRootPath + "/" + Data[0].FilePath + "/" + Data[0].FileID + Data[0].Ext;
                    if (System.IO.File.Exists(Path))
                    {
                        //删除文件
                        System.IO.File.Delete(Path);
                        //删除数据库记录
                        obj = Data[0];
                        _Context.Attachments.Remove(obj);
                        _Context.SaveChanges();
                        rv["success"] = true;
                        rv["msg"] = "删除成功";
                    }
                    else
                    {
                        rv["success"] = false;
                        rv["msg"] = "文件不存在或已被删除";
                    }
                }
                else
                {
                    rv["success"] = false;
                    rv["msg"] = "未找到相关文件信息";
                }
            }
            catch (Exception ex)
            {
                rv["success"] = false;
                rv["msg"] = ex.Message;
            }
            return Ok(rv.ToString());
        }
    }
}