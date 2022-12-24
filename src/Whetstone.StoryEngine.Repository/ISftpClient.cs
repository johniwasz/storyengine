using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Configuration;

namespace Whetstone.StoryEngine.Repository
{
    public interface ISftpClient
    {

        Task UploadFileAsync(SftpConfig serverConfig, string destinationPath, string contents);

        Task UploadFileAsync(SftpConfig serverConfig, string destinationPath, string contents, Encoding textEncoding);
    }
}
