using System.Collections.Generic;

public class ProfileValidator
{
    private string catalystPublicKey; // FD:: this needs to be fetched

    /// <summary>
    /// Calls appropriate functions to validate the profile
    /// </summary>
    /// <param name="model"></param>
    /// <param name="checksum"></param>
    /// <param name="signedChecksum"></param>
    /// <param name="catalystUrl"></param>
    /// <returns></returns>
    public bool ValidateUserProfile(UserProfileModel model, string checksum, string signedChecksum, string catalystUrl)
    {
        if (IsTrustedCatalyst(catalystUrl))
        {
            if (VerifyChecksum(model, checksum) && VerifySignature(catalystPublicKey, checksum, signedChecksum))
            {
                // Profile is valid
                return true;
            }
        }
        // Profile is invalid or the Catalyst is untrusted
        return false;
    }

    /// <summary>
    /// Check if the Catalyst is trusted
    /// </summary>
    /// <param name="catalystUrl"></param>
    /// <returns></returns>
    private static bool IsTrustedCatalyst(string catalystUrl)
    {
        // FD:: placeholder for Catalyst validation
        return true;
    }

    /// <summary>
    /// Verify the checksum against the profile data
    /// </summary>
    /// <param name="model"></param>
    /// <param name="checksum"></param>
    /// <returns></returns>
    private bool VerifyChecksum(UserProfileModel model, string checksum)
    {
        // Implement checksum logic here
        string calculatedChecksum = CalculateChecksum(model.name, model.avatar.emotes, model.avatar.wearables);
        return calculatedChecksum == checksum;
    }

    /// <summary>
    /// Calculate the checksum from the profile information
    /// </summary>
    /// <param name="name"></param>
    /// <param name="emotes"></param>
    /// <param name="wearables"></param>
    /// <returns></returns>
    private string CalculateChecksum(string name, List<AvatarModel.AvatarEmoteEntry> emotes, List<string> wearables)
    {
        // Calculate the checksum from the profile information
        // FD:: this is a stupid example
        return HashString(name + string.Join("", emotes) + string.Join("", wearables));
    }

    /// <summary>
    /// Verify the signature of a hash
    /// </summary>
    /// <param name="publicKey"></param>
    /// <param name="hash"></param>
    /// <param name="signedHash"></param>
    /// <returns>true if verified</returns>
    public static bool VerifySignature(string publicKey, string hash, string signedHash)
    {
        // FD:: this is mocked and commented because we would need to add the Nethereum dependency

        // var signer = new EthereumMessageSigner();
        // string recoveredAddress = signer.EcRecover(hash, signedHash);
        // return recoveredAddress.ToLower() == publicKey.ToLower();

        return true;
    }

    /// <summary>
    /// Gets the hash of a string using SHA256
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string HashString(string input)
    {
        // FD:: this is mocked and commented because we would need to add the Nethereum dependency

        // using (SHA256 sha256Hash = SHA256.Create())
        // {
        //     byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
        //     return bytes.ToHex();
        // }

        return "hash0123456789";
    }
}
