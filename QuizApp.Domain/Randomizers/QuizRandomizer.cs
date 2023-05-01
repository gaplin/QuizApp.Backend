using QuizApp.Domain.Entities;
using QuizApp.Domain.Interfaces.Randomizers;

namespace QuizApp.Domain.Randomizers;

internal class QuizRandomizer : IQuizRandomizer
{
    private static readonly Random Random = new();
    public void Randomize(Quiz quiz)
    {
        foreach (var question in quiz.Questions)
        {
            ShuffleQuestion(question);
        }
        Shuffle(quiz.Questions);
    }

    private static void ShuffleQuestion(Question question)
    {
        var correctAnswer = question.Answers[question.CorrectAnswer];
        Shuffle(question.Answers);
        question.CorrectAnswer = question.Answers.IndexOf(correctAnswer);
    }

    private static void Shuffle<T>(IList<T> values)
    {
        var n = values.Count;
        while (n > 1)
        {
            int k = Random.Next(n--);
            (values[k], values[n]) = (values[n], values[k]);
        }
    }
}