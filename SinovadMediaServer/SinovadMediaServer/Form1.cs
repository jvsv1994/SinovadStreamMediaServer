using SinovadMediaServer.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace SinovadMediaServer
{
    public partial class Form1 : Form
    {
        static HttpClient client = new HttpClient();
        public static IConfiguration _config { get; set; }

        public Form1(IConfiguration config)
        {
            _config = config;
            InitializeComponent();
        }

    }
}