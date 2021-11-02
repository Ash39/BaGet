using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BaGet.Core;
using Dropbox.Api;
using Dropbox.Api.Files;
using Microsoft.Extensions.Options;

namespace BaGet.DropBox
{
    public class DropBoxService : IStorageService
    {
        DropboxClient _client;

        string _accessToken;
        
        public DropBoxService(IOptionsSnapshot<DropBoxOptions> options,DropboxClient client)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            
            _accessToken = options.Value.AccessToken;
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<Stream> GetAsync(string path, CancellationToken cancellationToken = default)
        {
            Stream stream = null;

            try
            {
                var request = await _client.Files.DownloadAsync("/" + path);
                stream =  await request.GetContentAsStreamAsync();

            }
            catch (Exception)
            {
                stream.Dispose();

                // TODO
                throw;
            }

            return stream;
        }

        public Task<Uri> GetDownloadUriAsync(string path, CancellationToken cancellationToken = default)
        {
            var response =  _client.Files.DownloadAsync("/" + path);
            string url = response.Result.GetContentAsStringAsync().Result;

            return Task.FromResult(new Uri(url));
        }

        public async Task<StoragePutResult> PutAsync(string path, Stream content, string contentType, CancellationToken cancellationToken = default)
        {
            // TODO: Uploads should be idempotent. This should fail if and only if the blob
            // already exists but has different content.

            using (var seekableContent = new MemoryStream())
            {
                await content.CopyToAsync(seekableContent, 4096, cancellationToken);

                seekableContent.Seek(0, SeekOrigin.Begin);

                await _client.Files.UploadAsync("/" + path, WriteMode.Overwrite.Instance, body: seekableContent);
            }

            return StoragePutResult.Success;
        }

        public async Task DeleteAsync(string path, CancellationToken cancellationToken = default)
        {
            await _client.Files.DeleteV2Async("/" + path);
        }
    }
}