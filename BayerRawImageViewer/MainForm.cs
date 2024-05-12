using OpenCvSharp;
using System.Windows.Forms;

namespace BayerRawImageViewer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            comboBoxOutputRawDepth.Items.Clear();
            comboBoxOutputRawDepth.Items.Add(new ComboboxItemBitNumber("8 bit", 8));
            comboBoxOutputRawDepth.Items.Add(new ComboboxItemBitNumber("10 bit", 10));
            comboBoxOutputRawDepth.Items.Add(new ComboboxItemBitNumber("12 bit", 12));
            comboBoxOutputRawDepth.Items.Add(new ComboboxItemBitNumber("14 bit", 14));
            comboBoxOutputRawDepth.Items.Add(new ComboboxItemBitNumber("16 bit", 16));
            comboBoxOutputRawDepth.SelectedIndex = 0;
        }

        struct ComboboxItemBitNumber
        {
            public ComboboxItemBitNumber(string displayName, int bit)
            {
                DisplayName = displayName;
                Bit = bit;
            }
            public string DisplayName { get; set; }
            public int Bit { get; set; }
            // must have this override method to display the right string.
            public override string ToString()
            {
                return DisplayName;
            }
        }


        private void MainForm_Load(object sender, EventArgs e)
        {
            string version = System.Windows.Forms.Application.ProductVersion;
            this.Text = $"Bayer Raw Image Viewr {version}";
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (s.Length > 0)
            {
                pathname = s[0];
                directorypath = Path.GetDirectoryName(pathname);
                filename = Path.GetFileNameWithoutExtension(pathname);

                if (getUserSelections())
                {
                    processImage();
                }
            }
        }

        private void radioButtonBayer_CheckedChanged(object sender, EventArgs e)
        {
            if (pathname.Length > 0)
            {
                if (getUserSelections())
                {
                    processImage();
                }
            }
        }
        private void processImage()
        {
            using BayerRaw bayerRaw = new BayerRaw(pathname, this.imgWidth, this.imgHeight, this.imgStride, depth, rawType);
            bayerRaw.toggleAWB(enableAwb, ob);
            bayerRaw.SetBayerPattern(bayerPattern);
 
            if (bmp != null)
            {
                bmp.Dispose();
            }
            bmp = bayerRaw.ToBitmap();
            pictureBox.Image = bmp;
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;

            if (saveUnpackedRaw)
            {
                bayerRaw.saveUnpackRaw(outputRawDepth, directorypath + "/" + filename + "_unpacked_raw" + outputRawDepth + ".raw");
            }
            if (saveBmp)
            {
                bmp.Save(directorypath + "/" + filename + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            }
            if (saveJpeg)
            {
                bmp.Save(directorypath + "/" + filename + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            }
        }

        private bool getUserSelections()
        {
            if (!getResolution())
            {
                MessageBox.Show("Please enter valid width, height, and stride between 1 and 10000.");
                return false;
            }

            getRawTypeSelection();
            getDepthSelection();
            getBayerSelection();

            getOutputOption();
            outputRawDepth = ((ComboboxItemBitNumber)comboBoxOutputRawDepth.SelectedItem).Bit;

            if (checkBoxAWB.Checked)
            {
                enableAwb = true;

                int level;
                if (!int.TryParse(textBoxOB.Text, out level) || level < 0 || level > 10000)
                {
                    MessageBox.Show("Please enter valid OB between 0 and 10000.");
                    return false;
                }

                ob = level;
            }
            else
            {
                enableAwb = false;
                ob = 0;
            }

            return true;
        }


        private void getRawTypeSelection()
        {
            if (radioButtonTypeMipi.Checked)
                rawType = BayerRaw.RawType.RawType_MIPI;
            else if (radioButtonTypePacked.Checked)
                rawType = BayerRaw.RawType.RawType_Packed;
            else
                rawType = BayerRaw.RawType.RawType_Unpacked;
        }

        private void getDepthSelection()
        {
            if (radioButton8bit.Checked)
                depth = 8;
            else if (radioButton10bit.Checked)
                depth = 10;
            else if (radioButton12bit.Checked)
                depth = 12;
            else if (radioButton14bit.Checked)
                depth = 14;
        }
        private void getBayerSelection()
        {
            if (radioButtonBayerBG.Checked)
                bayerPattern = ColorConversionCodes.BayerBG2RGB;
            else if (radioButtonBayerRG.Checked)
                bayerPattern = ColorConversionCodes.BayerRG2RGB;
            else if (radioButtonBayerGR.Checked)
                bayerPattern = ColorConversionCodes.BayerGR2RGB;
            else if (radioButtonBayerGB.Checked)
                bayerPattern = ColorConversionCodes.BayerGB2RGB;
        }

        private void getOutputOption()
        {
            if (checkBoxSaveUnpackedRaw.Checked)
            {
                saveUnpackedRaw = true;
            }
            else
            {
                saveUnpackedRaw = false;
            }

            if (checkBoxSaveBmp.Checked)
            {
                saveBmp = true;
            }
            else
            {
                saveBmp = false;
            }

            if (checkBoxSaveJpg.Checked)
            {
                saveJpeg = true;
            }
            else
            {
                saveJpeg = false;
            }
        }

        private bool getResolution()
        {
            int width, height, stride;

            if (!int.TryParse(textBoxWidth.Text, out width) || width < 1 || width > 10000)
            {
                return false;
            }
            if (!int.TryParse(textBoxHeight.Text, out height) || height < 1 || height > 10000)
            {
                return false;
            }
            if (!int.TryParse(textBoxStride.Text, out stride) || stride < 1 || stride > 10000)
            {
                return false;
            }

            this.imgWidth = width;
            this.imgHeight = height;
            this.imgStride = stride;

            return true;
        }

        private void comboBoxOutputRawDepth_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (pathname.Length > 0)
            {
                if (getUserSelections())
                {
                    processImage();
                }
            }
        }

        private string pathname = "";
        private string directorypath = "", filename = "";
        private ColorConversionCodes bayerPattern = ColorConversionCodes.BayerBG2RGB;
        private int imgWidth, imgHeight, imgStride;
        private Bitmap bmp;
        private bool saveUnpackedRaw = false;
        private bool saveBmp = false;
        private bool saveJpeg = false;
        private int outputRawDepth = 8;
        private bool enableAwb = false;
        private int ob = 0;
        private BayerRaw.RawType rawType;
        private int depth;

        private void checkBoxSaveUnpackedRaw_CheckedChanged(object sender, EventArgs e)
        {
            if (pathname.Length > 0)
            {
                if (getUserSelections())
                {
                    processImage();
                }
            }
        }

        private void checkBoxSaveBmp_CheckedChanged(object sender, EventArgs e)
        {
            if (pathname.Length > 0)
            {
                if (getUserSelections())
                {
                    processImage();
                }
            }
        }

        private void checkBoxSaveJpg_CheckedChanged(object sender, EventArgs e)
        {
            if (pathname.Length > 0)
            {
                if (getUserSelections())
                {
                    processImage();
                }
            }
        }

        private void checkBoxAWB_CheckedChanged(object sender, EventArgs e)
        {
            if (pathname.Length > 0)
            {
                if (getUserSelections())
                {
                    processImage();
                }
            }
        }
    }
}
