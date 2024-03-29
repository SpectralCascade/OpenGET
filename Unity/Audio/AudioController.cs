using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace OpenGET {

    public class AudioController : MonoBehaviour
    {

        public enum Channel {
            SFX,
            UI,
            Music
        }

        /// <summary>
        /// The total number of audio sources per pool.
        /// </summary>
        public const int MaxPoolSources = 30;
        public const float CrossfadeTime = 2;

        /// <summary>
        /// A limited pool of audio sources to play sounds from.
        /// </summary>
        private AudioSource[] sfxPool = new AudioSource[MaxPoolSources];
        private AudioSource[] uiPool = new AudioSource[MaxPoolSources];
        private AudioSource[] musicPool = new AudioSource[MaxPoolSources];

        /// <summary>
        /// The audio mixer to use.
        /// </summary>
        private AudioMixer mixer;

        public const string FallbackMixer = "TemplateMixer";
        public static string mixerPath = "TemplateMixer";
        public static string sfxGroup = "SFX";
        public static string uiGroup = "UI";
        public static string musicGroup = "Music";
        public static string masterGroup = "Master";

        /// <summary>
        /// The primary audio mixer groups.
        /// </summary>
        private AudioMixerGroup sfxBus;
        private AudioMixerGroup uiBus;
        private AudioMixerGroup musicBus;
        private AudioMixerGroup masterBus;

        /// <summary>
        /// Points to the last free audio source in the pools.
        /// </summary>
        private int[] poolHeads = new int[3];

        /// <summary>
        /// Whether the primary or secondary music channel should be playing
        /// </summary>
        private static int primaryMusicIndex = 0;

        /// <summary>
        /// Retrieve the singleton instance of this class.
        /// </summary>
        public static AudioController Instance {
            get {
                if (_instance == null) {
                    GameObject gob = new GameObject("AudioController");
                    if (gob == null) {
                        Log.Debug("ERROR NULL GAMEOBJECT");
                    }
                    _instance = gob.AddComponent<AudioController>();
                }
                return _instance;
            }
        }
        private static AudioController _instance;

        public void Awake() {
            DontDestroyOnLoad(gameObject);

            mixer = Resources.Load<AudioMixer>(mixerPath);
            if (mixer == null) {
                mixer = Resources.Load<AudioMixer>(FallbackMixer);
            }

            // Hook up the mixer groups
            AudioMixerGroup[] matches = mixer.FindMatchingGroups(sfxGroup);
            sfxBus = matches.Length > 0 ? matches[0] : mixer.outputAudioMixerGroup;
            matches = mixer.FindMatchingGroups(uiGroup);
            uiBus = matches.Length > 0 ? matches[0] : mixer.outputAudioMixerGroup;
            matches = mixer.FindMatchingGroups(musicGroup);
            musicBus = matches.Length > 0 ? matches[0] : mixer.outputAudioMixerGroup;
            matches = mixer.FindMatchingGroups(masterGroup);
            masterBus = matches.Length > 0 ? matches[0] : mixer.outputAudioMixerGroup;

            // Setup the audio sources
            for (int i = 0; i < MaxPoolSources; i++) {
                sfxPool[i] = gameObject.AddComponent<AudioSource>();
                sfxPool[i].outputAudioMixerGroup = sfxBus;

                uiPool[i] = gameObject.AddComponent<AudioSource>();
                uiPool[i].outputAudioMixerGroup = uiBus;

                musicPool[i] = gameObject.AddComponent<AudioSource>();
                musicPool[i].outputAudioMixerGroup = musicBus;
                musicPool[i].volume = 0;
                musicPool[i].loop = true;
            }
        }

        // Fading takes place here using Time.deltaTime
        public void Update() {
            // Fade in primary, fade out others
            musicPool[primaryMusicIndex].volume = Mathf.Clamp01(musicPool[primaryMusicIndex].volume + (Time.deltaTime * CrossfadeTime));
            for (int i = 0, counti = 2; i < counti; i++) {
                if (i != primaryMusicIndex) {
                    musicPool[i].volume = Mathf.Clamp01(musicPool[primaryMusicIndex].volume - (Time.deltaTime * CrossfadeTime));
                }
            }
        }

        public static AudioSource Play(string resource, Channel channel = Channel.SFX, float volume = 1.0f, bool loop = false) {
            AudioClip clip = Resources.Load<AudioClip>(resource);
            AudioSource source = null;
            if (clip != null) {
                source = Play(clip, channel, volume, loop);
            } else {
                Log.Warning("Resource \"{0}\" could not be loaded.", resource);
            }
            return source;
        }

        public static AudioSource Play(AudioClip clip, Channel channel = Channel.SFX, float volume = 1.0f, bool loop = false) {
            int head = Instance.poolHeads[(int)channel];
            if (head >= MaxPoolSources) {
                Instance.poolHeads[(int)channel] = 0;
                head = 0;
            } else {
                Instance.poolHeads[(int)channel]++;
            }

            AudioSource source = Instance.sfxPool[head];
            source.clip = clip;
            source.volume = volume;
            source.loop = loop;
            source.Play();
            return source;
        }

        public static AudioSource PlayMusic(AudioClip clip) {
            primaryMusicIndex++;
            if (primaryMusicIndex > 1) {
                primaryMusicIndex = 0;
            }
            AudioSource source = Instance.musicPool[primaryMusicIndex];
            if (source.clip == null || source.clip.name != clip.name) {
                source.clip = clip;
                source.Stop();
                source.Play();
            }
            if (!source.isPlaying) {
                source.Play();
            }
            source.loop = true;
            source.volume = 0;
            return source;
        }

    }

}
