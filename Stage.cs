using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using SFML.Window;
using SFML.Graphics;
using SharpCompress.Common;
using SharpCompress.Reader;

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
            { "dad",    new CharacterSetting { TextColor = Color.Blue } } };

        public static void Start()
        {
            var audioRes = new AudioResources();
            var cursorTexture = new Texture("resources/cursor.png");

            var canvas = new RenderTexture(
                ScreenWidth, ScreenHeight);

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

            var choiceDisplay = new ChoiceDisplay(new List<string>());

            Action AdvancementProgression = () => {

                if (director.GaveDirective("c"))
                    animatedText.ClearAndRestart(director.GetDirective("dialogue").Value);
                else animatedText.FullText.AppendText(
                    director.GetDirective("dialogue").Value, 
                    true);
                animatedText.StartAnimation();

                if ((director.GaveDirective("character")) &&
                (director.GaveDirective("c")))
                    textbox.TextColor = 
						CharacterSettings[director.GetDirective("character").Value].TextColor;

                if (director.Choices.Count > 0)
                    choiceDisplay = new ChoiceDisplay(director.Choices);

//                if ((director.GaveDirective("music") && 
//                audioRes.MusicDict[director.GetDirective("music").Value].Status 
//                        != SFML.Audio.SoundStatus.Playing))
//                    audioRes.MusicDict[director.GetDirective("music").Value].Play();
            };

            var window = new RenderWindow(
                new VideoMode(WindowWidth, WindowHeight),
                WindowTitle,
                Styles.Close);
            window.SetMouseCursorVisible(false);

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
                if (highlighted >= 0) {
                    audioRes.SoundDict["choose"].Play();
                    director.PlanChoice(highlighted);
                    director.AdvanceOnce();
                    choiceDisplay.Deactivate();
                    AdvancementProgression();
                }
            };

            AdvancementProgression();

            string soundSpeed = 
                (animatedText.TextAppearanceSpeed >= 50)
                ? "text" : "text-slower";
            audioRes.SoundDict[soundSpeed].Play();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (window.IsOpen()) {

                stopwatch.Stop();
                double elapsedTime = stopwatch.Elapsed.TotalSeconds;
                stopwatch = Stopwatch.StartNew();

                window.DispatchEvents();

                animatedText.UpdateAnimation(elapsedTime);

                if ((!animatedText.AnimationActive) || 
                    (animatedText.MostRecentCharacter == ','))
                    audioRes.SoundDict[soundSpeed].Loop = false;
                else if (!audioRes.SoundDict[soundSpeed].Loop) {
                    audioRes.SoundDict[soundSpeed].Loop = true;
                    audioRes.SoundDict[soundSpeed].Play();
                }

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
            var scripts = new Dictionary<string, Script>();

            using (Stream stream = File.OpenRead("resources/script.zip")) {
                var reader = ReaderFactory.Open(stream);
                while ((reader.MoveToNextEntry()) &&
                    (!reader.Entry.IsDirectory)) {
                    EntryStream entryStream = reader.OpenEntryStream();
                    using (TextReader textReader = new StreamReader(entryStream))
                    {
                        string scriptName = Path.GetFileName(reader.Entry.FilePath);
                        var contents = Script.Parse(textReader.ReadToEnd());
                        scripts.Add(scriptName, contents);
                    }
                }
            }

            var scriptTrees = new Dictionary<string, ScriptTree>();
            foreach (KeyValuePair<string, Script> s in scripts) {
                scriptTrees.Add(s.Key, new ScriptTree(s.Value));
            }

            scriptTrees["common"].AddPath(new ScriptPath(
                scriptTrees["path-A-common"], 
                "Option 1"));
            scriptTrees["common"].AddPath(new ScriptPath(
                scriptTrees["path-BC-common"], 
                "Option 2"));

            scriptTrees["path-A-common"].AddPath(new ScriptPath(
                scriptTrees["path-A-bad-ending"], 
                "Option 1"));
            scriptTrees["path-A-common"].AddPath(new ScriptPath(
                scriptTrees["path-A-good-ending"], 
                "Option 2"));

            scriptTrees["path-BC-common"].AddPath(new ScriptPath(
                scriptTrees["path-B-common"], 
                "Option 1"));
            scriptTrees["path-BC-common"].AddPath(new ScriptPath(
                scriptTrees["path-C-common"], 
                "Option 2"));

            scriptTrees["path-B-common"].AddPath(new ScriptPath(
                scriptTrees["path-B-bad-ending"], 
                "Option 1"));
            scriptTrees["path-B-common"].AddPath(new ScriptPath(
                scriptTrees["path-B-good-ending"], 
                "Option 2"));

            scriptTrees["path-C-common"].AddPath(new ScriptPath(
                scriptTrees["path-C-bad-ending"], 
                "Option 1"));
            scriptTrees["path-C-common"].AddPath(new ScriptPath(
                scriptTrees["path-C-good-ending"], 
                "Option 2"));

            return scriptTrees["common"];
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

