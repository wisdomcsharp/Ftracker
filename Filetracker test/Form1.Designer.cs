namespace Filetracker_test
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.Filelabel = new System.Windows.Forms.Label();
            this.Processedlabel = new System.Windows.Forms.Label();
            this.Activitylabel = new System.Windows.Forms.Label();
            this.Savedlabel = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.Errorlabel = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Filelabel
            // 
            this.Filelabel.AutoSize = true;
            this.Filelabel.Font = new System.Drawing.Font("Candara", 21.75F);
            this.Filelabel.ForeColor = System.Drawing.Color.DimGray;
            this.Filelabel.Location = new System.Drawing.Point(15, 45);
            this.Filelabel.Name = "Filelabel";
            this.Filelabel.Size = new System.Drawing.Size(76, 36);
            this.Filelabel.TabIndex = 0;
            this.Filelabel.Text = "Files:";
            // 
            // Processedlabel
            // 
            this.Processedlabel.AutoSize = true;
            this.Processedlabel.Font = new System.Drawing.Font("Candara", 21.75F);
            this.Processedlabel.ForeColor = System.Drawing.Color.DimGray;
            this.Processedlabel.Location = new System.Drawing.Point(12, 81);
            this.Processedlabel.Name = "Processedlabel";
            this.Processedlabel.Size = new System.Drawing.Size(147, 36);
            this.Processedlabel.TabIndex = 1;
            this.Processedlabel.Text = "Processed:";
            // 
            // Activitylabel
            // 
            this.Activitylabel.AutoSize = true;
            this.Activitylabel.Font = new System.Drawing.Font("Candara", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Activitylabel.ForeColor = System.Drawing.Color.DimGray;
            this.Activitylabel.Location = new System.Drawing.Point(12, 9);
            this.Activitylabel.Name = "Activitylabel";
            this.Activitylabel.Size = new System.Drawing.Size(168, 36);
            this.Activitylabel.TabIndex = 2;
            this.Activitylabel.Tag = "";
            this.Activitylabel.Text = "Last Activity:";
            // 
            // Savedlabel
            // 
            this.Savedlabel.AutoSize = true;
            this.Savedlabel.Font = new System.Drawing.Font("Candara", 21.75F);
            this.Savedlabel.ForeColor = System.Drawing.Color.DimGray;
            this.Savedlabel.Location = new System.Drawing.Point(12, 117);
            this.Savedlabel.Name = "Savedlabel";
            this.Savedlabel.Size = new System.Drawing.Size(171, 36);
            this.Savedlabel.TabIndex = 3;
            this.Savedlabel.Text = "Saved Items:";
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkLabel1.Location = new System.Drawing.Point(12, 247);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(182, 108);
            this.linkLabel1.TabIndex = 4;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Support - Skype:\r\nWisdom.Oparaocha,\r\ndozie@cleanatom.com\r\n\r\nGitHub:\r\ngithub.com/w" +
    "isdomcsharp";
            // 
            // Errorlabel
            // 
            this.Errorlabel.AutoSize = true;
            this.Errorlabel.Font = new System.Drawing.Font("Candara", 21.75F);
            this.Errorlabel.ForeColor = System.Drawing.Color.DimGray;
            this.Errorlabel.Location = new System.Drawing.Point(15, 153);
            this.Errorlabel.Name = "Errorlabel";
            this.Errorlabel.Size = new System.Drawing.Size(95, 36);
            this.Errorlabel.TabIndex = 5;
            this.Errorlabel.Text = "Errors:";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Candara", 26.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.label1.Location = new System.Drawing.Point(366, 76);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(106, 42);
            this.label1.TabIndex = 6;
            this.label1.Text = "Async";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 197);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Errorlabel);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.Savedlabel);
            this.Controls.Add(this.Activitylabel);
            this.Controls.Add(this.Processedlabel);
            this.Controls.Add(this.Filelabel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(500, 236);
            this.Name = "Form1";
            this.Text = "File scanner - driveIT consulting GmbH";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.SizeChanged += new System.EventHandler(this.Form1_SizeChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label Filelabel;
        private System.Windows.Forms.Label Processedlabel;
        private System.Windows.Forms.Label Activitylabel;
        private System.Windows.Forms.Label Savedlabel;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label Errorlabel;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label label1;
    }
}

