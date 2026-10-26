namespace SimRisorse.Core.Content;

internal static class ContentIdentifier
{
    public static void Validate(string value, string parameterName)
    {
        if (string.IsNullOrEmpty(value) || !char.IsAsciiLetterLower(value[0]) ||
            value.Any(character => !char.IsAsciiLetterLower(character) && !char.IsAsciiDigit(character) && character != '-'))
            throw new ArgumentException("Identifiers must start with a lowercase ASCII letter and contain letters, digits or hyphens.", parameterName);
    }
}
