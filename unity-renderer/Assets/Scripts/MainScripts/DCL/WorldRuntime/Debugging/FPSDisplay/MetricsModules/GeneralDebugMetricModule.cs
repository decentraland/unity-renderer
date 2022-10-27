using System;
using System.Collections.Generic;
using DCL;
using DCL.Helpers;
using UnityEngine;
using Variables.RealmsInfo;

public class GeneralDebugMetricModule : IDebugMetricModule
{

    private string currentNetwork = string.Empty;
    private string currentRealmValue = string.Empty;
    private int lastPlayerCount;
    private BaseDictionary<string, Player> otherPlayers => DataStore.i.player.otherPlayers;
    private CurrentRealmVariable currentRealm => DataStore.i.realm.playerRealm;
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
        currentRealm.OnChange -= UpdateRealm;
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
        currentRealm.OnChange += UpdateRealm;
        UpdateRealm(currentRealm.Get(), null);
    }
    
    private void UpdateRealm(CurrentRealmModel current, CurrentRealmModel previous)
    {
        if (current == null) return;
        currentRealmValue = current.serverName ?? string.Empty;
    }

    public void Dispose() { }

}