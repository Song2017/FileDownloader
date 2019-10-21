namespace Services.Infrastructure
{
    public interface IEncryptionService
    {
        string GetHash(string sourcePassword, string saltPa);
    }
}
