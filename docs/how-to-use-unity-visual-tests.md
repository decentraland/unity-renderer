## How to use Unity Visual Tests

### SDK 6 Components

#### Steps

1. Create a new test class that inherits from VisualTestsBase
2. Initialize the visual tests using `VisualTestsBase.InitVisualTestsScene(string)` passing the test name as parameter
3. Setup your scene as wanted and call `TestHelpers.TakeSnapshot(Vector3)`
4. Tag the method with the attribute `[VisualTest]`. This isn't used yet but will be used to streamline the baseline images creation.

The pngs will be named automatically using the `InitVisualTestsScene` parameter.

Example:

```
public class VisualTests : VisualTestsBase
{
    [UnityTest][VisualTest]
    public IEnumerator VisualTestStub()
    {
        yield return InitVisualTestsScene("VisualTestStub");

        // Set up scene

        yield return VisualTestHelpers.TakeSnapshot(new Vector3(10f, 10f, 0f));
    }
}
```

#### How to create visual tests baseline images

1. Create a new test inside the same class of the desired visual test. Give it the same name followed by `_Generate`.
2. call `VisualTestHelpers.GenerateBaselineForTest(IEnumerator)` inside the method, passing the actual test method as parameter.
3. remember to make the test `[Explicit]` or the test will give false positives

Example:

```
[UnityTest][Explicit]
public IEnumerator VisualTestStub_Generate()
{
    yield return VisualTestHelpers.GenerateBaselineForTest(VisualTestStub());
}
```

-------------------------

### SDK 7 Components

#### Steps

1. Create a new test class that inherits from `ECSVisualTestsBase`
2. Create an `IEnumerator` method and implement the test code inside (`VisualTestUtils.RepositionVisualTestsCamera` can be used to reposition the snapshot camera)
3. At the end of the method call `yield return VisualTestUtils.TakeSnapshot()` with its corresponding parameters to finally take the visual test snapshot
4. After the take snapshot call dispose of any handler or object that is not disposed of in the TearDown
5. Tag the method with the attributes: `[UnityTest, VisualTest]`
6. Create another method that will be used to create the baseline image of the previously-created visual test, add the attributes: `[UnityTest, VisualTest, Explicit]`  

Example:

```
public class SDK7SomeComponentVisualTest : ECSVisualTestsBase
{
    private const string SNAPSHOT_BASE_FILENAME = "SDK7_SomeComponentVisualTests_";

    // Manually run to generate baseline image for later comparisons
    [UnityTest, VisualTest, Explicit]
    public IEnumerator VisualTest1_Generate() { yield return VisualTestUtils.GenerateBaselineForTest(VisualTest1()); }

    [UnityTest, VisualTest]
    public IEnumerator VisualTest1()
    {
        Vector3 cameraPos = (Vector3.up * 10) + (Vector3.back * 4);
        VisualTestUtils.RepositionVisualTestsCamera( camera, cameraPos, cameraPos + Vector3.forward);
        
        // Implement here testing code

        yield return VisualTestUtils.TakeSnapshot(SNAPSHOT_BASE_FILENAME + "VisualTest1", camera);

        // Dispose here of any remaining handler, etc.
    }
}
```

#### How to create visual tests baseline images

Call the "generate" method of a test to override its baseline image that will be used to compare against future runs of that visual test.

In the previous example, running `VisualTest1_Generate` test in Unity Editor's Test Runner will generate the baseline image for `VisualTest1`. Then when `VisualTest1` is run, it will compare its snapshot against the baseline image. 