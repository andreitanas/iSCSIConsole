using System;
using System.IO;
using Newtonsoft.Json.Linq;
using Microsoft.WindowsAzure.Storage.Blob;
using ISCSIDisk;
using Microsoft.WindowsAzure.Storage.Auth;

namespace AzureStorage
{
    public class PageBlobDiskImage : ExternalDisk
    {
        public const int DEFAULT_BYTES_PER_SECTOR = 4096;
        private CloudPageBlob cloudBlob;

        public PageBlobDiskImage() { }

        public override void SetParameters(object parameters)
        {
            var jParameters = parameters as JObject;
            if (jParameters == null)
                throw new ArgumentException("Expecting JObject parameters");

            var settings = jParameters.ToObject<PageBlobSettings>();
            var client = new CloudBlobClient(
                    new Uri(string.Format("https://{0}.blob.core.windows.net/", settings.AccountName)),
                    new StorageCredentials(settings.AccountName, settings.AccountKey));

            var container = client.GetContainerReference(settings.ContainerName.ToLower());
            container.CreateIfNotExistsAsync().Wait();
            cloudBlob = container.GetPageBlobReference(settings.BlobName);
            if (!cloudBlob.ExistsAsync().Result)
                cloudBlob.CreateAsync(settings.BlobSizeGB * 1024L * 1024L * 1024L).Wait();

            cloudBlob.FetchAttributesAsync().Wait();
        }

        public override int BytesPerSector
        {
            get
            {
                return DEFAULT_BYTES_PER_SECTOR;
            }
        }

        public override long Size
        {
            get
            {
                return cloudBlob.Properties.Length;
            }
        }

        public override byte[] ReadSectors(long sectorIndex, int sectorCount)
        {
            var buffer = new byte[BytesPerSector * sectorCount];
            cloudBlob.DownloadRangeToByteArrayAsync(buffer, 0, BytesPerSector * sectorIndex, buffer.Length).Wait();
            return buffer;
        }

        public override void WriteSectors(long sectorIndex, byte[] data)
        {
            cloudBlob.WritePagesAsync(new MemoryStream(data), sectorIndex * BytesPerSector, null).Wait();
        }
    }

    public class PageBlobSettings
    {
        public string AccountKey;
        public string AccountName;
        public string ContainerName;
        public string BlobName;
        public uint BlobSizeGB;
    }
}
