using QuizApp.Infrastructure.DbModels;

namespace QuizApp.Tests.TestsUtils;

internal static class RandomDataProvider
{
    internal static UserModel UserModel(EUserTypeModel userType)
    {
        return new UserModel
        {
            HPassword = Path.GetRandomFileName(),
            Login = Path.GetRandomFileName(),
            UserName = Path.GetRandomFileName(),
            UserType = userType
        };
    }

    internal static QuizModel QuizModelWithoutQuestions(string? authorName = null, string? authorId = null)
    {
        authorName ??= Path.GetRandomFileName();
        authorId ??= Path.GetRandomFileName();
        return new QuizModel
        {
            Author = authorName,
            Category = Path.GetRandomFileName(),
            AuthorId = authorId,
            Title = Path.GetRandomFileName(),
            Questions = []
        };
    }

    internal static List<QuizModel> QuizModels(int count)
    {
        var result = new List<QuizModel>();
        while (count-- > 0)
        {
            var quiz = QuizModel(Path.GetRandomFileName(), Path.GetRandomFileName(), Random.Shared.Next(1, 5));
            result.Add(quiz);
        }
        return result;
    }

    internal static QuizModel QuizModel(string authorName, string authorId, int numOfQuestions)
    {
        return new QuizModel
        {
            Author = authorName,
            Category = Path.GetRandomFileName(),
            AuthorId = authorId,
            Title = Path.GetRandomFileName(),
            Questions = Enumerable.Range(0, numOfQuestions).Select(_ => CreateQuestion(Random.Shared.Next(1, 5))).ToList()
        };
    }

    private static QuestionModel CreateQuestion(int numOfAnswers)
    {
        var question = new QuestionModel
        {
            Text = Path.GetRandomFileName(),
            CorrectAnswer = Random.Shared.Next(0, numOfAnswers - 1),
            Answers = Enumerable.Range(0, numOfAnswers).Select(_ => Path.GetRandomFileName()).ToList()
        };
        return question;
    }
}