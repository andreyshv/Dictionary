using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Models
{
    /// <summary>
    /// Card class is an entity class
    /// </summary>
    public class Card
    {
        private string _imageName;
        private string _soundName;
        private string _extImageURL;
        private string _extSoundURL;

        public int Id { get; set; }
        public string Word { get; set; }
        public string Translation { get; set; }
        public string Transcription { get; set; }
        public string Context { get; set; }

        /// <summary>
        /// ImageName is a name of associated file in storage
        /// Field excluded from exchange with frontend
        /// </summary>
        [JsonIgnore]
        public string ImageName { get { return _imageName; } set { _imageName = value; } }
        
        /// <summary>
        /// SoundName is a name of associated file in storage
        /// Field excluded from exchange with frontend
        /// </summary>
        [JsonIgnore]
        public string SoundName { get { return _soundName; } set { _soundName = value; } }

        public int CollectionId { get; set; }
        [JsonIgnore]
        public Collection Collection { get; set; }

        /// <summary>
        /// ImageURL field used to exchange with frontend
        /// </summary>
        [NotMapped]
        public string ImageURL 
        { 
            get
            {
                return GetURL(ImageName);
            } 
            set
            {
                SetURL(value, ref _imageName, ref _extImageURL);
            } 
        }

        /// <summary>
        /// SoundURL field used to exchange with frontend
        /// </summary>
        [NotMapped]
        public string SoundURL
        { 
            get
            {    
                return GetURL(SoundName);
            } 
            set
            {
                SetURL(value, ref _soundName, ref _extSoundURL);
            } 
        }
        
        /// <summary>
        /// Check if we got external urls then download files to storage and update names
        /// </summary>
        /// <param name="repository">File repository</param>
        /// <returns>Task</returns>
        public async Task StoreExtFilesAsync(IFileRepository repository)
        {
            if (_extImageURL != null)
            {
                ImageName = await repository.AddFileAsync(_extImageURL, Word);
                ImageURL = null;
            }

            if (_extSoundURL != null)
            {
                SoundName = await repository.AddFileAsync(_extSoundURL, Word);
                SoundURL = null;
            }
        }

        public override string ToString()
        {
            return $"Card Id: {Id} Word: {Word} Img: {ImageName} Snd: {SoundName} CollectionId: {CollectionId}";
        }

        private string GetURL(string name)
        {
            return string.IsNullOrEmpty(name) ? "" : $"/{FileRepository.MEDIA_DIR}/{name}";
        }

        private void SetURL(string value, ref string name, ref string url)
        {
            if (string.IsNullOrEmpty(value)) 
            {
                // cleanup
                name = null;
                url = null;
            }
            else if (value.StartsWith("/" + FileRepository.MEDIA_DIR))
            {
                // got local storage URL
                name = value.Substring(FileRepository.MEDIA_DIR.Length + 2);
                url = null;
            }
            else
            {
                // got external URL
                name = null;
                url = value;
            }
        }
    }
}
