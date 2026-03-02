using System;
using System.Collections;
using System.Collections.Generic;
using UAppToolKit.Animations;
using UAppToolKit.Animations.EasingFunction;
using UnityEngine;
using UnityEngine.UI;

namespace UAppToolKit.PageNavigation.Popup
{
    public class PopUpAnimationController : MonoBehaviour
    {
        public List<PopUpAnimation> AnimationElements;
        public PopUpAnimationSettings PopUpAnimationSettings;
        public event Action OnOpenAnimationCompleted;

        protected void Show()
        {
            foreach (var animationElement in AnimationElements)
            {
                animationElement.Transform.localScale = Vector3.zero;
            }
            StartCoroutine(PlayAnimations());

            ShowOpenAnimation(PopUpAnimationSettings);
        }
        
        private IEnumerator PlayAnimations()
        {
            yield return new WaitForEndOfFrame();
            foreach (var element in AnimationElements)
            {
                Play(element);
                yield return new WaitForSeconds(element.DelayUntilNextAnimation);
            }
        }

        private void Play(PopUpAnimation elem)
        {
            var storyboard = gameObject.AddComponent<Storyboard>();
            
            var scaleAnimation = new Vector2Animation
            {
                From = Vector2.zero,
                To = Vector2.one,
                Duration = TimeSpan.FromSeconds(0.3d),
                EasingFunction = new CurveEase { Curve = elem.Curve }
            };

            scaleAnimation.Tick += value =>
            {
                elem.Transform.localScale = (Vector2)value;
            };
            storyboard.Children.Add(scaleAnimation);
            storyboard.Begin();
        }

        private void ShowOpenAnimation(PopUpAnimationSettings settings)
        {
            var storyboard = gameObject.AddComponent<Storyboard>();

            foreach (var scaleAnimation in settings.ScaleAnimations)
            {
                if (scaleAnimation.Transform == null) continue;
                storyboard.Children.Add(BuildScaleAnimation(scaleAnimation));
            }

            foreach (var alphaAnimation in settings.AlphaAnimations)
            {
                if (alphaAnimation.Graphic == null) continue;
                storyboard.Children.Add(BuildAlphaAnimation(alphaAnimation));
            }

            storyboard.Completed += (sender, args) =>
            {
                if (OnOpenAnimationCompleted != null) OnOpenAnimationCompleted();
            };
            storyboard.Begin();
        }

        private SimpleAnimationBase BuildScaleAnimation(ScaleAnimationSettings setting)
        {
            var animTransform = setting.Transform;
            var zScale = animTransform.localScale.z;
            var xValueCurve = setting.XValueCurve;
            var yValueCurve = setting.YValueCurve;
            var scaleAnimation = new DoubleAnimation
            {
                From = 0, 
                To = 1,                
                Duration = TimeSpan.FromSeconds(setting.DurationSec),
                BeginTime = TimeSpan.FromSeconds(setting.BeginTimeSec)
            };

            scaleAnimation.Tick += time =>
            {
                var xScale = xValueCurve.Evaluate((float)time);
                var yScale = yValueCurve.Evaluate((float)time);
                animTransform.localScale = new Vector3(xScale, yScale, zScale);
            };

            return scaleAnimation;
        }

        private SimpleAnimationBase BuildAlphaAnimation(AlphaAnimationSettings setting)
        {
            var graphic = setting.Graphic;
            var baseColor = graphic.color;
            var value = setting.Value;
            var alphaAnimation = new DoubleAnimation
            {
                From = 0, 
                To = 1, 
                Duration = TimeSpan.FromSeconds(setting.DurationSec),
                BeginTime = TimeSpan.FromSeconds(setting.BeginTimeSec)
            };
            alphaAnimation.Tick += time =>
            {
                var alpha = value.Evaluate((float)time);
                graphic.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            };

            return alphaAnimation;
        }
    }

    [Serializable]
    public class PopUpAnimation
    {
        public RectTransform Transform;
        public AnimationCurve Curve;
        public float DelayUntilNextAnimation;
    }

    [Serializable]
    public class PopUpAnimationSettings
    {
        public List<ScaleAnimationSettings> ScaleAnimations;
        public List<AlphaAnimationSettings> AlphaAnimations;
    }

    [Serializable]
    public class ScaleAnimationSettings
    {
        public Transform Transform;
        public AnimationCurve XValueCurve;
        public AnimationCurve YValueCurve;
        public float DurationSec;
        public float BeginTimeSec;
    }

    [Serializable]
    public class AlphaAnimationSettings
    {
        public Graphic Graphic;
        public AnimationCurve Value;
        public float DurationSec;
        public float BeginTimeSec;
    }
}