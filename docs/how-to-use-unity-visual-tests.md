### How to use Unity Visual Tests

#### Overview

TBD

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