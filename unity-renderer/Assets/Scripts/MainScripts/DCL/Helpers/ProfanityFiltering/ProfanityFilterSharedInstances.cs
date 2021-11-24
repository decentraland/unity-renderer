public static class ProfanityFilterSharedInstances
{
    private static RegexProfanityFilter regexFilterInstance;

    public static RegexProfanityFilter regexFilter
    {
        get
        {
            return regexFilterInstance ??= new RegexProfanityFilter(
                new ProfanityWordProviderFromResourcesJson("Profanity/badwords"));
        }
    }
}