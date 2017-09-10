using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Models
{
    public interface IFileRepository
    {
        // Add archive file, every file named by associated word
        int AddArchive(Stream file, string origin);
        // Add single file associated with word
        bool AddFile(Stream file, string word);
        // Add single file associated with word
        Task<string> AddFileAsync(string url, string word);
        // Get list of files by associated word
        Task<WordToFile[]> GetByWordAsync(string word);        
    }
}