using System;
using SFML.Window;
using SFML.Graphics;

namespace SpriteNovel
{
	static class Stage
	{
		public static readonly string WindowTitle = "Story of Logs";
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

			RenderTexture canvas = new RenderTexture(
				ScreenWidth, ScreenHeight);


			var director = new Director(LoadScriptTree());

			var animatedText = new AnimatedWrappedText(
				director.GetDirective("dialogue").Value,
				Textbox.TextFont,
				Textbox.FontSize,
				ScreenWidth - Textbox.BorderWidth * 2);
			animatedText.TextAppearanceSpeed = 80;
			animatedText.StartAnimation();

			var textbox = new Textbox(animatedText);

			window.Closed += OnClose;
			window.MouseButtonPressed += (sender, e) => {
				if (!animatedText.AnimationActive) {
					if (director.AdvanceOnce() == DirectorStatus.Success) {
						if (director.GaveDirective("clear"))
							animatedText.ClearAndRestart(director.GetDirective("dialogue").Value);
						else animatedText.FullText.AppendText(
							director.GetDirective("dialogue").Value, true);
						animatedText.StartAnimation();
					}
				}
			};
		
			DateTime now = DateTime.Now;

			while (window.IsOpen())
			{
				double elapsedTime = (DateTime.Now - now).TotalSeconds;
				now = DateTime.Now;

				window.DispatchEvents();

				animatedText.UpdateAnimation(elapsedTime);

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

		static ScriptTree LoadScriptTree()
		{
			Script scriptRoot = Script.Parse("n  music=lovely  `Hello.` `What do you want to do today?` n `Shit on people?` `The biggest difference in the 2nd edition was redefining the target audience.` `Remember I said that the first edition was intended for advanced and beginning users?` `Well, now there were four competing books on the market.`");
			Script scriptA    = Script.Parse("n `You want to run? Okay, let's run.`");
			Script scriptB    = Script.Parse("n `Yeah? Just walking? That's fine. What do you want to eat?` `Shit?`");
			Script scriptBA   = Script.Parse("n `I'm tasty.`");
			Script scriptBB   = Script.Parse("n `I do like to eat you.`");

			var scriptTreeRoot  = new ScriptTree(scriptRoot);
			var scriptTreeA 	= new ScriptTree(scriptA);
			var scriptTreeB 	= new ScriptTree(scriptB);
			var scriptTreeBA    = new ScriptTree(scriptBA);
			var scriptTreeBB    = new ScriptTree(scriptBB);

			scriptTreeRoot.AddPath(new ScriptPath(scriptTreeA, "Run."));
			scriptTreeRoot.AddPath(new ScriptPath(scriptTreeB, "Walk."));
			scriptTreeB.AddPath(new ScriptPath(scriptTreeBA, "You."));
			scriptTreeB.AddPath(new ScriptPath(scriptTreeBB, "Me."));

			return scriptTreeRoot;
		}
		
		static void OnClose(object sender, EventArgs e)
		{
			RenderWindow window = (RenderWindow)sender;
			window.Close();
		}
	}
}

