using Screensaver.Models;
using Timer = System.Windows.Forms.Timer;

namespace Screensaver
{
    /// <summary>
    /// Окно
    /// </summary>
    public partial class ScreensaverForm : Form
    {
        private Bitmap backgroundBitmap;
        private Bitmap overlayBitmap;
        private Bitmap bufferBitmap;
        private Graphics bufferGraphics;

        private const int GraphicsStartPositionX = 0;
        private const int GraphicsStartPositionY = 0;
        private const int TimerInterval = 50;
        private const int SnowflakeCount = 100;
        private const int MinSnowflakeSize = 18;
        private const int MaxSnowflakeSize = 40;
        private const float MinSnowflakeSpeed = 4.0f;
        private const float MaxSnowflakeSpeed = 12.0f;
        private const float HorizontalMovementFactor = 5.0f;

        private List<Snowflake> snowflakes = [];
        private Random random = new();
        private Timer timer;

        /// <summary>
        /// ctor
        /// </summary>
        public ScreensaverForm()
        {
            InitializeComponent();

            backgroundBitmap = new Bitmap(Properties.Resources.background);
            overlayBitmap = new Bitmap(Properties.Resources.snejinka);

            timer = new()
            {
                Interval = TimerInterval
            };
            timer.Tick += Timer_Tick;
        }

        private void FormShown(object sender, EventArgs e)
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
            var width = ClientSize.Width;
            var height = ClientSize.Height;

            for (var flakeCounter = 0; flakeCounter < SnowflakeCount; flakeCounter++)
            {
                var size = random.Next(MinSnowflakeSize, MaxSnowflakeSize + 1);

                var sizeRatio = (float)(size - MinSnowflakeSize) / (MaxSnowflakeSize - MinSnowflakeSize);
                var speed = MinSnowflakeSpeed + sizeRatio * (MaxSnowflakeSpeed - MinSnowflakeSpeed);

                snowflakes.Add(new Snowflake
                {
                    X = random.Next(GraphicsStartPositionX, width),
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
                formGraphics.DrawImage(bufferBitmap, GraphicsStartPositionX, GraphicsStartPositionY);
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
                {
                    flake.X = width + flake.Size;
                }
                else if (flake.X > width + teleportThreshold)
                {
                    flake.X = -flake.Size;
                }
            }
        }

        private void DrawToBuffer()
        {
            var width = ClientSize.Width;
            var height = ClientSize.Height;

            bufferGraphics.DrawImage(backgroundBitmap, GraphicsStartPositionX, GraphicsStartPositionY, width, height);
            foreach (var flake in snowflakes)
            {
                if (flake.Y + flake.Size >= GraphicsStartPositionY && flake.Y <= height &&
                    flake.X + flake.Size >= GraphicsStartPositionX && flake.X <= width)
                {
                    bufferGraphics.DrawImage(overlayBitmap, flake.X, flake.Y, flake.Size, flake.Size);
                }
            }
        }

        private void CloseForm(object sender, FormClosingEventArgs e)
        {
            timer.Stop();
        }

        private void ScreensaverForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            Close();
        }

        private void ScreensaverForm_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}