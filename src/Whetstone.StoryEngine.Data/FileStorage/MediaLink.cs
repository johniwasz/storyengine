using System;
using System.Collections.Generic;
using System.Text;


namespace Whetstone.StoryEngine.Data.FileStorage
{

    public class MediaLink
    {

        public MediaLink()
        {


        }

        public MediaLink(string fileName, DateTime expirationTime)
        {
            this.FileName = fileName;
            this.AbsoluteExpireTime = expirationTime;
        }



        public string FileName { get; set; }


        public DateTime AbsoluteExpireTime { get; set; }

    }
}
