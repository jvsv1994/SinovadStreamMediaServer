

namespace SinovadMediaServer.Enums
{

    public enum CatalogEnum
    {
        TransmissionMethod = 1,
        Preset = 2
    }
    public enum TransmissionMethod
    {
        NONE = 1,
        MPEGDASH = 2,
        HLS = 3
    }
    public enum ServerState
    {
        Started = 1,
        Stopped = 2
    }

    public enum HttpMethodType
    {
        GET=1,
        POST=2,
        PUT=3,
        DELETE=4
    }
}
