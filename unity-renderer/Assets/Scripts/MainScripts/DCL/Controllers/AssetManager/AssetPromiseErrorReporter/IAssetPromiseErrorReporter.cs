using System;

namespace AssetPromiseErrorReporter
{
    public interface IAssetPromiseErrorReporter
    {
        void Report(Exception exception);
    }
}