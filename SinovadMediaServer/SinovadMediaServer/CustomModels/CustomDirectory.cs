#nullable disable

namespace SinovadMediaServer.CustomModels
{
    public class CustomDirectory
    {
        public string path { get; set; }
        public string name { get; set; }
        public Boolean isMainDirectory { get; set; }
        public List<CustomDirectory> listSubdirectory { get; set; }

    }
}
