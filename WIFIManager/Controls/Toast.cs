using System;
using System.Diagnostics;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace ACTV.Controls
{
    public class Toast
    {
        private static readonly Color defaultForegroundColor = Color.FromArgb(255, 17, 117, 240);
        private static Color errorColor = Color.FromArgb(255, 255, 80, 17);
        private static readonly Color defaultBackgroundColor = Color.FromArgb(40, 0, 0, 0);

        public static void Show(string text, Action complete = null, double height = 60)
        {
            Show(text, defaultForegroundColor, defaultBackgroundColor, complete, height);
        }

        public static void ShowError(string text)
        {
            Show(text, errorColor, defaultBackgroundColor);
        }

        public static void Show(string text, Color foregroundColor, Color backgroundColor, Action complete = null,
            double height = 60, double life = 3, double Top = 20)
        {
            Show(text, new SolidColorBrush(foregroundColor), new SolidColorBrush(backgroundColor), complete, height, Top);
        }

        public static void Show(string text, Brush foregroundBrush, Brush backgroundBrush, Action complete = null,
            double height = 50, double life = 3, double Top = 20)
        {
            var textblock = new TextBlock
            {
                Text = text,
                TextWrapping = TextWrapping.Wrap,
                Foreground = foregroundBrush,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 20, // (double)Application.Current.Resources["PhoneFontSizeNormal"],
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20, Top, 20, 0)
            };
            var stack = new StackPanel //() new Border
            {
                Width = Window.Current.CoreWindow.Bounds.Width,
                Background = backgroundBrush,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            stack.Children.Add(textblock);
            var p = new Popup
            {

            };
            p.Child = stack;
            Show(p, complete, height);
        }

        private static void Show(Popup popup, Action complete = null, double height = 60, double life = 3)
        {
            try
            {
                if (!(popup.Child.RenderTransform is CompositeTransform))
                {
                    popup.Child.RenderTransform = new CompositeTransform();
                }
                ((CompositeTransform)popup.Child.RenderTransform).TranslateY = -height;

                Debug.Assert(popup.Child is FrameworkElement);

                var element = popup.Child as FrameworkElement;
                element.Height = height;

                var storyboard = new Storyboard
                {
                    AutoReverse = false
                };

                var doubleAnimaionUsingKeyFrames = new DoubleAnimationUsingKeyFrames();
                Storyboard.SetTarget(doubleAnimaionUsingKeyFrames, popup.Child);
                Storyboard.SetTargetProperty(doubleAnimaionUsingKeyFrames, "(UIElement.Opacity)");
                storyboard.Children.Add(doubleAnimaionUsingKeyFrames);

                //0.5秒透明度从0-1
                var doubleKeyFrame = new EasingDoubleKeyFrame
                {
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.5)),
                    Value = 1,
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                doubleAnimaionUsingKeyFrames.KeyFrames.Add(doubleKeyFrame);
                doubleKeyFrame = new EasingDoubleKeyFrame
                {
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(2.4)),
                    Value = 0.995,
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
                };
                doubleAnimaionUsingKeyFrames.KeyFrames.Add(doubleKeyFrame);
                doubleKeyFrame = new EasingDoubleKeyFrame
                {
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(3)),
                    Value = 0.1,
                };
                doubleAnimaionUsingKeyFrames.KeyFrames.Add(doubleKeyFrame);



                doubleAnimaionUsingKeyFrames = new DoubleAnimationUsingKeyFrames();
                Storyboard.SetTarget(doubleAnimaionUsingKeyFrames, popup.Child);
                Storyboard.SetTargetProperty(doubleAnimaionUsingKeyFrames,
                    "(UIElement.RenderTransform).(CompositeTransform.TranslateY)");
                storyboard.Children.Add(doubleAnimaionUsingKeyFrames);
                doubleKeyFrame = new EasingDoubleKeyFrame
                {
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.5)),
                    Value = 0,
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                doubleAnimaionUsingKeyFrames.KeyFrames.Add(doubleKeyFrame);
                doubleKeyFrame = new EasingDoubleKeyFrame
                {
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(2.4)),
                    Value = 0,
                };
                doubleAnimaionUsingKeyFrames.KeyFrames.Add(doubleKeyFrame);
                doubleKeyFrame = new EasingDoubleKeyFrame
                {
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(life)),
                    Value = -height,
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
                };
                doubleAnimaionUsingKeyFrames.KeyFrames.Add(doubleKeyFrame);

                var objectAnimaionUsingKeyFrames = new ObjectAnimationUsingKeyFrames();
                Storyboard.SetTarget(objectAnimaionUsingKeyFrames, popup);
                Storyboard.SetTargetProperty(objectAnimaionUsingKeyFrames, "(Popup.IsOpen)");
                storyboard.Children.Add(objectAnimaionUsingKeyFrames);

                var discreteObjectKeyFrame = new DiscreteObjectKeyFrame
                {
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0)),
                    Value = true
                };
                objectAnimaionUsingKeyFrames.KeyFrames.Add(discreteObjectKeyFrame);

                discreteObjectKeyFrame = new DiscreteObjectKeyFrame
                {
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(life)),
                    Value = false
                };
                objectAnimaionUsingKeyFrames.KeyFrames.Add(discreteObjectKeyFrame);

                if (complete != null)
                {
                    storyboard.Completed += (s, e) => complete.Invoke();
                }

                storyboard.Begin();
            }
            catch
            {

            }
        }

    }
}
