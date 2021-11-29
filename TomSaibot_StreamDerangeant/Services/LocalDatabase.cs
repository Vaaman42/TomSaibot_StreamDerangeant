using SQLite;
using System.IO;
using TomSaibot_StreamDerangeant.Models;

namespace TomSaibot_StreamDerangeant.Services;

public class LocalDatabase
{
    private SQLiteConnection database;
    public SQLiteConnection Database
    {
        get 
        {
            if (database == null)
            {
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Stream Derangeant\\";
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                }
                var path = Path.Combine(basePath, "StreamDerangeant.db3");
                if (!File.Exists(path))
                {
                    File.Create(path);
                }
                database = new SQLiteConnection(path, openFlags: SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
                database.CreateTable<Question>();
                database.CreateTable<Answer>();
            }
            return database;
        }
    }
}
