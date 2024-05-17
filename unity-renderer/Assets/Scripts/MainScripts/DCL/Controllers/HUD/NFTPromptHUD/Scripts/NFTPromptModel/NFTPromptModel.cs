public class NFTPromptModel
{
    public string contactAddress;
    public string comment;
    public string tokenId;
    public string chain;

    public NFTPromptModel(string chain, string contactAddress, string tokenId, string comment)
    {
        this.chain = chain;
        this.contactAddress = contactAddress;
        this.tokenId = tokenId;
        this.comment = comment;
    }
}