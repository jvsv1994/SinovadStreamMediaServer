using System.Collections.Generic;

#nullable disable

namespace SinovadMediaServer.DTOs
{
    public class UpdateStorageVideosDto
    {
        public List<AccountStorageDto> ListAccountStorages { get; set; }
        public string LogIdentifier { get; set; }

    }
}
