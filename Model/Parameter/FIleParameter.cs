using System.ComponentModel.DataAnnotations;

namespace DotNetCoreWebApi
{
    public class FileParameter
    {
        public string FileID { get; set; }

        public int PageNum{get;set;}

        public int PageSize{get;set;}
    }
}