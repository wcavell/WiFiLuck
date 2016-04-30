using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace WIFIManager.Controls
{
    public class PullToRefreshAdorner : DependencyObject
    {
        public static readonly DependencyProperty ExtenderProperty = DependencyProperty.RegisterAttached(
            "Extender", typeof(PullToRefreshExtender), typeof(PullToRefreshAdorner), new PropertyMetadata(default(PullToRefreshExtender)));

        public static void SetExtender(DependencyObject element, PullToRefreshExtender value)
        {
            (element.GetValue(ExtenderProperty) as PullToRefreshExtender)?.Detach(element);
            element.SetValue(ExtenderProperty, value);
            value?.Attach(element);
        }

        public static PullToRefreshExtender GetExtender(DependencyObject element)
        {
            return (PullToRefreshExtender)element.GetValue(ExtenderProperty);
        }
    }
}
