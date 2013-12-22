using System;
using System.Collections.Generic;
using SFML.Window;
using SFML.Graphics;

namespace SpriteNovel
{
    public class ChoiceDisplay : Drawable
    {
        public bool Deactivated
        {
            get {
                foreach (var box in choiceBoxes)
                    if (!box.Deactivated)
                        return false;
                return true;
            }
        }

        public ChoiceDisplay(List<string> choiceStrings)
        {
            this.choiceStrings = choiceStrings;
            float yPosition = startPosition.Y;
            foreach (var str in choiceStrings) {
                var choiceBox = new ChoiceBox(
                    str, 
                    startPosition + new Vector2f(0, yPosition));
                yPosition += choiceBox.Height + spaceBetweenBoxes;
                choiceBoxes.Add(choiceBox);
            }
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            if (!Deactivated)
                foreach (var box in choiceBoxes)
                    target.Draw(box, states);    
        }

        public int CheckIfAnyHighlighted(Vector2f mousePosition)
        {
            if (!Deactivated) {
                int i = 0;
                foreach (var box in choiceBoxes) {
                    if (box.CheckIfHighlighted(mousePosition))
                        return i;
                    i++;
                }
            }
            return -1;
        }

        public void Deactivate()
        {
            foreach (var box in choiceBoxes)
                box.Deactivate();
        }

        readonly List<ChoiceBox> choiceBoxes   = new List<ChoiceBox>();
        readonly List<string>    choiceStrings = new List<string>();

        readonly Vector2f startPosition = new Vector2f(Stage.ScreenWidth/2, 20);
        readonly int spaceBetweenBoxes  = 10;

    }

    public class ChoiceBox : Drawable
    {
        public float Height
        {
            get { return boxSprite.GetLocalBounds().Height; }
        }

        public bool Deactivated { get; private set; }

        public ChoiceBox(string message, Vector2f position)
        {
            choiceText.DisplayedString = message;
            choiceText.Color = Color.Black;

            boxSprite.Position = position;
            boxSprite.Origin = new Vector2f(boxSprite.GetLocalBounds().Width/2, 0);

            choiceText.Position = position + new Vector2f(0, 4);
            choiceText.Origin = new Vector2f(
                choiceText.GetLocalBounds().Width/2, 0);
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            if (!deactivating)
            {
                if (increasingAlpha < 255)
                    increasingAlpha += fadeSpeed;
                if (increasingAlpha > 255)
                    increasingAlpha = 255;
            } else {
                if (increasingAlpha > 0)
                    increasingAlpha -= fadeSpeed*4;
                if (increasingAlpha < 0)
                {
                    increasingAlpha = 0;
                    Deactivated = true;
                }    
            }

            boxSprite.Color = new Color(
                boxSprite.Color.R,
                boxSprite.Color.G,
                boxSprite.Color.B,
                (byte)increasingAlpha);

            target.Draw(boxSprite, states);
            target.Draw(choiceText, states);
        }

        public bool CheckIfHighlighted(Vector2f mousePosition)
        {
            if (boxSprite.GetGlobalBounds().Contains(mousePosition.X, mousePosition.Y)) {
                boxSprite.Color = new Color(127, 127, 127);
                return true;
            }

            boxSprite.Color = new Color(255, 255, 255, boxSprite.Color.A);
            return false; 
        }

        public void Deactivate()
        {
            deactivating = true;
        }

        int increasingAlpha;
        readonly int fadeSpeed = 10;
        bool deactivating = false;

        static readonly Texture boxTexture = new Texture("resources/choicebox.png");
        readonly Sprite boxSprite  = new Sprite(boxTexture);
        readonly Text   choiceText = new Text("", Textbox.TextFont, Textbox.FontSize);
    }
}


