using System;

namespace DCL.FatalErrorReporter
{
    public interface IFatalErrorReporter
    {
        void Report(Exception exception);
    }
}