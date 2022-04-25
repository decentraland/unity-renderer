public static class ProfanityFilterSharedInstances
{
    // Check https://github.com/decentraland/unity-renderer/issues/2201 for more info about this number
    private const int PARTITION_SIZE = 20;
    
    private static IProfanityFilter regexFilterInstance;

    public static IProfanityFilter regexFilter
    {
        get
        {
            return regexFilterInstance ??= new ThrottledRegexProfanityFilter(
                new ProfanityWordProviderFromResourcesJson("Profanity/badwords"), PARTITION_SIZE);
        }
    }
}