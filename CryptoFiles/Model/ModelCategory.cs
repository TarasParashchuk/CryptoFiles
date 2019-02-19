using SQLite;

namespace CryptoFiles.Model
{
    [Table("InfoUser")]
    public class ModelCategory
    {
        [PrimaryKey, AutoIncrement, Column("id")]
        public int id { get; set; }
        public string Category { get; set; }
        public string Password { get; set; }
        public int Count { get; set; }
    }
}