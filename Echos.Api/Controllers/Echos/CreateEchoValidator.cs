namespace Echos.Api.Controllers.Echos
{
    public static class CreateEchoValidator
    {
        public static void Validate(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("Your echo cannot be empty.");
            }

            if (content.Length > 280)
            {
                throw new ArgumentException("Your echo cannot exceed 280 characters");
            }
        }
    }
}
