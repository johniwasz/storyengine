namespace Whetstone.StoryEngine.Test
{

    public class PollyTests : TestServerFixture
    {

        //        [Fact]
        //        public async Task GetPollyVoice()
        //        {

        //            IPollyRequestProcessor pollyProcessor = new PollyRequestProcessor(_envOptions);


        //            Stream memStream =await pollyProcessor.GetPollyVoiceAsync("Hear me roar!");

        //            using (var fileStream = File.Create("roar.mp3"))
        //            {
        //                memStream.Seek(0, SeekOrigin.Begin);
        //                memStream.CopyTo(fileStream);
        //            }
        //        }

        //        [Fact()]
        //        public async Task GetPollyRequest()
        //        {

        //            DataSpeechText dataText = new DataSpeechText("It was a dark and stormy night");
        //            dataText.Voice = "Emma";

        //            List<DataSpeechFragment> speechFragments = new List<DataSpeechFragment>();
        //            speechFragments.Add(dataText);
        //            string apiKey = "mMBrUnBi1j16MHq8cEHTw4auIPrxbLvX2mRJaU5l";
        //            using (HttpClient clientGet = new HttpClient())
        //            {
        //                JsonSerializerSettings serializerSettings = GetJsonSettings();
        //                clientGet.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

        //                string postContent = JsonConvert.SerializeObject(speechFragments, serializerSettings);

        //                string escapedString = JsonConvert.ToString(postContent);

        //                var content = new StringContent(postContent, Encoding.UTF8, "application/json");

        //                content.Headers.Add("x-api-key", new List<string>() { apiKey} );
        //               // content.Headers.Add("Accept", "application/octet-stream");


        //                var result = await clientGet.PostAsync(
        //                    "https://5w93lr93ub.execute-api.us-east-1.amazonaws.com/Prod/api/polly/animalfarmtest", content);

        //                byte[] byteOut = await result.Content.ReadAsByteArrayAsync();

        ////                byte[] byteOut = Base64.Decode(base64);


        //                await File.WriteAllBytesAsync("emma.mp3", byteOut);
        //            }
        //        }




        //[Fact]
        //public async Task ProduceMp3()
        //{

        //    IMediaStreamer streamer = new S3MediaStreamer(_encryptionOptions);

        //    IPollyRequestProcessor pollyProcessor = new PollyRequestProcessor();

        //    ISpeechFragmentVoiceProcessor fragProcessor = new SpeechFragmentVoiceProcessor(pollyProcessor, streamer);

        //    List<DataSpeechFragment> speechFragments = new List<DataSpeechFragment>();
        //    DataAudioFile audFile = new DataAudioFile("Act1-OpeningMusic-alexa.mp3");
        //    DataSpeechText dataText = new DataSpeechText("It was a dark and stormy night");
        //    speechFragments.Add(dataText);
        //    speechFragments.Add(audFile);




        //    MemoryStream memStream =await fragProcessor.GetPollyVoiceAsync("env", "animalfarmtest", speechFragments);

        //    using (var fileStream = File.Create("joined.mp3"))
        //    {
        //        memStream.Seek(0, SeekOrigin.Begin);
        //        memStream.CopyTo(fileStream);
        //    }


        //}


    }
}
