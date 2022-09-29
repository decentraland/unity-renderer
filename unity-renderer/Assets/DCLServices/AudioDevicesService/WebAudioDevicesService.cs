namespace DCL.Services
{
    public class WebAudioDevicesService : IAudioDevicesService
    {
        private readonly IAudioDevicesBridge bridge;

        public WebAudioDevicesService (IAudioDevicesBridge bridge) { this.bridge = bridge; }

        public string[] InputDevices { get; private set; }
        public string[] OutputDevices { get; private set; }

        public void Initialize()
        {
            if (bridge.AudioDevices == null)
                bridge.OnAudioDevicesRecieved += ChacheAudioDevices;
            else
                ChacheAudioDevices(bridge.AudioDevices);
        }

        public void Dispose() =>
            bridge.OnAudioDevicesRecieved -= ChacheAudioDevices;

        private void ChacheAudioDevices(AudioDevicesResponse audioDevicesResponse)
        {
            bridge.OnAudioDevicesRecieved -= ChacheAudioDevices;

            InputDevices = bridge.AudioDevices.inputDevices;
            OutputDevices = bridge.AudioDevices.outputDevices;
        }
    }

}