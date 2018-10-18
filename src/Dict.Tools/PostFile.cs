using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dict.Tools
{
    class PostFile 
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        /*static PostFile()  
        {  
            HttpClient = new HttpClient();  
        }*/

        public async Task<bool> UploadAsync(string url, string path, string word, string origin)
        {
            string fileName = Path.GetFileName(path);
            using (var formData = new MultipartFormDataContent())
            {
                if (!string.IsNullOrEmpty(word))
                {
                    formData.Add(new StringContent(word), "word");
                }
                if (!string.IsNullOrEmpty(origin))
                {
                    formData.Add(new StringContent(origin), "origin");
                }
                var streamContent = new StreamContent(new FileStream(path, FileMode.Open));
                formData.Add(streamContent, "files", fileName);
                
                var response = await HttpClient.PostAsync(url, formData);
                return response.IsSuccessStatusCode;
            }
        }
    }
}