﻿using QuizApp.Domain.DTOs;
using QuizApp.Domain.Entities;
using System.Security.Claims;

namespace QuizApp.Domain.Interfaces.Services;

public interface IQuizService
{
    Task<(bool forbidden, bool notFound)> DeleteAsync(string id, ClaimsPrincipal claims);
    Task DeleteAsync();
    Task<List<QuizBase>> GetBaseInfoAsync();
    Task<List<Quiz>> GetAsync();
    Task<Quiz?> GetAsync(string id, bool shuffle);
    Task<IDictionary<string, string[]>?> InsertAsync(CreateQuizDTO newQuiz, ClaimsPrincipal claims);
}