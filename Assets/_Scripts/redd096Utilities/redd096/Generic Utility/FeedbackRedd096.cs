using System.Collections.Generic;
using UnityEngine;

namespace redd096
{
    public abstract class FeedbackRedd096<T> : FeedbackRedd096
    {
        [Header("Necessary Components - default get in parent")]
        [SerializeField] protected T owner;

        protected override void OnEnable()
        {
            //get owner
            if (owner == null)
                owner = GetComponentInParent<T>();

            //add events
            if (owner != null)
                AddEvents();
        }

        protected override void OnDisable()
        {
            //remove events
            if (owner != null)
                RemoveEvents();
        }

    }

    public abstract class FeedbackRedd096 : MonoBehaviour
    {
        protected virtual void OnEnable()
        {
            //add events
            AddEvents();
        }

        protected virtual void OnDisable()
        {
            //remove events
            RemoveEvents();
        }

        protected virtual void AddEvents() { }
        protected virtual void RemoveEvents() { }

        /// <summary>
        /// Instantiate feedbacks using parent or self
        /// </summary>
        /// <param name="feedback"></param>
        /// <param name="parent"></param>
        /// <param name="worldPositionStays"></param>
        protected void InstantiateFeedback(FeedbackStructRedd096 feedback, Transform parent = null, bool worldPositionStays = true)
        {
            feedback.InstantiateFeedback(parent ? parent : transform, worldPositionStays);
        }

        /// <summary>
        /// Instantiate feedbacks using parent or self. And return first instantiated object for every fx
        /// </summary>
        /// <param name="feedback"></param>
        /// <param name="outInstantiatedGameObject"></param>
        /// <param name="outInstantiatedParticle"></param>
        /// <param name="outInstantiatedAudio"></param>
        /// <param name="parent"></param>
        /// <param name="worldPositionStays"></param>
        protected void InstantiateFeedback(FeedbackStructRedd096 feedback, out GameObject outInstantiatedGameObject, out ParticleSystem outInstantiatedParticle, out AudioSource outInstantiatedAudio, Transform parent = null, bool worldPositionStays = true)
        {
            feedback.InstantiateFeedback(out outInstantiatedGameObject, out outInstantiatedParticle, out outInstantiatedAudio, parent ? parent : transform, worldPositionStays);
        }

        /// <summary>
        /// Instantiate feedbacks using parent or self. And return every instantiated fx
        /// </summary>
        /// <param name="feedback"></param>
        /// <param name="outInstantiatedGameObjects"></param>
        /// <param name="outInstantiatedParticles"></param>
        /// <param name="outInstantiatedAudios"></param>
        /// <param name="parent"></param>
        /// <param name="worldPositionStays"></param>
        protected void InstantiateFeedback(FeedbackStructRedd096 feedback, out GameObject[] outInstantiatedGameObjects, out ParticleSystem[] outInstantiatedParticles, out AudioSource[] outInstantiatedAudios, Transform parent = null, bool worldPositionStays = true)
        {
            feedback.InstantiateFeedback(out outInstantiatedGameObjects, out outInstantiatedParticles, out outInstantiatedAudios, parent ? parent : transform, worldPositionStays);
        }
    }

    [System.Serializable]
    public struct FeedbackStructRedd096
    {
        [Tooltip("Use array to instantiate every single element, or just one picked random from it")] public bool InstantiateOneRandom;
        [Tooltip("When instantiated, set child of the object which instantiate it?")] public bool SetParent;
        public InstantiatedGameObjectStruct[] GameObjectsFeedback;
        public ParticleSystem[] ParticlesFeedback;
        public AudioClass[] AudiosFeedback;

        List<GameObject> instantiatedGameObjects;
        List<ParticleSystem> instantiatedParticles;
        List<AudioSource> instantiatedAudios;

        public void InstantiateFeedback(out GameObject outInstantiatedGameObject, out ParticleSystem outInstantiatedParticle, out AudioSource outInstantiatedAudio, Transform parent, bool worldPositionStays = true)
        {
            //instantiate
            InstantiateFeedback(parent, worldPositionStays);

            //out first vars
            outInstantiatedGameObject = instantiatedGameObjects.Count > 0 ? instantiatedGameObjects[0] : null;
            outInstantiatedParticle = instantiatedParticles.Count > 0 ? instantiatedParticles[0] : null;
            outInstantiatedAudio = instantiatedAudios.Count > 0 ? instantiatedAudios[0] : null;
        }

        public void InstantiateFeedback(out GameObject[] outInstantiatedGameObjects, out ParticleSystem[] outInstantiatedParticles, out AudioSource[] outInstantiatedAudios, Transform parent, bool worldPositionStays = true)
        {
            //instantiate
            InstantiateFeedback(parent, worldPositionStays);

            //out array vars
            outInstantiatedGameObjects = instantiatedGameObjects.ToArray();
            outInstantiatedParticles = instantiatedParticles.ToArray();
            outInstantiatedAudios = instantiatedAudios.ToArray();
        }

        public void InstantiateFeedback(Vector3 position, Quaternion rotation, Transform parent = null, bool worldPositionStays = true)
        {
            //instantiate
            InstantiateFeedback(parent, worldPositionStays);

            //set custom position and rotation
            instantiatedGameObjects.ForEach(x => { x.transform.position = position; x.transform.rotation = rotation; });
            instantiatedParticles.ForEach(x => { x.transform.position = position; x.transform.rotation = rotation; });
            instantiatedAudios.ForEach(x => { x.transform.position = position; x.transform.rotation = rotation; });
        }

        public void InstantiateFeedback(Transform parent, bool worldPositionStays = true)
        {
            //be sure lists are created
            if (instantiatedGameObjects == null) instantiatedGameObjects = new List<GameObject>();
            if (instantiatedParticles == null) instantiatedParticles = new List<ParticleSystem>();
            if (instantiatedAudios == null) instantiatedAudios = new List<AudioSource>();

            //clear lists
            instantiatedGameObjects.Clear();
            instantiatedParticles.Clear();
            instantiatedAudios.Clear();

            //instantiate single vfx and sfx from array
            if (InstantiateOneRandom)
            {
                instantiatedGameObjects.Add(InstantiateGameObjectManager.instance.Play(GameObjectsFeedback, parent.position, parent.rotation));
                instantiatedParticles.Add(ParticlesManager.instance.Play(ParticlesFeedback, parent.position, parent.rotation));
                instantiatedAudios.Add(SoundManager.instance.Play(AudiosFeedback, parent.position));
            }
            //else, instantiate every element from array
            else
            {
                foreach (InstantiatedGameObjectStruct gameObjectStruct in GameObjectsFeedback)
                    instantiatedGameObjects.Add(InstantiateGameObjectManager.instance.Play(gameObjectStruct, parent.position, parent.rotation));

                foreach (ParticleSystem particleSystem in ParticlesFeedback)
                    instantiatedParticles.Add(ParticlesManager.instance.Play(particleSystem, parent.position, parent.rotation));

                foreach (AudioClass audio in AudiosFeedback)
                    instantiatedAudios.Add(SoundManager.instance.Play(audio, parent.position));
            }

            //set parent if necessary
            if (SetParent)
            {
                foreach (GameObject instantiated in instantiatedGameObjects)
                    if (instantiated)
                        instantiated.transform.SetParent(parent, worldPositionStays);

                foreach (ParticleSystem instantiated in instantiatedParticles)
                    if (instantiated)
                        instantiated.transform.SetParent(parent, worldPositionStays);

                foreach (AudioSource instantiated in instantiatedAudios)
                    if (instantiated)
                        instantiated.transform.SetParent(parent, worldPositionStays);
            }
        }
    }
}