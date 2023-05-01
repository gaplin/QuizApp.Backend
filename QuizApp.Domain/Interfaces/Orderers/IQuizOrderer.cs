using QuizApp.Domain.Entities;

namespace QuizApp.Domain.Interfaces.Orderers;

internal interface IQuizReorderer
{
    void Reorder(IList<Quiz> quizzes);
}