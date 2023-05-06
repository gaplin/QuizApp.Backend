namespace QuizApp.Domain.Interfaces.Services;

internal interface IHashService
{
    string PasswordHash(string password);
    bool VerifyPassword(string password, string hash);
}