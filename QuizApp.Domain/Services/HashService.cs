using QuizApp.Domain.Interfaces.Services;
using BC = BCrypt.Net.BCrypt;

namespace QuizApp.Domain.Services;

internal class HashService : IHashService
{
    public string PasswordHash(string password)
    {
        var hash = BC.EnhancedHashPassword(password);
        return hash;
    }

    public bool VerifyPassword(string password, string hash)
    {
        var match = BC.EnhancedVerify(password, hash);
        return match;
    }
}