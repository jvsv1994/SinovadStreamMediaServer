#nullable disable

namespace SinovadMediaServer.Transversal.Common
{
    public class ResponseBase<T>
    {
        public T Data { get; set; }

        public bool IsSuccess { get; set; }

        public string Message { get; set; }

    }
}
