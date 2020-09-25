using System.IO;

namespace DCL
{
    public interface IFile
    {
        void Delete(string path);
        bool Exists(string path);

        void Copy(string srcPath, string dstPath);
        void Move(string srcPath, string dstPath);

        string ReadAllText(string path);
        void WriteAllText(string path, string text);
        void WriteAllBytes(string path, byte[] bytes);
        Stream OpenRead(string path);
    }
}