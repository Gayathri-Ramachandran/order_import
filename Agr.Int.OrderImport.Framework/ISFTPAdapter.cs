using System;

namespace Agr.Int.HDShippingChargesFeed.Framework
{
    public interface ISFTPAdapter : IDisposable
    {
        void Upload();
    }
}
