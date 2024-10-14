using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace OpenGET {

    /// <summary>
    /// Inherit from this class to implement your own buses & effects.
    /// </summary>
    public class AudioController : AutoBehaviour
    {

        /// <summary>
        /// Represents an audio bus.
        /// </summary>
        public class Bus
        {
            /// <summary>
            /// Name of this audio bus.
            /// </summary>
            public readonly string name;

            /// <summary>
            /// The actual mixer group.
            /// </summary>
            public AudioMixerGroup group;

            /// <summary>
            /// Mixer volume (linear level between 0 and 1).
            /// </summary>
            public float volume {
                get { return Mathf.Pow(10, dB / 20f); }
                set { dB = 20f * Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)); }
            }

            /// <summary>
            /// Mixer volume/level in dB (logarithmic level from -80dB to 0dB).
            /// </summary>
            public float dB {
                get {
                    group.audioMixer.GetFloat(name + _volumeParam, out float value);
                    return value;
                }
                set { group.audioMixer.SetFloat(name + _volumeParam, value); }
            }
            protected const string _volumeParam = "/Volume";

            /// <summary>
            /// Pool of audio sources.
            /// </summary>
            public AudioSource[] pool = new AudioSource[0];

            /// <summary>
            /// Pool head (the pool index for the next available audio source).
            /// </summary>
            public int head { get; protected set; }

            /// <summary>
            /// Previous pool head (i.e. the most recently played audio source index for the pool)
            /// </summary>
            public int previous => head > 0 ? head - 1 : 0;

            /// <summary>
            /// Whether this is a music bus or sound effect bus.
            /// Music buses have some specific functionality best suited for music.
            /// </summary>
            public bool isMusicBus;

            /// <summary>
            /// Create a new audio bus.
            /// </summary>
            public Bus(string name, int poolAmount = 10, bool isMusicBus = false)
            {
                this.name = name;
                pool = new AudioSource[poolAmount];
                this.isMusicBus = isMusicBus;
            }

            /// <summary>
            /// Initialise the bus (create pool).
            /// </summary>
            public void Init(AudioMixer mixer, GameObject poolObj)
            {
                AudioMixerGroup[] groups = mixer.FindMatchingGroups(name);
                group = groups.Length > 0 ? groups[0] : null;

                if (group == null)
                {
                    Log.Error("No such mixer group \"{0}\", have you created one in mixer \"{1}\"?", name, mixer.name);
                }

                for (int i = 0, counti = pool.Length; i < counti; i++)
                {
                    pool[i] = pool[i] == null ? poolObj.AddComponent<AudioSource>() : pool[i];
                    pool[i].outputAudioMixerGroup = group;

                    if (isMusicBus)
                    {
                        // Volume is automatically faded & always looped.
                        pool[i].volume = 0;
                        pool[i].loop = true;
                    }
                }
            }

            /// <summary>
            /// Play an audio clip on a free audio source.
            /// Returns the audio source used.
            /// To crossfade music, set volume to zero.
            /// </summary>
            public AudioSource Play(AudioClip clip, float volume = 1, bool loop = false)
            {
                AudioSource source = pool[head];
                source.clip = clip;
                source.loop = loop;
                source.volume = volume;
                source.Play();

                // Update pool head
                head++;
                if (head >= pool.Length)
                {
                    head = 0;
                }

                return source;
            }

            /// <summary>
            /// Play an audio clip given a resource path.
            /// </summary>
            public AudioSource Play(string resource, float volume = 1, bool loop = false)
            {
                AudioClip clip = Resources.Load<AudioClip>(resource);
                AudioSource source = null;
                if (clip != null)
                {
                    source = Play(clip, volume, loop);
                }
                else
                {
                    Log.Warning("Resource \"{0}\" could not be loaded.", resource);
                }
                return source;
            }

            public void StopAll()
            {
                for (int i = 0, counti = pool.Length; i < counti; i++)
                {
                    if (pool[i] != null)
                    {
                        pool[i].Stop();
                    }
                }
            }

        }

        /// <summary>
        /// The maximum number of audio sources per pool.
        /// </summary>
        public virtual int MaxPoolSources => 15;

        /// <summary>
        /// How long should crossfades be?
        /// </summary>
        public virtual float CrossfadeTime => 2;

        /// <summary>
        /// The audio mixer to use.
        /// </summary>
        private AudioMixer mixer;

        /// <summary>
        /// Fallback to the OpenGET mixer.
        /// </summary>
        public const string FallbackMixer = "TemplateMixer";

        /// <summary>
        /// Path to the mixer asset.
        /// </summary>
        public virtual string MixerPath => FallbackMixer;

        /// <summary>
        /// Custom audio buses, i.e. all other buses in the mixer.
        /// </summary>
        protected List<Bus> buses = new List<Bus>();

        /// <summary>
        /// Master bus.
        /// </summary>
        public virtual Bus master => _master ??= new Bus("Master", 0);
        protected Bus _master = null;

        /// <summary>
        /// Music bus.
        /// </summary>
        public virtual Bus music => _music ??= new Bus("Music", 2, true);
        protected Bus _music = null;

        /// <summary>
        /// Retrieve the singleton instance of this AudioController.
        /// </summary>
        public static AudioController Instance {
            get {
                if (_instance == null) {
                    GameObject gob = new GameObject(typeof(AudioController).Name);
                    _instance = gob.AddComponent<AudioController>();
                }
                return _instance;
            }
        }
        private static AudioController _instance;

        /// <summary>
        /// Initialise all buses.
        /// </summary>
        protected override void Awake() {
            base.Awake();

            DontDestroyOnLoad(gameObject);

            mixer = Resources.Load<AudioMixer>(MixerPath);
            if (mixer == null) {
                mixer = Resources.Load<AudioMixer>(FallbackMixer);
            }

            // Hook up the mixer groups automagically
            AudioMixerGroup[] allMixerGroups = mixer.FindMatchingGroups(string.Empty);
            for (int i = 0, counti = allMixerGroups.Length; i < counti; i++)
            {
                // Ignore the special master and music buses
                if (allMixerGroups[i].name != master.name && allMixerGroups[i].name != music.name) {
                    Bus bus = new Bus(allMixerGroups[i].name);
                    bus.Init(mixer, gameObject);
                    buses.Add(bus);
                }
            }
            master.Init(mixer, gameObject);
            music.Init(mixer, gameObject);
        }

        /// <summary>
        /// Get an audio bus by name.
        /// </summary>
        public Bus GetBus(string busName)
        {
            for (int i = 0, counti = buses.Count; i < counti; i++)
            {
                if (buses[i].name == busName)
                {
                    return buses[i];
                }
            }
            return busName == master.name ? master : (busName == music.name ? music : null);
        }

        /// <summary>
        /// Get an audio bus by group.
        /// </summary>
        public Bus GetBus(AudioMixerGroup group)
        {
            for (int i = 0, counti = buses.Count; i < counti; i++)
            {
                if (buses[i].group == group)
                {
                    return buses[i];
                }
            }
            return group == master.group ? master : (group == music.group ? music : null);
        }

        /// <summary>
        /// Get an audio bus by name.
        /// </summary>
        public static Bus Channel(string busName)
        {
            return Instance.GetBus(busName);
        }

        /// <summary>
        /// Get an audio bus by group.
        /// </summary>
        public static Bus Channel(AudioMixerGroup group)
        {
            return Instance.GetBus(group);
        }

        /// <summary>
        /// Stop all audio channels playing.
        /// </summary>
        public static void StopAll()
        {
            for (int i = 0, counti = Instance.buses.Count; i < counti; i++)
            {
                Instance.buses[i].StopAll();
            }
        }

        /// <summary>
        /// Handle time-based effects (such as crossfading music).
        /// </summary>
        protected virtual void Update() {
            // Fade in primary music pools, fade out others
            for (int i = 0, counti = buses.Count; i < counti; i++)
            {
                Bus bus = buses[i];
                if (bus.isMusicBus)
                {
                    bus.pool[bus.previous].volume = Mathf.Clamp01(
                        bus.pool[bus.previous].volume + (Time.unscaledDeltaTime * CrossfadeTime)
                    );
                    for (int j = 0, countj = bus.pool.Length; j < countj; j++)
                    {
                        if (j != bus.previous)
                        {
                            bus.pool[j].volume = Mathf.Clamp01(
                                bus.pool[bus.previous].volume - (Time.unscaledDeltaTime * CrossfadeTime)
                            );
                        }
                    }
                }
            }
        }

    }

}
