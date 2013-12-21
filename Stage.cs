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

			var testText = new AnimatedWrappedText(
				"She made her way to the bathroom, where she washed her face and hands, and fixed her hair. When she was done she sat on the sink and massaged some moisturiser into her tired hands. Then, while she was waiting for it to suck into her skin, she smoked, opening a window and positioning the mirrors so they were parallel to each other, and the smoke became infinite.", 
				Textbox.TextFont, Textbox.FontSize, ScreenWidth - Textbox.BorderWidth * 2);
			testText.TextAppearanceSpeed = 50;

			var textbox = new Textbox(testText);

			testText.StartAnimation();

			DateTime now = DateTime.Now;

			while (window.IsOpen())
			{
				double elapsedTime = (DateTime.Now - now).TotalSeconds;
				now = DateTime.Now;

				window.DispatchEvents();

				testText.UpdateAnimation(elapsedTime);

				canvas.Clear(Color.Black);
				canvas.Draw(textbox);
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

