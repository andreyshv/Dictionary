using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Models;

namespace Dict.Models.UnitTest
{
    public class InMemoryFileRepository : IFileRepository
    {
        private List<WordToFile> _files = new List<WordToFile>();
        
        public bool AddArchive(Stream file)
        {
            throw new NotImplementedException();
        }

        public bool AddFile(Stream file, string word)
        {
            throw new NotImplementedException();
        }

        public Task<string> AddFileAsync(string url, string word)
        {
            /*_files.Add(new WordToFile { 
                Id = _files.Count + 1,
                Word = word,
                FileName = url
            });*/

            throw new NotImplementedException();
        }

        public Task<WordToFile[]> GetByWordAsync(string word)
        {
            throw new NotImplementedException();
        }
    }
}