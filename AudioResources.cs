using System;
using System.Collections.Generic;
using SFML.Audio;

namespace SpriteNovel
{
    public class AudioResources
    {

        public readonly Dictionary<string, Sound> SoundDict
            = new Dictionary<string, Sound> {
            {"text", new Sound(new SoundBuffer("resources/audio/test.wav"))},
            {"advance", new Sound(new SoundBuffer("resources/audio/advance.wav"))}
        };

        public readonly Dictionary<string, Music> MusicDict 
            = new Dictionary<string, Music>() {
            {"wrunga", new Music("resources/audio/wrunga.ogg")}

        };

        public AudioResources()
        {
            SoundDict["text"].Loop = true;
            SoundDict["text"].Volume = 70;
            SoundDict["advance"].Volume = 100;

            foreach (KeyValuePair<string, Music> m in MusicDict)
                m.Value.Loop = true;
        }


    }
}

