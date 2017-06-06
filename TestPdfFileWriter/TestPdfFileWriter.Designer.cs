namespace TestPdfFileWriter {
    partial class TestPdfFileWriter {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
        if (disposing && (components != null)) {
        components.Dispose();
        }
        base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
			this.ArticleButton = new System.Windows.Forms.Button();
			this.GraphicsButton = new System.Windows.Forms.Button();
			this.DebugCheckBox = new System.Windows.Forms.CheckBox();
			this.FontFamiliesButton = new System.Windows.Forms.Button();
			this.CopyrightTextBox = new System.Windows.Forms.RichTextBox();
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// ArticleButton
			// 
			this.ArticleButton.Location = new System.Drawing.Point(12, 342);
			this.ArticleButton.Name = "ArticleButton";
			this.ArticleButton.Size = new System.Drawing.Size(77, 54);
			this.ArticleButton.TabIndex = 1;
			this.ArticleButton.Text = "Article\nExample";
			this.ArticleButton.UseVisualStyleBackColor = true;
			this.ArticleButton.Click += new System.EventHandler(this.OnArticleExample);
			// 
			// GraphicsButton
			// 
			this.GraphicsButton.Location = new System.Drawing.Point(98, 342);
			this.GraphicsButton.Name = "GraphicsButton";
			this.GraphicsButton.Size = new System.Drawing.Size(77, 54);
			this.GraphicsButton.TabIndex = 2;
			this.GraphicsButton.Text = "Other\nExample";
			this.GraphicsButton.UseVisualStyleBackColor = true;
			this.GraphicsButton.Click += new System.EventHandler(this.OnOtherExample);
			// 
			// DebugCheckBox
			// 
			this.DebugCheckBox.AutoSize = true;
			this.DebugCheckBox.Location = new System.Drawing.Point(539, 360);
			this.DebugCheckBox.Name = "DebugCheckBox";
			this.DebugCheckBox.Size = new System.Drawing.Size(64, 20);
			this.DebugCheckBox.TabIndex = 7;
			this.DebugCheckBox.Text = "Debug";
			this.DebugCheckBox.UseVisualStyleBackColor = true;
			// 
			// FontFamiliesButton
			// 
			this.FontFamiliesButton.Location = new System.Drawing.Point(442, 342);
			this.FontFamiliesButton.Name = "FontFamiliesButton";
			this.FontFamiliesButton.Size = new System.Drawing.Size(77, 54);
			this.FontFamiliesButton.TabIndex = 6;
			this.FontFamiliesButton.Text = "Font\nFamilies";
			this.FontFamiliesButton.UseVisualStyleBackColor = true;
			this.FontFamiliesButton.Click += new System.EventHandler(this.OnFontFamilies);
			// 
			// CopyrightTextBox
			// 
			this.CopyrightTextBox.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.CopyrightTextBox.Location = new System.Drawing.Point(33, 26);
			this.CopyrightTextBox.MaxLength = 2048;
			this.CopyrightTextBox.Name = "CopyrightTextBox";
			this.CopyrightTextBox.ReadOnly = true;
			this.CopyrightTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
			this.CopyrightTextBox.Size = new System.Drawing.Size(548, 268);
			this.CopyrightTextBox.TabIndex = 0;
			this.CopyrightTextBox.Text = "";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(184, 342);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(77, 54);
			this.button1.TabIndex = 3;
			this.button1.Text = "Chart\nExample";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.OnChartExample);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(270, 342);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(77, 54);
			this.button2.TabIndex = 4;
			this.button2.Text = "Print\nExample";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.OnPrintExample);
			// 
			// button3
			// 
			this.button3.Location = new System.Drawing.Point(356, 342);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(77, 54);
			this.button3.TabIndex = 5;
			this.button3.Text = "Table\nExample";
			this.button3.UseVisualStyleBackColor = true;
			this.button3.Click += new System.EventHandler(this.OnTableExample);
			// 
			// TestPdfFileWriter
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(614, 408);
			this.Controls.Add(this.button3);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.CopyrightTextBox);
			this.Controls.Add(this.FontFamiliesButton);
			this.Controls.Add(this.DebugCheckBox);
			this.Controls.Add(this.GraphicsButton);
			this.Controls.Add(this.ArticleButton);
			this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.Name = "TestPdfFileWriter";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Test PDF File Writer";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnClosing);
			this.Load += new System.EventHandler(this.OnLoad);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ArticleButton;
		private System.Windows.Forms.Button GraphicsButton;
		private System.Windows.Forms.CheckBox DebugCheckBox;
		private System.Windows.Forms.Button FontFamiliesButton;
		private System.Windows.Forms.RichTextBox CopyrightTextBox;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button3;
    }
}

