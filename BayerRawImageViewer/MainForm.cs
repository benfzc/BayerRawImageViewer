using OpenCvSharp;
using System.Configuration;
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

            restoreUserSelections();
        }
        void restoreUserSelections()
        {
            // Output image Depth
            var OutDepthStr = ConfigurationManager.AppSettings["OutputImageDepth"];
            if (OutDepthStr != null)
            {
                int depth;
                if (int.TryParse(OutDepthStr, out depth))
                {
                    switch (depth)
                    {
                        case 8:
                            comboBoxOutputRawDepth.SelectedIndex = 0;
                            break;
                        case 10: // pass through
                        default:
                            comboBoxOutputRawDepth.SelectedIndex = 1;
                            break;
                        case 12:
                            comboBoxOutputRawDepth.SelectedIndex = 2;
                            break;
                        case 14:
                            comboBoxOutputRawDepth.SelectedIndex = 3;
                            break;
                        case 16:
                            comboBoxOutputRawDepth.SelectedIndex = 4;
                            break;
                    }
                }
            }


            // Raw Type
            var RawTypeStr = ConfigurationManager.AppSettings["RawType"];
            if (RawTypeStr != null)
            {
                switch (RawTypeStr)
                {
                    case "MIPI":
                        radioButtonTypeMipi.Checked = true;
                        break;
                    case "Packed": // pass through
                    default:
                        radioButtonTypePacked.Checked = true;
                        break;
                    case "Unpacked":
                        radioButtonTypeUnpacked.Checked = true;
                        break;
                }
            }

            // Resolution
            var ImageWidth = ConfigurationManager.AppSettings["ImageWidth"];
            if (ImageWidth != null) { textBoxWidth.Text = ImageWidth; }

            var ImageHeight = ConfigurationManager.AppSettings["ImageHeight"];
            if (ImageHeight != null) { textBoxHeight.Text = ImageHeight; }

            var ImageStride = ConfigurationManager.AppSettings["ImageStride"];
            if (ImageStride != null) { textBoxStride.Text = ImageStride; }

            // Input image Depth
            var DepthStr = ConfigurationManager.AppSettings["InputImageDepth"];
            if (DepthStr != null)
            {
                int depth;
                if (int.TryParse(DepthStr, out depth))
                {
                    switch (depth)
                    {
                        case 8:
                            radioButton8bit.Checked = true;
                            break;
                        case 10: // pass through
                        default:
                            radioButton10bit.Checked = true;
                            break;
                        case 12:
                            radioButton12bit.Checked = true;
                            break;
                        case 14:
                            radioButton14bit.Checked = true;
                            break;
                    }
                }
            }

            // Bayer Pattern format
            var BayerPatternStr = ConfigurationManager.AppSettings["BayerPattern"];
            if (BayerPatternStr != null)
            {
                switch (BayerPatternStr)
                {
                    case "BG":
                        radioButtonBayerBG.Checked = true;
                        break;
                    case "GB":
                        radioButtonBayerGB.Checked = true;
                        break;
                    case "RG":
                        radioButtonBayerRG.Checked = true;
                        break;
                    case "GR":
                        radioButtonBayerGR.Checked = true;
                        break;
                }
            }

            // Post processing
            var EnableAWBStr = ConfigurationManager.AppSettings["EnableAWB"];
            if (EnableAWBStr != null)
            {
                if (EnableAWBStr.Equals("true", StringComparison.OrdinalIgnoreCase))
                    checkBoxAWB.Checked = true;
                else
                    checkBoxAWB.Checked = false;
            }

            var EnableOBCStr = ConfigurationManager.AppSettings["EnableOBC"];
            if (EnableOBCStr != null)
            {
                if (EnableOBCStr.Equals("true", StringComparison.OrdinalIgnoreCase))
                    checkBoxOBC.Checked = true;
                else
                    checkBoxOBC.Checked = false;
            }

            var BlackLevelStr = ConfigurationManager.AppSettings["BlackLevel"];
            if (BlackLevelStr != null) { textBoxOB.Text = BlackLevelStr; }

            // Output
            var SaveUnpackedRawStr = ConfigurationManager.AppSettings["SaveUnpackedRaw"];
            if (SaveUnpackedRawStr != null)
            {
                if (SaveUnpackedRawStr.Equals("true", StringComparison.OrdinalIgnoreCase))
                    checkBoxSaveUnpackedRaw.Checked = true;
                else
                    checkBoxSaveUnpackedRaw.Checked = false;
            }

            var SaveBmpStr = ConfigurationManager.AppSettings["SaveBmp"];
            if (SaveBmpStr != null)
            {
                if (SaveBmpStr.Equals("true", StringComparison.OrdinalIgnoreCase))
                    checkBoxSaveBmp.Checked = true;
                else
                    checkBoxSaveBmp.Checked = false;
            }

            var SaveJpegStr = ConfigurationManager.AppSettings["SaveJpeg"];
            if (SaveJpegStr != null)
            {
                if (SaveJpegStr.Equals("true", StringComparison.OrdinalIgnoreCase))
                    checkBoxSaveJpg.Checked = true;
                else
                    checkBoxSaveJpg.Checked = false;
            }
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
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data == null || !e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            string[]? s = e.Data.GetData(DataFormats.FileDrop, false) as string[];
            if (s != null && s.Length > 0)
            {
                pathname = s[0];
                directorypath = Path.GetDirectoryName(pathname) ?? Environment.CurrentDirectory;
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
                RadioButton? radioButton = sender as RadioButton;
                if (radioButton != null && radioButton.Checked && getUserSelections())
                {
                    processImage();
                }
            }
        }
        private void processImage()
        {
            BayerRaw bayerRaw;

            try
            {
                bayerRaw = new BayerRaw(pathname, this.imgWidth, this.imgHeight, this.imgStride, depth, rawType);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK);
                return;
            }


            bayerRaw.EnableAwb = enableAwb;
            bayerRaw.OB = ob;
            bayerRaw.SetBayerPattern(bayerPattern);

            if (bmp != null)
            {
                bmp.Dispose();
            }
            bmp = bayerRaw.ToBitmap();
            pictureBox.Image = bmp;
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;

            string outputPathname = directorypath + "/" + filename;
            if (enableOBC) { outputPathname += "_obc"; }
            if (enableAwb) { outputPathname += "_awb"; }

            if (saveUnpackedRaw)
            {
                bayerRaw.saveUnpackRaw(outputRawDepth, outputPathname + "_unpacked" + outputRawDepth + ".raw");
            }
            if (saveBmp)
            {
                bmp.Save(outputPathname + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            }
            if (saveJpeg)
            {
                bmp.Save(outputPathname + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            }

            bayerRaw.Dispose();
        }

        private bool getUserSelections()
        {
            getResolution();

            getRawTypeSelection();
            getDepthSelection();
            getBayerSelection();

            getOutputOption();
            ComboboxItemBitNumber selectedItem = (ComboboxItemBitNumber)comboBoxOutputRawDepth.SelectedItem!;
            outputRawDepth = selectedItem.Bit;

            enableAwb = checkBoxAWB.Checked;
            if (checkBoxOBC.Checked)
            {
                int level;
                if (!int.TryParse(textBoxOB.Text, out level) || level < 0 || level > 10000)
                {
                    level = 0;
                }

                ob = level;
                enableOBC = true;
            }
            else
            {
                ob = 0;
                enableOBC = false;
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
        private Bitmap? bmp;
        private bool saveUnpackedRaw = false;
        private bool saveBmp = false;
        private bool saveJpeg = false;
        private int outputRawDepth = 8;
        private bool enableAwb = false;
        private bool enableOBC = false;
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

        private void checkBoxOB_CheckedChanged(object sender, EventArgs e)
        {
            if (pathname.Length > 0)
            {
                if (getUserSelections())
                {
                    processImage();
                }
            }
        }

        private void updateAppSetting(ref KeyValueConfigurationCollection settings, string key, string value)
        {
            if (settings[key] == null)
            {
                settings.Add(key, value);
            }
            else
            {
                settings[key].Value = value;
            }
        }

        private void saveUserSelections()
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;

            // Raw Type
            string RawTypeStr;
            switch (rawType)
            {
                case BayerRaw.RawType.RawType_MIPI:
                    RawTypeStr = "MIPI"; break;
                case BayerRaw.RawType.RawType_Packed:
                    RawTypeStr = "Packed"; break;
                case BayerRaw.RawType.RawType_Unpacked:
                default:
                    RawTypeStr = "Unpacked"; break;
            }
            updateAppSetting(ref settings, "RawType", RawTypeStr);

            // Resolution
            updateAppSetting(ref settings, "ImageWidth", imgWidth.ToString());
            updateAppSetting(ref settings, "ImageHeight", imgHeight.ToString());
            updateAppSetting(ref settings, "ImageStride", imgStride.ToString());

            // Input Image Depth
            updateAppSetting(ref settings, "InputImageDepth", depth.ToString());

            // Bayer Pattern
            string bayerPatternStr;
            switch (bayerPattern)
            {
                case ColorConversionCodes.BayerBG2RGB:
                    bayerPatternStr = "BG"; break;
                case ColorConversionCodes.BayerRG2RGB:
                    bayerPatternStr = "RG"; break;
                case ColorConversionCodes.BayerGR2RGB:
                    bayerPatternStr = "GR"; break;
                case ColorConversionCodes.BayerGB2RGB:
                default:
                    bayerPatternStr = "GB"; break;
            }
            updateAppSetting(ref settings, "BayerPattern", bayerPatternStr);

            // Post processing
            updateAppSetting(ref settings, "EnableAWB", enableAwb.ToString());
            updateAppSetting(ref settings, "EnableOBC", enableOBC.ToString());
            updateAppSetting(ref settings, "BlackLevel", ob.ToString());

            // Output
            updateAppSetting(ref settings, "SaveUnpackedRaw", saveUnpackedRaw.ToString());
            updateAppSetting(ref settings, "SaveBmp", saveBmp.ToString());
            updateAppSetting(ref settings, "SaveJpeg", saveJpeg.ToString());

            // Output Image Depth
            updateAppSetting(ref settings, "OutputImageDepth", outputRawDepth.ToString());

            configFile.Save(ConfigurationSaveMode.Modified);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            getUserSelections();
            saveUserSelections();
        }
    }
}
