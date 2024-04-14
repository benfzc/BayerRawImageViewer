using OpenCvSharp;
using System.Windows.Forms;

namespace BayerRawImageViewer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
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
                if (!getResolution())
                {
                    MessageBox.Show("Please enter valid width, height, and stride between 1 and 10000.");
                    return;
                }
                // FIXME: image infomation should get from window form
                using BayerRaw bayerRaw = new BayerRaw(pathname, this.imgWidth, this.imgHeight, this.imgStride, 10, 1);
                getBayerSelection();
                Bitmap bmp = bayerRaw.ToBitmap();
                pictureBox.Image = bmp;
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            }
        }

        private void radioButtonBayer_CheckedChanged(object sender, EventArgs e)
        {
            if (pathname.Length > 0)
            {
                if (!getResolution())
                {
                    MessageBox.Show("Please enter valid width, height, and stride between 1 and 10000.");
                    return;
                }
                // FIXME: image infomation should get from window form
                using BayerRaw bayerRaw = new BayerRaw(pathname, this.imgWidth, this.imgHeight, this.imgStride, 10, 1);
                getBayerSelection();
                bayerRaw.SetBayerPattern(bayerPattern);
                Bitmap bmp = bayerRaw.ToBitmap();
                pictureBox.Image = bmp;
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            }
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

        private string pathname = "";
        private ColorConversionCodes bayerPattern = ColorConversionCodes.BayerBG2RGB;
        private int imgWidth, imgHeight, imgStride;

    }
}
