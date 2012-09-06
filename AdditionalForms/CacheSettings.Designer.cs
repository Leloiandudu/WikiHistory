namespace WikiHistory.AdditionalForms
{
  partial class CacheSettings
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
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.button3 = new System.Windows.Forms.Button();
      this.label3 = new System.Windows.Forms.Label();
      this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
      this.checkBox2 = new System.Windows.Forms.CheckBox();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.checkBox1 = new System.Windows.Forms.CheckBox();
      this.button1 = new System.Windows.Forms.Button();
      this.button2 = new System.Windows.Forms.Button();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.button3);
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Controls.Add(this.numericUpDown1);
      this.groupBox1.Controls.Add(this.checkBox2);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.checkBox1);
      this.groupBox1.Location = new System.Drawing.Point(12, 12);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(627, 334);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      // 
      // button3
      // 
      this.button3.Enabled = false;
      this.button3.Location = new System.Drawing.Point(6, 298);
      this.button3.Name = "button3";
      this.button3.Size = new System.Drawing.Size(615, 30);
      this.button3.TabIndex = 6;
      this.button3.Text = "Удалить все файлы кеша";
      this.button3.UseVisualStyleBackColor = true;
      this.button3.Click += new System.EventHandler(this.button3_Click);
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(182, 62);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(23, 13);
      this.label3.TabIndex = 5;
      this.label3.Text = "МБ";
      // 
      // numericUpDown1
      // 
      this.numericUpDown1.Enabled = false;
      this.numericUpDown1.Location = new System.Drawing.Point(122, 60);
      this.numericUpDown1.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
      this.numericUpDown1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.numericUpDown1.Name = "numericUpDown1";
      this.numericUpDown1.Size = new System.Drawing.Size(60, 20);
      this.numericUpDown1.TabIndex = 4;
      this.numericUpDown1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.numericUpDown1.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
      // 
      // checkBox2
      // 
      this.checkBox2.AutoSize = true;
      this.checkBox2.Enabled = false;
      this.checkBox2.Location = new System.Drawing.Point(9, 61);
      this.checkBox2.Name = "checkBox2";
      this.checkBox2.Size = new System.Drawing.Size(116, 17);
      this.checkBox2.TabIndex = 3;
      this.checkBox2.Text = "Лимит кеша:";
      this.checkBox2.UseVisualStyleBackColor = true;
      this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
      // 
      // label2
      // 
      this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.label2.Location = new System.Drawing.Point(6, 25);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(615, 32);
      this.label2.TabIndex = 2;
      this.label2.Text = "WikiHistory может кешировать загруженные версии статей для быстрого доступа в дальнейшем. ";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(6, 282);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(81, 13);
      this.label1.TabIndex = 1;
      this.label1.Text = "Caching folder: ";
      // 
      // checkBox1
      // 
      this.checkBox1.AutoSize = true;
      this.checkBox1.Location = new System.Drawing.Point(9, 0);
      this.checkBox1.Name = "checkBox1";
      this.checkBox1.Size = new System.Drawing.Size(93, 17);
      this.checkBox1.TabIndex = 0;
      this.checkBox1.Text = "Включить кеширование";
      this.checkBox1.UseVisualStyleBackColor = true;
      this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(519, 352);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(120, 30);
      this.button1.TabIndex = 1;
      this.button1.Text = "OK";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // button2
      // 
      this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.button2.Location = new System.Drawing.Point(393, 352);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(120, 30);
      this.button2.TabIndex = 2;
      this.button2.Text = "Отменить";
      this.button2.UseVisualStyleBackColor = true;
      this.button2.Click += new System.EventHandler(this.button2_Click);
      // 
      // CacheSettings
      // 
      this.AcceptButton = this.button1;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.button2;
      this.ClientSize = new System.Drawing.Size(651, 394);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.groupBox1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "CacheSettings";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Настройки кеширования";
      this.Load += new System.EventHandler(this.CacheSettings_Load);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.CheckBox checkBox1;
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Button button2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.NumericUpDown numericUpDown1;
    private System.Windows.Forms.CheckBox checkBox2;
    private System.Windows.Forms.Button button3;
  }
}