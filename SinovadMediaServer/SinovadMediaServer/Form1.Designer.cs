using SinovadMediaServer.Configuration;
using Microsoft.Web.WebView2.Core;
using System.ComponentModel;
using static System.Net.WebRequestMethods;

namespace SinovadMediaServer
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.FormClosed += new FormClosedEventHandler(Form1_FormClosed);
            this.webView21 = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)(this.webView21)).BeginInit();
            this.SuspendLayout();
            this.Width = Screen.PrimaryScreen.WorkingArea.Width;
            this.Height = Screen.PrimaryScreen.WorkingArea.Height;
            this.Activate();
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.WindowState = FormWindowState.Normal;

            this.webView21.AllowExternalDrop = true;
            this.webView21.CreationProperties = null;
            this.webView21.DefaultBackgroundColor = System.Drawing.Color.Black;
            this.webView21.Location = new System.Drawing.Point(0, 0);
            this.webView21.Name = "webView21";
            this.webView21.Size = new System.Drawing.Size(this.ClientRectangle.Width, this.ClientRectangle.Height);

            this.Controls.Add(this.webView21);
            this.Name = "SinovadMediaServerForm";
            this.Text = "Sinovad Media Server";
            ((System.ComponentModel.ISupportInitialize)(this.webView21)).EndInit();
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.ResizeEnd += new System.EventHandler(this.Form1_ResizeEnd);
            this.ResumeLayout(false);
        }

        private async void Form1_Resize(object sender, EventArgs e)
        {
            Control control = (Control)sender;
            this.webView21.Size = new System.Drawing.Size(control.ClientRectangle.Width, control.ClientRectangle.Height);
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            webView21.CoreWebView2InitializationCompleted += new System.EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs>(webView21_CoreWebView2InitializationCompleted);
            var userDataFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EasyMediaServer");
            CoreWebView2EnvironmentOptions options = new CoreWebView2EnvironmentOptions("--disable-features=BlockInsecurePrivateNetworkRequests");
            var webView2Environment = CoreWebView2Environment.CreateAsync(null, userDataFolder, options).Result;
            await webView21.EnsureCoreWebView2Async(webView2Environment);
            webView21.CoreWebView2.ContainsFullScreenElementChanged += (obj, args) =>
            {
                this.FullScreen = webView21.CoreWebView2.ContainsFullScreenElement;
            };
        }

        private void Form1_ResizeEnd(object sender, System.EventArgs e)
        {
            Control control = (Control)sender;
            this.webView21.Size = new System.Drawing.Size(control.ClientRectangle.Width, control.ClientRectangle.Height);
        }

        private bool fullScreen = false;
        [DefaultValue(false)]
        public bool FullScreen
        {
            get { return fullScreen; }
            set
            {
                fullScreen = value;
                if (value)
                {
                    this.WindowState = FormWindowState.Normal;
                    this.FormBorderStyle = FormBorderStyle.None;
                    this.WindowState = FormWindowState.Maximized;
                    this.webView21.Size = new System.Drawing.Size(this.ClientRectangle.Width, this.ClientRectangle.Height);
                }
                else
                {
                    this.Activate();
                    this.FormBorderStyle = FormBorderStyle.Sizable;
                    this.WindowState = FormWindowState.Normal;
                    this.webView21.Size = new System.Drawing.Size(this.ClientRectangle.Width, this.ClientRectangle.Height);
                }
            }
        }

        private async void webView21_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            if (webView21 != null && webView21.CoreWebView2 != null)
            {
                //await webView21.CoreWebView2.Profile.ClearBrowsingDataAsync();
                //webView21.CoreWebView2.CookieManager.DeleteAllCookies();
                var appWebUrl = "http://www.sinovad.com?currenthost=" + _config["WebUrl"] + "&localipaddress=" + _config["IpAddress"];
                webView21.CoreWebView2.Navigate(appWebUrl);
            }
        }

        void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Do something
        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 webView21;

    }
}