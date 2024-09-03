namespace ConsoleApplication1
{
    partial class NetFiddler
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NetFiddler));
            this.启动 = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.清屏 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // 启动
            // 
            this.启动.Font = new System.Drawing.Font("宋体", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.启动.Location = new System.Drawing.Point(12, 12);
            this.启动.Name = "启动";
            this.启动.Size = new System.Drawing.Size(591, 42);
            this.启动.TabIndex = 0;
            this.启动.Text = "启动";
            this.启动.UseVisualStyleBackColor = true;
            this.启动.Click += new System.EventHandler(this.启动_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.Font = new System.Drawing.Font("宋体", 18F);
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(14, 60);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(590, 32);
            this.comboBox1.TabIndex = 1;
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("宋体", 18F);
            this.button1.Location = new System.Drawing.Point(14, 98);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(591, 42);
            this.button1.TabIndex = 2;
            this.button1.Text = "重新加载fiddler配置";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.richTextBox1.Location = new System.Drawing.Point(12, 194);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(591, 385);
            this.richTextBox1.TabIndex = 3;
            this.richTextBox1.Text = "";
            // 
            // 清屏
            // 
            this.清屏.Font = new System.Drawing.Font("宋体", 18F);
            this.清屏.Location = new System.Drawing.Point(12, 585);
            this.清屏.Name = "清屏";
            this.清屏.Size = new System.Drawing.Size(591, 42);
            this.清屏.TabIndex = 4;
            this.清屏.Text = "清屏";
            this.清屏.UseVisualStyleBackColor = true;
            this.清屏.Click += new System.EventHandler(this.清屏_Click);
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("宋体", 18F);
            this.button2.Location = new System.Drawing.Point(14, 146);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(591, 42);
            this.button2.TabIndex = 5;
            this.button2.Text = "手工上送";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // NetFiddler
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(617, 639);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.清屏);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.启动);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "NetFiddler";
            this.Text = "NetFiddler";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.NetFiddler_FormClosed);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button 启动;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button 清屏;
        private System.Windows.Forms.Button button2;
    }
}