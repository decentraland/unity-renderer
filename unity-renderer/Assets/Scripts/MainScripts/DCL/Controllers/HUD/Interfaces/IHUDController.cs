using System;

public interface IHUDController : IDisposable
{
    void ConfigureHUDElement(HUDElementID hudElementId, HUDConfiguration configuration, string extraPayload = null);
    IHUD GetHUDElement(HUDElementID id);
    void Cleanup();
}