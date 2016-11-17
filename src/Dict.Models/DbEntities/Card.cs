using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Models
{
    public class Card
    {
        public int Id { get; set; }
        public string Word { get; set; }
        public string Translation { get; set; }
        public string Transcription { get; set; }
        public string Context { get; set; }

        [JsonIgnore]
        public string ImageName { get; set; }
        
        [JsonIgnore]
        public string SoundName { get; set; }

        public int CollectionId { get; set; }
        [JsonIgnore]
        public Collection Collection { get; set; }

        [NotMapped]
        public string ExtImageURL { get; private set; } 
        [NotMapped]
        public string ImageURL 
        { 
            get
            {
                return string.IsNullOrEmpty(ImageName) ? "" : $"/{CardRepository.MEDIA_DIR}/{ImageName}";
            } 
            set
            {
                if (!string.IsNullOrEmpty(value)) 
                {
                    if (value.StartsWith("/" + CardRepository.MEDIA_DIR))
                    {
                        ImageName = value.Substring(CardRepository.MEDIA_DIR.Length);
                        ExtImageURL = null; 
                    }
                    else
                    {
                        ExtImageURL = value;
                        ImageName = null;
                    }
                }
                else
                {
                    ExtImageURL = null; 
                }
            } 
        }

        ///<summary>
        /// value defined by set_SoundURL
        ///</summary>
        [NotMapped]
        public string ExtSoundURL { get; private set; }

        ///<summary>
        /// get return response value
        /// set updates SoundId or extSoundUrl value
        ///</summary>
        [NotMapped]
        public string SoundURL
        { 
            get
            {    
                return string.IsNullOrEmpty(SoundName) ? "" : $"/{CardRepository.MEDIA_DIR}/{SoundName}";
            } 
            set
            {
                if (!string.IsNullOrEmpty(value)) 
                {
                    if (value.StartsWith("/" + CardRepository.MEDIA_DIR))
                    {
                        SoundName = value.Substring(CardRepository.MEDIA_DIR.Length);
                        ExtSoundURL = null; 
                    }
                    else
                    {
                        ExtSoundURL = value;
                    }
                }
                else
                {
                    ExtSoundURL = null; 
                }
            } 
        }
        
        public override string ToString()
        {
            return $"Card Id: {Id} Word: {Word} Img: {ImageName} Snd: {SoundName} CollectionId: {CollectionId}";
        }
    }
}
