### Javascript-Unity Communication Performance Test
We ran numerous consecutive function calls starting at the built-WebGL html javascript, communicating with a unity scene object script, measuring the time it took to run every call and back.

#### Test Results
A built-in javascript library function on the project calls a unity-instanced object script method passing a 1024 byte string parameter.

* 1000 calls take ~65 milliseconds
* 100000 calls take ~6384 milliseconds

#### Test Implementation

##### Javascript code in HTML (with embedded unity-webgl player)
```javascript
var functionCalls = 100000;

startPerformanceTest = function (intendedFunctionCalls = 100000) {
	functionCalls = intendedFunctionCalls;

	// Pre-warm
	window.unityCallsPerformanceTester.runTestFunction();

	window.performanceTestStartTime = performance.now();
	window.testFunctionCallsCounter = 0;

	for (let index = 0; index < functionCalls; index++) {
			window.unityCallsPerformanceTester.runTestFunction();
	}
};

window.finishTestFunctionRun = function () {
	window.testFunctionCallsCounter++;

	if (window.testFunctionCallsCounter == functionCalls) {
			window.performanceTestFinishTime = performance.now();

			console.log("Performance test time (" + functionCalls + " calls): " + (window.performanceTestFinishTime - window.performanceTestStartTime));
	}
};
```

##### Javascript code in Unity Project (as a JSLib)
```javascript
mergeInto(LibraryManager.library, {
  InitializePerformanceTests: function () {
    window.unityCallsPerformanceTester = {
      runTestFunction: function () {
        SendMessage("SceneController", "StartTestFunctionRun", "x".repeat(1024));
      }
    };
  },
  FinishTestFunctionRun: function () {
    window.finishTestFunctionRun();
	}
}
```

##### CSharp code in Unity Scene's object called "SceneController"
```cs
[DllImport("__Internal")] static extern void InitializePerformanceTests();
[DllImport("__Internal")] static extern void FinishTestFunctionRun();

void Awake() {
	InitializePerformanceTests();
}

public void StartTestFunctionRun(string strigParam) {
	FinishTestFunctionRun();
}
```

#### Test Running
1. Load html with built Unity-Webgl player and wait for the scene to load.
2. Execute from console the startPerformanceTest();