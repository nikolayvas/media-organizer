using System.Threading;
using System.Threading.Tasks;

namespace Image.Engine
{
    public interface IFileProcessor
    {
        Task CopyFileToDetinationDriveAsync(string filePath, string destinationDrive, CancellationToken cancelToken);
    }
}
