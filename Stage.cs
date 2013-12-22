using System;
using System.Collections.Generic;
using System.Diagnostics;
using SFML.Window;
using SFML.Graphics;

namespace SpriteNovel
{
    static class Stage
    {
        public static readonly string WindowTitle = "Story of Logs";
        public static readonly uint ScreenWidth   = 300;
        public static readonly uint ScreenHeight  = 200;
        public static readonly uint ScreenScale   = 2;
        public static readonly uint WindowWidth   = ScreenWidth * ScreenScale;
        public static readonly uint WindowHeight  = ScreenHeight * ScreenScale;

        public static readonly Dictionary<string, CharacterSetting> CharacterSettings 
			= new Dictionary<string, CharacterSetting> {
            { "bustin", new CharacterSetting { TextColor = new Color(215, 40, 40) } },
            { "lamber", new CharacterSetting { TextColor = new Color(60, 70, 140) } },
            { "mom",    new CharacterSetting { TextColor = Color.Blue } },
            { "dad",    new CharacterSetting { TextColor = Color.Blue } }
        };

        public static void Start()
        {

            // Resource loading (should probably be done before
            // the window is created)

            var audioRes = new AudioResources();
            var cursorTexture = new Texture("resources/cursor.png");


            var window = new RenderWindow(
                new VideoMode(WindowWidth, WindowHeight),
                WindowTitle,
                Styles.Close);
            window.SetMouseCursorVisible(false);

            var canvas = new RenderTexture(
                ScreenWidth, ScreenHeight);

            canvas.Clear(new Color(127, 127, 127));
            DrawScaled(window, canvas);

            var director = new Director(LoadScriptTree());

            var animatedText = new AnimatedWrappedText(
                director.GetDirective("dialogue").Value,
                Textbox.TextFont,
                Textbox.FontSize,
                ScreenWidth - Textbox.BorderWidth * 2);
            animatedText.TextAppearanceSpeed = AnimatedWrappedText.DefaultSpeed;
            animatedText.StartAnimation();

            var textbox = new Textbox(animatedText);

            var cursor = new Sprite(cursorTexture);
            cursor.Origin = new Vector2f((int)cursor.GetLocalBounds().Width / 2, 0);

//            var choiceTests = new List<string> {
//                "Yes, sir!",
//                "No, sir!"
//            };
//            var choiceDisplay = new ChoiceDisplay(choiceTests);

            var choiceDisplay = new ChoiceDisplay(new List<string>());

            Action AdvancementProgression = () => {

                if (director.GaveDirective("clear"))
                    animatedText.ClearAndRestart(director.GetDirective("dialogue").Value);
                else animatedText.FullText.AppendText(
                    director.GetDirective("dialogue").Value, 
                    true);
                animatedText.StartAnimation();

                if ((director.GaveDirective("character")) &&
                (director.GaveDirective("clear")))
                    textbox.TextColor = 
						CharacterSettings[director.GetDirective("character").Value].TextColor;

                if (director.Choices.Count > 0)
                {
                    choiceDisplay = new ChoiceDisplay(director.Choices);
                }

//                if ((director.GaveDirective("music") && 
//                audioRes.MusicDict[director.GetDirective("music").Value].Status 
//                        != SFML.Audio.SoundStatus.Playing))
//                    audioRes.MusicDict[director.GetDirective("music").Value].Play();

            };

            window.Closed += OnClose;
            window.MouseButtonPressed += (sender, e) => {
                if ((Mouse.GetPosition(window).Y / ScreenScale) >= Textbox.Position.Y) {
                    if ((!animatedText.AnimationActive) &&
                        (director.AdvanceOnce() == DirectorAdvancementStatus.Success)) {
                        audioRes.SoundDict["advance"].Play();
                        AdvancementProgression();
                    } else {
                        if (animatedText.AnimationActive) audioRes.SoundDict["advance"].Play();
                        animatedText.SkipAnimation();
                    }
                }

                int highlighted = choiceDisplay.CheckIfAnyHighlighted(cursor.Position);
                if (highlighted >= 0)
                {
                    audioRes.SoundDict["choose"].Play();
                    director.PlanChoice(highlighted);
                    director.AdvanceOnce();
                    choiceDisplay.Deactivate();
                    AdvancementProgression();
                }
            };

            AdvancementProgression();

            audioRes.SoundDict["text"].Play();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (window.IsOpen()) {
                stopwatch.Stop();
                double elapsedTime = stopwatch.Elapsed.TotalSeconds;
                stopwatch = Stopwatch.StartNew();

                window.DispatchEvents();

                animatedText.UpdateAnimation(elapsedTime);

                if ((!animatedText.AnimationActive) || (animatedText.MostRecentCharacter == ','))
                    audioRes.SoundDict["text"].Loop = false;
                else if (!audioRes.SoundDict["text"].Loop)
                {
                    audioRes.SoundDict["text"].Loop = true;
                    audioRes.SoundDict["text"].Play();
                }

                director.GaveDirective("yes");

                cursor.Position = window.MapPixelToCoords(Mouse.GetPosition(window));
                cursor.Position = window.MapPixelToCoords(
                    window.MapCoordsToPixel(cursor.Position) / (int)ScreenScale);

                choiceDisplay.CheckIfAnyHighlighted(cursor.Position);

                canvas.Clear(Color.Black);
                canvas.Draw(textbox);
                canvas.Draw(choiceDisplay);
                canvas.Draw(cursor);
                DrawScaled(window, canvas);
            }
        }

        static void DrawScaled(RenderWindow window, RenderTexture buffer)
        {
            buffer.Display();
            var screenSprite = new Sprite(buffer.Texture);
            screenSprite.Scale = new Vector2f(ScreenScale, ScreenScale);
            window.Draw(screenSprite);
            window.Display();
        }

        static ScriptTree LoadScriptTree()
        {
            Script scriptRoot = Script.Parse("n clear character=bustin music=wrunga  `Hello.` `What do you want to do today?` n `Shit on people?`");
            Script scriptA = Script.Parse("n clear `You want to run?` `Okay, let's run.`");
            Script scriptB = Script.Parse("n clear `Just walking?` `That's fine.` `What do you want to eat?`");
            Script scriptBA = Script.Parse("n clear  `I'm tasty.` `This could go very well.`");
            Script scriptBB = Script.Parse("n clear `I do like to eat you.` `That can work out.`");

            var scriptTreeRoot = new ScriptTree(scriptRoot);
            var scriptTreeA = new ScriptTree(scriptA);
            var scriptTreeB = new ScriptTree(scriptB);
            var scriptTreeBA = new ScriptTree(scriptBA);
            var scriptTreeBB = new ScriptTree(scriptBB);

            scriptTreeRoot.AddPath(new ScriptPath(scriptTreeA, "Run."));
            scriptTreeRoot.AddPath(new ScriptPath(scriptTreeB, "Walk."));
            scriptTreeB.AddPath(new ScriptPath(scriptTreeBA, "You."));
            scriptTreeB.AddPath(new ScriptPath(scriptTreeBB, "Me."));

            return scriptTreeRoot;
        }

        static void OnClose(object sender, EventArgs e)
        {
            var window = (RenderWindow)sender;
            window.Close();
        }
    }

    struct CharacterSetting
    {
        public Color TextColor;
    }
}

