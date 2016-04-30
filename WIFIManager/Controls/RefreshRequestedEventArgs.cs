using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace WIFIManager.Controls
{
    public class RefreshRequestedEventArgs
    {
        private DeferralCompletedHandler _deferralCompletedHandler;
        private bool _isDeferred;

        internal bool IsDeferred => _isDeferred;

        internal RefreshRequestedEventArgs(DeferralCompletedHandler deferralCompletedHandler)
        {
            _deferralCompletedHandler = deferralCompletedHandler;
        }

        public bool Cancel { get; set; }

        public Deferral GetDeferral()
        {
            _isDeferred = true;
            return new Deferral(_deferralCompletedHandler);
        }
    }
}
