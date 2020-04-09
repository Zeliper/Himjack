using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Timers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using WindowsInput.Native;
using WindowsInput;

namespace HimJack
{
    public partial class Main : Form
    {
        Bitmap Img_MainMenu = new Bitmap(@"img\MainMenu.png");
        Bitmap Img_Skills = new Bitmap(@"img\Skills.png");
        Bitmap Img_Que = new Bitmap(@"img\Que.png");
        Bitmap Img_Ingame = new Bitmap(@"img\InGame.png");
        Bitmap Img_EndGame = new Bitmap(@"img\EndGame.png");
        Bitmap Img_Program = new Bitmap(@"img\Program.png");
        InputSimulator Keymac = new InputSimulator();
        Thread T;
        OpenCvSharp.Point FoundedLocation;
        public Main()
        {

            InitializeComponent();
            System.Windows.Forms.Timer T1 = new System.Windows.Forms.Timer();
            T1.Tick += T_Tick;
            T1.Interval = 250;
            T1.Start();
        }

        private void T_Tick(object sender, EventArgs e)
        {
            label4.Text = Cursor.Position.ToString();
        }

        int GameCount = 0;
        [DllImport("user32")]
        public static extern int SetCursorPos(int x, int y);
        private const int MOUSEEVENTF_MOVE = 0x0001; /* mouse move */
        private const int MOUSEEVENTF_LEFTDOWN = 0x0002; /* left button down */
        private const int MOUSEEVENTF_LEFTUP = 0x0004; /* left button up */
        [DllImport("user32",CharSet = CharSet.Auto, CallingConvention= CallingConvention.StdCall)]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons,int dwExtraInfo);

        private static void delay(int Time_delay)
        {
            /*int i = 0;
            //  ameTir = new System.Timers.Timer();
            System.Timers.Timer _delayTimer = new System.Timers.Timer();
            _delayTimer.Interval = Time_delay;
            _delayTimer.AutoReset = false; //so that it only calls the method once
            _delayTimer.Elapsed += (s, args) => i = 1;
            _delayTimer.Start();
            while (i == 0) { };*/
            Thread.Sleep(Time_delay);
        }

        public double SearchIMG(Bitmap screen_img, Bitmap find_img)
        {
            try
            {
                //스크린 이미지 선언
                using (Mat ScreenMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(screen_img))
                //찾을 이미지 선언
                using (Mat FindMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(find_img))
                //스크린 이미지에서 FindMat 이미지를 찾아라
                using (Mat res = ScreenMat.MatchTemplate(FindMat, TemplateMatchModes.CCoeffNormed))
                {
                    //찾은 이미지의 유사도를 담을 더블형 최대 최소 값을 선언합니다.
                    double minval, maxval = 0;
                    //찾은 이미지의 위치를 담을 포인트형을 선업합니다.
                    OpenCvSharp.Point minloc, maxloc;
                    //찾은 이미지의 유사도 및 위치 값을 받습니다. 
                    Cv2.MinMaxLoc(res, out minval, out maxval, out minloc, out maxloc);
                    FoundedLocation = maxloc;
                    if (maxval > 0.8)
                    { //같을경우
                        Bitmap B = screen_img;
                        Graphics g = Graphics.FromImage(B as Image);
                        g.DrawRectangle(new Pen(Color.Red),new Rectangle(new System.Drawing.Point(maxloc.X,maxloc.Y),find_img.Size));
                        SetImage(pictureBox1,B);
                    }
                    //MessageBox.Show("찾은 이미지의 유사도 : " + maxval);
                    return maxval;
                }
            }
            catch
            {
                return 0;
            }
        }

        public Bitmap ScreenCapture()
        {
            Bitmap output = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics g = Graphics.FromImage(output as Image);
            g.CopyFromScreen(0, 0, 0, 0, output.Size);
            return output;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            T = new Thread(TestThread);
            T.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            T = new Thread(AutoHimjak);
            T.Start();
        }


        private void TestThread()
        {
            while (true)
            {
                delay(30);
                CSafeSetText((Control)label1, "X : " + Cursor.Position.X + Environment.NewLine + "Y : " + Cursor.Position.Y);
            }
        }

        bool isInGame = false;
        private void AutoHimjak()
        {
            delay(2000);
            int SNumber;
            bool KeyDown = false;
            int KeyState = 0;
            int TimeCount = 0;
            int QueLimit = 0;
            bool CheckSkill = false;
            while (true)
            {
                delay(1500);
                SNumber = GetScreenNumber();
                switch (SNumber)
                {
                    case 0: //웅엥웅
                        CSafeSetText((Control)label1, "로딩중...");
                        break;
                    case 1: //메인화면
                        QueLimit=0;
                        isInGame = false;
                        CSafeSetText((Control)label1, "메인화면");
                        MClick(970, 630);
                        delay(1500);
                        MClick(970, 950);
                        delay(1500);
                        MClick(676, 333);
                        delay(1500);
                        MClick(1210, 995);
                        delay(1500);
                        break;
                    case 2: //큐잡는중
                        CSafeSetText((Control)label1, "큐잡는중");
                        QueLimit++;
                        if (QueLimit > 800) //게임종료 (20분동안)
                        {
                            CSafeSetText((Control)label1, "게임 종료중");
                            QueLimit = 0;
                            Keymac.Keyboard.KeyDown(VirtualKeyCode.MENU);
                            Keymac.Keyboard.KeyPress(VirtualKeyCode.F4);
                            Keymac.Keyboard.KeyUp(VirtualKeyCode.MENU);
                        }
                        //Keymac.Keyboard.KeyPress(VirtualKeyCode.ESCAPE); ESC 누르기
                        break;
                    case 3: //인게임
                        QueLimit = 0;
                        if (!isInGame)
                        {
                            GameCount++;
                            CSafeSetText((Control)label3, GameCount.ToString());
                            isInGame = true;
                        }
                        if (!KeyDown)
                        {
                            CSafeSetText((Control)label1, "인게임");
                            //W키 다운으로 만드는거 추가
                            Keymac.Keyboard.KeyDown(VirtualKeyCode.VK_W);
                            KeyState = 0;
                            KeyDown = true;
                        }
                        else if(TimeCount >= 1)
                        {
                            switch (KeyState)
                            {
                                case 0: //w키가 눌러지고있을때
                                    SetCursorPos(1920, 540);
                                    Keymac.Keyboard.KeyUp(VirtualKeyCode.VK_W);
                                    Keymac.Keyboard.KeyDown(VirtualKeyCode.VK_A);
                                    KeyState = 1;
                                    if (CheckSkill)
                                    {
                                        Keymac.Keyboard.KeyPress(VirtualKeyCode.TAB);
                                        delay(1500);
                                        MClick(477, 21);
                                        delay(1000);
                                        Keymac.Keyboard.KeyPress(VirtualKeyCode.TAB);
                                        delay(1000);
                                        Keymac.Keyboard.KeyPress(VirtualKeyCode.TAB);
                                        delay(1000);
                                        Keymac.Keyboard.KeyPress(VirtualKeyCode.TAB);
                                        CheckSkill = false;
                                    }
                                    else
                                    {
                                        CheckSkill = true;
                                    }
                                    break;
                                case 1: //a키가 눌러지고있을때
                                    SetCursorPos(1920, 540);
                                    Keymac.Keyboard.KeyUp(VirtualKeyCode.VK_A);
                                    Keymac.Keyboard.KeyDown(VirtualKeyCode.VK_S);
                                    KeyState = 2;
                                    break;
                                case 2: //a키가 눌러지고있을때
                                    SetCursorPos(1920, 540);
                                    Keymac.Keyboard.KeyUp(VirtualKeyCode.VK_S);
                                    Keymac.Keyboard.KeyDown(VirtualKeyCode.VK_D);
                                    KeyState = 3;
                                    break;
                                case 3: //a키가 눌러지고있을때
                                    SetCursorPos(1920, 540);
                                    Keymac.Keyboard.KeyUp(VirtualKeyCode.VK_D);
                                    Keymac.Keyboard.KeyDown(VirtualKeyCode.VK_W);
                                    KeyState = 0;
                                    break;
                            }
                            TimeCount = 0;
                        }
                        break;
                    case 4: //엔드게임
                        Keymac.Keyboard.KeyUp(VirtualKeyCode.VK_W);
                        KeyDown = false;
                        CSafeSetText((Control)label1, "엔드게임");
                        MClick(960, 957);
                        delay(1500);
                        MClick(960, 957);
                        delay(1500);
                        MClick(960, 957);
                        delay(1500);
                        MClick(960, 957);
                        delay(1500);
                        break;
                    case 5: //프로그램 종료됬을때
                        CSafeSetText((Control)label1, "게임 실행중");
                        delay(2000);
                        MClick(1300, 900);
                        delay(60000);
                        break;
                    case 6:
                        delay(1000);
                        MClick(1800, 30);
                        break;
                }
                GC.Collect();
                TimeCount++;
            }
        }
        public void MClick(int x, int y)
        {
            SetCursorPos(x, y);
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }


        public int GetScreenNumber()
        {
            int output = 0;
            double Cvalue = SearchIMG(ScreenCapture(), Img_MainMenu);
            if (Cvalue > 0.8)
                output = 1;
            Cvalue = SearchIMG(ScreenCapture(), Img_EndGame);
            if (Cvalue > 0.8)
                output = 4;
            Cvalue = SearchIMG(ScreenCapture(), Img_Ingame);
            if (Cvalue > 0.8)
                output = 3;
            Cvalue = SearchIMG(ScreenCapture(), Img_Que);
            if (output != 4 & Cvalue > 0.8)
                output = 2;
            Cvalue = SearchIMG(ScreenCapture(), Img_Program);
            if (Cvalue > 0.8)
                output = 5;
            Cvalue = SearchIMG(ScreenCapture(), Img_Skills);
            if (Cvalue > 0.8)
                output = 6;


            return output;
        }



        delegate void CrossThreadSafetySetText(Control ctl, String text);
        delegate void CrossSetImage(PictureBox ctl, Bitmap B);

        private void SetImage(PictureBox ctl, Bitmap B)        {
            if (ctl.InvokeRequired)
                ctl.Invoke(new CrossSetImage(SetImage), ctl, B);
            else
                ctl.Image = B;
        }

        private void CSafeSetText(Control ctl, String text)
        {

            /*
             * InvokeRequired 속성 (Control.InvokeRequired, MSDN)
             *   짧게 말해서, 이 컨트롤이 만들어진 스레드와 현재의 스레드가 달라서
             *   컨트롤에서 스레드를 만들어야 하는지를 나타내는 속성입니다.  
             * 
             * InvokeRequired 속성의 값이 참이면, 컨트롤에서 스레드를 만들어 텍스트를 변경하고,
             * 그렇지 않은 경우에는 그냥 변경해도 아무 오류가 없기 때문에 텍스트를 변경합니다.
             */
            if (ctl.InvokeRequired)
                ctl.Invoke(new CrossThreadSafetySetText(CSafeSetText), ctl, text);
            else
                ctl.Text = text;
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            T.Abort();
        }

        private void Main_Load(object sender, EventArgs e)
        {

        }
    }
}
