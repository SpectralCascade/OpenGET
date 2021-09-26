using OpenGET;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenGET {

    public abstract class Window : MonoBehaviour {
        public const float DefaultFadeTime = 0.2f;

        private Fader fader;

        protected virtual void Awake() {
            fader = new Fader(gameObject.AddComponentOnce<CanvasGroup>());
            fader.OnFadeComplete += OnFaded;
        }

        private void OnFaded(Fader f, int fadeDir) {
            if (fadeDir > 0) {
                OnDidShow();
            } else {
                gameObject.SetActive(false);
                OnDidHide();
            }
        }

        public void Show(float fadeTime = DefaultFadeTime) {
            gameObject.SetActive(true);
            OnWillShow();
            fader.FadeIn(fadeTime);
        }

        public void Hide(float fadeTime = DefaultFadeTime) {
            OnWillHide();
            fader.FadeOut(fadeTime);
        }

        protected virtual void OnWillShow() { }
        protected virtual void OnWillHide() { }
        protected virtual void OnDidShow() { }
        protected virtual void OnDidHide() { }

    }

}
