public class NFTPromptModel
{
    public string contactAddress;
    public string comment;
    public string tokenId;

    public NFTPromptModel(string contactAddress, string tokenId, string comment)
    {
        this.contactAddress = contactAddress;
        this.tokenId = tokenId;
        this.comment = comment;
    }
}