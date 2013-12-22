using System;
using System.Collections.Generic;
using System.Linq;
using SFML.Window;
using SFML.Graphics;

namespace SpriteNovel
{
    struct WrappedText
    {
        public List<string> Strings { get; set; }

        public string FlatString { 
            get {
                string flatString = "";
                foreach (string str in Strings)
                    flatString += str + "\n";
                return flatString.TrimEnd('\n');
            }
        }

        public Font FontReference { get; private set; }

        public uint FontReferenceSize { get; private set; }

        public uint WrapWidth { get; private set; }

        public int  EndPosition {
            get {
                int runningTotal = 0;
                foreach (string str in Strings)
                    runningTotal += str.Length;
                return runningTotal - 1;
            }
        }

        public WrappedText(string rawStr, Font font, uint fontSize, uint width) : this()
        {
            rawText = rawStr;
            FontReference = font;
            FontReferenceSize = fontSize;
            WrapWidth = width;
            Strings = new List<string>();
            CreateTextStrings();
        }

        public void ChangeText(string rawStr)
        {
            rawText = rawStr;
            Strings = new List<string>();
            CreateTextStrings();
        }

        public void AppendText(string str, bool space)
        {
            if (space)
                str = " " + str;
            CreateTextStrings(str);
        }

        public char GetCharacterAtPosition(int position)
        {
            if (position <= EndPosition) {
                int row = 0;
                int column = 0;
                foreach (string str in Strings) {
                    if ((column + str.Length) <= position) {
                        column += str.Length;
                        ++row;
                    } else {
                        column = position - column;
                        break;
                    }
                }
                return Strings[row][column];
            }
            return '\0';
        }

        public void RemoveLine(int line)
        {
            Strings.RemoveAt(line);
        }

        public List<string> StringHead(int characters)
        {
            var newWrappedText = new List<string>();

            if (characters == 0)
                return newWrappedText;
            if (characters <= EndPosition + 1) {
                int row = 1;
                int column = 1;
                foreach (string str in Strings) {
                    if ((column + str.Length) <= characters) {
                        ++row;
                        characters -= str.Length;
                    } else {
                        column = characters;
                        break;
                    }
                }

                var newStrings = new List<string>();
                for (int i = 0; i < row; i++)
                    newStrings.Add(string.Copy(Strings[i]));
					
                if (column > 0)
                    newStrings[row - 1] = Strings[row - 1].Substring(0, column);

                return newStrings;
            }

            throw new ArgumentException();
        }

        string rawText;

        void CreateTextStrings(string stringToAdd = "")
        {
            var words = (stringToAdd == "")
            	? rawText.Split(' ').ToList()
            	: stringToAdd.Split(' ').ToList();

            string line;
            if (Strings.Count > 0) {
                line = Strings.Last();
                Strings.RemoveAt(Strings.Count - 1);
            } else line = "";

            while (words.Count > 0) {

                int wordsAdded = 0;
                foreach (string word in words) {
                    string testLine = line + word + " ";
                    int calculatedWidth = CalculateWidthOfString(testLine);
                    if (calculatedWidth > WrapWidth) {
                        // Maybe it will fit without the extra space added
                        if ((calculatedWidth - CalculateWidthOfString(" ")) > WrapWidth)
                            break;
                        testLine = testLine.TrimEnd(' ');
                    }
                    line = testLine;
                    ++wordsAdded;
                }

                words.RemoveRange(0, wordsAdded);
                line = line.TrimEnd(' ');
                Strings.Add(line);
                line = "";
            }
        }

        int CalculateWidthOfString(string str)
        {
            return (int)(new Text(str, FontReference, FontReferenceSize).GetLocalBounds().Width);
        }
    }

    class AnimatedWrappedText
    {
        public static readonly int InstantSpeed = -1;
        public static readonly int DefaultSpeed = 50;
        WrappedText fullText;

        public WrappedText FullText { 
            get { return fullText; } 

            set {
                if (!AnimationActive)
                    fullText = value;
            }
        }

        public WrappedText VisibleText { get; private set; }

        public double TextAppearanceSpeed { get; set; }

        public bool AnimationActive { get; private set; }

        public char MostRecentCharacter { get; private set; }

        public AnimatedWrappedText(string rawStr, Font font, uint fontsize, uint width)
        {
            AnimationActive = false;
            FullText = new WrappedText(rawStr, font, fontsize, width);
            VisibleText = new WrappedText("", font, fontsize, width);

            invisibleCharacters = FullText.EndPosition + 1;
        }

        public void StartAnimation()
        {
            AnimationActive = true;
            timeUntilNextCharacter = 0;
            invisibleCharacters = (FullText.EndPosition + 1)
                                - (VisibleText.EndPosition + 1);
        }

        public void UpdateAnimation(double elapsedTime)
        {
            if (AnimationActive) {
                if (TextAppearanceSpeed > InstantSpeed) {
                    timeUntilNextCharacter -= elapsedTime;
                    if (timeUntilNextCharacter <= 0) {
                        if (invisibleCharacters > 0) {
                            AddCharacter();
                            // Add multiple characters, if appearance speed calls for it
                            while (((timeUntilNextCharacter + (1 / TextAppearanceSpeed)) < 0) &&
                                (MostRecentCharacter != ',')) {
                                timeUntilNextCharacter += (1 / TextAppearanceSpeed);
                                AddCharacter();
                            }
                        } else AnimationActive = false;
                        timeUntilNextCharacter = (1 / TextAppearanceSpeed) 
                                                 + ((MostRecentCharacter != ',') ? timeUntilNextCharacter : 0);
                        if (MostRecentCharacter == ',')
                            timeUntilNextCharacter *= commaPause;
                    }
                } else SkipAnimation();
            }
        }

        public void SkipAnimation()
        {
            timeUntilNextCharacter = 0;
            while (invisibleCharacters > 0)
                AddCharacter();
            AnimationActive = false;
        }

        public void ClearAndRestart(string rawStr)
        {
            FullText = new WrappedText(
                rawStr, 
                FullText.FontReference, 
                FullText.FontReferenceSize, 
                FullText.WrapWidth);
            VisibleText = new WrappedText(
                "", 
                FullText.FontReference, 
                FullText.FontReferenceSize, 
                FullText.WrapWidth);
            invisibleCharacters = FullText.EndPosition + 1;
        }

        readonly int textboxRows = 4;
        int invisibleCharacters;
        double timeUntilNextCharacter;

        readonly char[] pauseCharacters = { '.', '!', '?', ',' };
        readonly double commaPause = 15;

        void AddCharacter()
        {
            --invisibleCharacters;
            if (invisibleCharacters < 0) {
                AnimationActive = false;
                return;
            }

            MostRecentCharacter = FullText.GetCharacterAtPosition(
                FullText.EndPosition - invisibleCharacters);

            var newWrappedText = new WrappedText(
                "",
                FullText.FontReference,
                FullText.FontReferenceSize,
                FullText.WrapWidth); 
            newWrappedText.Strings = FullText.StringHead(FullText.EndPosition + 1 - invisibleCharacters);
            VisibleText = newWrappedText;

            while (VisibleText.Strings.Count > textboxRows) {
                VisibleText.RemoveLine(0);
                FullText.RemoveLine(0);
            }	
        }
    }

    class Textbox : Drawable
    {
        public static readonly Vector2f Position   = new Vector2f(0, 135);
        public static readonly Font TextFont       = new Font("resources/gohufont-11.pcf");
        public static readonly uint FontSize       = 11;
        public static readonly uint LineSpacing    = 2;
        public static readonly uint BorderWidth    = 8;
        public static readonly uint BorderHeight   = 5;

        public Color TextColor = Color.Black;

        public AnimatedWrappedText Content { get; private set; }

        public Textbox(AnimatedWrappedText textSource)
        {
            boxSprite = new Sprite(boxTexture);
            boxSprite.Position = Position;
            Content = textSource;
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            target.Draw(boxSprite, states);

            int i = 0;
            foreach (string str in Content.VisibleText.Strings) {
                var text = new Text(str, TextFont, FontSize);
                text.Position = boxSprite.Position + new Vector2f(
                    BorderWidth, 
                    BorderHeight + (FontSize + LineSpacing) * i);
                text.Color = TextColor;
                target.Draw(text, states);
                i++;
            }
        }

        static readonly Texture boxTexture  = new Texture("resources/textbox.png");
        readonly Sprite boxSprite;
    }
}
