# How to create a new UI Component

Please follow these steps as reference to create a new UI Component:

## 1. Create the UI Component prefab

The first step would be simply to create the visual part of the UI Component. We would create a prefab with the corresponding visual design as usual. This prefab can contain any Unity component that give it the behaviour we need.

If the new UI Component is to be part of our common library (because it could be something that could be used in different places of our application), we would locate the prefab in the folder `Assets/UIComponents/Resources`. However, if we know that the new UI Component will be something specific for a particular feature, we would locate it in the own feature folder. If we need to use this UI Component for more than one feature in the future, we would move the corresponding assets to the common library folders.

![Untitled](how-to-create-new-components/Untitled.png)

![Untitled](how-to-create-new-components/Untitled%201.png)

## 2. Implement the UI Component behaviour (model and logic)

Once we have created the visual part of our new UI Component, we need to implement its logic. For that, in the `Assets/UIComponents/Scripts/Components`, we are going to create a new folder with the new UI Component name. Here we are going to create 2 scripts:

- `<UIComponent name>ComponentModel.cs` (*if needed*): It will contain the **model** of the new UI Component.
- `<UIComponent name>ComponentView.cs`: It will contain the **logic** of the new UI Component.

### The model

The model will be a `Serializable` class that will contain all the needed data that we want to configure for the UI Component. This class must inherits from `BaseComponentModel`:

```csharp
using System;
using UnityEngine;

[Serializable]
public class ButtonComponentModel : BaseComponentModel
{
    public string text;
    public Sprite icon;
}
```

### The logic

This class will contain all the needed logic that will make the UI Component behaves as we expect. It must inherits from `BaseComponentView` and implements its own interface that will contain all the methods/properties we want to expose. The logic class can contain any reference from the prefab we need to manage in the code and also should expose the model in its inspector:

```csharp
public interface IButtonComponentView
{
    /// <summary>
    /// Event that will be triggered when the button is clicked.
    /// </summary>
    Button.ButtonClickedEvent onClick { get; }

    /// <summary>
    /// Set the button text.
    /// </summary>
    /// <param name="newText">New text.</param>
    void SetText(string newText);

    /// <summary>
    /// Set the button icon.
    /// </summary>
    /// <param name="newIcon">New Icon. Null for hide the icon.</param>
    void SetIcon(Sprite newIcon);
}

public class ButtonComponentView : BaseComponentView, IButtonComponentView
{
    [Header("Prefab References")]
    [SerializeField] internal Button button;
    [SerializeField] internal TMP_Text text;
    [SerializeField] internal Image icon;

    [Header("Configuration")]
    [SerializeField] internal ButtonComponentModel model;

    public Button.ButtonClickedEvent onClick => button?.onClick;

    public void SetText(string newText)
    {
        model.text = newText;

        if (text == null)
            return;

        text.text = newText;
    }

    public void SetIcon(Sprite newIcon)
    {
        model.icon = newIcon;

        if (icon == null)
            return;

        icon.gameObject.SetActive(newIcon != null);
        icon.sprite = newIcon;
    }
}
```

In order to make the UI Component easier to configure from Unity editor (for example for designers that want to configure the UI Component without the needed of implement any code), the `BaseComponentView` contains a **[REFRESH]** button in its inspector that will ease us to update the state of the UI with the data currently set in the model.

To make it possible, you will notice the `BaseComponentView` base class force us to implement the `RefreshControl()` method. This method should be the responsible of refreshing the whole UI correctly:

```csharp
public class ButtonComponentView : BaseComponentView, IButtonComponentView
{
		// ...

    public override void **RefreshControl**()
    {
        if (model == null)
            return;

        SetText(model.text);
        SetIcon(model.icon);
    }

		// ...
}
```

![Untitled](how-to-create-new-components/Untitled%202.png)

Finally, if we want our UI Component exposes a method that fully configure the whole component directly with a model (it is useful for cases when we want to set all the configuration of an UI Component from code in one go), we need to implement the interface `IComponentModelConfig` which will force us to implement the method `Configure()`. This method only should update the `model` variable with the incoming data and call the `RefreshControl()` method:

```csharp
public class ButtonComponentView : BaseComponentView, IButtonComponentView, **IComponentModelConfig**
{
    // ...

    public void **Configure**(BaseComponentModel newModel)
    {
        model = (ButtonComponentModel)newModel;
        RefreshControl();
    }

    // ...
}
```

## 3. Implement the tests coverage for the UI Component

In this step we will already have a new UI Component ready to use in any place of our application. In order to keep its implementation safe and maintainable, we need to create the corresponding unit tests. For that, we are going to create a new test script called `<UIComponent name>ComponentViewTests.cs` in `Assets/UIComponents/Tests` folder and add the needed tests for covering the whole implementation of the new UI Component.

```csharp
using NUnit.Framework;
using UnityEngine;

public class ButtonComponentViewTests
{
    [SetUp]
    public void SetUp() { ... }

    [TearDown]
    public void TearDown() { ... }

    [Test]
    public void SetOnClickCorrectly() { ... }

    [Test]
    public void ConfigureButtonCorrectly() { ... }

    [Test]
    public void SetButtonTextCorrectly() { ... }

    [Test]
    public void SetButtonIconCorrectly() { ... }
}
```

## 4. Create documentation for the UI Component

As last point, in order to keep an updated documentation about our UI Components Library, we should add the corresponding documentation here:

[UI Components Reference](ui-components-reference.md)