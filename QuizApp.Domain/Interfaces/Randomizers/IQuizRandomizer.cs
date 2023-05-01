using QuizApp.Domain.Entities;

namespace QuizApp.Domain.Interfaces.Randomizers;

internal interface IQuizRandomizer
{
    void Randomize(Quiz quiz);
}