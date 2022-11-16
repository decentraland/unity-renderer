using System;
using DCL.Interface;

namespace DCL.FatalErrorReporter
{
    public class WebFatalErrorReporter : IFatalErrorReporter
    {
        public void Report(Exception exception)
        {
            WebInterface.ReportAvatarFatalError(exception.ToString());
        }
    }
}