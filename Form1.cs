using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace 相机标定
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }

        #region 参数配置

        string BDPicture_path;//标定板路径
        string matrix_path;
        string target_path;
        Size image_size = new Size();//标定板尺寸（像素尺寸）;
        Size board_size = new Size();//标定板角数量
        int image_count;//图片数量
        VectorOfPointF Npointsl = new VectorOfPointF();//图像坐标位置
        int i; //
        int j; //
        int T; //
        int pic_n;//标定板照片数量
        float pic_h; //标定板每格长度数据MM
        float pic_w;//标定板每格宽度数据MM
        int board_N;
        int count_time;
        double accuracy;
        bool ready = false;

        MCvPoint3D32f temple_points = new MCvPoint3D32f();//创建初始角点坐标集合
        Mat cameraMatrix = new Mat(3, 3, DepthType.Cv32F, 1);//相机内参矩阵
        Mat distCoeffs = new Mat(1, 5, DepthType.Cv32F, 1);//相机5个畸变参数

        #endregion
        private void Form1_Load(object sender, EventArgs e)
        {
            board_size.Width = Convert.ToInt16(textBox4.Text);
            board_size.Height = Convert.ToInt16(textBox5.Text);
            pic_n = Convert.ToInt16(textBox3.Text);
            pic_h = Convert.ToInt16(textBox7.Text);
            pic_w = Convert.ToInt16(textBox6.Text);
            board_N = board_size.Width * board_size.Height;
            count_time = Convert.ToInt16(textBox8.Text);
            accuracy = Convert.ToDouble(textBox9.Text);
        }



        private void button1_Click(object sender, EventArgs e)//生成校准矩阵文件
        {


            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择文件路径";
            // dialog.SelectedPath = path;                        
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = dialog.SelectedPath;
            }
            BDPicture_path = textBox2.Text.ToString();
            DirectoryInfo folder = new DirectoryInfo(BDPicture_path);
            image_count = 0;//图片数量 

            Mat[] rotateMat = new Mat[pic_n];//旋转矩阵
            Mat[] transMat = new Mat[pic_n];//平移矩阵                    
            MCvPoint3D32f[][] object_points = new MCvPoint3D32f[pic_n][]; //创建目标坐标集合
            List<MCvPoint3D32f> object_list = new List<MCvPoint3D32f>();
            PointF[][] corner_count = new PointF[pic_n][];  //角点数量          
            Mat view_gray = new Mat();
            //Matrix
            foreach (FileInfo file in folder.GetFiles("*.jpg").OrderBy(p => p.CreationTime))//定标版图片扫描处理
            {
                image_count++;
                // image = new Bitmap(file.FullName);
                //获取图像的BitmapData对像 
                // Image<Bgr, byte> imageInput = new Image<Bgr, byte>(new Bitmap(Image.FromFile(filename)));
                Image<Bgr, byte> imageInput = new Image<Bgr, byte>(new Bitmap(Image.FromFile(file.FullName)));
                if (image_count == 1)
                {
                    image_size.Width = imageInput.Cols;
                    image_size.Height = imageInput.Rows;
                    textBox1.AppendText(image_size.Width.ToString() + "\r\n" + image_size.Height.ToString() + "\r\n");
                }
                CvInvoke.CvtColor(imageInput, view_gray, ColorConversion.Rgb2Gray);
                CvInvoke.FindChessboardCorners(view_gray, board_size, Npointsl, CalibCbType.AdaptiveThresh);
                corner_count[image_count - 1] = new PointF[board_N];
                for (int S = 0; S < board_N; S++)
                {

                    corner_count[image_count - 1][S] = Npointsl.ToArray()[S];

                }
                textBox1.AppendText(image_count.ToString() + "\r\n");

            }


            for (T = 0; T < pic_n; T++) ///把角坐标保存数组
            {
                object_list.Clear();
                for (i = 0; i < board_size.Height; i++)
                {
                    for (j = 0; j < board_size.Width; j++)
                    {
                        temple_points.X = j * pic_w;
                        temple_points.Y = i * pic_h;
                        temple_points.Z = 0;
                        object_list.Add(temple_points);
                    }
                }
                object_points[T] = object_list.ToArray();

            }

            CvInvoke.CalibrateCamera(object_points, corner_count, image_size, cameraMatrix, distCoeffs,
            CalibType.RationalModel, new MCvTermCriteria(count_time, accuracy), out rotateMat, out transMat);
            if (!ready)
            {
                MessageBox.Show("请选择矩阵输出路径！");
            }
            else
            {
                try
                {
                    FileStorage storage_came = new FileStorage(@matrix_path + "//" + "cameraMatrix.txt", FileStorage.Mode.Write);//路径不能出现中文名字。。。。。
                    storage_came.Write(cameraMatrix);
                    storage_came.ReleaseAndGetString();
                    FileStorage storage_dist = new FileStorage(@matrix_path + "//" + "distCoeffs.txt", FileStorage.Mode.Write);
                    storage_dist.Write(distCoeffs);//distCoeffs：输入参数，相机的畸变系数：，有4，5,8,12或14个元素。如果这个向量是空的，就认为是零畸变系数。
                    storage_dist.ReleaseAndGetString();
                    textBox1.AppendText("矩阵输出：" + matrix_path + "\r\n" + "-----------------------------------------------------" + "\r\n");
                    // cameraMatrix(1,1) =
                    MessageBox.Show("标定成功！");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

        }

        private void button2_Click(object sender, EventArgs e) //畸变校准测试
        {
            OpenFileDialog dialog1 = new OpenFileDialog();

            dialog1.Title = "请选择文件";
            if (dialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text += dialog1.FileName + "---------------------------------------------" + "\r\n";
            }
            FileStream filereader = new FileStream((@matrix_path + "//" + "cameraMatrix.txt"), FileMode.Open, FileAccess.Read);
            StreamReader streamReader = new StreamReader(filereader);
            CamerMatrix_value = streamReader.ReadToEnd();
            CamerMatrix_start_index = CamerMatrix_value.IndexOf("[");
            CamerMatrix_end_index = CamerMatrix_value.IndexOf("]");
            CamerMatrix_value = CamerMatrix_value.Substring(CamerMatrix_start_index + 1, CamerMatrix_end_index - CamerMatrix_start_index - 1);
            CamerMatrix_value = CamerMatrix_value.Replace("\r\n", "");
            textBox1.Text += CamerMatrix_value + "\r\n" + "-----------------------------------------------------" + "\r\n";
            FileStream filereader1 = new FileStream((@matrix_path + "//" + "distCoeffs.txt"), FileMode.Open, FileAccess.Read);
            StreamReader streamReader1 = new StreamReader(filereader1);
            distCoeffs_value = streamReader1.ReadToEnd();
            distCoeffs_start_index = distCoeffs_value.IndexOf("[");
            distCoeffs_end_index = distCoeffs_value.IndexOf("]");
            distCoeffs_value = distCoeffs_value.Substring(distCoeffs_start_index + 1, distCoeffs_end_index - distCoeffs_start_index - 1);
            distCoeffs_value = distCoeffs_value.Replace("\r\n", "");
            textBox1.Text += distCoeffs_value + "\r\n" + "-----------------------------------------------------" + "\r\n";
            Mat cameraMatrix_T = new Mat(3, 3, DepthType.Cv64F, 1);//相机内参矩阵
            Mat distCoeffs_T = new Mat(1, 5, DepthType.Cv64F, 1);//相机5个畸变参数
            string[] A = new string[9];
            string[] B = new string[15];
            char[] separator = { ',' };////需要空空格代替回车换行
            A = CamerMatrix_value.Split(separator);
            B = distCoeffs_value.Split(separator);
            int k = 0;
            for (int i = 0; i < 3; i++)
            {
                for (int y = 0; y < 3; y++)
                {

                    MatExtension.SetValue(cameraMatrix_T, i, y, Convert.ToDouble(A[k]));
                    k++;
                }
            }
            k = 0;
            for (int i = 0; i < 5; i++)
            {

                MatExtension.SetValue(distCoeffs_T, 0, i, Convert.ToDouble(B[i]));

            }

            Image<Bgr, byte> imageSource = new Image<Bgr, byte>(new Bitmap(Image.FromFile(dialog1.FileName)));
            Image<Bgr, byte> newimage = imageSource.Clone();
            CvInvoke.Undistort(imageSource, newimage, cameraMatrix_T, distCoeffs_T);
            imageBox1.Image = imageSource;
            imageBox2.Image = newimage;
            CvInvoke.WaitKey(500);



        }

        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog1 = new FolderBrowserDialog();
            dialog1.Description = "请选择文件路径";
            // dialog.SelectedPath = path;                        
            if (dialog1.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = dialog1.SelectedPath;
            }
            BDPicture_path = textBox2.Text.ToString();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog2 = new FolderBrowserDialog();
            dialog2.Description = "请选择文件路径";
            // dialog.SelectedPath = path;                        
            if (dialog2.ShowDialog() == DialogResult.OK)
            {
                textBox10.Text = dialog2.SelectedPath;
            }
            matrix_path = textBox10.Text.ToString();
            ready = true;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog3 = new FolderBrowserDialog();
            dialog3.Description = "请选择文件路径";
            // dialog.SelectedPath = path;                        
            if (dialog3.ShowDialog() == DialogResult.OK)
            {
                textBox11.Text = dialog3.SelectedPath;
            }
            target_path = textBox11.Text.ToString();
        }

        #region Mat值修改类

        #endregion


        int CamerMatrix_start_index;
        int CamerMatrix_end_index;
        string CamerMatrix_value;
        int distCoeffs_start_index;
        int distCoeffs_end_index;
        string distCoeffs_value;
        private void button6_Click(object sender, EventArgs e)//读取校准矩阵
        {
            FileStream filereader = new FileStream((@matrix_path + "//" + "cameraMatrix.txt"), FileMode.Open, FileAccess.Read);
            StreamReader streamReader = new StreamReader(filereader);
            CamerMatrix_value = streamReader.ReadToEnd();
            CamerMatrix_start_index = CamerMatrix_value.IndexOf("[");
            CamerMatrix_end_index = CamerMatrix_value.IndexOf("]");
            CamerMatrix_value = CamerMatrix_value.Substring(CamerMatrix_start_index + 1, CamerMatrix_end_index - CamerMatrix_start_index - 1);
            textBox1.Text += CamerMatrix_value + "\r\n" + "-----------------------------------------------------" + "\r\n";
            FileStream filereader1 = new FileStream((@matrix_path + "//" + "distCoeffs.txt"), FileMode.Open, FileAccess.Read);
            StreamReader streamReader1 = new StreamReader(filereader1);
            CamerMatrix_value = streamReader1.ReadToEnd();
            CamerMatrix_start_index = CamerMatrix_value.IndexOf("[");
            CamerMatrix_end_index = CamerMatrix_value.IndexOf("]");
            CamerMatrix_value = CamerMatrix_value.Substring(CamerMatrix_start_index + 1, CamerMatrix_end_index - CamerMatrix_start_index - 1);
            textBox1.Text += CamerMatrix_value + "\r\n" + "-----------------------------------------------------" + "\r\n";



        }
    }
}
