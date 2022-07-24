using Amazon.S3;
using Amazon.S3.Transfer;

namespace TestUtilities;

public class FakeTransferUtility : ITransferUtility
{

    public FakeTransferUtility(DirectoryInfo baseDirectory, IAmazonS3 amazonS3)
    {
        this.BaseDirectory = baseDirectory;
        this.S3Client = amazonS3;
    }

    public DirectoryInfo BaseDirectory { get; }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public Task UploadAsync(string filePath, string bucketName, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task UploadAsync(string filePath, string bucketName, string key,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task UploadAsync(Stream stream, string bucketName, string key,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task UploadAsync(TransferUtilityUploadRequest request, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task AbortMultipartUploadsAsync(string bucketName, DateTime initiatedDate,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public async Task DownloadAsync(TransferUtilityDownloadRequest request, CancellationToken cancellationToken = new CancellationToken())
    {
        File.Copy(Path.Combine(this.BaseDirectory.FullName, request.Key), request.FilePath, true);

    }

    public Task<Stream> OpenStreamAsync(string bucketName, string key, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task<Stream> OpenStreamAsync(TransferUtilityOpenStreamRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task UploadDirectoryAsync(string directory, string bucketName,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task UploadDirectoryAsync(string directory, string bucketName, string searchPattern, SearchOption searchOption,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task UploadDirectoryAsync(TransferUtilityUploadDirectoryRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task DownloadDirectoryAsync(string bucketName, string s3Directory, string localDirectory,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task DownloadDirectoryAsync(TransferUtilityDownloadDirectoryRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public async Task DownloadAsync(string filePath, string bucketName, string key,
        CancellationToken cancellationToken = new CancellationToken())
    {
        File.Copy(new FileInfo(Path.Combine(this.BaseDirectory.FullName, key)).FullName, filePath);
    }

    public void UploadDirectory(string directory, string bucketName)
    {
        throw new NotImplementedException();
    }

    public void UploadDirectory(string directory, string bucketName, string searchPattern, SearchOption searchOption)
    {
        throw new NotImplementedException();
    }

    public void UploadDirectory(TransferUtilityUploadDirectoryRequest request)
    {
        throw new NotImplementedException();
    }

    public void Upload(string filePath, string bucketName)
    {
        throw new NotImplementedException();
    }

    public void Upload(string filePath, string bucketName, string key)
    {
        throw new NotImplementedException();
    }

    public void Upload(Stream stream, string bucketName, string key)
    {
        throw new NotImplementedException();
    }

    public void Upload(TransferUtilityUploadRequest request)
    {
        throw new NotImplementedException();
    }

    public Stream OpenStream(string bucketName, string key)
    {
        throw new NotImplementedException();
    }

    public Stream OpenStream(TransferUtilityOpenStreamRequest request)
    {
        throw new NotImplementedException();
    }

    public void Download(string filePath, string bucketName, string key)
    {
        throw new NotImplementedException();
    }

    public void Download(TransferUtilityDownloadRequest request)
    {
        throw new NotImplementedException();
    }

    public void DownloadDirectory(string bucketName, string s3Directory, string localDirectory)
    {
        throw new NotImplementedException();
    }

    public void DownloadDirectory(TransferUtilityDownloadDirectoryRequest request)
    {
        throw new NotImplementedException();
    }

    public void AbortMultipartUploads(string bucketName, DateTime initiatedDate)
    {
        throw new NotImplementedException();
    }

    public IAmazonS3 S3Client { get; }
}