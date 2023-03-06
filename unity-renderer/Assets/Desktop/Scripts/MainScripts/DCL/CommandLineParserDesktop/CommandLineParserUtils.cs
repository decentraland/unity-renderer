using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DCL
{
    public static class CommandLineParserUtils
    {
        public static int startPort = 7666;
        public static bool withSSL = true;

        public static void ParseArguments()
        {
            var arguments = System.Environment.GetCommandLineArgs();

            for (var i = 0; i < arguments.Length; ++i)
            {
                var argumentsLeft = arguments.Length - i - 1;
                var argument = arguments[i];

                if (argumentsLeft >= 1) // Arguments with at least 1 parameter
                {
                    switch (argument)
                    {
                        case "--port":
                            i++; // shift
                            startPort = int.Parse(arguments[i]);
                            break;
                        case "--no-ssl":
                            withSSL = false;
                            break;
                    }
                }
            }
        }
    }
}
