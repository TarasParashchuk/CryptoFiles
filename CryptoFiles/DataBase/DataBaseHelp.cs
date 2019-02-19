using System.Collections.Generic;
using System.Linq;

namespace CryptoFiles
{
    class DataBaseHelp
    {
        SQLite.SQLiteConnection database;

        public DataBaseHelp(SQLite.SQLiteConnection database)
        {
            this.database = database;
        }

        public List<T> GetItems<T>() where T : new()
        {
            return (from i in database.Table<T>() select i).ToList();
        }

        public int GetCountItems<T>() where T : new()
        {
            return database.Table<T>().Count();
        }

        public T GetItem<T>(int id) where T : new()
        {
            return database.Get<T>(id);
        }
         
        public int DeleteItem<T>(int id) where T : new()
        {
            return database.Delete<T>(id);
        }

        public int SaveItem<T>(T item, bool flag) where T : new()
        {
            if(flag)
                return database.Update(item);
            else return database.Insert(item);
        }
    }
}