namespace DCL.Wallet
{
    public static class WalletUtils
    {
        public const float FETCH_MANA_INTERVAL = 60;

        public static string FormatBalanceToString(double balance)
        {
            return balance switch
                   {
                       >= 100000000 => (balance / 1000000D).ToString("0.#M"),
                       >= 1000000 => (balance / 1000000D).ToString("0.##M"),
                       >= 100000 => (balance / 1000D).ToString("0.#K"),
                       >= 10000 => (balance / 1000D).ToString("0.##K"),
                       < 0.001 => "0",
                       <= 1 => balance.ToString("0.###"),
                       < 100 => balance.ToString("0.##"),
                       _ => balance.ToString("#,0"),
                   };
        }
    }
}
