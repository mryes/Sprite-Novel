using System;
using SFML.Window;
using SFML.Graphics;

namespace SpriteNovel
{
	static class Stage
	{
		public static readonly string WindowTitle = "Sprite Novel";
		public static readonly uint ScreenWidth  = 300;
		public static readonly uint ScreenHeight = 200;
		public static readonly uint ScreenScale  = 2;
		public static readonly uint WindowWidth  = ScreenWidth * ScreenScale;
		public static readonly uint WindowHeight = ScreenHeight * ScreenScale;

		public static void Start()
		{
			RenderWindow window = new RenderWindow(
				new VideoMode(WindowWidth, WindowHeight),
				WindowTitle,
				Styles.Close);

			window.Closed += OnClose;

			RenderTexture canvas = new RenderTexture(
				ScreenWidth, ScreenHeight);

			while (window.IsOpen())
			{
				window.DispatchEvents();

				canvas.Clear(Color.Black);
				DrawScaled(window, canvas);

			}
		}

		static void DrawScaled(RenderWindow window, RenderTexture buffer)
		{
			buffer.Display();
			Sprite screenSprite = new Sprite(buffer.Texture);
			screenSprite.Scale  = new Vector2f(ScreenScale, ScreenScale);
			window.Draw(screenSprite);
			window.Display();
		}

		static void OnClose(object sender, EventArgs e)
		{
			RenderWindow window = (RenderWindow)sender;
			window.Close();
		}

	}
}

