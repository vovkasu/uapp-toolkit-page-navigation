using System;
using System.Collections.Generic;
using UAppToolKit.Animations;
using UnityEngine;
using UnityEngine.UI;

namespace UAppToolKit.PageNavigation.Loading
{
    public class LoadingScreen : MonoBehaviour
    {
        public Image Background;

        public List<Graphic> Items = new List<Graphic>();

        public float AnmationDurationSec = 0.5f;
        public AnimationCurve FadeInBackgroundAlphaCurve;
        public AnimationCurve FadeInForegroundAlphaCurve;

        public AnimationCurve FadeOutBackgroundAlphaCurve;
        public AnimationCurve FadeOutForegroundAlphaCurve;

        public virtual void Init()
        {
            gameObject.SetActive(false);
        }

        public void FadeIn(Action onComplete)
        {
            gameObject.SetActive(true);
            var storyboard = BuildAnimation(
                FadeInBackgroundAlphaCurve,
                FadeInForegroundAlphaCurve, 
                onComplete);
            storyboard.Begin();
        }

        public void FadeOut(Action onComplete)
        {
            if(!gameObject.activeSelf) return;

            var storyboard = BuildAnimation(
                FadeOutBackgroundAlphaCurve,
                FadeOutForegroundAlphaCurve,
                () =>
                {
                    if (onComplete != null)
                    {
                        onComplete();
                    }

                    gameObject.SetActive(false);
                });

            storyboard.Begin();
        }

        private Storyboard BuildAnimation(AnimationCurve backgroundAlphaCurve, AnimationCurve foregroundAlphaCurve,
            Action callback)
        {
            var storyboard = gameObject.AddComponent<Storyboard>();
            var alphaAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(AnmationDurationSec)
            };

            var backgroundColor = Background.color;
            backgroundColor.a = backgroundAlphaCurve.Evaluate(0f);
            Background.color = backgroundColor;

            alphaAnimation.Tick += t =>
            {
                var time = (float)t;
                var color = Background.color;
                color.a = backgroundAlphaCurve.Evaluate(time);
                Background.color = color;

                var alpha = foregroundAlphaCurve.Evaluate(time);
                foreach (var graphic in Items)
                {
                    if (graphic == null) continue;
                    var graphicColor = graphic.color;
                    graphicColor.a = alpha;
                    graphic.color = graphicColor;
                }
            };
            storyboard.Children.Add(alphaAnimation);
            storyboard.Completed += (sender, args) =>
            {
                if (callback != null) callback();
            };
            return storyboard;
        }
    }
}