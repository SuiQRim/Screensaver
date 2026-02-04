using Screensaver.Models;
using Timer = System.Windows.Forms.Timer;

namespace Screensaver
{
    /// <summary>
    /// Окно
    /// </summary>
    public partial class ScreensaverWindow : Form
    {
        private Bitmap backgroundBitmap;
        private Bitmap overlayBitmap;
        private Bitmap bufferBitmap;
        private Graphics bufferGraphics;

        private string backGroundPath = Path.Combine(Application.StartupPath, "Resources", "background.jpg");
        private string snowflakePath = Path.Combine(Application.StartupPath, "Resources", "snejinka.png");

        private const int TimerInterval = 50;
        private const int SnowflakeCount = 100;
        private const int MinSnowflakeSize = 18;
        private const int MaxSnowflakeSize = 40;
        private const float MinSnowflakeSpeed = 4.0f;
        private const float MaxSnowflakeSpeed = 12.0f;
        private const float HorizontalMovementFactor = 5.0f;

        private List<Snowflake> snowflakes = new List<Snowflake>();
        private Random random = new Random();
        private Timer timer;

        /// <summary>
        /// ctor
        /// </summary>
        public ScreensaverWindow()
        {
            InitializeComponent();

            backgroundBitmap = new Bitmap(backGroundPath);
            overlayBitmap = new Bitmap(snowflakePath);

            Load += Form1_Load;

            KeyDown += (_, _) => Close();
            MouseClick += (_, _) => Close();

            timer = new Timer();
            timer.Interval = TimerInterval;
            timer.Tick += Timer_Tick; 
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeBuffer();
            InitializeSnowflakes();
            timer.Start();
        }

        private void InitializeBuffer()
        {
            bufferBitmap = new Bitmap(ClientSize.Width, ClientSize.Height);
            bufferGraphics = Graphics.FromImage(bufferBitmap);
        }

        private void InitializeSnowflakes()
        {
            snowflakes.Clear();
            var width = ClientSize.Width;
            var height = ClientSize.Height;

            for (var flakeCouter = 0; flakeCouter < SnowflakeCount; flakeCouter++)
            {
                var size = random.Next(MinSnowflakeSize, MaxSnowflakeSize + 1);

                var sizeRatio = (float)(size - MinSnowflakeSize) / (MaxSnowflakeSize - MinSnowflakeSize);
                var speed = MinSnowflakeSpeed + sizeRatio * (MaxSnowflakeSpeed - MinSnowflakeSpeed);

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
            UpdateSnowflakes();
            DrawToBuffer();

            using (Graphics formGraphics = CreateGraphics())
            {
                formGraphics.DrawImage(bufferBitmap, 0, 0);
            }
        }

        private void UpdateSnowflakes()
        {
            var width = ClientSize.Width;
            var height = ClientSize.Height;


            foreach (var flake in snowflakes)
            {
                flake.Y += (int)Math.Round(flake.Speed);

                var horizontalFactor = (float)flake.Size / MaxSnowflakeSize;
                var horizontalRange = (int)(HorizontalMovementFactor * horizontalFactor);
                flake.X += random.Next(-horizontalRange, horizontalRange + 1);

                if (flake.Y > height)
                {
                    flake.Y = -flake.Size;

                    var horizontalOffset = flake.Size;
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
            var width = ClientSize.Width;
            var height = ClientSize.Height;

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