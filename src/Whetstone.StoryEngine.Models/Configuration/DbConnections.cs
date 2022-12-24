namespace Whetstone.StoryEngine.Models.Configuration
{





    public class EncryptionConfig
    {
        public string Key { get; set; }

        public string KeyVector { get; set; }


        public int? LinkTimeout { get; set; }

        public string MediaRoute { get; set; }
    }

}
