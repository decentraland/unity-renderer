/// <summary>
/// Plugin feature that initialize the ExploreV2 feature.
/// </summary>
public class TransactionFeature : IPlugin
{
    public readonly ITransactionHUDController transactionHUDController;

    public TransactionFeature()
    {
        transactionHUDController = CreateController();
        transactionHUDController.Initialize();
    }

    internal virtual ITransactionHUDController CreateController() => new TransactionHUDController();

    public void Dispose()
    {
        transactionHUDController.Dispose();
    }
}