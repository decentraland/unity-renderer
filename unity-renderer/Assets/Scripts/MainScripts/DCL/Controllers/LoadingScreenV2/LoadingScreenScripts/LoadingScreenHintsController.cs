using System;

namespace DCL.Controllers.LoadingScreenV2
{
    /// <summary>
    ///  - The view should receive a list of IHint and show them.
    ///  - The view should contain the max amount of hints that can be displayed, and they should be set up after the list of IHint arrives. We could also use a pool.
    ///  - All HintViews should initialize as disabled and hidden (no text, no image)
    ///  - If the list of hints is empty or the amount is less than the max amount, we should disable the rest of the HintViews.
    ///  - When the loading is finished, this class should handle the disposal of the IHint and their textures.
    ///  - When the loading screen is triggered again, we should make sure that old Hints are not loaded or shown.
    ///  - The hints carousel goes to the next hints after a few (n) seconds.
    ///  - The hints carousel allows user input from keys (A or D) to go to the next or previous hint.
    ///  - The hints carousel allows mouse input to select a specific hint.
    ///  - When any hint changes, the next hint timer gets reset.
    /// </summary>
    public class LoadingScreenHintsController
    {

    }
}

