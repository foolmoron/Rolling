    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Xna.Framework.Audio;
    using Microsoft.Xna.Framework.Content;

    namespace Rolling
    {
        class Sound
        {
            private static SoundEffect soundEngine;
            private static List<SoundEffectInstance> mySounds = new List<SoundEffectInstance>();
            private static int numSounds = 0;
            private static Dictionary<string, int> lookUpTable = new Dictionary<string, int>();

            public static void playSoundOnce(string s, ContentManager Content)
            {

                soundEngine = Content.Load<SoundEffect>(s);
                SoundEffectInstance soundEngineInstanceOne = soundEngine.CreateInstance();
                soundEngineInstanceOne.Play();
            }

            //plays a sound once and doesn’t store the reference into memory
            public static void playSound(string filename, ContentManager Content)
            {
                int index = 1;
                SoundEffectInstance soundEngineInstance;
                if (lookUpTable.ContainsKey(filename))
                {

                    lookUpTable.TryGetValue(filename, out index);
                    soundEngineInstance = mySounds.ElementAt(index);
                    if (soundEngineInstance.State == SoundState.Paused)
                    {
                        soundEngineInstance.Resume();
                    }
                    else if (soundEngineInstance.State == SoundState.Stopped)
                    {
                        soundEngineInstance.Play();
                    }
                }
                else
                {
                    soundEngine = Content.Load<SoundEffect>(filename);
                    soundEngineInstance = soundEngine.CreateInstance();
                    mySounds.Add(soundEngineInstance);

                    lookUpTable.Add(filename, numSounds);
                    numSounds++;
                    soundEngineInstance.IsLooped = false;
                    soundEngineInstance.Volume = 0.5f;
                    soundEngineInstance.Play();
                }
            }
            public static void playSound(string filename, ContentManager Content, float volume)
            {
                int index = 1;
                SoundEffectInstance soundEngineInstance;
                if (lookUpTable.ContainsKey(filename))
                {

                    lookUpTable.TryGetValue(filename, out index);
                    soundEngineInstance = mySounds.ElementAt(index);
                    if (soundEngineInstance.State == SoundState.Paused)
                    {
                        soundEngineInstance.Resume();
                    }
                    else if (soundEngineInstance.State == SoundState.Stopped)
                    {
                        soundEngineInstance.Play();
                    }
                }
                else
                {
                    soundEngine = Content.Load<SoundEffect>(filename);
                    soundEngineInstance = soundEngine.CreateInstance();
                    mySounds.Add(soundEngineInstance);

                    lookUpTable.Add(filename, numSounds);
                    numSounds++;
                    soundEngineInstance.IsLooped = false;
                    soundEngineInstance.Volume = volume;
                    soundEngineInstance.Play();
                }
            }
            public static void playSoundLoop(string filename, ContentManager Content)
            {

                int index = 1;
                SoundEffectInstance soundEngineInstance;
                if (lookUpTable.ContainsKey(filename))
                {

                    lookUpTable.TryGetValue(filename, out index);
                    soundEngineInstance = mySounds.ElementAt(index);
                    if (soundEngineInstance.State == SoundState.Paused)
                    {
                        soundEngineInstance.Resume();
                    }
                    else if (soundEngineInstance.State == SoundState.Stopped)
                    {
                        soundEngineInstance.Play();
                    }
                }
                else
                {
                    soundEngine = Content.Load<SoundEffect>(filename);
                    soundEngineInstance = soundEngine.CreateInstance();
                    mySounds.Add(soundEngineInstance);

                    lookUpTable.Add(filename, numSounds);
                    numSounds++;
                    soundEngineInstance.IsLooped = true;
                    soundEngineInstance.Volume = 0.5f;
                    soundEngineInstance.Play();
                }

            }
            public static void playSoundLoop(string s, ContentManager Content, float volume)
            {
                int index = 1;
                SoundEffectInstance soundEngineInstance;
                if (lookUpTable.ContainsKey(s))
                {

                    lookUpTable.TryGetValue(s, out index);
                    soundEngineInstance = mySounds.ElementAt(index);
                    if (soundEngineInstance.State == SoundState.Paused)
                    {
                        soundEngineInstance.Resume();
                    }
                    else if (soundEngineInstance.State == SoundState.Stopped)
                    {
                        soundEngineInstance.Play();
                    }
                }
                else
                {
                    soundEngine = Content.Load<SoundEffect>(s);
                    soundEngineInstance = soundEngine.CreateInstance();
                    mySounds.Add(soundEngineInstance);

                    lookUpTable.Add(s, numSounds);
                    numSounds++;
                    soundEngineInstance.IsLooped = true;
                    soundEngineInstance.Volume = volume;
                    soundEngineInstance.Play();
                }

            }
            public static void pauseSound(string s, ContentManager Content)
            {

                int index;
                SoundEffectInstance soundEngineInstance;
                if (lookUpTable.TryGetValue(s, out index))
                {
                    soundEngineInstance = mySounds.ElementAt(index);
                    if (soundEngineInstance.State == SoundState.Playing)
                    {
                        soundEngineInstance.Pause();
                    }
                    mySounds[index] = soundEngineInstance;
                }
            }
        }
    }