using Screensaver.Models;
using Timer = System.Windows.Forms.Timer;

namespace Screensaver
{
    public partial class ScreensaverWindow : Form
    {
        private Bitmap backgroundBitmap;
        private Bitmap overlayBitmap;
        private Bitmap bufferBitmap;
        private Graphics bufferGraphics;

        private string backGroundPath = Path.Combine(Application.StartupPath, "Resources", "background.jpg");
        private string snowflakePath = Path.Combine(Application.StartupPath, "Resources", "snejinka.png");

        private const int TIMER_INTERVAL = 50;
        private const int SNOWFLAKE_COUNT = 100;
        private const int MIN_SIZE = 18;
        private const int MAX_SIZE = 40;
        private const float MIN_SPEED = 4.0f;
        private const float MAX_SPEED = 12.0f;
        private const float HORIZONTAL_MOVEMENT_FACTOR = 5.0f;

        private List<Snowflake> snowflakes = new List<Snowflake>();
        private Random random = new Random();
        private Timer timer;
        private bool initialized = false;

        public ScreensaverWindow()
        {
            InitializeComponent();

            backgroundBitmap = new Bitmap(backGroundPath);
            overlayBitmap = new Bitmap(snowflakePath);

            this.Load += Form1_Load;

            this.KeyDown += (sender, e) => this.Close();
            this.MouseClick += (sender, e) => this.Close();

            timer = new Timer();
            timer.Interval = TIMER_INTERVAL;
            timer.Tick += Timer_Tick; 
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeBuffer();
            InitializeSnowflakes();
            timer.Start();
            initialized = true;
        }

        private void InitializeBuffer()
        {
            bufferBitmap = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
            bufferGraphics = Graphics.FromImage(bufferBitmap);
        }

        private void InitializeSnowflakes()
        {
            snowflakes.Clear();
            int width = this.ClientSize.Width;
            int height = this.ClientSize.Height;

            for (int i = 0; i < SNOWFLAKE_COUNT; i++)
            {
                int size = random.Next(MIN_SIZE, MAX_SIZE + 1);

                float sizeRatio = (float)(size - MIN_SIZE) / (MAX_SIZE - MIN_SIZE);
                float speed = MIN_SPEED + sizeRatio * (MAX_SPEED - MIN_SPEED);

                snowflakes.Add(new Snowflake
                {
                    X = random.Next(0, width),
                    Y = random.Next(-height, height),
                    Size = size,
                    Speed = speed
                });
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!initialized) return;

            UpdateSnowflakes();
            DrawToBuffer();

            using (Graphics formGraphics = this.CreateGraphics())
            {
                formGraphics.DrawImage(bufferBitmap, 0, 0);
            }
        }

        private void UpdateSnowflakes()
        {
            int width = this.ClientSize.Width;
            int height = this.ClientSize.Height;


            foreach (var flake in snowflakes)
            {
                flake.Y += (int)Math.Round(flake.Speed);

                float horizontalFactor = (float)flake.Size / MAX_SIZE;
                int horizontalRange = (int)(HORIZONTAL_MOVEMENT_FACTOR * horizontalFactor);
                flake.X += random.Next(-horizontalRange, horizontalRange + 1);

                if (flake.Y > height)
                {
                    flake.Y = -flake.Size;

                    int horizontalOffset = flake.Size;
                    flake.X = random.Next(-horizontalOffset, width + horizontalOffset);
                }

                int teleportThreshold = flake.Size;
                if (flake.X < -teleportThreshold)
                    flake.X = width + flake.Size;
                else if (flake.X > width + teleportThreshold)
                    flake.X = -flake.Size;
            }
        }

        private void DrawToBuffer()
        {
            int width = this.ClientSize.Width;
            int height = this.ClientSize.Height;

            bufferGraphics.DrawImage(backgroundBitmap, 0, 0, width, height);
            foreach (var flake in snowflakes)
            {
                if (flake.Y + flake.Size >= 0 && flake.Y <= height &&
                    flake.X + flake.Size >= 0 && flake.X <= width)
                {
                    bufferGraphics.DrawImage(overlayBitmap, flake.X, flake.Y, flake.Size, flake.Size);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (bufferBitmap != null)
            {
                e.Graphics.DrawImage(bufferBitmap, 0, 0);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            timer?.Stop();
            base.OnFormClosing(e);
        }
    }
}