namespace HMSKeyGenerator
{
    partial class HMSKeyGenrator_Home
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HMSKeyGenrator_Home));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button_Copy = new System.Windows.Forms.Button();
            this.button_Clear = new System.Windows.Forms.Button();
            this.button_Decrypt = new System.Windows.Forms.Button();
            this.button_Encrypt = new System.Windows.Forms.Button();
            this.textBox_ComputerName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.errorProvider_Home = new System.Windows.Forms.ErrorProvider(this.components);
            this.label_Version = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider_Home)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button_Copy);
            this.groupBox1.Controls.Add(this.button_Clear);
            this.groupBox1.Controls.Add(this.button_Decrypt);
            this.groupBox1.Controls.Add(this.button_Encrypt);
            this.groupBox1.Controls.Add(this.textBox_ComputerName);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(2, -1);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(689, 131);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // button_Copy
            // 
            this.button_Copy.BackColor = System.Drawing.Color.White;
            this.button_Copy.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.button_Copy.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.button_Copy.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_Copy.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_Copy.Image = global::HMSKeyGenerator.Properties.Resources.Ahmadhania_Spherical_File_copy;
            this.button_Copy.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.button_Copy.Location = new System.Drawing.Point(373, 62);
            this.button_Copy.Name = "button_Copy";
            this.button_Copy.Size = new System.Drawing.Size(127, 41);
            this.button_Copy.TabIndex = 14;
            this.button_Copy.Text = "Copy";
            this.button_Copy.UseVisualStyleBackColor = false;
            this.button_Copy.Click += new System.EventHandler(this.button_Copy_Click);
            // 
            // button_Clear
            // 
            this.button_Clear.BackColor = System.Drawing.Color.White;
            this.button_Clear.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.button_Clear.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.button_Clear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_Clear.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_Clear.Image = global::HMSKeyGenerator.Properties.Resources.Matiasam_Ios7_Style_Clear_Tick;
            this.button_Clear.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.button_Clear.Location = new System.Drawing.Point(507, 62);
            this.button_Clear.Name = "button_Clear";
            this.button_Clear.Size = new System.Drawing.Size(127, 41);
            this.button_Clear.TabIndex = 13;
            this.button_Clear.Text = "Clear";
            this.button_Clear.UseVisualStyleBackColor = false;
            this.button_Clear.Click += new System.EventHandler(this.button_Clear_Click);
            // 
            // button_Decrypt
            // 
            this.button_Decrypt.BackColor = System.Drawing.Color.White;
            this.button_Decrypt.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.button_Decrypt.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.button_Decrypt.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_Decrypt.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_Decrypt.Image = global::HMSKeyGenerator.Properties.Resources.Graphicloads_100_Flat_Unlock;
            this.button_Decrypt.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.button_Decrypt.Location = new System.Drawing.Point(240, 62);
            this.button_Decrypt.Name = "button_Decrypt";
            this.button_Decrypt.Size = new System.Drawing.Size(125, 41);
            this.button_Decrypt.TabIndex = 12;
            this.button_Decrypt.Text = "Decrypt";
            this.button_Decrypt.UseVisualStyleBackColor = false;
            this.button_Decrypt.Click += new System.EventHandler(this.button_Decrypt_Click);
            // 
            // button_Encrypt
            // 
            this.button_Encrypt.BackColor = System.Drawing.Color.White;
            this.button_Encrypt.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.button_Encrypt.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.button_Encrypt.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_Encrypt.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_Encrypt.Image = global::HMSKeyGenerator.Properties.Resources.Graphicloads_Colorful_Long_Shadow_Lock;
            this.button_Encrypt.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.button_Encrypt.Location = new System.Drawing.Point(106, 62);
            this.button_Encrypt.Name = "button_Encrypt";
            this.button_Encrypt.Size = new System.Drawing.Size(127, 41);
            this.button_Encrypt.TabIndex = 11;
            this.button_Encrypt.Text = "Encrypt";
            this.button_Encrypt.UseVisualStyleBackColor = false;
            this.button_Encrypt.Click += new System.EventHandler(this.button_Encrypt_Click);
            // 
            // textBox_ComputerName
            // 
            this.textBox_ComputerName.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_ComputerName.Location = new System.Drawing.Point(106, 23);
            this.textBox_ComputerName.Name = "textBox_ComputerName";
            this.textBox_ComputerName.Size = new System.Drawing.Size(529, 22);
            this.textBox_ComputerName.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(11, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Computer Name:";
            // 
            // errorProvider_Home
            // 
            this.errorProvider_Home.ContainerControl = this;
            // 
            // label_Version
            // 
            this.label_Version.AutoSize = true;
            this.label_Version.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.label_Version.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label_Version.Location = new System.Drawing.Point(595, 119);
            this.label_Version.Name = "label_Version";
            this.label_Version.Size = new System.Drawing.Size(44, 13);
            this.label_Version.TabIndex = 1;
            this.label_Version.Text = "version";
            // 
            // HMSKeyGenrator_Home
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(697, 136);
            this.Controls.Add(this.label_Version);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "HMSKeyGenrator_Home";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "HMS Key Generator";
            this.Load += new System.EventHandler(this.HMSKeyGenrator_Home_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider_Home)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBox_ComputerName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_Clear;
        private System.Windows.Forms.Button button_Decrypt;
        private System.Windows.Forms.Button button_Encrypt;
        private System.Windows.Forms.ErrorProvider errorProvider_Home;
        private System.Windows.Forms.Button button_Copy;
        private System.Windows.Forms.Label label_Version;
    }
}