using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace EN.ListUpdater
{

    public partial class PulToRefreshListBox : UserControl, IDisposable
    {
        #region Dependency Objects

        public static readonly DependencyProperty ListBoxComponentProperty = DependencyProperty.Register("ListBoxComponent", typeof(ListBox), typeof(PulToRefreshListBox), new PropertyMetadata(default(ListBox)));

        public static readonly DependencyProperty HeaderComponentProperty = DependencyProperty.Register("HeaderComponnet", typeof(FrameworkElement), typeof(PulToRefreshListBox), new PropertyMetadata(default(StackPanel)));

        #endregion

        private Storyboard goBackAnimation;

        private ScrollViewer scrollViewer = null;

        private bool moveStarted = false;
        private double startY = 0.0;

        private bool __isActivatedValue = false;
        private bool isActivated
        {
            get
            {
                return __isActivatedValue;
            }
            set
            {
                if (value)
                {
                    if (OnHeaderFullHeight != null)
                        OnHeaderFullHeight(this, null);
                }
                else
                {
                    if (OnHeaderHidden != null)
                        OnHeaderHidden(this, null);
                }

                __isActivatedValue = value;
            }
        }

        /// <summary>
        /// Fired when Header View is displayed in full height
        /// </summary>
        public event EventHandler OnHeaderFullHeight;
        /// <summary>
        /// Fired when Header View was displayed in full height and than height is scaled down, so Header View is not in full height
        /// </summary>
        public event EventHandler OnHeaderHidden;
        /// <summary>
        /// Fired when Header View was in full height and touch was released
        /// </summary>
        public event EventHandler OnActivatedDrop;

        #region External Components

        /// <summary>
        /// ListBox which will be displayed
        /// </summary>
        public ListBox ListBoxComponent {
            get
            {
                return (ListBox)GetValue(ListBoxComponentProperty);
            }
            set
            {
                ListBox oldLB = ListBoxComponent; 
                if (LayoutRoot.Children.Contains(oldLB) && oldLB != value)
                {
                    LayoutRoot.Children.Remove(oldLB);
                }

                if (!LayoutRoot.Children.Contains(value))
                {
                    Grid.SetRow(value, 1);
                    LayoutRoot.Children.Add(value);
                }

                SetValue(ListBoxComponentProperty, value);
            }
        }

        /// <summary>
        /// Header which will appear on top of scroll view
        /// </summary>
        public FrameworkElement HeaderComponnet
        {
            get
            {
                return (FrameworkElement)GetValue(HeaderComponentProperty);
            }
            set
            {
                FrameworkElement oldElement = HeaderComponnet;
                if (LayoutRoot.Children.Contains(oldElement) && oldElement != value)
                {
                    LayoutRoot.Children.Remove(oldElement);
                }

                if (!LayoutRoot.Children.Contains(value))
                {
                    Grid.SetRow(value, 0);
                    LayoutRoot.Children.Add(value);
                    value.Height = 0;
                }

                SetValue(HeaderComponentProperty, value);
            }
        }

        #endregion

        private double touchSensebility = 0.25;
        /// <summary>
        /// Value which determine sensibility of drag gesture
        /// </summary>
        public double TouchSensebility
        {
            get
            {
                return touchSensebility;
            }
            set
            {
                if (0 > value && value <= 1)
                {
                    touchSensebility = value;
                }
            }
        }

        public PulToRefreshListBox()
        {
            InitializeComponent();

            Touch.FrameReported += Touch_FrameReported;

        }

        void Touch_FrameReported(object sender, TouchFrameEventArgs e)
        {
            if (!(ListBoxComponent.ActualHeight > 0))
            {
                return;
            }

            foreach (TouchPoint _touchPoint in e.GetTouchPoints(ListBoxComponent))
            {
                if (_touchPoint.Action == TouchAction.Down)
                {
                    if (scrollViewer == null)
                    {
                        if (ListBoxComponent != null)
                            scrollViewer = FindScrollViewer(this);

                        if (scrollViewer == null)
                            return;
                    }
                    
                }
                else if (_touchPoint.Action == TouchAction.Move && e.GetPrimaryTouchPoint(ListBoxComponent) != null)
                {
                    if (!moveStarted && scrollViewer != null)
                    {
                        var offset = scrollViewer.VerticalOffset;

                        if (offset < 0.0001)
                        {
                            if(goBackAnimation != null)
                                goBackAnimation.SkipToFill();

                            moveStarted = true;
                            startY = _touchPoint.Position.Y;
                        }
                    }

                    if (moveStarted)
                    {
                        double visibleHeight = (_touchPoint.Position.Y - startY) * touchSensebility;

                        if (HeaderComponnet != null)
                        {
                            double newHeight = HeaderComponnet.Height + visibleHeight;
                            
                            if (newHeight < 0) {
                                newHeight = 0;
                            }

                            bool inFullHeight = false;

                            if (newHeight >= HeaderComponnet.ActualHeight) {
                                newHeight = HeaderComponnet.ActualHeight;

                                inFullHeight = true;

                                if (!isActivated)
                                {
                                    isActivated = true;
                                }
                            }

                            if (!inFullHeight && isActivated)
                            {
                                isActivated = false;
                            }

                            HeaderComponnet.Height = newHeight;
                        }

                        startY = _touchPoint.Position.Y;
                    }
                } else if (_touchPoint.Action == TouchAction.Up) {
                    moveStarted = false;
                    startY = 0.0f;

                    if (isActivated)
                    {
                        if (OnActivatedDrop != null)
                            OnActivatedDrop(this, null);

                        isActivated = false;
                    }

                    if (HeaderComponnet.Height > 0)
                    {
                        goBackAnimation = CreateGoBackAnimation();
                        goBackAnimation.Begin();
                    }

                }
            }
        }

        private Storyboard CreateGoBackAnimation()
        {
            var storyboard = new Storyboard();

            var goBackAnimation = new DoubleAnimation();
            Storyboard.SetTarget(goBackAnimation, HeaderComponnet);
            Storyboard.SetTargetProperty(goBackAnimation, new PropertyPath(FrameworkElement.HeightProperty));
            goBackAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(500));
            goBackAnimation.To = 0.0;

            storyboard.Children.Add(goBackAnimation);

            return storyboard;
        }

        private ScrollViewer FindScrollViewer(DependencyObject parent)
        {
            var childCount = VisualTreeHelper.GetChildrenCount(parent);

            for (var i = 0; i < childCount; i++)
            {
                var elt = VisualTreeHelper.GetChild(parent, i);
                if (elt is ScrollViewer) return (ScrollViewer)elt;

                var result = FindScrollViewer(elt);
                if (result != null) return result;
            }

            return null;
        }

        #region IDisposable

        public void Dispose()
        {
            Touch.FrameReported -= Touch_FrameReported;
        }

        #endregion
    }
}
