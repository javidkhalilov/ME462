using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AForge.Video.DirectShow;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Video;
using USBLibrary;
using MechaBoardClasses;


namespace ME_462_Final_Project
{
    public partial class Form1 : Form
    {
        MechaBoard mb;              //initialize a mechaboard
        Byte[] data = new Byte[64];     //initialize the data to send
        Byte[] rdata = new Byte[64];

        static FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
        static VideoCaptureDevice videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);

        int[,] threshold_Array = new int[3, 7]; int i, j, a, b, th_minus, th_1st, Stats_storage_minus, Stats_storage_1st;
        int[,] Stats_storage = new int[3, 7];
        int[] value = new int[3];

        byte X_Start;
        byte X_End;
        byte stepsize;
        byte intervals;
        int x_value;
        double result;

        Rectangle[,] rectangle_Array = new Rectangle[3, 7];

        ImageStatistics Stats_minus, Stats_1st;
        ImageStatistics[,] Stats = new ImageStatistics[3, 7];

        Bitmap Video, VideoClone;
        Bitmap cropped_minus, cropped_1st, gray_minus, gray_1st, binary_minus, binary_1st;
        Bitmap[,] cropped_Array = new Bitmap[3,7];
        Bitmap[,] gray_Array = new Bitmap [3,7];
        Bitmap[,] binary_Array = new Bitmap[3, 7];

        GrayscaleBT709[,] grayfilter_Array = new GrayscaleBT709[3,7];
        GrayscaleBT709 grayfilter_minus = new GrayscaleBT709();
        GrayscaleBT709 grayfilter_1st = new GrayscaleBT709();

        Threshold[,] binfilter_Array = new Threshold[3,7];
        Threshold threshold_minus = new Threshold();
        Threshold threshold_1st = new Threshold();

        Crop[,] cropfilter_Array = new Crop[3,7];
        Crop crop_minus = new Crop(new Rectangle(46, 119, 10, 5));
        Crop crop_1st = new Crop(new Rectangle(66, 90, 5, 10));
        
        TextBox[,] tb_Array = new TextBox[3,7];
        PictureBox[,] pb_Array = new PictureBox[3, 7];

       

        public Form1()
        {
            InitializeComponent();

            a = 5; b = 10; rdata[1] = 1; rdata[2] = 0;

            rectangle_Array[0, 0] = new Rectangle(125, 93, a, b); rectangle_Array[0, 1] = new Rectangle(122, 135, a, b); rectangle_Array[0, 2] = new Rectangle(105, 167, b, a); rectangle_Array[0, 3] = new Rectangle(95, 133, a, b); rectangle_Array[0, 4] = new Rectangle(95, 90, a, b); rectangle_Array[0, 5] = new Rectangle(111, 74, b, a); rectangle_Array[0, 6] = new Rectangle(107, 121, b, a);
            rectangle_Array[1, 0] = new Rectangle(184, 97, a, b); rectangle_Array[1, 1] = new Rectangle(181, 136, a, b); rectangle_Array[1, 2] = new Rectangle(160, 166, b, a); rectangle_Array[1, 3] = new Rectangle(153, 138, a, b); rectangle_Array[1, 4] = new Rectangle(157, 92, a, b); rectangle_Array[1, 5] = new Rectangle(172, 74, b, a); rectangle_Array[1, 6] = new Rectangle(168, 120, b, a);
            rectangle_Array[2, 0] = new Rectangle(244, 92, a, b); rectangle_Array[2, 1] = new Rectangle(238, 140, a, b); rectangle_Array[2, 2] = new Rectangle(222, 166, b, a); rectangle_Array[2, 3] = new Rectangle(211, 136, a, b); rectangle_Array[2, 4] = new Rectangle(216, 90, a, b); rectangle_Array[2, 5] = new Rectangle(232, 76, b, a); rectangle_Array[2, 6] = new Rectangle(223,119, b, a);

            tb_Array[0,0] = textBox00; tb_Array[0,1] = textBox01; tb_Array[0,2] = textBox02; tb_Array[0,3] = textBox03; tb_Array[0,4] = textBox04; tb_Array[0,5] = textBox05; tb_Array[0,6] = textBox06;
            tb_Array[1,0] = textBox10; tb_Array[1,1] = textBox11; tb_Array[1,2] = textBox12; tb_Array[1,3] = textBox13; tb_Array[1,4] = textBox14; tb_Array[1,5] = textBox15; tb_Array[1,6] = textBox16;
            tb_Array[2,0] = textBox20; tb_Array[2,1] = textBox21; tb_Array[2,2] = textBox22; tb_Array[2,3] = textBox23; tb_Array[2,4] = textBox24; tb_Array[2,5] = textBox25; tb_Array[2,6] = textBox26;

            pb_Array[0, 0] = pictureBox00; pb_Array[0, 1] = pictureBox01; pb_Array[0, 2] = pictureBox02; pb_Array[0, 3] = pictureBox03; pb_Array[0, 4] = pictureBox04; pb_Array[0, 5] = pictureBox05; pb_Array[0, 6] = pictureBox06;
            pb_Array[1, 0] = pictureBox10; pb_Array[1, 1] = pictureBox11; pb_Array[1, 2] = pictureBox12; pb_Array[1, 3] = pictureBox13; pb_Array[1, 4] = pictureBox14; pb_Array[1, 5] = pictureBox15; pb_Array[1, 6] = pictureBox16;
            pb_Array[2, 0] = pictureBox20; pb_Array[2, 1] = pictureBox21; pb_Array[2, 2] = pictureBox22; pb_Array[2, 3] = pictureBox23; pb_Array[2, 4] = pictureBox24; pb_Array[2, 5] = pictureBox25; pb_Array[2, 6] = pictureBox26;

            for (i = 0; i < 3; i++)
            {
                for (j = 0; j < 7; j++)
                {
                    grayfilter_Array[i, j] = new GrayscaleBT709();
                    binfilter_Array[i, j] = new Threshold();
                    cropfilter_Array[i, j] = new Crop(rectangle_Array[i, j]);
                }
            }

            mb = new MechaBoard("BDDBF604-85D6-4078-92D2-9FA6FAEFDDD1", this);
            
        }
        
        

        private void Form1_Load(object sender, EventArgs e)
        {
            videoSource.DesiredFrameSize = new Size(320, 240);
            videoSource.DesiredFrameRate = 2;
            videoSource.Start();
            videoSource.NewFrame += new NewFrameEventHandler(videoSource_NewFrame);
        }

        void videoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Video = (Bitmap)eventArgs.Frame.Clone();
        }

        private void pictureBoxes_MouseDown(object sender, MouseEventArgs e)
        {
            this.Text = "X = " + e.X.ToString() + ", Y = " + e.Y.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            for (i = 0; i < 3; i++) value[i] = 0;
            
            pb_Original.Image = Video;
            VideoClone = (Bitmap)Video.Clone();       

            for (i = 0; i < 3; i++)
            {
                for (j = 0; j < 7; j++)
                {
                    int.TryParse(tb_Array[i, j].Text, out threshold_Array[i, j]);
                    binfilter_Array[i, j].ThresholdValue = threshold_Array[i, j];
                    cropped_Array[i, j] = cropfilter_Array[i, j].Apply(VideoClone);
                    gray_Array[i, j] = grayfilter_Array[i, j].Apply(cropped_Array[i, j]);
                    binary_Array[i, j] = binfilter_Array[i, j].Apply(gray_Array[i, j]);
                    Stats[i, j] = new ImageStatistics(binary_Array[i, j]);
                    pb_Array[i, j].Image = binary_Array[i, j];
                }
            }

                int.TryParse(textBox_minus.Text, out th_minus);
                int.TryParse(textBox_1st.Text, out th_1st);

                threshold_minus.ThresholdValue = th_minus;
                threshold_1st.ThresholdValue = th_1st;

                cropped_minus = crop_minus.Apply(VideoClone);
                cropped_1st = crop_1st.Apply(VideoClone);

                gray_minus = grayfilter_minus.Apply(cropped_minus);
                gray_1st = grayfilter_1st.Apply(cropped_1st);

                binary_minus = threshold_minus.Apply(gray_minus);
                binary_1st = threshold_1st.Apply(gray_1st);

                pictureBox_minus.Image = binary_minus;
                pictureBox_1st.Image = binary_1st;
                pictureBox1.Image = pictureBox_1st.Image;

                Stats_minus = new ImageStatistics(binary_minus);
                Stats_1st = new ImageStatistics(binary_1st);

                for (i = 0; i < 3; i++)
                {
                    for (j = 0; j < 7; j++)
                    {
                        if (Stats[i, j].PixelsCountWithoutBlack < 25) Stats_storage[i, j] = 1;
                        if (Stats[i, j].PixelsCountWithoutBlack >= 25) Stats_storage[i, j] = 0;
                    }
                }

                if (Stats_minus.PixelsCountWithoutBlack < 25) Stats_storage_minus = 1;
                if (Stats_minus.PixelsCountWithoutBlack >= 25) Stats_storage_minus = 0;

                if (Stats_1st.PixelsCountWithoutBlack < 25) Stats_storage_1st = 1;
                if (Stats_1st.PixelsCountWithoutBlack >= 25) Stats_storage_1st = 0;


                for (i = 0; i < 3; i++) 
                {
                    for (j = 0; j < 7; j++)
                    {
                        if (Stats_storage[i, j] == 1) value[i] = value[i] + (int)Math.Pow(2, (double)j);
                    }
                    if (value[i] == 63) value[i] = 0;
                    else if (value[i] == 3) value[i] = 1;
                    else if (value[i] == 109) value[i] = 2;
                    else if (value[i] == 103) value[i] = 3;
                    else if (value[i] == 83) value[i] = 4;
                    else if (value[i] == 118) value[i] = 5;
                    else if (value[i] == 126) value[i] = 6;
                    else if (value[i] == 35) value[i] = 7;
                    else if (value[i] == 127) value[i] = 8;
                    else if (value[i] == 119) value[i] = 9;
                    else value[i] = 0;
                }

                result = Stats_storage_1st * 1000 + value[0] * 100 + value[1] * 10 + value[2];
                if (Stats_storage_minus == 1) result = 0 - result;
                result = result / 100;
                this.Text = result.ToString();
                

        }

        private void btn_Connect_Click(object sender, EventArgs e)
        {
            //connects to mechaboard device
            if (mb.FindMyDevice())
            {
                btn_Connect.Enabled = false;
            }
            else
            {
                MessageBox.Show("Please make sure that Mechaboard is properly connected to your system", "Ooops No Mechaboard Detected");
                this.Close();
            }

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            mb.ShutDownDevice();
        }


        private void btn_Go_Click(object sender, EventArgs e)
        {
            data[1] = 50; 
            data[2] = 5;
            data[3] = 5; 
            data[4] = 1; 
            data[5] = 2;

            rdata[1] = 0;
            mb.SendDataViaBulkTransfer(data, 64);
            groupBox2.Enabled = false;
            groupBox3.Enabled = false;
        }

        private void btn_Init_Click(object sender, EventArgs e)
        {
            data[4] = 2; 
            mb.SendDataViaBulkTransfer(data, 64);
        }

        private void btn_Zero_Click(object sender, EventArgs e)
        {
            data[4] = 3;
            mb.SendDataViaBulkTransfer(data, 64);
        }


        private void textBox_Step_Size_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox_X_Start_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox_X_End_TextChanged(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            mb.ReadDataViaBulkTransfer(ref rdata, 64);
            if (rdata[1] == 1)
            {
                groupBox2.Enabled = true;
                groupBox3.Enabled = true;
            }
            if (rdata[2] == 1)
            {
                rdata[2] = 0;
                for (i = 0; i < 2; i++)
                {
                    #region

                    for (i = 0; i < 3; i++) value[i] = 0;

                    pb_Original.Image = Video;
                    VideoClone = (Bitmap)Video.Clone();

                    for (i = 0; i < 3; i++)
                    {
                        for (j = 0; j < 7; j++)
                        {
                            int.TryParse(tb_Array[i, j].Text, out threshold_Array[i, j]);
                            binfilter_Array[i, j].ThresholdValue = threshold_Array[i, j];
                            cropped_Array[i, j] = cropfilter_Array[i, j].Apply(VideoClone);
                            gray_Array[i, j] = grayfilter_Array[i, j].Apply(cropped_Array[i, j]);
                            binary_Array[i, j] = binfilter_Array[i, j].Apply(gray_Array[i, j]);
                            Stats[i, j] = new ImageStatistics(binary_Array[i, j]);
                            pb_Array[i, j].Image = binary_Array[i, j];
                        }
                    }

                    int.TryParse(textBox_minus.Text, out th_minus);
                    int.TryParse(textBox_1st.Text, out th_1st);

                    threshold_minus.ThresholdValue = th_minus;
                    threshold_1st.ThresholdValue = th_1st;

                    cropped_minus = crop_minus.Apply(VideoClone);
                    cropped_1st = crop_1st.Apply(VideoClone);

                    gray_minus = grayfilter_minus.Apply(cropped_minus);
                    gray_1st = grayfilter_1st.Apply(cropped_1st);

                    binary_minus = threshold_minus.Apply(gray_minus);
                    binary_1st = threshold_1st.Apply(gray_1st);

                    pictureBox_minus.Image = binary_minus;
                    pictureBox_1st.Image = binary_1st;
                    pictureBox1.Image = pictureBox_1st.Image;

                    Stats_minus = new ImageStatistics(binary_minus);
                    Stats_1st = new ImageStatistics(binary_1st);

                    for (i = 0; i < 3; i++)
                    {
                        for (j = 0; j < 7; j++)
                        {
                            if (Stats[i, j].PixelsCountWithoutBlack < 25) Stats_storage[i, j] = 1;
                            if (Stats[i, j].PixelsCountWithoutBlack >= 25) Stats_storage[i, j] = 0;
                        }
                    }

                    if (Stats_minus.PixelsCountWithoutBlack < 25) Stats_storage_minus = 1;
                    if (Stats_minus.PixelsCountWithoutBlack >= 25) Stats_storage_minus = 0;

                    if (Stats_1st.PixelsCountWithoutBlack < 25) Stats_storage_1st = 1;
                    if (Stats_1st.PixelsCountWithoutBlack >= 25) Stats_storage_1st = 0;


                    for (i = 0; i < 3; i++)
                    {
                        for (j = 0; j < 7; j++)
                        {
                            if (Stats_storage[i, j] == 1) value[i] = value[i] + (int)Math.Pow(2, (double)j);
                        }
                        if (value[i] == 63) value[i] = 0;
                        else if (value[i] == 3) value[i] = 1;
                        else if (value[i] == 109) value[i] = 2;
                        else if (value[i] == 103) value[i] = 3;
                        else if (value[i] == 83) value[i] = 4;
                        else if (value[i] == 118) value[i] = 5;
                        else if (value[i] == 126) value[i] = 6;
                        else if (value[i] == 35) value[i] = 7;
                        else if (value[i] == 127) value[i] = 8;
                        else if (value[i] == 119) value[i] = 9;
                        else value[i] = 0;
                    }

                    result = Stats_storage_1st * 1000 + value[0] * 100 + value[1] * 10 + value[2];
                    if (Stats_storage_minus == 1) result = 0 - result;
                    result = result / 100;

                    #endregion
                }

                this.Text += result.ToString() + " ";
              

            }

        }


    }
}
