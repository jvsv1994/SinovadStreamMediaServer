using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SinovadMediaServer.Shared;
using System.Diagnostics;

namespace SinovadMediaServer
{
    public partial class Form1 : Form
    {

        private static SharedService _sharedService;

        private static SharedData _sharedData;

        public Form1(IWebHost webHost)
        {
            var sharedService = webHost.Services.GetService<SharedService>();
            var sharedData = webHost.Services.GetService<SharedData>();
            _sharedService = sharedService;
            _sharedData = sharedData;
            InitializeComponent();
            ValidateMediaServer();
            Task.Run(() =>
            {
                webHost.Run();
            });
        }

        private async void ValidateMediaServer()
        {
            var response = await _sharedService.AuthenticateMediaServer();
            if (response.IsSuccess && response.Data != null)
            {
                var data = response.Data;
                _sharedData.ApiToken = data.ApiToken;
                _sharedData.MediaServerData = data.MediaServer;
                if (data.User != null)
                {
                    _sharedData.UserData = data.User;
                    var completed = await _sharedService.SetupMediaServer();
                    if (completed)
                    {
                        usernameLabel.Text = _sharedData.UserData.UserName;
                        ipAddressAndPortLabel.Text = _sharedData.MediaServerData.IpAddress + " : " + _sharedData.MediaServerData.Port;
                        deviceNameLabel.Text = _sharedData.MediaServerData.DeviceName;
                        familyNameLabel.Text = _sharedData.MediaServerData.FamilyName != null && _sharedData.MediaServerData.FamilyName != "" ? _sharedData.MediaServerData.FamilyName : "No especificado";
                        _sharedService.InjectTranscodeMiddleware();
                        panelMediaServerInfo.Visible = true;
                    }
                }
                else
                {
                    panelValidateCredentials.Visible = true;
                }
            }
            else
            {
                panelValidateCredentials.Visible = true;
            }
        }

        private void webView21_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private async void validateUserButton_Click(object sender, EventArgs e)
        {
            validateUserButton.Enabled = false;
            textBoxUser.Enabled = false;
            textBoxPassword.Enabled = false;
            authenticationMessageLabel.Visible = false;
            var res = await _sharedService.ValidateUser(textBoxUser.Text, textBoxPassword.Text);
            if (res.IsSuccess)
            {
                var data = res.Data;
                _sharedData.ApiToken = data.ApiToken;
                _sharedData.UserData = data.User;
                authenticationMessageLabel.ForeColor = Color.Green;
                authenticationMessageLabel.Text = "Autenticación exitosa";
                authenticationMessageLabel.Visible = true;
                authenticationMessageLabel.Text = "Configurando Servidor multimedia, espere un momento por favor";
                var completed = await _sharedService.SetupMediaServer();
                if (completed)
                {
                    usernameLabel.Text = _sharedData.UserData.UserName;
                    ipAddressAndPortLabel.Text = _sharedData.MediaServerData.IpAddress + " : " + _sharedData.MediaServerData.Port;
                    deviceNameLabel.Text = _sharedData.MediaServerData.DeviceName;
                    familyNameLabel.Text = _sharedData.MediaServerData.FamilyName != null && _sharedData.MediaServerData.FamilyName != "" ? _sharedData.MediaServerData.FamilyName : "No especificado";
                    _sharedService.InjectTranscodeMiddleware();
                    panelValidateCredentials.Visible = false;
                    panelMediaServerInfo.Visible = true;
                }
            }
            else
            {
                authenticationMessageLabel.ForeColor = Color.Red;
                authenticationMessageLabel.Text = res.Message;
                authenticationMessageLabel.Visible = true;
                validateUserButton.Enabled = true;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void panelValidateCredentials_Paint(object sender, PaintEventArgs e)
        {

        }

        private void manageLibrariesButton_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo("http://streamweb.sinovad.com/settings/server/" + _sharedData.MediaServerData.Guid + "/manage/libraries?apiToken=" + _sharedData.ApiToken) { UseShellExecute = true });
        }

        private void authenticationMessageLabel_Click(object sender, EventArgs e)
        {

        }

        private void buttonRegisterUser_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo("http://streamweb.sinovad.com/register") { UseShellExecute = true });
        }
    }
}