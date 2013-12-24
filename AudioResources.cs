using System.Collections.Generic;
using SFML.Audio;

namespace SpriteNovel
{
    public class AudioResources
    {
        public readonly Dictionary<string, Sound> SoundDict
            = new Dictionary<string, Sound> {
            {"text",        new Sound(new SoundBuffer("resources/audio/text-normal.wav"))},
            {"text-slower", new Sound(new SoundBuffer("resources/audio/text-slower.wav"))},
            {"advance",     new Sound(new SoundBuffer("resources/audio/advance.wav"))},
            {"choose",      new Sound(new SoundBuffer("resources/audio/choose.wav"))},
            {"history",     new Sound(new SoundBuffer("resources/audio/history.wav"))} };

        public readonly Dictionary<string, Music> MusicDict
            = new Dictionary<string, Music> {
            {"wrunga",  new Music("resources/audio/wrunga.ogg")},
            {"butadog", new Music("resources/audio/butadog.ogg")} };

        public AudioResources()
        {
            SoundDict["text"].Loop = true;
            SoundDict["text"].Volume = 70;
            SoundDict["text-slower"].Loop = true;
            SoundDict["text-slower"].Volume = 70;
            SoundDict["advance"].Volume = 100;

            foreach (KeyValuePair<string, Music> m in MusicDict)
                m.Value.Loop = true;
            MusicDict["butadog"].Volume = 50;
        }
    }
}

