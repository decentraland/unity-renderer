using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DCL.LoadingScreen
{
    /// <summary>
    /// Loading screen tips provider. The responsibility of this class is to create the loading tips and provide them as needed
    /// </summary>
    public class LoadingScreenTipsController
    {

        private List<LoadingTip> currentLoadingTips;
        private int currentRandomIndex;

        public void LoadDefaults()
        {
            LoadingTip mana = new LoadingTip("MANA is Decentraland’s virtual currency. Use it to buy LAND and other premium items, vote on key policies and pay platform fees.", "TipsImages/ManaImg");
            LoadingTip marketplace = new LoadingTip("Buy and sell LAND, Estates, Avatar wearables and names in the Decentraland Marketplace: stocking the very best digital goods and paraphernalia backed by the ethereum blockchain.", "TipsImages/MarketplaceImg");
            LoadingTip land = new LoadingTip("Decentraland is made up of over 90,000 LANDs: virtual spaces backed by cryptographic tokens. Only LAND owners can determine the content that sits on their LAND.", "TipsImages/LandImg");
            LoadingTip wearable = new LoadingTip("Except for the default set of wearables you get when you start out, each wearable model has a limited supply. The rarest ones can get to be super valuable. You can buy and sell them in the Marketplace.", "TipsImages/WearablesImg");
            LoadingTip dao = new LoadingTip("Decentraland is the first fully decentralized virtual world. By voting through the DAO  ('Decentralized Autonomous Organization'), you are in control of the policies created to determine how the world behaves.", "TipsImages/DAOImg");
            LoadingTip genesis = new LoadingTip("Genesis Plaza is built and maintained by the Decentraland Foundation but is still in many ways a community project. Around here you'll find several teleports that can take you directly to special scenes marked as points of interest.", "TipsImages/GenesisPlazaImg");
            currentLoadingTips = new List<LoadingTip>() { mana, marketplace, land, wearable, dao, genesis };
            currentRandomIndex = Random.Range(0, currentLoadingTips.Count);
        }

        //TODO: We will use this method when the WORLDs loads downloaded images and tips. This will come with the
        // AboutResponse var on the RealmPlugin change
        public void LoadCustomTips(List<Tuple<string, Sprite>> customList)
        {
            currentLoadingTips = new List<LoadingTip>();
            foreach (var tipsTuple in customList)
            {
                currentLoadingTips.Add(new LoadingTip(tipsTuple.Item1, tipsTuple.Item2));
            }
            currentRandomIndex = Random.Range(0, currentLoadingTips.Count);
        }

        public LoadingTip GetNextLoadingTip()
        {
            currentRandomIndex = (currentRandomIndex + 1) % currentLoadingTips.Count;
            return currentLoadingTips[currentRandomIndex];
        }


        [Serializable]
        public class LoadingTip
        {
            public string text;
            public Sprite sprite;

            public LoadingTip(string text, string spriteURL)
            {
                this.text = text;
                this.sprite = Resources.Load<Sprite>(spriteURL);
            }

            public LoadingTip(string text, Sprite sprite)
            {
                this.text = text;
                this.sprite = sprite;
            }
        }
    }
}
