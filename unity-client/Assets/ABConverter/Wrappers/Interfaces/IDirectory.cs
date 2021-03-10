namespace DCL
{
    public interface IDirectory
    {
        void CreateDirectory(string path);
        void InitializeDirectory(string path, bool deleteIfExists);
        void Delete(string path, bool recursive = true);
        bool Exists(string path);
    }
}