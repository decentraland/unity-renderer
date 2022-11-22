using System;
using System.Collections.Generic;
using DCL;
using DCL.Helpers;
using Decentraland.Bff;
using UnityEngine;
using Variables.RealmsInfo;

public class GeneralDebugMetricModule : IDebugMetricModule
{

    private string currentNetwork = string.Empty;
    private string currentRealmValue = string.Empty;
    private int lastPlayerCount;
    private BaseDictionary<string, Player> otherPlayers => DataStore.i.player.otherPlayers;
    private BaseVariable<string> currentRealmName => DataStore.i.realm.realmName;

    private Promise<KernelConfigModel> kernelConfigPromise;


    
    public void SetUpModule(Dictionary<DebugValueEnum, Func<string>> updateValueDictionary)
    {
        updateValueDictionary.Add(DebugValueEnum.General_Network, () => currentNetwork.ToUpper());
        updateValueDictionary.Add(DebugValueEnum.General_Realm, () => currentRealmValue.ToUpper());
        updateValueDictionary.Add(DebugValueEnum.General_NearbyPlayers, () => lastPlayerCount.ToString());
    }

    public void UpdateModule() { }
    public void EnableModule()
    {
        lastPlayerCount = otherPlayers.Count();
        otherPlayers.OnAdded += OnOtherPlayersModified;
        otherPlayers.OnRemoved += OnOtherPlayersModified;
        
        SetupKernelConfig();
        SetupRealm();

    }
    public void DisableModule()
    {
        otherPlayers.OnAdded -= OnOtherPlayersModified;
        otherPlayers.OnRemoved -= OnOtherPlayersModified;
        currentRealmName.OnChange -= UpdateRealm;
        kernelConfigPromise.Dispose();
        KernelConfig.i.OnChange -= OnKernelConfigChanged;
    }
    
    private void OnOtherPlayersModified(string playerName, Player player)
    {
        lastPlayerCount = otherPlayers.Count();
    }
    
    private void OnKernelConfigChanged(KernelConfigModel current, KernelConfigModel previous)
    {
        OnKernelConfigChanged(current);
    }

    private void OnKernelConfigChanged(KernelConfigModel kernelConfig)
    {
        currentNetwork = kernelConfig.network ?? string.Empty;
    }
    
    private void SetupKernelConfig()
    {
        kernelConfigPromise = KernelConfig.i.EnsureConfigInitialized();
        kernelConfigPromise.Catch(Debug.Log);
        kernelConfigPromise.Then(OnKernelConfigChanged);
        KernelConfig.i.OnChange += OnKernelConfigChanged;
    }
    
    private void SetupRealm()
    {
        currentRealmName.OnChange += UpdateRealm;
        UpdateRealm(currentRealmName.Get(), null);
    }
    
    private void UpdateRealm(string current, string previous)
    {
        if (current == null) return;
        currentRealmValue = current ?? string.Empty;
    }

    public void Dispose() { }

}