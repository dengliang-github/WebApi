using System.ComponentModel.DataAnnotations;

namespace DotNetCoreWebApi
{
    public class UserList
    {
        public string Account { get; set; }

        public int PageNum{get;set;}

        public int PageSize{get;set;}
    }
}