using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace redd096
{
    #region classes

    [System.Serializable]
    public class AudioClassBase
    {
        public AudioClip audioClip = default;
        [Range(0f, 1f)] public float volume = 1;
    }

    [System.Serializable]
    public class AudioClass : AudioClassBase
    {
        public bool is3D = false;
    }

    #endregion

    [AddComponentMenu("redd096/Singletons/Sound Manager")]
    public class SoundManager : Singleton<SoundManager>
    {
        #region variables

        [Header("Background Music")]
        [SerializeField] AudioSource musicPrefab = default;
        [Tooltip("From 0 to 1, where 0 is 0 and 1 is volume to set")][SerializeField] AnimationCurve fadeInMusic = default;
        [Tooltip("From 1 to 0, where 1 is current volume and 0 is 0")][SerializeField] AnimationCurve fadeOutMusic = default;

        [Header("Edit Background Music for this scene")]
        [SerializeField] bool stopBackgroundMusicThisScene = false;
        [SerializeField] AudioClassBase musicThisScene = default;
        [SerializeField] bool loopMusicThisScene = true;
        [Tooltip("When change scene, if setted same clip and volume, restart anyway?")][SerializeField] bool forceReplayThisScene = false;

        [Header("Instantiate sound at point")]
        [Tooltip("Used also for SoundsOnClickButton")][SerializeField] AudioSource sound2DPrefab = default;
        [SerializeField] AudioSource sound3DPrefab = default;

        [Header("Sounds On Click Button (random from array)")]
        [SerializeField] AudioClassBase[] soundsOnClick = default;

        //sound parent (instantiate if null)
        private Transform soundsParent;
        public Transform SoundsParent
        {
            get
            {
                if (soundsParent == null) { soundsParent = new GameObject("Sounds Parent").transform; }
                return soundsParent;
            }
        }

        //audio sources in scene
        AudioSource musicBackgroundAudioSource;
        Pooling<AudioSource> pooling2D = new Pooling<AudioSource>();
        Pooling<AudioSource> pooling3D = new Pooling<AudioSource>();

        //coroutines
        Dictionary<AudioSource, Coroutine> coroutines = new Dictionary<AudioSource, Coroutine>();   //fade in and fade out coroutines, or deactive sound at point coroutines

        //used for volume settings
        Dictionary<AudioSource, float> savedVolumes = new Dictionary<AudioSource, float>();
        float volumeMusic = 1;
        float volumeSFX = 1;

        #endregion

        protected override void Awake()
        {
            base.Awake();

            //if this is the instance, if miss some prefab, create it
            if (instance == this)
            {
                if (musicPrefab == null)
                {
                    musicPrefab = new GameObject("Music Prefab", typeof(AudioSource)).GetComponent<AudioSource>();
                    musicPrefab.transform.SetParent(transform);     //set child to not destroy when change scene
                    musicPrefab.spatialBlend = 0.0f;                //set 2d sound
                }
                if (sound2DPrefab == null)
                {
                    sound2DPrefab = new GameObject("Sound 2D Prefab", typeof(AudioSource)).GetComponent<AudioSource>();
                    sound2DPrefab.transform.SetParent(transform);   //set child to not destroy when change scene
                    sound2DPrefab.spatialBlend = 0.0f;              //set 2d sound
                }
                if (sound3DPrefab == null)
                {
                    sound3DPrefab = new GameObject("Sound 3D Prefab", typeof(AudioSource)).GetComponent<AudioSource>();
                    sound3DPrefab.transform.SetParent(transform);   //set child to not destroy when change scene
                    sound3DPrefab.spatialBlend = 1.0f;              //set 3d sound
                }

                //set also fade in and fade out, if not setted
                if (fadeInMusic.keys.Length <= 0)
                    fadeInMusic.keys = new Keyframe[2] { new Keyframe(0, 0), new Keyframe(1, 1) };
                if (fadeOutMusic.keys.Length <= 0)
                    fadeOutMusic.keys = new Keyframe[2] { new Keyframe(0, 1), new Keyframe(1, 0) };
            }

            //stop background music if playing
            if (stopBackgroundMusicThisScene)
            {
                instance.StopBackgroundMusic(true);
            }
            //else, on the instance, play new background music
            else if (musicThisScene != null)
            {
                instance.PlayBackgroundMusic(musicThisScene.audioClip, true, forceReplayThisScene, musicThisScene.volume, loopMusicThisScene);
            }
        }

        #region utilities

        public IEnumerator DeactiveSoundAtPointCoroutine(AudioSource audioToDeactivate)
        {
            //wait to end the clip
            if (audioToDeactivate)
                yield return new WaitForSeconds(audioToDeactivate.clip.length);

            //and deactive
            if (audioToDeactivate)
                audioToDeactivate.gameObject.SetActive(false);
        }

        public IEnumerator FadeAudioCoroutine(AudioSource audioSource, AudioClip clip, AnimationCurve fadeIn, AnimationCurve fadeOut, float volume = 1, bool loop = false)
        {
            //if playing, do fade out (only if there is an animation curve and volume is not already at 0)
            if (audioSource.isPlaying && fadeOut != null && fadeOut.keys.Length > 0 && audioSource.volume > 0)
            {
                yield return FadeCoroutine(audioSource, audioSource.volume, fadeOut);
            }

            //set new clip, volume and loop - then be sure to play
            audioSource.clip = clip;
            audioSource.volume = fadeIn.keys.Length > 0 ? 0 : volume * GetVolumeSettingsForThisAudioSource(audioSource);    //if there is animation curve, set volume at 0, else set necessary volume
            audioSource.loop = loop;
            audioSource.Play();

            //start fade in (only if there is an animation curve)
            if (fadeIn != null && fadeIn.keys.Length > 0)
            {
                yield return FadeCoroutine(audioSource, volume, fadeIn);
            }
        }

        public IEnumerator FadeCoroutine(AudioSource audioSource, float volume, AnimationCurve fadeCurve)
        {
            //if there is no curve, set immediatly sound and stop coroutine
            if (fadeCurve == null || fadeCurve.keys.Length <= 0)
            {
                audioSource.volume = volume * GetVolumeSettingsForThisAudioSource(audioSource);                                                     //volumeAudio * optionsVolume
                yield break;
            }

            float currentTime = 0;

            //do fade
            while (currentTime < fadeCurve.keys[fadeCurve.length - 1].time)
            {
                currentTime += Time.deltaTime;

                //set volume using animation curve
                audioSource.volume = Mathf.Lerp(0, volume * GetVolumeSettingsForThisAudioSource(audioSource), fadeCurve.Evaluate(currentTime));     //volumeAudio * optionsVolume

                yield return null;
            }
        }

        #endregion

        #region static Play

        /// <summary>
        /// Start audio clip. Can set volume and loop
        /// </summary>
        public static void Play(AudioSource audioSource, AudioClip clip, bool forceReplay, float volume = 1, bool loop = false)
        {
            //be sure to have audio source
            if (audioSource == null)
                return;

            //if running fade coroutine for this audiosource, stop it
            if (instance.coroutines.ContainsKey(audioSource))
            {
                if (instance.coroutines[audioSource] != null) instance.StopCoroutine(instance.coroutines[audioSource]);
                instance.coroutines.Remove(audioSource);
            }

            //change only if different (so we can have same music in different scenes without stop) - or if set forceReplay or audioSource is not playing
            if (forceReplay || audioSource.isPlaying == false || audioSource.clip != clip || instance.CheckIsEqualToSavedVolume(audioSource, volume) == false || audioSource.loop != loop)
            {
                audioSource.clip = clip;
                audioSource.volume = volume * instance.GetVolumeSettingsForThisAudioSource(audioSource);    //volumeAudio * optionsVolume
                audioSource.loop = loop;

                //save volume settings
                instance.SaveVolumeAudioSource(audioSource, volume);

                audioSource.Play();
            }
        }

        /// <summary>
        /// Start audio clip. Can set volume and loop. It use animation curve to fade out previous audio clip and fade in new one
        /// </summary>
        public static void PlayWithFade(AudioSource audioSource, AudioClip clip, AnimationCurve fadeIn, AnimationCurve fadeOut, bool forceReplay, float volume = 1, bool loop = false)
        {
            //be sure to have audio source
            if (audioSource == null)
                return;

            //change only if different (so we can have same music in different scenes without stop) - or if set forceReplay or audioSource is not playing
            if (forceReplay || audioSource.isPlaying == false || audioSource.clip != clip || instance.CheckIsEqualToSavedVolume(audioSource, volume) == false || audioSource.loop != loop)
            {
                //if already running fade coroutine for this audiosource, stop it
                if (instance.coroutines.ContainsKey(audioSource))
                {
                    if (instance.coroutines[audioSource] != null) instance.StopCoroutine(instance.coroutines[audioSource]);
                    instance.coroutines.Remove(audioSource);
                }

                //save volume settings
                instance.SaveVolumeAudioSource(audioSource, volume);

                //start coroutine
                instance.coroutines.Add(audioSource, instance.StartCoroutine(instance.FadeAudioCoroutine(audioSource, clip, fadeIn, fadeOut, volume, loop)));
            }
        }

        #endregion

        #region music background

        /// <summary>
        /// Start audio clip for background music. Can set volume and loop
        /// </summary>
        public AudioSource PlayBackgroundMusic(AudioClip clip, bool doFade, bool forceReplay = false, float volume = 1, bool loop = true)
        {
            if (clip == null)
                return null;

            //if there is no music audioSource, instantiate it
            if (musicBackgroundAudioSource == null)
            {
                if (musicPrefab == null)
                    return null;

                musicBackgroundAudioSource = Instantiate(musicPrefab, transform);       //child of this
                musicBackgroundAudioSource.transform.localPosition = Vector3.zero;      //and same position
            }

            //play it (with fade or not)
            if (doFade)
                PlayWithFade(musicBackgroundAudioSource, clip, fadeInMusic, fadeOutMusic, forceReplay, volume, loop);
            else
                Play(musicBackgroundAudioSource, clip, forceReplay, volume, loop);

            return musicBackgroundAudioSource;
        }

        /// <summary>
        /// Start background music with clip setted in sound manager
        /// </summary>
        /// <param name="doFade"></param>
        public AudioSource PlayBackgroundMusic(bool doFade)
        {
            return PlayBackgroundMusic(musicThisScene.audioClip, doFade, forceReplayThisScene, musicThisScene.volume, loopMusicThisScene);
        }

        /// <summary>
        /// Stop background music if playing
        /// </summary>
        public AudioSource StopBackgroundMusic(bool doFade)
        {
            //stop old background music
            if (musicBackgroundAudioSource)
            {
                //if running fade coroutine for this audiosource, stop it
                if (coroutines.ContainsKey(musicBackgroundAudioSource))
                {
                    if (coroutines[musicBackgroundAudioSource] != null) StopCoroutine(coroutines[musicBackgroundAudioSource]);
                    coroutines.Remove(musicBackgroundAudioSource);
                }

                //with fade start coroutine
                if (doFade)
                {
                    coroutines.Add(musicBackgroundAudioSource, StartCoroutine(FadeCoroutine(musicBackgroundAudioSource, musicBackgroundAudioSource.volume, fadeOutMusic)));
                }
                //or immediatly
                else
                {
                    musicBackgroundAudioSource.Stop();
                }
            }

            return musicBackgroundAudioSource;
        }

        #endregion

        #region sound at point

        /// <summary>
        /// Start audio clip at point. Can set volume. Use specific pooling and audio source
        /// </summary>
        public AudioSource Play(Pooling<AudioSource> pool, AudioSource prefab, AudioClip clip, Vector3 position, float volume = 1)
        {
            if (clip == null)
                return null;

            //instantiate (if didn't find deactivated, take first one in the pool)
            AudioSource audioSource = pool.Instantiate(prefab);
            if (audioSource == null && pool.PooledObjects.Count > 0)
                audioSource = pool.PooledObjects[0];

            //if still null, return
            if (audioSource == null)
                return null;

            //set position, rotation and parent
            audioSource.transform.position = position;
            audioSource.transform.SetParent(SoundsParent);

            //play and start coroutine to deactivate
            Play(audioSource, clip, true, volume);
            coroutines.Add(audioSource, StartCoroutine(DeactiveSoundAtPointCoroutine(audioSource)));

            return audioSource;
        }

        /// <summary>
        /// Start audio clip at point. Can set volume
        /// </summary>
        public AudioSource Play(bool is3D, AudioClip clip, Vector3 position, float volume = 1)
        {
            //3d use 3dPooling and 3dPrefab, 2d use 2dPooling and 2dPrefab
            return Play(is3D ? pooling3D : pooling2D, is3D ? sound3DPrefab : sound2DPrefab, clip, position, volume);
        }

        /// <summary>
        /// Start audio clip at point. Can set volume. Get clip random from the array
        /// </summary>
        public AudioSource Play(bool is3D, AudioClip[] clips, Vector3 position, float volume = 1)
        {
            //do only if there are elements in the array
            if (clips.Length > 0)
            {
                return Play(is3D, clips[Random.Range(0, clips.Length)], position, volume);
            }

            return null;
        }

        /// <summary>
        /// Start audio clip at point, with selected volume
        /// </summary>
        public AudioSource Play(bool is3D, AudioClassBase audio, Vector3 position)
        {
            return Play(is3D, audio.audioClip, position, audio.volume);
        }

        /// <summary>
        /// Start audio clip at point. Get clip and volume random from the array
        /// </summary>
        public AudioSource Play(bool is3D, AudioClassBase[] audios, Vector3 position)
        {
            //do only if there are elements in the array
            if (audios.Length > 0)
            {
                return Play(is3D, audios[Random.Range(0, audios.Length)], position);
            }

            return null;
        }

        /// <summary>
        /// Start audio clip at point, with selected volume and selected 3d or 2d
        /// </summary>
        public AudioSource Play(AudioClass audio, Vector3 position)
        {
            return Play(audio.is3D, audio.audioClip, position, audio.volume);
        }

        /// <summary>
        /// Start audio clip at point. Get clip, volume and 3d or 2d, random from the array
        /// </summary>
        public AudioSource Play(AudioClass[] audios, Vector3 position)
        {
            //do only if there are elements in the array
            if (audios.Length > 0)
            {
                return Play(audios[Random.Range(0, audios.Length)], position);
            }

            return null;
        }

        #endregion

        #region sounds on click button

        /// <summary>
        /// Called by buttons in UI - play random sound on click
        /// </summary>
        public void PlayOnClick()
        {
            //in instance, call Play 2D
            instance.Play(false, soundsOnClick, Vector3.zero);
        }

        /// <summary>
        /// Called by buttons in UI - play specific sound
        /// </summary>
        public void PlayOnClick(AudioClip sound)
        {
            //in instance, call Play 2D
            instance.Play(false, sound, Vector3.zero);
        }

        #endregion

        #region volume settings

        /// <summary>
        /// Set volume settings for the background music
        /// </summary>
        /// <param name="volumeMusic"></param>
        public void SetVolumeMusic(float volumeMusic)
        {
            this.volumeMusic = volumeMusic;

            //update music audio source's volume
            if (musicBackgroundAudioSource && savedVolumes.ContainsKey(musicBackgroundAudioSource))
                musicBackgroundAudioSource.volume = savedVolumes[musicBackgroundAudioSource] * volumeMusic;
        }

        /// <summary>
        /// Set volume settings for the SFXs
        /// </summary>
        /// <param name="volumeSFX"></param>
        public void SetVolumeSFX(float volumeSFX)
        {
            this.volumeSFX = volumeSFX;

            //update every audio source's volume
            foreach (AudioSource audioSource in savedVolumes.Keys)
            {
                //be sure it isn't music audio source
                if (audioSource && audioSource != musicBackgroundAudioSource)
                    audioSource.volume = savedVolumes[audioSource] * volumeSFX;
            }
        }

        /// <summary>
        /// Get volumeMusic or volumeSFX based on audioSource
        /// </summary>
        /// <param name="audioSource"></param>
        /// <returns></returns>
        public float GetVolumeSettingsForThisAudioSource(AudioSource audioSource)
        {
            if (audioSource == musicBackgroundAudioSource)
                return volumeMusic;
            else //if (savedVolumes.ContainsKey(audioSource))
                return volumeSFX;
            //else 
            //    return 1;
        }

        public void SaveVolumeAudioSource(AudioSource audioSource, float volume)
        {
            if (audioSource == null)
                return;

            //save audio source with volume
            if (savedVolumes.ContainsKey(audioSource) == false)
            {
                savedVolumes.Add(audioSource, volume);
            }
            //else, update saved volume
            else
            {
                savedVolumes[audioSource] = volume;
            }
        }

        public bool CheckIsEqualToSavedVolume(AudioSource audioSource, float volume)
        {
            //if there is no audio source or is not saved, return false
            if (audioSource == null || savedVolumes.ContainsKey(audioSource) == false)
                return false;

            //if volume is the same already saved, return true
            if (Mathf.Approximately(savedVolumes[audioSource], volume))
                return true;

            return false;
        }

        #endregion
    }
}