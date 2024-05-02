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
            label4 = new Label();
            label3 = new Label();
            txtShowWindchillUserName = new TextBox();
            txtShowWindchillServerName = new TextBox();
            txtShowCatalog = new TextBox();
            txtShowServerName = new TextBox();
            btnConnectionReflesh = new Button();
            label2 = new Label();
            label1 = new Label();
            groupBox3 = new GroupBox();
            txtWindchillApi = new TextBox();
            txtBasicPassword = new TextBox();
            txtBasicUsername = new TextBox();
            btnApiEkle = new Button();
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
            btnStartAutoPost.Location = new Point(836, 300);
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
            groupBox1.Size = new Size(610, 175);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "SQL Bağlantı Ayarları";
            // 
            // btnBaglantiKur
            // 
            btnBaglantiKur.Location = new Point(505, 129);
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
            rbLocalChoose.CheckedChanged += rbLocalChoose_CheckedChanged;
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
            rbServerChoose.CheckedChanged += rbServerChoose_CheckedChanged;
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
            groupBox2.Controls.Add(label4);
            groupBox2.Controls.Add(label3);
            groupBox2.Controls.Add(txtShowWindchillUserName);
            groupBox2.Controls.Add(txtShowWindchillServerName);
            groupBox2.Controls.Add(txtShowCatalog);
            groupBox2.Controls.Add(txtShowServerName);
            groupBox2.Controls.Add(btnConnectionReflesh);
            groupBox2.Controls.Add(label2);
            groupBox2.Controls.Add(label1);
            groupBox2.Location = new Point(630, 12);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(320, 282);
            groupBox2.TabIndex = 2;
            groupBox2.TabStop = false;
            groupBox2.Text = "Bağlantı Durumu";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(6, 195);
            label4.Name = "label4";
            label4.Size = new Size(159, 20);
            label4.TabIndex = 9;
            label4.Text = "Windchill User Name : ";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(6, 142);
            label3.Name = "label3";
            label3.Size = new Size(171, 20);
            label3.TabIndex = 8;
            label3.Text = "Windchill Server Name : ";
            // 
            // txtShowWindchillUserName
            // 
            txtShowWindchillUserName.BorderStyle = BorderStyle.None;
            txtShowWindchillUserName.Enabled = false;
            txtShowWindchillUserName.Location = new Point(11, 218);
            txtShowWindchillUserName.Name = "txtShowWindchillUserName";
            txtShowWindchillUserName.Size = new Size(189, 20);
            txtShowWindchillUserName.TabIndex = 7;
            // 
            // txtShowWindchillServerName
            // 
            txtShowWindchillServerName.BorderStyle = BorderStyle.None;
            txtShowWindchillServerName.Enabled = false;
            txtShowWindchillServerName.Location = new Point(11, 165);
            txtShowWindchillServerName.Name = "txtShowWindchillServerName";
            txtShowWindchillServerName.Size = new Size(189, 20);
            txtShowWindchillServerName.TabIndex = 6;
            // 
            // txtShowCatalog
            // 
            txtShowCatalog.BorderStyle = BorderStyle.None;
            txtShowCatalog.Enabled = false;
            txtShowCatalog.Location = new Point(10, 112);
            txtShowCatalog.Name = "txtShowCatalog";
            txtShowCatalog.Size = new Size(190, 20);
            txtShowCatalog.TabIndex = 5;
            // 
            // txtShowServerName
            // 
            txtShowServerName.BorderStyle = BorderStyle.None;
            txtShowServerName.Enabled = false;
            txtShowServerName.Location = new Point(11, 59);
            txtShowServerName.Name = "txtShowServerName";
            txtShowServerName.Size = new Size(189, 20);
            txtShowServerName.TabIndex = 4;
            // 
            // btnConnectionReflesh
            // 
            btnConnectionReflesh.Location = new Point(220, 26);
            btnConnectionReflesh.Name = "btnConnectionReflesh";
            btnConnectionReflesh.Size = new Size(94, 29);
            btnConnectionReflesh.TabIndex = 3;
            btnConnectionReflesh.Text = "Yenile";
            btnConnectionReflesh.UseVisualStyleBackColor = true;
            btnConnectionReflesh.Click += btnConnectionReflesh_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(6, 89);
            label2.Name = "label2";
            label2.Size = new Size(157, 20);
            label2.TabIndex = 1;
            label2.Text = "SQL Database Name : ";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 34);
            label1.Name = "label1";
            label1.Size = new Size(135, 20);
            label1.TabIndex = 0;
            label1.Text = "SQL Server Name : ";
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(txtWindchillApi);
            groupBox3.Controls.Add(txtBasicPassword);
            groupBox3.Controls.Add(txtBasicUsername);
            groupBox3.Controls.Add(btnApiEkle);
            groupBox3.Location = new Point(12, 193);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(610, 101);
            groupBox3.TabIndex = 3;
            groupBox3.TabStop = false;
            groupBox3.Text = "Windchill bağlantı ayarı";
            groupBox3.Enter += groupBox3_Enter;
            // 
            // txtWindchillApi
            // 
            txtWindchillApi.Cursor = Cursors.IBeam;
            txtWindchillApi.Location = new Point(11, 26);
            txtWindchillApi.Name = "txtWindchillApi";
            txtWindchillApi.PlaceholderText = "Windchill Server Name";
            txtWindchillApi.Size = new Size(192, 27);
            txtWindchillApi.TabIndex = 7;
            txtWindchillApi.TextChanged += txtWindchillApi_TextChanged;
            // 
            // txtBasicPassword
            // 
            txtBasicPassword.Cursor = Cursors.IBeam;
            txtBasicPassword.Location = new Point(407, 26);
            txtBasicPassword.Name = "txtBasicPassword";
            txtBasicPassword.PasswordChar = '*';
            txtBasicPassword.PlaceholderText = "Password";
            txtBasicPassword.Size = new Size(192, 27);
            txtBasicPassword.TabIndex = 11;
            // 
            // txtBasicUsername
            // 
            txtBasicUsername.Cursor = Cursors.IBeam;
            txtBasicUsername.Location = new Point(209, 26);
            txtBasicUsername.Name = "txtBasicUsername";
            txtBasicUsername.PlaceholderText = "Username";
            txtBasicUsername.Size = new Size(192, 27);
            txtBasicUsername.TabIndex = 7;
            // 
            // btnApiEkle
            // 
            btnApiEkle.Location = new Point(505, 59);
            btnApiEkle.Name = "btnApiEkle";
            btnApiEkle.Size = new Size(94, 29);
            btnApiEkle.TabIndex = 9;
            btnApiEkle.Text = "Ekle";
            btnApiEkle.UseVisualStyleBackColor = true;
            btnApiEkle.Click += btnApiEkle_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(743, 14);
            label5.Name = "label5";
            label5.Size = new Size(129, 20);
            label5.TabIndex = 4;
            label5.Text = "İşlenen veri sayısı :";
            label5.Click += label5_Click;
            // 
            // button1btnStopAutoPost
            // 
            button1btnStopAutoPost.Location = new Point(736, 300);
            button1btnStopAutoPost.Name = "button1btnStopAutoPost";
            button1btnStopAutoPost.Size = new Size(94, 29);
            button1btnStopAutoPost.TabIndex = 5;
            button1btnStopAutoPost.Text = "Durdur";
            button1btnStopAutoPost.UseVisualStyleBackColor = true;
            button1btnStopAutoPost.Click += button1btnStopAutoPost_Click;
            // 
            // btnKapat
            // 
            btnKapat.Location = new Point(636, 300);
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
            lblDataCount.Location = new Point(878, 14);
            lblDataCount.Name = "lblDataCount";
            lblDataCount.Size = new Size(17, 20);
            lblDataCount.TabIndex = 7;
            lblDataCount.Text = "0";
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(listBox1);
            groupBox4.Controls.Add(lblDataCount);
            groupBox4.Controls.Add(label5);
            groupBox4.Location = new Point(12, 335);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new Size(938, 301);
            groupBox4.TabIndex = 8;
            groupBox4.TabStop = false;
            groupBox4.Text = "PLM LOG";
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.HorizontalScrollbar = true;
            listBox1.IntegralHeight = false;
            listBox1.Location = new Point(11, 37);
            listBox1.Name = "listBox1";
            listBox1.ScrollAlwaysVisible = true;
            listBox1.Size = new Size(921, 251);
            listBox1.TabIndex = 0;
            listBox1.SelectedIndexChanged += listBox1_SelectedIndexChanged_1;
            // 
            // groupBox5
            // 
            groupBox5.Controls.Add(btnListbox2Reflesh);
            groupBox5.Controls.Add(listBox2);
            groupBox5.Location = new Point(956, 12);
            groupBox5.Name = "groupBox5";
            groupBox5.Size = new Size(247, 624);
            groupBox5.TabIndex = 9;
            groupBox5.TabStop = false;
            groupBox5.Text = "Log dosya listesi";
            // 
            // btnListbox2Reflesh
            // 
            btnListbox2Reflesh.Location = new Point(137, 34);
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
            listBox2.Location = new Point(5, 27);
            listBox2.Name = "listBox2";
            listBox2.Size = new Size(237, 584);
            listBox2.TabIndex = 0;
            listBox2.SelectedIndexChanged += listBox2_SelectedIndexChanged;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.ImageScalingSize = new Size(20, 20);
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { kapatToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(118, 28);
            contextMenuStrip1.Opening += contextMenuStrip1_Opening;
            // 
            // kapatToolStripMenuItem
            // 
            kapatToolStripMenuItem.Name = "kapatToolStripMenuItem";
            kapatToolStripMenuItem.Size = new Size(117, 24);
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
            ClientSize = new Size(1210, 644);
            Controls.Add(groupBox5);
            Controls.Add(groupBox4);
            Controls.Add(button1btnStopAutoPost);
            Controls.Add(btnStartAutoPost);
            Controls.Add(groupBox3);
            Controls.Add(btnKapat);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Designtech PLM Entegrasyon v1.8.0";
            FormClosing += Form1_FormClosing_1;
            FormClosed += Form1_FormClosed;
            Load += Form1_Load;
            Resize += Form1_Resize;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            groupBox5.ResumeLayout(false);
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
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
        private Label label2;
        private Label label1;
        private RadioButton rbLocalChoose;
        private Button btnBaglantiKur;
        private GroupBox groupBox3;
        private Button btnApiEkle;
        private Label label5;
        private Button button1btnStopAutoPost;
        private Button btnKapat;
        private Label lblDataCount;
        private GroupBox groupBox4;
        private ListBox listBox1;
        private GroupBox groupBox5;
        private ListBox listBox2;
        private TextBox txtShowCatalog;
        private TextBox txtShowServerName;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem kapatToolStripMenuItem;
        private NotifyIcon notifyIcon1;
		private TextBox txtBasicPassword;
		private TextBox txtBasicUsername;
        private Button btnListbox2Reflesh;
        private TextBox txtWindchillApi;
        private TextBox txtShowWindchillUserName;
        private TextBox txtShowWindchillServerName;
        private Label label4;
        private Label label3;
    }
}
