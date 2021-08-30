using System.ComponentModel.DataAnnotations;

namespace DotNetCoreWebApi
{
    public class Attachments
    {
        [Key]
        public int id { get; set; }
        public string FileName { get; set; }
        public string FileID { get; set; }
        public long Size { get; set; }
        public string FilePath { get; set; }
        public string CreateUser { get; set; }
        public DateTime CreateDate { get; set; }
        public string Ext { get; set; }
    }
}