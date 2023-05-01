using QuizApp.Domain.Entities;
using QuizApp.Domain.Interfaces.Orderers;

namespace QuizApp.Domain.Orderers;

internal class RandomQuizReorderer : IQuizReorderer
{
    private static readonly Random Random = new();
    public void Reorder(IList<Quiz> quizzes)
    {
        foreach (var question in quizzes.SelectMany(x => x.Questions))
        {
            ShuffleQuestion(question);
        }
        Shuffle(quizzes);
    }

    private static void ShuffleQuestion(Question question)
    {
        var correctAnswer = question.Answers[question.CorrectAnswer];
        Shuffle(question.Answers);
        question.CorrectAnswer = question.Answers.IndexOf(correctAnswer);
    }

    private static void Shuffle<T>(IList<T> values)
    {
        int n = values.Count;
        while (n > 1)
        {
            int k = Random.Next(n--);
            (values[k], values[n]) = (values[n], values[k]);
        }
    }
}