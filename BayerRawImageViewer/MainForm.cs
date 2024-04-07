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
                // FIXME: image infomation should get from window form
                using BayerRaw bayerRaw = new BayerRaw(pathname, 4224, 3120, 5280, 10, 1);
                Bitmap bmp = bayerRaw.ToBitmap();
                pictureBox.Image = bmp;
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            }
        }

        private string pathname = "";
    }
}
