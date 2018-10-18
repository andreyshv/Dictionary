using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models;

namespace WebDictionary.Controllers
{
    [Route("api/files")]
    public class MediaController: Controller
    {
        private readonly IFileRepository _fileRepository;
        private ILogger _logger;

        public MediaController(IFileRepository fileRepository, ILogger<MediaController> logger)
        {
            _fileRepository = fileRepository;
            _logger = logger;    
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new {result = "api/files get!"});
        }

        [HttpPost]
        public async Task<IActionResult> AddFilesAsync(List<IFormFile> files, string origin)
        {
            _logger.LogDebug($"Files count {files.Count}");
            int count = 0;

            foreach (var formFile in files)
            {
                var headers = string.Join("; ", formFile.Headers.Select(kv => $"{kv.Key}={kv.Value}"));
                _logger.LogDebug($"ContentType: {formFile.ContentType}; FileName: {formFile.FileName}; Headers: {headers}");

                if (formFile.Length > 0)
                {
                    if (formFile.FileName.EndsWith(".zip"))
                    {
                        using (var stream = formFile.OpenReadStream())
                        {
                            count = await Task.Run(() => _fileRepository.AddArchive(stream, origin));
                        }

                        if (count > 0)
                        {
                            _logger.LogInformation("Archive file added");
                        }
                    }
                    else if (formFile.FileName.EndsWith(".mp3"))
                    {
                        // ...
                    }
                    else if (formFile.ContentType == "image/png")
                    {
                        // ...

                        _logger.LogInformation("Image file added");
                    }
                    else
                    {

                    }

                    /*using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }*/
                }
            }

            // process uploaded files
            // Don't rely on or trust the FileName property without validation.

            return Ok(new { count = count});
        }
    }
}