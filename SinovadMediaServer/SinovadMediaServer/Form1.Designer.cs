using Microsoft.Web.WebView2.Core;
using System.ComponentModel;
using System.Windows.Forms;

namespace SinovadMediaServer
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            label1 = new Label();
            label2 = new Label();
            textBoxUser = new TextBox();
            textBoxPassword = new TextBox();
            validateUserButton = new Button();
            label3 = new Label();
            panelValidateCredentials = new Panel();
            authenticationMessageLabel = new Label();
            panelMediaServerInfo = new Panel();
            familyNameLabel = new Label();
            deviceNameLabel = new Label();
            ipAddressAndPortLabel = new Label();
            manageLibrariesButton = new Button();
            label9 = new Label();
            label8 = new Label();
            label7 = new Label();
            label6 = new Label();
            usernameLabel = new Label();
            label4 = new Label();
            pictureBox1 = new PictureBox();
            buttonRegisterUser = new Button();
            panelValidateCredentials.SuspendLayout();
            panelMediaServerInfo.SuspendLayout();
            ((ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 13F, FontStyle.Regular, GraphicsUnit.Point);
            label1.Location = new Point(380, 285);
            label1.Name = "label1";
            label1.Size = new Size(138, 47);
            label1.TabIndex = 0;
            label1.Text = "Usuario";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 13F, FontStyle.Regular, GraphicsUnit.Point);
            label2.Location = new Point(386, 441);
            label2.Name = "label2";
            label2.Size = new Size(196, 47);
            label2.TabIndex = 1;
            label2.Text = "Contraseña";
            // 
            // textBoxUser
            // 
            textBoxUser.Font = new Font("Segoe UI", 13F, FontStyle.Regular, GraphicsUnit.Point);
            textBoxUser.Location = new Point(386, 352);
            textBoxUser.Name = "textBoxUser";
            textBoxUser.Size = new Size(405, 54);
            textBoxUser.TabIndex = 2;
            // 
            // textBoxPassword
            // 
            textBoxPassword.Font = new Font("Segoe UI", 13F, FontStyle.Regular, GraphicsUnit.Point);
            textBoxPassword.Location = new Point(386, 521);
            textBoxPassword.Name = "textBoxPassword";
            textBoxPassword.PasswordChar = '*';
            textBoxPassword.Size = new Size(405, 54);
            textBoxPassword.TabIndex = 3;
            // 
            // validateUserButton
            // 
            validateUserButton.BackColor = SystemColors.MenuHighlight;
            validateUserButton.FlatAppearance.BorderColor = Color.White;
            validateUserButton.FlatAppearance.BorderSize = 0;
            validateUserButton.FlatStyle = FlatStyle.Flat;
            validateUserButton.ForeColor = SystemColors.HighlightText;
            validateUserButton.Location = new Point(386, 619);
            validateUserButton.Name = "validateUserButton";
            validateUserButton.Size = new Size(405, 64);
            validateUserButton.TabIndex = 4;
            validateUserButton.Text = "Vincular cuenta al Servidor";
            validateUserButton.UseVisualStyleBackColor = false;
            validateUserButton.Click += validateUserButton_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 13F, FontStyle.Regular, GraphicsUnit.Point);
            label3.Location = new Point(129, 61);
            label3.Name = "label3";
            label3.Size = new Size(993, 47);
            label3.TabIndex = 5;
            label3.Text = "Ingrese sus credenciales para vincular su cuenta a este servidor";
            // 
            // panelValidateCredentials
            // 
            panelValidateCredentials.Controls.Add(buttonRegisterUser);
            panelValidateCredentials.Controls.Add(authenticationMessageLabel);
            panelValidateCredentials.Controls.Add(label3);
            panelValidateCredentials.Controls.Add(validateUserButton);
            panelValidateCredentials.Controls.Add(textBoxPassword);
            panelValidateCredentials.Controls.Add(textBoxUser);
            panelValidateCredentials.Controls.Add(label2);
            panelValidateCredentials.Controls.Add(label1);
            panelValidateCredentials.Location = new Point(205, 134);
            panelValidateCredentials.Name = "panelValidateCredentials";
            panelValidateCredentials.Size = new Size(1181, 817);
            panelValidateCredentials.TabIndex = 6;
            panelValidateCredentials.Visible = false;
            panelValidateCredentials.Paint += panelValidateCredentials_Paint;
            // 
            // authenticationMessageLabel
            // 
            authenticationMessageLabel.AutoSize = true;
            authenticationMessageLabel.ForeColor = Color.Red;
            authenticationMessageLabel.Location = new Point(386, 145);
            authenticationMessageLabel.MaximumSize = new Size(405, 1021);
            authenticationMessageLabel.Name = "authenticationMessageLabel";
            authenticationMessageLabel.Size = new Size(318, 32);
            authenticationMessageLabel.TabIndex = 6;
            authenticationMessageLabel.Text = "authenticationMessageLabel";
            authenticationMessageLabel.Visible = false;
            authenticationMessageLabel.Click += authenticationMessageLabel_Click;
            // 
            // panelMediaServerInfo
            // 
            panelMediaServerInfo.Controls.Add(familyNameLabel);
            panelMediaServerInfo.Controls.Add(deviceNameLabel);
            panelMediaServerInfo.Controls.Add(ipAddressAndPortLabel);
            panelMediaServerInfo.Controls.Add(manageLibrariesButton);
            panelMediaServerInfo.Controls.Add(label9);
            panelMediaServerInfo.Controls.Add(label8);
            panelMediaServerInfo.Controls.Add(label7);
            panelMediaServerInfo.Controls.Add(label6);
            panelMediaServerInfo.Controls.Add(usernameLabel);
            panelMediaServerInfo.Controls.Add(label4);
            panelMediaServerInfo.Location = new Point(205, 134);
            panelMediaServerInfo.Name = "panelMediaServerInfo";
            panelMediaServerInfo.Size = new Size(1175, 811);
            panelMediaServerInfo.TabIndex = 6;
            panelMediaServerInfo.Visible = false;
            // 
            // familyNameLabel
            // 
            familyNameLabel.AutoSize = true;
            familyNameLabel.Location = new Point(423, 409);
            familyNameLabel.Name = "familyNameLabel";
            familyNameLabel.Size = new Size(199, 32);
            familyNameLabel.TabIndex = 9;
            familyNameLabel.Text = "familyNameLabel";
            // 
            // deviceNameLabel
            // 
            deviceNameLabel.AutoSize = true;
            deviceNameLabel.Location = new Point(423, 344);
            deviceNameLabel.Name = "deviceNameLabel";
            deviceNameLabel.Size = new Size(203, 32);
            deviceNameLabel.TabIndex = 8;
            deviceNameLabel.Text = "deviceNameLabel";
            // 
            // ipAddressAndPortLabel
            // 
            ipAddressAndPortLabel.AutoSize = true;
            ipAddressAndPortLabel.Location = new Point(423, 285);
            ipAddressAndPortLabel.Name = "ipAddressAndPortLabel";
            ipAddressAndPortLabel.Size = new Size(259, 32);
            ipAddressAndPortLabel.TabIndex = 7;
            ipAddressAndPortLabel.Text = "ipAddressAndPortLabel";
            // 
            // manageLibrariesButton
            // 
            manageLibrariesButton.BackColor = SystemColors.Highlight;
            manageLibrariesButton.FlatAppearance.BorderColor = Color.White;
            manageLibrariesButton.FlatAppearance.BorderSize = 0;
            manageLibrariesButton.FlatStyle = FlatStyle.Flat;
            manageLibrariesButton.Location = new Point(386, 520);
            manageLibrariesButton.Name = "manageLibrariesButton";
            manageLibrariesButton.Size = new Size(321, 64);
            manageLibrariesButton.TabIndex = 6;
            manageLibrariesButton.Text = "Administrar bibliotecas";
            manageLibrariesButton.UseVisualStyleBackColor = false;
            manageLibrariesButton.Click += manageLibrariesButton_Click;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(114, 409);
            label9.Name = "label9";
            label9.Size = new Size(188, 32);
            label9.TabIndex = 5;
            label9.Text = "Nombre familiar";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(114, 344);
            label8.Name = "label8";
            label8.Size = new Size(223, 32);
            label8.TabIndex = 4;
            label8.Text = "Nombre del Equipo";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(114, 285);
            label7.Name = "label7";
            label7.Size = new Size(93, 32);
            label7.TabIndex = 3;
            label7.Text = "Privado";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 13F, FontStyle.Regular, GraphicsUnit.Point);
            label6.Location = new Point(113, 204);
            label6.Name = "label6";
            label6.Size = new Size(306, 47);
            label6.TabIndex = 2;
            label6.Text = "Datos del Servidor";
            // 
            // usernameLabel
            // 
            usernameLabel.AutoSize = true;
            usernameLabel.Location = new Point(521, 83);
            usernameLabel.Name = "usernameLabel";
            usernameLabel.Size = new Size(175, 32);
            usernameLabel.TabIndex = 1;
            usernameLabel.Text = "usernameLabel";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(113, 83);
            label4.Name = "label4";
            label4.Size = new Size(327, 32);
            label4.TabIndex = 0;
            label4.Text = "Servidor vinculado al usuario:";
            // 
            // pictureBox1
            // 
            pictureBox1.ImageLocation = "http://cdn.sinovad.com/stream/web/assets/icon/sinovad-stream-large-logo.png";
            pictureBox1.Location = new Point(25, 12);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(360, 77);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 7;
            pictureBox1.TabStop = false;
            pictureBox1.Click += pictureBox1_Click;
            // 
            // buttonRegisterUser
            // 
            buttonRegisterUser.BackColor = SystemColors.MenuHighlight;
            buttonRegisterUser.FlatAppearance.BorderColor = Color.White;
            buttonRegisterUser.FlatAppearance.BorderSize = 0;
            buttonRegisterUser.FlatStyle = FlatStyle.Flat;
            buttonRegisterUser.ForeColor = SystemColors.HighlightText;
            buttonRegisterUser.Location = new Point(386, 711);
            buttonRegisterUser.Name = "buttonRegisterUser";
            buttonRegisterUser.Size = new Size(405, 64);
            buttonRegisterUser.TabIndex = 7;
            buttonRegisterUser.Text = "Registrarse";
            buttonRegisterUser.UseCompatibleTextRendering = true;
            buttonRegisterUser.UseVisualStyleBackColor = false;
            buttonRegisterUser.Click += buttonRegisterUser_Click;
            // 
            // Form1
            // 
            BackColor = SystemColors.ActiveCaptionText;
            ClientSize = new Size(1625, 1143);
            Controls.Add(panelValidateCredentials);
            Controls.Add(pictureBox1);
            Controls.Add(panelMediaServerInfo);
            ForeColor = SystemColors.ButtonHighlight;
            Name = "Form1";
            Text = "Sinovad Media Server";
            FormClosed += Form1_FormClosed;
            Load += Form1_Load;
            ResizeEnd += Form1_ResizeEnd;
            Resize += Form1_Resize;
            panelValidateCredentials.ResumeLayout(false);
            panelValidateCredentials.PerformLayout();
            panelMediaServerInfo.ResumeLayout(false);
            panelMediaServerInfo.PerformLayout();
            ((ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        private void BuilderWebViewMode()
        {
            //webView21 = new Microsoft.Web.WebView2.WinForms.WebView2();
            //((ISupportInitialize)webView21).BeginInit();
            //SuspendLayout();
            //// 
            //// webView21
            //// 
            //webView21.AllowExternalDrop = true;
            //webView21.CreationProperties = null;
            //webView21.DefaultBackgroundColor = Color.Black;
            //webView21.Location = new Point(0, 0);
            //webView21.Name = "webView21";
            //webView21.Size = new Size(274, 229);
            //webView21.TabIndex = 0;
            //webView21.ZoomFactor = 1D;
            //webView21.Click += webView21_Click;
            //// 
            //// Form1
            //// 
            //ClientSize = new Size(3814, 2009);
            //Controls.Add(webView21);
            //Name = "Form1";
            //Text = "Sinovad Media Server";
            //FormClosed += Form1_FormClosed;
            //Load += Form1_Load;
            //ResizeEnd += Form1_ResizeEnd;
            //Resize += Form1_Resize;
            //((ISupportInitialize)webView21).EndInit();
            //ResumeLayout(false);
        }

        private async void Form1_Resize(object sender, EventArgs e)
        {
            //Control control = (Control)sender;
            //this.webView21.Size = new Size(control.ClientRectangle.Width, control.ClientRectangle.Height);
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            //webView21.CoreWebView2InitializationCompleted += new System.EventHandler<CoreWebView2InitializationCompletedEventArgs>(webView21_CoreWebView2InitializationCompleted);
            //var userDataFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EasyMediaServer");
            //CoreWebView2EnvironmentOptions options = new CoreWebView2EnvironmentOptions("--disable-features=BlockInsecurePrivateNetworkRequests");
            //var webView2Environment = CoreWebView2Environment.CreateAsync(null, userDataFolder, options).Result;
            //await webView21.EnsureCoreWebView2Async(webView2Environment);
            //webView21.CoreWebView2.ContainsFullScreenElementChanged += (obj, args) =>
            //{
            //    this.FullScreen = webView21.CoreWebView2.ContainsFullScreenElement;
            //};
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            //Control control = (Control)sender;
            //this.webView21.Size = new Size(control.ClientRectangle.Width, control.ClientRectangle.Height);
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
                    //this.webView21.Size = new Size(this.ClientRectangle.Width, this.ClientRectangle.Height);
                }
                else
                {
                    this.Activate();
                    this.FormBorderStyle = FormBorderStyle.Sizable;
                    this.WindowState = FormWindowState.Normal;
                    //this.webView21.Size = new Size(this.ClientRectangle.Width, this.ClientRectangle.Height);
                }
            }
        }

        private async void webView21_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            //if (webView21 != null && webView21.CoreWebView2 != null)
            //{
            //    await webView21.CoreWebView2.Profile.ClearBrowsingDataAsync();
            //    webView21.CoreWebView2.CookieManager.DeleteAllCookies();
            //    var appWebUrl = "http://streamdesktop.sinovad.com?currenthost=" + _config.Value.WebUrl + "&localipaddress=" + _config.Value.IpAddress;
            //    webView21.CoreWebView2.Navigate(appWebUrl);
            //}
        }

        void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Do something
        }

        private Label label1;
        private Label label2;
        private TextBox textBoxUser;
        private TextBox textBoxPassword;
        private Button validateUserButton;
        private Label label3;
        private Panel panelValidateCredentials;
        private PictureBox pictureBox1;
        private Panel panelMediaServerInfo;
        private Label label4;
        private Label usernameLabel;
        private Label label8;
        private Label label7;
        private Label label6;
        private Label label9;
        private Button manageLibrariesButton;
        private Label authenticationMessageLabel;
        private Label familyNameLabel;
        private Label deviceNameLabel;
        private Label ipAddressAndPortLabel;
        private Button buttonRegisterUser;

        #endregion

        //private Microsoft.Web.WebView2.WinForms.WebView2 webView21;

    }
}