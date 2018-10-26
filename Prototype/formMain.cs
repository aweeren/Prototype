using Emgu.CV;
using Emgu.CV.Dnn;
using System;
using System.Windows.Forms;

namespace Prototype
{
    public partial class formMain : Form
    {
        private CascadeClassifier face_cascade = null;
        private Net netAge = null;
        private Net netGender = null;

        private double maxCalorie = 0;
        private double maxFat = 0;
        private double advProtein = 0;
        private double maxSugar = 0;
        private double advFibers = 0;

        public formMain()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBoxAdvice.Text = "";
            if (button3.Visible)
            {
                button3.Visible = false;
            }
            using (var frm = new formCapture(netAge, netGender, face_cascade))
            {
                frm.ShowDialog();
                textBoxAge.Text = frm.Age.ToString();
                if (frm.Gender == "Male")
                {
                    comboBoxGender.SelectedIndex = 0;
                }
                else
                {
                    comboBoxGender.SelectedIndex = 1;
                }
                if (frm.Image != null)
                {
                    frm.Image.ROI = frm.ROI;
                    pictureBox1.Image = frm.Image.ToBitmap();
                }
            }
        }

        private void formMain_Load(object sender, EventArgs e)
        {
            LoadDeepNet();
            face_cascade = new CascadeClassifier("Data\\haarcascade_frontalface_default.xml");
            textBoxAge.Text = "35";
            comboBoxGender.SelectedIndex = 1;
            comboBoxActivity.SelectedIndex =1;
            textBoxHeight.Text = "175";
            textBoxWeight.Text = "75";
        }

        private void LoadDeepNet()
        {
            try
            {
                netAge = DnnInvoke.ReadNetFromCaffe("Data\\deploy_age.prototxt", "Data\\age_net.caffemodel");
                netGender = DnnInvoke.ReadNetFromCaffe("Data\\deploy_gender.prototxt", "Data\\gender_net.caffemodel");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception thrown", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            double age;
            var success = double.TryParse(textBoxAge.Text, out age);
            if (!success)
            {
                age = 35;
            }
            double height;
            success = double.TryParse(textBoxHeight.Text, out height);
            if (!success)
            {
                height = 175;
            }
            double weight;
            success = double.TryParse(textBoxWeight.Text, out weight);
            if (!success)
            {
                weight = 75;
            }
            ActivityLevel activity = ActivityLevel.Medium;
            switch (comboBoxActivity.SelectedText)
            {
                case "Office worker" :
                    activity = ActivityLevel.Light;
                    break;
                case "Active":
                    activity = ActivityLevel.Medium;
                    break;
                case "Laborious":
                    activity = ActivityLevel.Heavy;
                    break;
            }

            maxCalorie = Formulas.AdvisedCalories(age, weight, height, comboBoxGender.Text, activity);
            advProtein = Formulas.AdvicedProtein(weight);
            maxSugar = Formulas.AdvicedSugars(maxCalorie);
            maxFat = Formulas.AdvicedFat(maxCalorie);
            advFibers = Formulas.AdvicedFiber(maxCalorie);

            var adviceString = "Personal recommendation:\r\n";
            adviceString += $"Recommended amount of calories:\t{maxCalorie:F0}\r\n";
            adviceString += $"Maximum amount of fat:\t\t{ maxFat:F0} grams\r\n";
            adviceString += $"Advised daily protein:\t\t{ advProtein:F0} grams\r\n";
            adviceString += $"Maximum amount of sugars:\t\t{ maxSugar:F0} grams\r\n";
            adviceString += $"Advised daily fibers:\t\t{ advFibers:F0} grams";
            
            textBoxAdvice.Text = adviceString;

            button3.Visible = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var frm = new formDiet();

            frm.MaxCalorie = maxCalorie;
            frm.MaxFat = maxFat;
            frm.AdvProtein = advProtein;
            frm.MaxSugar = maxSugar;
            frm.AdvFibers = advFibers;
            frm.MaxCholesterol = 0.6;

            frm.ShowDialog();

            // Go back to initial state
            button3.Visible = false;
            textBoxAdvice.Text = "";
            pictureBox1.Image = null;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
