using Emgu.CV;
using Emgu.CV.Dnn;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Prototype
{
    public partial class formCapture : Form
    {
        public double Age { get; set; }
        public string Gender { get; set; }
        public Image<Bgr,byte> Image { get; set; }
        public Rectangle ROI { get; set; }

        private static readonly string[] ageLabels = new string[]
            { "0-2", "4-6", "8-12", "15-20", "25-32", "38-43", "48-53", "60-100" };

        private VideoCapture capture;
        private Net netAge;
        private Net netGender;
        private CascadeClassifier face_cascade;

        public formCapture(Net netAge, Net netGender, CascadeClassifier cascade)
        {
            InitializeComponent();

            this.netAge = netAge;
            this.netGender = netGender;
            face_cascade = cascade;
        }

        private void formCapture_Load(object sender, EventArgs e)
        {
            capture = new VideoCapture();
            capture.ImageGrabbed += Capture_ImageGrabbed;
        }

        private void Capture_ImageGrabbed(object sender, EventArgs e)
        {
            var mat = new Mat();
            capture.Read(mat);
            var img = mat.ToImage<Bgr, byte>();
            var faces = GetFaces(img);
            foreach (var face in faces)
            {
                CvInvoke.Rectangle(img, face, new MCvScalar(0, 255, 255),2);
            }
            if (faces.Count() > 0)
            {
                EstimateAge(img, faces);
            }

            pictureBox1.Image = img.ToBitmap();

        }

        private Rectangle[] GetFaces(Image<Bgr, byte> img)
        {
            var gray = img.Convert<Gray, byte>();
            var faces = face_cascade.DetectMultiScale(gray, 1.15, 5);
            return faces;
        }

        private void EstimateAge(Image<Bgr, byte> img, Rectangle[] faces)
        {
            var origROI = img.ROI;
            int maxSize = 0;
            Rectangle maxFace = new Rectangle();
            foreach (var face in faces)
            {
                if (face.Width > maxSize)
                {
                    maxSize = face.Width;
                    maxFace = face;
                }
                if (face.Height > maxSize)
                {
                    maxSize = face.Height;
                    maxFace = face;
                }
            }
            img.ROI = maxFace;
            ROI = maxFace;
            var cropped = img.Copy();
            var blob = DnnInvoke.BlobFromImage(cropped.Mat, size: new Size(227, 227));
            netAge.SetInput(blob, "data");
            var prob = netAge.Forward("prob");
            double[] minVals = null;
            double[] maxVals = null;
            Point[] minInds = null;
            Point[] maxInds = null;
            prob.MinMax(out minVals, out maxVals, out minInds, out maxInds);
            var classInd = maxInds[0].X;
            var ageLabel = ageLabels[classInd];
            var pAge = maxVals[0];
            netGender.SetInput(blob, "data");
            prob = netGender.Forward("prob");
            prob.MinMax(out minVals, out maxVals, out minInds, out maxInds);
            Gender = (maxInds[0].X == 0 ? "Male" : "Female");
            var pGender = maxVals[0];
            var fstring = $"Age group:\t{ ageLabel}\t(p = {pAge:F2})\t gender:{ Gender }\t(p = {pGender:F2})";
            switch (classInd)
            {
                case 0 : Age = 1;
                    break;
                case 1 : Age = 5;
                    break;
                case 2: Age = 10;
                    break;
                case 3: Age = 18;
                    break;
                case 4: Age = 30;
                    break;
                case 5: Age = 40;
                    break;
                case 6: Age = 50;
                    break;
                case 7: Age = 65;
                    break;
            }

            if (!labelAge.IsDisposed)
            {
                labelAge.Invoke(new Action(() => labelAge.Text = fstring));
            }
            Image = img;
            img.ROI = origROI;
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            capture.Start();
            button2.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            capture.Stop();
            labelAge.Text = "Capture stopped";
            this.Close();
        }
    }
}
