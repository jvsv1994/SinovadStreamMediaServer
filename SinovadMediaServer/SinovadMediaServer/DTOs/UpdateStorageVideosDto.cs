using System.Collections.Generic;

#nullable disable

namespace SinovadMediaServer.DTOs
{
    public class UpdateStorageVideosDto
    {
        public List<StorageDto> ListStorages { get; set; }
        public string LogIdentifier { get; set; }

    }
}
