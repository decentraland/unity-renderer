namespace DCL
{
    public interface IGifProcessor
    {

        /// <summary>
        /// Notify processor that the gif is disposed.
        /// If using UniGif plugin we just cancel the download if pending
        /// If using webworker we send a message to kernel to cancel download and/or remove created texture from memory
        /// </summary>
        void DisposeGif();
    }
}