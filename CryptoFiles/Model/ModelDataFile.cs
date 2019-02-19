using SQLite;

namespace CryptoFiles.Model
{
    [Table("InfoFiles")]
    public class ModelDataFile
    {
        [PrimaryKey, AutoIncrement, Column("id")]
        public int id { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string FileType { get; set; }
        public string FileDate { get; set; }
        public int Key_size { get; set; }
        public string FileSHA { get; set; }
        public string FilePath { get; set; }
        public int id_users { get; set; }
    }
}