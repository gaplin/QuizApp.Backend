# QuizApp.Backend
[![.NET](https://github.com/gaplin/QuizApp.Backend/actions/workflows/dotnet.yml/badge.svg?kill_cache=1)](https://github.com/gaplin/QuizApp.Backend/actions/workflows/dotnet.yml)
[![Coverage Status](https://coveralls.io/repos/github/gaplin/QuizApp.Backend/badge.svg)](https://coveralls.io/github/gaplin/QuizApp.Backend)


ASP.NET Core Minimal Api.

## Requirements
- .NET8
- MongoDB connection (configuration in appsettings) *or* docker if using included docker-compose

## Features
- JWT tokens auth/generation
- Fluent Validation for DTO's
- Passwords hashed using Bcrypt
