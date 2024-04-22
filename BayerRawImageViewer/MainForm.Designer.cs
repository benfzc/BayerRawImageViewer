namespace BayerRawImageViewer
{
    partial class MainForm
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
            panelControl = new Panel();
            groupBoxRawType = new GroupBox();
            radioButtonTypeUnpacked = new RadioButton();
            radioButtonTypePacked = new RadioButton();
            radioButtonTypeMipi = new RadioButton();
            groupBoxDepth = new GroupBox();
            radioButton14bit = new RadioButton();
            radioButton12bit = new RadioButton();
            radioButton10bit = new RadioButton();
            radioButton8bit = new RadioButton();
            groupBoxResolution = new GroupBox();
            textBoxStride = new TextBox();
            textBoxHeight = new TextBox();
            textBoxWidth = new TextBox();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            groupBoxBayerPattern = new GroupBox();
            radioButtonBayerBG = new RadioButton();
            radioButtonBayerGB = new RadioButton();
            radioButtonBayerRG = new RadioButton();
            radioButtonBayerGR = new RadioButton();
            groupBoxAWB = new GroupBox();
            textBoxOB = new TextBox();
            label4 = new Label();
            checkBoxAWB = new CheckBox();
            groupBoxOutput = new GroupBox();
            checkBoxSaveJpg = new CheckBox();
            checkBoxSaveBmp = new CheckBox();
            comboBoxOutputRawDepth = new ComboBox();
            checkBoxSaveUnpackedRaw = new CheckBox();
            pictureBox = new PictureBox();
            panelControl.SuspendLayout();
            groupBoxRawType.SuspendLayout();
            groupBoxDepth.SuspendLayout();
            groupBoxResolution.SuspendLayout();
            groupBoxBayerPattern.SuspendLayout();
            groupBoxAWB.SuspendLayout();
            groupBoxOutput.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit();
            SuspendLayout();
            // 
            // panelControl
            // 
            panelControl.AutoSize = true;
            panelControl.Controls.Add(groupBoxRawType);
            panelControl.Controls.Add(groupBoxDepth);
            panelControl.Controls.Add(groupBoxResolution);
            panelControl.Controls.Add(groupBoxBayerPattern);
            panelControl.Controls.Add(groupBoxAWB);
            panelControl.Controls.Add(groupBoxOutput);
            panelControl.Dock = DockStyle.Right;
            panelControl.Location = new Point(715, 0);
            panelControl.Name = "panelControl";
            panelControl.Size = new Size(229, 499);
            panelControl.TabIndex = 0;
            // 
            // groupBoxRawType
            // 
            groupBoxRawType.Controls.Add(radioButtonTypeUnpacked);
            groupBoxRawType.Controls.Add(radioButtonTypePacked);
            groupBoxRawType.Controls.Add(radioButtonTypeMipi);
            groupBoxRawType.Location = new Point(3, 6);
            groupBoxRawType.Name = "groupBoxRawType";
            groupBoxRawType.Size = new Size(223, 94);
            groupBoxRawType.TabIndex = 0;
            groupBoxRawType.TabStop = false;
            groupBoxRawType.Text = "Type";
            // 
            // radioButtonTypeUnpacked
            // 
            radioButtonTypeUnpacked.AutoSize = true;
            radioButtonTypeUnpacked.Location = new Point(16, 68);
            radioButtonTypeUnpacked.Name = "radioButtonTypeUnpacked";
            radioButtonTypeUnpacked.Size = new Size(115, 21);
            radioButtonTypeUnpacked.TabIndex = 2;
            radioButtonTypeUnpacked.TabStop = true;
            radioButtonTypeUnpacked.Text = "Unpacked Raw";
            radioButtonTypeUnpacked.UseVisualStyleBackColor = true;
            // 
            // radioButtonTypePacked
            // 
            radioButtonTypePacked.AutoSize = true;
            radioButtonTypePacked.Checked = true;
            radioButtonTypePacked.Location = new Point(16, 46);
            radioButtonTypePacked.Name = "radioButtonTypePacked";
            radioButtonTypePacked.Size = new Size(97, 21);
            radioButtonTypePacked.TabIndex = 1;
            radioButtonTypePacked.TabStop = true;
            radioButtonTypePacked.Text = "Packed Raw";
            radioButtonTypePacked.UseVisualStyleBackColor = true;
            // 
            // radioButtonTypeMipi
            // 
            radioButtonTypeMipi.AutoSize = true;
            radioButtonTypeMipi.Location = new Point(16, 24);
            radioButtonTypeMipi.Name = "radioButtonTypeMipi";
            radioButtonTypeMipi.Size = new Size(83, 21);
            radioButtonTypeMipi.TabIndex = 0;
            radioButtonTypeMipi.TabStop = true;
            radioButtonTypeMipi.Text = "MIPI Raw";
            radioButtonTypeMipi.UseVisualStyleBackColor = true;
            // 
            // groupBoxDepth
            // 
            groupBoxDepth.Controls.Add(radioButton14bit);
            groupBoxDepth.Controls.Add(radioButton12bit);
            groupBoxDepth.Controls.Add(radioButton10bit);
            groupBoxDepth.Controls.Add(radioButton8bit);
            groupBoxDepth.Location = new Point(3, 104);
            groupBoxDepth.Name = "groupBoxDepth";
            groupBoxDepth.Size = new Size(223, 72);
            groupBoxDepth.TabIndex = 1;
            groupBoxDepth.TabStop = false;
            groupBoxDepth.Text = "Depth";
            // 
            // radioButton14bit
            // 
            radioButton14bit.AutoSize = true;
            radioButton14bit.Location = new Point(16, 46);
            radioButton14bit.Name = "radioButton14bit";
            radioButton14bit.Size = new Size(42, 21);
            radioButton14bit.TabIndex = 3;
            radioButton14bit.Text = "14";
            radioButton14bit.UseVisualStyleBackColor = true;
            // 
            // radioButton12bit
            // 
            radioButton12bit.AutoSize = true;
            radioButton12bit.Location = new Point(165, 24);
            radioButton12bit.Name = "radioButton12bit";
            radioButton12bit.Size = new Size(42, 21);
            radioButton12bit.TabIndex = 2;
            radioButton12bit.Text = "12";
            radioButton12bit.UseVisualStyleBackColor = true;
            // 
            // radioButton10bit
            // 
            radioButton10bit.AutoSize = true;
            radioButton10bit.Checked = true;
            radioButton10bit.Location = new Point(88, 24);
            radioButton10bit.Name = "radioButton10bit";
            radioButton10bit.Size = new Size(42, 21);
            radioButton10bit.TabIndex = 1;
            radioButton10bit.TabStop = true;
            radioButton10bit.Text = "10";
            radioButton10bit.UseVisualStyleBackColor = true;
            // 
            // radioButton8bit
            // 
            radioButton8bit.AutoSize = true;
            radioButton8bit.Location = new Point(16, 24);
            radioButton8bit.Name = "radioButton8bit";
            radioButton8bit.Size = new Size(34, 21);
            radioButton8bit.TabIndex = 0;
            radioButton8bit.Text = "8";
            radioButton8bit.UseVisualStyleBackColor = true;
            // 
            // groupBoxResolution
            // 
            groupBoxResolution.Controls.Add(textBoxStride);
            groupBoxResolution.Controls.Add(textBoxHeight);
            groupBoxResolution.Controls.Add(textBoxWidth);
            groupBoxResolution.Controls.Add(label3);
            groupBoxResolution.Controls.Add(label2);
            groupBoxResolution.Controls.Add(label1);
            groupBoxResolution.Location = new Point(3, 180);
            groupBoxResolution.Name = "groupBoxResolution";
            groupBoxResolution.Size = new Size(223, 114);
            groupBoxResolution.TabIndex = 2;
            groupBoxResolution.TabStop = false;
            groupBoxResolution.Text = "Resolution";
            // 
            // textBoxStride
            // 
            textBoxStride.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            textBoxStride.Location = new Point(104, 80);
            textBoxStride.MaxLength = 5;
            textBoxStride.Name = "textBoxStride";
            textBoxStride.Size = new Size(110, 24);
            textBoxStride.TabIndex = 1;
            textBoxStride.Text = "5280";
            // 
            // textBoxHeight
            // 
            textBoxHeight.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            textBoxHeight.Location = new Point(104, 52);
            textBoxHeight.MaxLength = 5;
            textBoxHeight.Name = "textBoxHeight";
            textBoxHeight.Size = new Size(110, 24);
            textBoxHeight.TabIndex = 1;
            textBoxHeight.Text = "3120";
            // 
            // textBoxWidth
            // 
            textBoxWidth.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            textBoxWidth.Location = new Point(104, 24);
            textBoxWidth.MaxLength = 5;
            textBoxWidth.Name = "textBoxWidth";
            textBoxWidth.Size = new Size(110, 24);
            textBoxWidth.TabIndex = 1;
            textBoxWidth.Text = "4224";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(16, 84);
            label3.Name = "label3";
            label3.Size = new Size(43, 17);
            label3.TabIndex = 0;
            label3.Text = "Stride";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(16, 56);
            label2.Name = "label2";
            label2.Size = new Size(49, 17);
            label2.TabIndex = 0;
            label2.Text = "Height";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(16, 28);
            label1.Name = "label1";
            label1.Size = new Size(45, 17);
            label1.TabIndex = 0;
            label1.Text = "Width";
            // 
            // groupBoxBayerPattern
            // 
            groupBoxBayerPattern.Controls.Add(radioButtonBayerBG);
            groupBoxBayerPattern.Controls.Add(radioButtonBayerGB);
            groupBoxBayerPattern.Controls.Add(radioButtonBayerRG);
            groupBoxBayerPattern.Controls.Add(radioButtonBayerGR);
            groupBoxBayerPattern.Location = new Point(3, 298);
            groupBoxBayerPattern.Name = "groupBoxBayerPattern";
            groupBoxBayerPattern.Size = new Size(221, 54);
            groupBoxBayerPattern.TabIndex = 4;
            groupBoxBayerPattern.TabStop = false;
            groupBoxBayerPattern.Text = "Bayper Pattern";
            // 
            // radioButtonBayerBG
            // 
            radioButtonBayerBG.AutoSize = true;
            radioButtonBayerBG.Location = new Point(166, 24);
            radioButtonBayerBG.Name = "radioButtonBayerBG";
            radioButtonBayerBG.Size = new Size(44, 21);
            radioButtonBayerBG.TabIndex = 0;
            radioButtonBayerBG.Text = "BG";
            radioButtonBayerBG.UseVisualStyleBackColor = true;
            radioButtonBayerBG.CheckedChanged += radioButtonBayer_CheckedChanged;
            // 
            // radioButtonBayerGB
            // 
            radioButtonBayerGB.AutoSize = true;
            radioButtonBayerGB.Location = new Point(116, 24);
            radioButtonBayerGB.Name = "radioButtonBayerGB";
            radioButtonBayerGB.Size = new Size(44, 21);
            radioButtonBayerGB.TabIndex = 0;
            radioButtonBayerGB.Text = "GB";
            radioButtonBayerGB.UseVisualStyleBackColor = true;
            radioButtonBayerGB.CheckedChanged += radioButtonBayer_CheckedChanged;
            // 
            // radioButtonBayerRG
            // 
            radioButtonBayerRG.AutoSize = true;
            radioButtonBayerRG.Location = new Point(66, 24);
            radioButtonBayerRG.Name = "radioButtonBayerRG";
            radioButtonBayerRG.Size = new Size(44, 21);
            radioButtonBayerRG.TabIndex = 0;
            radioButtonBayerRG.Text = "RG";
            radioButtonBayerRG.UseVisualStyleBackColor = true;
            radioButtonBayerRG.CheckedChanged += radioButtonBayer_CheckedChanged;
            // 
            // radioButtonBayerGR
            // 
            radioButtonBayerGR.AutoSize = true;
            radioButtonBayerGR.Checked = true;
            radioButtonBayerGR.Location = new Point(16, 24);
            radioButtonBayerGR.Name = "radioButtonBayerGR";
            radioButtonBayerGR.Size = new Size(44, 21);
            radioButtonBayerGR.TabIndex = 0;
            radioButtonBayerGR.TabStop = true;
            radioButtonBayerGR.Text = "GR";
            radioButtonBayerGR.UseVisualStyleBackColor = true;
            radioButtonBayerGR.CheckedChanged += radioButtonBayer_CheckedChanged;
            // 
            // groupBoxAWB
            // 
            groupBoxAWB.Controls.Add(textBoxOB);
            groupBoxAWB.Controls.Add(label4);
            groupBoxAWB.Controls.Add(checkBoxAWB);
            groupBoxAWB.Location = new Point(3, 356);
            groupBoxAWB.Name = "groupBoxAWB";
            groupBoxAWB.Size = new Size(223, 55);
            groupBoxAWB.TabIndex = 3;
            groupBoxAWB.TabStop = false;
            groupBoxAWB.Text = "AWB";
            // 
            // textBoxOB
            // 
            textBoxOB.Location = new Point(118, 24);
            textBoxOB.MaxLength = 5;
            textBoxOB.Name = "textBoxOB";
            textBoxOB.Size = new Size(96, 24);
            textBoxOB.TabIndex = 2;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(88, 27);
            label4.Name = "label4";
            label4.Size = new Size(27, 17);
            label4.TabIndex = 1;
            label4.Text = "OB";
            // 
            // checkBoxAWB
            // 
            checkBoxAWB.AutoSize = true;
            checkBoxAWB.Location = new Point(16, 26);
            checkBoxAWB.Name = "checkBoxAWB";
            checkBoxAWB.Size = new Size(67, 21);
            checkBoxAWB.TabIndex = 0;
            checkBoxAWB.Text = "Enable";
            checkBoxAWB.UseVisualStyleBackColor = true;
            // 
            // groupBoxOutput
            // 
            groupBoxOutput.Controls.Add(checkBoxSaveJpg);
            groupBoxOutput.Controls.Add(checkBoxSaveBmp);
            groupBoxOutput.Controls.Add(comboBoxOutputRawDepth);
            groupBoxOutput.Controls.Add(checkBoxSaveUnpackedRaw);
            groupBoxOutput.Location = new Point(3, 414);
            groupBoxOutput.Name = "groupBoxOutput";
            groupBoxOutput.Size = new Size(223, 83);
            groupBoxOutput.TabIndex = 5;
            groupBoxOutput.TabStop = false;
            groupBoxOutput.Text = "Output";
            // 
            // checkBoxSaveJpg
            // 
            checkBoxSaveJpg.AutoSize = true;
            checkBoxSaveJpg.Location = new Point(86, 58);
            checkBoxSaveJpg.Name = "checkBoxSaveJpg";
            checkBoxSaveJpg.Size = new Size(57, 21);
            checkBoxSaveJpg.TabIndex = 3;
            checkBoxSaveJpg.Text = "JPEG";
            checkBoxSaveJpg.UseVisualStyleBackColor = true;
            checkBoxSaveJpg.CheckedChanged += checkBoxSaveJpg_CheckedChanged;
            // 
            // checkBoxSaveBmp
            // 
            checkBoxSaveBmp.AutoSize = true;
            checkBoxSaveBmp.Location = new Point(16, 58);
            checkBoxSaveBmp.Name = "checkBoxSaveBmp";
            checkBoxSaveBmp.Size = new Size(56, 21);
            checkBoxSaveBmp.TabIndex = 2;
            checkBoxSaveBmp.Text = "BMP";
            checkBoxSaveBmp.UseVisualStyleBackColor = true;
            checkBoxSaveBmp.CheckedChanged += checkBoxSaveBmp_CheckedChanged;
            // 
            // comboBoxOutputRawDepth
            // 
            comboBoxOutputRawDepth.FormattingEnabled = true;
            comboBoxOutputRawDepth.Items.AddRange(new object[] { "8 bit", "10 bit", "12 bit" });
            comboBoxOutputRawDepth.Location = new Point(136, 22);
            comboBoxOutputRawDepth.Name = "comboBoxOutputRawDepth";
            comboBoxOutputRawDepth.Size = new Size(77, 25);
            comboBoxOutputRawDepth.TabIndex = 1;
            comboBoxOutputRawDepth.Text = "8 bit";
            comboBoxOutputRawDepth.SelectedIndexChanged += comboBoxOutputRawDepth_SelectedIndexChanged;
            // 
            // checkBoxSaveUnpackedRaw
            // 
            checkBoxSaveUnpackedRaw.AutoSize = true;
            checkBoxSaveUnpackedRaw.Location = new Point(16, 26);
            checkBoxSaveUnpackedRaw.Name = "checkBoxSaveUnpackedRaw";
            checkBoxSaveUnpackedRaw.Size = new Size(116, 21);
            checkBoxSaveUnpackedRaw.TabIndex = 0;
            checkBoxSaveUnpackedRaw.Text = "Unpacked Raw";
            checkBoxSaveUnpackedRaw.UseVisualStyleBackColor = true;
            checkBoxSaveUnpackedRaw.CheckedChanged += checkBoxSaveUnpackedRaw_CheckedChanged;
            // 
            // pictureBox
            // 
            pictureBox.BackColor = Color.Gray;
            pictureBox.Dock = DockStyle.Fill;
            pictureBox.Location = new Point(0, 0);
            pictureBox.Name = "pictureBox";
            pictureBox.Size = new Size(944, 499);
            pictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
            pictureBox.TabIndex = 0;
            pictureBox.TabStop = false;
            // 
            // MainForm
            // 
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(8F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(944, 499);
            Controls.Add(panelControl);
            Controls.Add(pictureBox);
            Name = "MainForm";
            Text = "Bayer Raw Image Viewer";
            Load += MainForm_Load;
            DragDrop += MainForm_DragDrop;
            DragEnter += MainForm_DragEnter;
            panelControl.ResumeLayout(false);
            groupBoxRawType.ResumeLayout(false);
            groupBoxRawType.PerformLayout();
            groupBoxDepth.ResumeLayout(false);
            groupBoxDepth.PerformLayout();
            groupBoxResolution.ResumeLayout(false);
            groupBoxResolution.PerformLayout();
            groupBoxBayerPattern.ResumeLayout(false);
            groupBoxBayerPattern.PerformLayout();
            groupBoxAWB.ResumeLayout(false);
            groupBoxAWB.PerformLayout();
            groupBoxOutput.ResumeLayout(false);
            groupBoxOutput.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        //  private SplitContainer splitContainer1;
        private Panel panelControl;
        private PictureBox pictureBox;
    //    private TableLayoutPanel tableLayoutPanel1;
        private GroupBox groupBoxRawType;
        private RadioButton radioButtonTypeUnpacked;
        private RadioButton radioButtonTypePacked;
        private RadioButton radioButtonTypeMipi;
        private GroupBox groupBoxDepth;
        private RadioButton radioButton14bit;
        private RadioButton radioButton12bit;
        private RadioButton radioButton10bit;
        private RadioButton radioButton8bit;
        private GroupBox groupBoxResolution;
        private TextBox textBoxStride;
        private TextBox textBoxHeight;
        private TextBox textBoxWidth;
        private Label label3;
        private Label label2;
        private Label label1;
        private GroupBox groupBoxAWB;
        private TextBox textBoxOB;
        private Label label4;
        private CheckBox checkBoxAWB;
        private GroupBox groupBoxBayerPattern;
        private RadioButton radioButtonBayerBG;
        private RadioButton radioButtonBayerGB;
        private RadioButton radioButtonBayerRG;
        private RadioButton radioButtonBayerGR;
        private GroupBox groupBoxOutput;
        private CheckBox checkBoxSaveUnpackedRaw;
        private ComboBox comboBoxOutputRawDepth;
        private CheckBox checkBoxSaveJpg;
        private CheckBox checkBoxSaveBmp;
    }
}
