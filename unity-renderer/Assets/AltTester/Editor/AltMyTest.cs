
using System;
using UnityEngine;

namespace Altom.AltTesterEditor
{
    [System.Serializable]
    public class AltMyTest
    {
        [SerializeField]
        private bool _selected;
        [SerializeField]
        private string _testName;
        [SerializeField]
        private string _testAssembly;
        [SerializeField]
        private int _status;
        [SerializeField]
        private bool _isSuite;
        [SerializeField]
        private string _type;
        [SerializeField]
        private string _parentName;
        [SerializeField]
        private int _testCaseCount;
        [SerializeField]
        private bool _foldOut;
        [SerializeField]
        private string _testResultMessage;
        [SerializeField]
        private string _testStackTrace;
        [SerializeField]
        private Double _testDuration;
        [SerializeField]
        private string path;
        [SerializeField]
        private int _testSelectedCount;

        public AltMyTest(bool selected, string testName, string testAssembly, int status, bool isSuite, string type, string parentName, int testCaseCount, bool foldOut, string testResultMessage, string testStackTrace, Double testDuration, string path, int testSelectedCount)
        {
            _selected = selected;
            _testName = testName;
            _testAssembly = testAssembly;
            _status = status;
            _isSuite = isSuite;
            _type = type;
            _parentName = parentName;
            _testCaseCount = testCaseCount;
            _foldOut = foldOut;
            _testResultMessage = testResultMessage;
            _testStackTrace = testStackTrace;
            _testDuration = testDuration;
            _testSelectedCount = testSelectedCount;
            this.path = path;
        }

        public bool Selected
        {
            get
            {
                return _selected;
            }

            set
            {
                _selected = value;
            }
        }

        public string TestName
        {
            get
            {
                return _testName;
            }

            set
            {
                _testName = value;
            }
        }

        public string TestAssembly
        {
            get
            {
                return _testAssembly;
            }

            set
            {
                _testAssembly = value;
            }
        }

        public int Status
        {
            get
            {
                return _status;
            }

            set
            {
                _status = value;
            }
        }

        public bool IsSuite
        {
            get
            {
                return _isSuite;
            }

            set
            {
                _isSuite = value;
            }
        }

        public string Type
        {
            get
            {
                return _type;
            }

            set
            {
                _type = value;
            }
        }

        public string ParentName
        {
            get
            {
                return _parentName;
            }

            set
            {
                _parentName = value;
            }
        }

        public int TestCaseCount
        {
            get
            {
                return _testCaseCount;
            }

            set
            {
                _testCaseCount = value;
            }
        }
        public int TestSelectedCount
        {
            get
            {
                return _testSelectedCount;
            }
            set
            {
                _testSelectedCount = value;
            }
        }

        public bool FoldOut
        {
            get
            {
                return _foldOut;
            }

            set
            {
                _foldOut = value;
            }
        }

        public string TestResultMessage
        {
            get
            {
                return _testResultMessage;
            }

            set
            {
                _testResultMessage = value;
            }
        }

        public string TestStackTrace
        {
            get
            {
                return _testStackTrace;
            }

            set
            {
                _testStackTrace = value;
            }
        }

        public Double TestDuration
        {
            get
            {
                return _testDuration;
            }

            set
            {
                _testDuration = value;
            }
        }

        public string Path
        {
            get
            {
                return path;
            }

            set
            {
                path = value;
            }
        }
    }
}