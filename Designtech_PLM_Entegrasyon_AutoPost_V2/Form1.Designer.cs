namespace Designtech_PLM_Entegrasyon_AutoPost_V2
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            btnStartAutoPost = new Button();
            groupBox1 = new GroupBox();
            btnBaglantiKur = new Button();
            rbLocalChoose = new RadioButton();
            rbServerChoose = new RadioButton();
            txtParola = new TextBox();
            txtKullaniciAdi = new TextBox();
            txtDatabaseAdi = new TextBox();
            txtServerName = new TextBox();
            groupBox2 = new GroupBox();
            txtShowApiURL = new TextBox();
            txtShowCatalog = new TextBox();
            txtShowServerName = new TextBox();
            btnConnectionReflesh = new Button();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            groupBox3 = new GroupBox();
            txtEndpointAltermatePart = new TextBox();
            label8 = new Label();
            txtEndpointRemoved = new TextBox();
            txtEndpointSendFile = new TextBox();
            txtEndpointCancelled = new TextBox();
            txtEndpointInwork = new TextBox();
            txtEndpointReleased = new TextBox();
            label6 = new Label();
            lblRemoved = new Label();
            lblSendFile = new Label();
            lblCancelled = new Label();
            lblInwork = new Label();
            lblReleased = new Label();
            lblBasicAuth = new Label();
            txtBasicPassword = new TextBox();
            txtBasicUsername = new TextBox();
            btnApiEkle = new Button();
            label4 = new Label();
            txtApiEndpoint = new TextBox();
            txtApiUrl = new TextBox();
            label5 = new Label();
            button1btnStopAutoPost = new Button();
            btnKapat = new Button();
            lblDataCount = new Label();
            groupBox4 = new GroupBox();
            listBox1 = new ListBox();
            groupBox5 = new GroupBox();
            btnListbox2Reflesh = new Button();
            listBox2 = new ListBox();
            contextMenuStrip1 = new ContextMenuStrip(components);
            çalıştırToolStripMenuItem = new ToolStripMenuItem();
            durdurToolStripMenuItem = new ToolStripMenuItem();
            kapatToolStripMenuItem = new ToolStripMenuItem();
            notifyIcon1 = new NotifyIcon(components);
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox4.SuspendLayout();
            groupBox5.SuspendLayout();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // btnStartAutoPost
            // 
            btnStartAutoPost.Location = new Point(1234, 527);
            btnStartAutoPost.Name = "btnStartAutoPost";
            btnStartAutoPost.Size = new Size(94, 29);
            btnStartAutoPost.TabIndex = 0;
            btnStartAutoPost.Text = "Başlat";
            btnStartAutoPost.UseVisualStyleBackColor = true;
            btnStartAutoPost.Click += btnStartAutoPost_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(btnBaglantiKur);
            groupBox1.Controls.Add(rbLocalChoose);
            groupBox1.Controls.Add(rbServerChoose);
            groupBox1.Controls.Add(txtParola);
            groupBox1.Controls.Add(txtKullaniciAdi);
            groupBox1.Controls.Add(txtDatabaseAdi);
            groupBox1.Controls.Add(txtServerName);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(647, 175);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "Bağlantı Ayarları";
            // 
            // btnBaglantiKur
            // 
            btnBaglantiKur.Location = new Point(541, 129);
            btnBaglantiKur.Name = "btnBaglantiKur";
            btnBaglantiKur.Size = new Size(94, 29);
            btnBaglantiKur.TabIndex = 6;
            btnBaglantiKur.Text = "Bağlan";
            btnBaglantiKur.UseVisualStyleBackColor = true;
            btnBaglantiKur.Click += btnBaglantiKur_Click;
            // 
            // rbLocalChoose
            // 
            rbLocalChoose.AutoSize = true;
            rbLocalChoose.Location = new Point(11, 121);
            rbLocalChoose.Name = "rbLocalChoose";
            rbLocalChoose.Size = new Size(65, 24);
            rbLocalChoose.TabIndex = 5;
            rbLocalChoose.Text = "Local";
            rbLocalChoose.UseVisualStyleBackColor = true;
            // 
            // rbServerChoose
            // 
            rbServerChoose.AutoSize = true;
            rbServerChoose.Checked = true;
            rbServerChoose.Location = new Point(11, 78);
            rbServerChoose.Name = "rbServerChoose";
            rbServerChoose.Size = new Size(71, 24);
            rbServerChoose.TabIndex = 4;
            rbServerChoose.TabStop = true;
            rbServerChoose.Text = "Server";
            rbServerChoose.UseVisualStyleBackColor = true;
            // 
            // txtParola
            // 
            txtParola.Cursor = Cursors.IBeam;
            txtParola.Location = new Point(204, 78);
            txtParola.Name = "txtParola";
            txtParola.PasswordChar = '*';
            txtParola.PlaceholderText = "Parola";
            txtParola.Size = new Size(192, 27);
            txtParola.TabIndex = 3;
            // 
            // txtKullaniciAdi
            // 
            txtKullaniciAdi.Cursor = Cursors.IBeam;
            txtKullaniciAdi.Location = new Point(402, 36);
            txtKullaniciAdi.Name = "txtKullaniciAdi";
            txtKullaniciAdi.PlaceholderText = "Kullanıcı Adı";
            txtKullaniciAdi.Size = new Size(192, 27);
            txtKullaniciAdi.TabIndex = 2;
            // 
            // txtDatabaseAdi
            // 
            txtDatabaseAdi.Cursor = Cursors.IBeam;
            txtDatabaseAdi.Location = new Point(204, 36);
            txtDatabaseAdi.Name = "txtDatabaseAdi";
            txtDatabaseAdi.PlaceholderText = "Database Adı";
            txtDatabaseAdi.Size = new Size(192, 27);
            txtDatabaseAdi.TabIndex = 1;
            // 
            // txtServerName
            // 
            txtServerName.Cursor = Cursors.IBeam;
            txtServerName.Location = new Point(6, 36);
            txtServerName.Name = "txtServerName";
            txtServerName.PlaceholderText = "Server Name";
            txtServerName.Size = new Size(192, 27);
            txtServerName.TabIndex = 0;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(txtShowApiURL);
            groupBox2.Controls.Add(txtShowCatalog);
            groupBox2.Controls.Add(txtShowServerName);
            groupBox2.Controls.Add(btnConnectionReflesh);
            groupBox2.Controls.Add(label3);
            groupBox2.Controls.Add(label2);
            groupBox2.Controls.Add(label1);
            groupBox2.Location = new Point(698, 12);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(647, 175);
            groupBox2.TabIndex = 2;
            groupBox2.TabStop = false;
            groupBox2.Text = "Bağlantı Durumu";
            // 
            // txtShowApiURL
            // 
            txtShowApiURL.Enabled = false;
            txtShowApiURL.Location = new Point(142, 118);
            txtShowApiURL.Name = "txtShowApiURL";
            txtShowApiURL.Size = new Size(389, 27);
            txtShowApiURL.TabIndex = 6;
            // 
            // txtShowCatalog
            // 
            txtShowCatalog.Enabled = false;
            txtShowCatalog.Location = new Point(142, 81);
            txtShowCatalog.Name = "txtShowCatalog";
            txtShowCatalog.Size = new Size(389, 27);
            txtShowCatalog.TabIndex = 5;
            // 
            // txtShowServerName
            // 
            txtShowServerName.Enabled = false;
            txtShowServerName.Location = new Point(142, 47);
            txtShowServerName.Name = "txtShowServerName";
            txtShowServerName.Size = new Size(389, 27);
            txtShowServerName.TabIndex = 4;
            // 
            // btnConnectionReflesh
            // 
            btnConnectionReflesh.Location = new Point(536, 26);
            btnConnectionReflesh.Name = "btnConnectionReflesh";
            btnConnectionReflesh.Size = new Size(94, 29);
            btnConnectionReflesh.TabIndex = 3;
            btnConnectionReflesh.Text = "Yenile";
            btnConnectionReflesh.UseVisualStyleBackColor = true;
            btnConnectionReflesh.Click += btnConnectionReflesh_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(27, 121);
            label3.Name = "label3";
            label3.Size = new Size(68, 20);
            label3.TabIndex = 2;
            label3.Text = "APİ URL :";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(27, 81);
            label2.Name = "label2";
            label2.Size = new Size(72, 20);
            label2.TabIndex = 1;
            label2.Text = "Catalog : ";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(27, 43);
            label1.Name = "label1";
            label1.Size = new Size(105, 20);
            label1.TabIndex = 0;
            label1.Text = "Server Name : ";
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(txtEndpointAltermatePart);
            groupBox3.Controls.Add(label8);
            groupBox3.Controls.Add(txtEndpointRemoved);
            groupBox3.Controls.Add(txtEndpointSendFile);
            groupBox3.Controls.Add(txtEndpointCancelled);
            groupBox3.Controls.Add(txtEndpointInwork);
            groupBox3.Controls.Add(txtEndpointReleased);
            groupBox3.Controls.Add(label6);
            groupBox3.Controls.Add(lblRemoved);
            groupBox3.Controls.Add(lblSendFile);
            groupBox3.Controls.Add(lblCancelled);
            groupBox3.Controls.Add(lblInwork);
            groupBox3.Controls.Add(lblReleased);
            groupBox3.Controls.Add(lblBasicAuth);
            groupBox3.Controls.Add(txtBasicPassword);
            groupBox3.Controls.Add(txtBasicUsername);
            groupBox3.Controls.Add(btnApiEkle);
            groupBox3.Controls.Add(label4);
            groupBox3.Controls.Add(txtApiEndpoint);
            groupBox3.Controls.Add(txtApiUrl);
            groupBox3.Location = new Point(12, 193);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(1333, 301);
            groupBox3.TabIndex = 3;
            groupBox3.TabStop = false;
            groupBox3.Text = "Api Bağlantı Ayarları";
            groupBox3.Enter += groupBox3_Enter;
            // 
            // txtEndpointAltermatePart
            // 
            txtEndpointAltermatePart.Cursor = Cursors.IBeam;
            txtEndpointAltermatePart.Location = new Point(828, 133);
            txtEndpointAltermatePart.Name = "txtEndpointAltermatePart";
            txtEndpointAltermatePart.PlaceholderText = "ALTERNATE PART";
            txtEndpointAltermatePart.Size = new Size(389, 27);
            txtEndpointAltermatePart.TabIndex = 31;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(670, 141);
            label8.Name = "label8";
            label8.Size = new Size(123, 20);
            label8.TabIndex = 30;
            label8.Text = "ALTERNATE PART";
            // 
            // txtEndpointRemoved
            // 
            txtEndpointRemoved.Cursor = Cursors.IBeam;
            txtEndpointRemoved.Location = new Point(828, 208);
            txtEndpointRemoved.Name = "txtEndpointRemoved";
            txtEndpointRemoved.PlaceholderText = "REMOVED";
            txtEndpointRemoved.Size = new Size(389, 27);
            txtEndpointRemoved.TabIndex = 29;
            // 
            // txtEndpointSendFile
            // 
            txtEndpointSendFile.Cursor = Cursors.IBeam;
            txtEndpointSendFile.Location = new Point(828, 171);
            txtEndpointSendFile.Name = "txtEndpointSendFile";
            txtEndpointSendFile.PlaceholderText = "SEND FILE";
            txtEndpointSendFile.Size = new Size(389, 27);
            txtEndpointSendFile.TabIndex = 28;
            // 
            // txtEndpointCancelled
            // 
            txtEndpointCancelled.Cursor = Cursors.IBeam;
            txtEndpointCancelled.Location = new Point(828, 100);
            txtEndpointCancelled.Name = "txtEndpointCancelled";
            txtEndpointCancelled.PlaceholderText = "CANCELLED";
            txtEndpointCancelled.Size = new Size(389, 27);
            txtEndpointCancelled.TabIndex = 27;
            // 
            // txtEndpointInwork
            // 
            txtEndpointInwork.Cursor = Cursors.IBeam;
            txtEndpointInwork.Location = new Point(828, 67);
            txtEndpointInwork.Name = "txtEndpointInwork";
            txtEndpointInwork.PlaceholderText = "INWORK";
            txtEndpointInwork.Size = new Size(389, 27);
            txtEndpointInwork.TabIndex = 26;
            // 
            // txtEndpointReleased
            // 
            txtEndpointReleased.Cursor = Cursors.IBeam;
            txtEndpointReleased.Location = new Point(828, 27);
            txtEndpointReleased.Name = "txtEndpointReleased";
            txtEndpointReleased.PlaceholderText = "RELEASED";
            txtEndpointReleased.Size = new Size(389, 27);
            txtEndpointReleased.TabIndex = 25;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(641, 29);
            label6.Name = "label6";
            label6.Size = new Size(15, 20);
            label6.TabIndex = 23;
            label6.Text = "/";
            // 
            // lblRemoved
            // 
            lblRemoved.AutoSize = true;
            lblRemoved.Location = new Point(670, 215);
            lblRemoved.Name = "lblRemoved";
            lblRemoved.Size = new Size(78, 20);
            lblRemoved.TabIndex = 17;
            lblRemoved.Text = "REMOVED";
            // 
            // lblSendFile
            // 
            lblSendFile.AutoSize = true;
            lblSendFile.Location = new Point(670, 178);
            lblSendFile.Name = "lblSendFile";
            lblSendFile.Size = new Size(77, 20);
            lblSendFile.TabIndex = 16;
            lblSendFile.Text = "SEND FILE";
            // 
            // lblCancelled
            // 
            lblCancelled.AutoSize = true;
            lblCancelled.Location = new Point(670, 104);
            lblCancelled.Name = "lblCancelled";
            lblCancelled.Size = new Size(89, 20);
            lblCancelled.TabIndex = 15;
            lblCancelled.Text = "CANCELLED";
            // 
            // lblInwork
            // 
            lblInwork.AutoSize = true;
            lblInwork.Location = new Point(670, 67);
            lblInwork.Name = "lblInwork";
            lblInwork.Size = new Size(67, 20);
            lblInwork.TabIndex = 14;
            lblInwork.Text = "INWORK";
            // 
            // lblReleased
            // 
            lblReleased.AutoSize = true;
            lblReleased.Location = new Point(670, 30);
            lblReleased.Name = "lblReleased";
            lblReleased.Size = new Size(78, 20);
            lblReleased.TabIndex = 13;
            lblReleased.Text = "RELEASED";
            // 
            // lblBasicAuth
            // 
            lblBasicAuth.AutoSize = true;
            lblBasicAuth.Location = new Point(11, 56);
            lblBasicAuth.Name = "lblBasicAuth";
            lblBasicAuth.Size = new Size(78, 20);
            lblBasicAuth.TabIndex = 12;
            lblBasicAuth.Text = "Basic Auth";
            // 
            // txtBasicPassword
            // 
            txtBasicPassword.Cursor = Cursors.IBeam;
            txtBasicPassword.Location = new Point(225, 79);
            txtBasicPassword.Name = "txtBasicPassword";
            txtBasicPassword.PasswordChar = '.';
            txtBasicPassword.PlaceholderText = "Password";
            txtBasicPassword.Size = new Size(192, 27);
            txtBasicPassword.TabIndex = 11;
            // 
            // txtBasicUsername
            // 
            txtBasicUsername.Cursor = Cursors.IBeam;
            txtBasicUsername.Location = new Point(11, 79);
            txtBasicUsername.Name = "txtBasicUsername";
            txtBasicUsername.PlaceholderText = "Username";
            txtBasicUsername.Size = new Size(192, 27);
            txtBasicUsername.TabIndex = 7;
            // 
            // btnApiEkle
            // 
            btnApiEkle.Location = new Point(1222, 253);
            btnApiEkle.Name = "btnApiEkle";
            btnApiEkle.Size = new Size(94, 29);
            btnApiEkle.TabIndex = 9;
            btnApiEkle.Text = "Ekle";
            btnApiEkle.UseVisualStyleBackColor = true;
            btnApiEkle.Click += btnApiEkle_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(423, 29);
            label4.Name = "label4";
            label4.Size = new Size(15, 20);
            label4.TabIndex = 8;
            label4.Text = "/";
            // 
            // txtApiEndpoint
            // 
            txtApiEndpoint.Cursor = Cursors.IBeam;
            txtApiEndpoint.Location = new Point(443, 26);
            txtApiEndpoint.Name = "txtApiEndpoint";
            txtApiEndpoint.PlaceholderText = "Endpoint";
            txtApiEndpoint.Size = new Size(192, 27);
            txtApiEndpoint.TabIndex = 7;
            // 
            // txtApiUrl
            // 
            txtApiUrl.Cursor = Cursors.IBeam;
            txtApiUrl.Location = new Point(11, 26);
            txtApiUrl.Name = "txtApiUrl";
            txtApiUrl.PlaceholderText = "Api URL";
            txtApiUrl.Size = new Size(406, 27);
            txtApiUrl.TabIndex = 7;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(668, 536);
            label5.Name = "label5";
            label5.Size = new Size(129, 20);
            label5.TabIndex = 4;
            label5.Text = "İşlenen veri sayısı :";
            // 
            // button1btnStopAutoPost
            // 
            button1btnStopAutoPost.Location = new Point(1107, 527);
            button1btnStopAutoPost.Name = "button1btnStopAutoPost";
            button1btnStopAutoPost.Size = new Size(94, 29);
            button1btnStopAutoPost.TabIndex = 5;
            button1btnStopAutoPost.Text = "Durdur";
            button1btnStopAutoPost.UseVisualStyleBackColor = true;
            button1btnStopAutoPost.Click += button1btnStopAutoPost_Click;
            // 
            // btnKapat
            // 
            btnKapat.Location = new Point(979, 527);
            btnKapat.Name = "btnKapat";
            btnKapat.Size = new Size(94, 29);
            btnKapat.TabIndex = 6;
            btnKapat.Text = "Kapat";
            btnKapat.UseVisualStyleBackColor = true;
            btnKapat.Click += btnKapat_Click;
            // 
            // lblDataCount
            // 
            lblDataCount.AutoSize = true;
            lblDataCount.Location = new Point(803, 536);
            lblDataCount.Name = "lblDataCount";
            lblDataCount.Size = new Size(17, 20);
            lblDataCount.TabIndex = 7;
            lblDataCount.Text = "0";
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(listBox1);
            groupBox4.Location = new Point(12, 569);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new Size(1333, 325);
            groupBox4.TabIndex = 8;
            groupBox4.TabStop = false;
            groupBox4.Text = "PLM Data Post List";
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.Location = new Point(11, 46);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(1310, 264);
            listBox1.TabIndex = 0;
            listBox1.SelectedIndexChanged += listBox1_SelectedIndexChanged_1;
            // 
            // groupBox5
            // 
            groupBox5.Controls.Add(btnListbox2Reflesh);
            groupBox5.Controls.Add(listBox2);
            groupBox5.Location = new Point(1351, 12);
            groupBox5.Name = "groupBox5";
            groupBox5.Size = new Size(279, 882);
            groupBox5.TabIndex = 9;
            groupBox5.TabStop = false;
            groupBox5.Text = "Log dosya listesi";
            // 
            // btnListbox2Reflesh
            // 
            btnListbox2Reflesh.Location = new Point(170, 34);
            btnListbox2Reflesh.Name = "btnListbox2Reflesh";
            btnListbox2Reflesh.Size = new Size(94, 29);
            btnListbox2Reflesh.TabIndex = 1;
            btnListbox2Reflesh.Text = "Yenile";
            btnListbox2Reflesh.UseVisualStyleBackColor = true;
            btnListbox2Reflesh.Click += btnListbox2Reflesh_Click;
            // 
            // listBox2
            // 
            listBox2.FormattingEnabled = true;
            listBox2.Location = new Point(7, 27);
            listBox2.Name = "listBox2";
            listBox2.Size = new Size(267, 844);
            listBox2.TabIndex = 0;
            listBox2.SelectedIndexChanged += listBox2_SelectedIndexChanged;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.ImageScalingSize = new Size(20, 20);
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { çalıştırToolStripMenuItem, durdurToolStripMenuItem, kapatToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(125, 76);
            contextMenuStrip1.Opening += contextMenuStrip1_Opening;
            // 
            // çalıştırToolStripMenuItem
            // 
            çalıştırToolStripMenuItem.Name = "çalıştırToolStripMenuItem";
            çalıştırToolStripMenuItem.Size = new Size(124, 24);
            çalıştırToolStripMenuItem.Text = "Çalıştır";
            çalıştırToolStripMenuItem.Click += çalıştırToolStripMenuItem_Click;
            // 
            // durdurToolStripMenuItem
            // 
            durdurToolStripMenuItem.Name = "durdurToolStripMenuItem";
            durdurToolStripMenuItem.Size = new Size(124, 24);
            durdurToolStripMenuItem.Text = "Durdur";
            durdurToolStripMenuItem.Click += durdurToolStripMenuItem_Click;
            // 
            // kapatToolStripMenuItem
            // 
            kapatToolStripMenuItem.Name = "kapatToolStripMenuItem";
            kapatToolStripMenuItem.Size = new Size(124, 24);
            kapatToolStripMenuItem.Text = "Kapat";
            kapatToolStripMenuItem.Click += kapatToolStripMenuItem_Click;
            // 
            // notifyIcon1
            // 
            notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            notifyIcon1.Icon = (Icon)resources.GetObject("notifyIcon1.Icon");
            notifyIcon1.Text = "Designtech PLM";
            notifyIcon1.Visible = true;
            notifyIcon1.DoubleClick += NotifyIcon_DoubleClick;
            notifyIcon1.MouseDoubleClick += notifyIcon1_MouseDoubleClick;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            ClientSize = new Size(1642, 940);
            Controls.Add(groupBox5);
            Controls.Add(groupBox4);
            Controls.Add(lblDataCount);
            Controls.Add(btnKapat);
            Controls.Add(button1btnStopAutoPost);
            Controls.Add(label5);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(btnStartAutoPost);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Designtech PLM Entegrasyon v1.7.0";
            FormClosing += Form1_FormClosing_1;
            FormClosed += Form1_FormClosed;
            Load += Form1_Load_1;
            Resize += Form1_Resize;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            groupBox4.ResumeLayout(false);
            groupBox5.ResumeLayout(false);
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnStartAutoPost;
        private GroupBox groupBox1;
        private RadioButton rbServerChoose;
        private TextBox txtParola;
        private TextBox txtKullaniciAdi;
        private TextBox txtDatabaseAdi;
        private TextBox txtServerName;
        private GroupBox groupBox2;
        private Button btnConnectionReflesh;
        private Label label3;
        private Label label2;
        private Label label1;
        private RadioButton rbLocalChoose;
        private Button btnBaglantiKur;
        private GroupBox groupBox3;
        private Label label4;
        private TextBox txtApiEndpoint;
        private TextBox txtApiUrl;
        private Button btnApiEkle;
        private Label label5;
        private Button button1btnStopAutoPost;
        private Button btnKapat;
        private Label lblDataCount;
        private GroupBox groupBox4;
        private ListBox listBox1;
        private GroupBox groupBox5;
        private ListBox listBox2;
        private TextBox txtShowApiURL;
        private TextBox txtShowCatalog;
        private TextBox txtShowServerName;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem çalıştırToolStripMenuItem;
        private ToolStripMenuItem durdurToolStripMenuItem;
        private ToolStripMenuItem kapatToolStripMenuItem;
        private NotifyIcon notifyIcon1;
		private Label lblBasicAuth;
		private TextBox txtBasicPassword;
		private TextBox txtBasicUsername;
        private Label lblRemoved;
        private Label lblSendFile;
        private Label lblCancelled;
        private Label lblInwork;
        private Label lblReleased;
        private TextBox txtEndpointRemoved;
        private TextBox txtEndpointSendFile;
        private TextBox txtEndpointCancelled;
        private TextBox txtEndpointInwork;
        private TextBox txtEndpointReleased;
        private Label label6;
        private TextBox txtEndpointAltermatePart;
        private Label label8;
        private Button btnListbox2Reflesh;
    }
}
