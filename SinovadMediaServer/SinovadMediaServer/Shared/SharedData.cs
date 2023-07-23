using SinovadMediaServer.CustomModels;
using SinovadMediaServer.DTOs;

#nullable disable

namespace SinovadMediaServer.Shared
{
    public class SharedData
    {
        public String ApiToken { get; set; }
        public MediaServerDto MediaServerData { get; set; }
        public TranscoderSettingsDto TranscoderSettingsData { get; set; }
        public UserDto UserData { get; set; }
        public List<CatalogDetailDto> ListPresets { get; set; }
    }
}
