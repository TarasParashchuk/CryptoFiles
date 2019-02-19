using CryptoFiles.Model;
using System;
using System.IO;

namespace CryptoFiles
{
    class DataBase
    {
        SQLite.SQLiteConnection database;
        public const string DBFileName = "CurrentDataBaseCryptoFiles.db";

        public DataBase()
        {
            var path = GetDatabasePath(DBFileName);
            if (!File.Exists(path))
            {
                database = new SQLite.SQLiteConnection(path);
                database.CreateTable<ModelCategory>();
                database.CreateTable<ModelDataFile>();
            }
            else database = new SQLite.SQLiteConnection(path);
        }

        public SQLite.SQLiteConnection SQLiteConnection()
        {
            return database;
        }

        public string GetDatabasePath(string sqliteFilename)
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var path = Path.Combine(documentsPath, sqliteFilename);
            return path;
        }
    }
}