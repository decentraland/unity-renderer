public static class ProfanityFilterSharedInstances
{
    private static IProfanityFilter regexFilterInstance;

    public static IProfanityFilter regexFilter
    {
        get
        {
            return regexFilterInstance ??= new ThrottledRegexProfanityFilter(
                new ProfanityWordProviderFromResourcesJson("Profanity/badwords"), 20);
        }
    }
}