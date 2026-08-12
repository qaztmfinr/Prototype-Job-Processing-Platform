# Contributing Guidelines

Thank you for contributing to the Job Processing Platform! Please follow these guidelines.

## Code of Conduct

Be respectful, inclusive, and professional in all interactions.

## How to Contribute

### Reporting Bugs
1. Check existing issues first
2. Include:
   - Clear description
   - Steps to reproduce
   - Expected vs actual behavior
   - Screenshots/logs if applicable
   - Environment (OS, .NET version, Docker version)

### Suggesting Features
1. Describe the feature
2. Explain the use case
3. Provide examples if possible
4. Discuss implementation approach

### Submitting Pull Requests

1. **Create a Fork** and clone locally
2. **Create Feature Branch**
   ```bash
   git checkout -b feature/my-amazing-feature
   ```
3. **Make Changes**
   - Write clean, well-documented code
   - Follow project code style
   - Add tests for new functionality
   - Update README/docs if needed

4. **Test Locally**
   ```bash
   dotnet build
   dotnet test
   docker-compose up -d  # Test with Docker if needed
   ```

5. **Commit with Clear Messages**
   ```bash
   git commit -m "feat: add amazing feature"
   git commit -m "fix: resolve issue with job processing"
   git commit -m "docs: update API documentation"
   ```

6. **Push & Create Pull Request**
   ```bash
   git push origin feature/my-amazing-feature
   ```
   - Link related issues
   - Describe changes clearly
   - Add screenshots/gifs for UI changes

7. **Address Feedback** in code review

## Development Standards

### Code Quality
- Follow C# naming conventions
- Add XML documentation comments
- Keep methods small and focused
- Use async/await for I/O operations

### Testing
- Write unit tests for new features
- Aim for >80% code coverage
- Use descriptive test names
- Follow AAA pattern (Arrange, Act, Assert)

### Git Commit Format
```
<type>: <subject>

<body>

<footer>
```

Types: `feat`, `fix`, `docs`, `refactor`, `test`, `chore`

Example:
```
feat: add job retry with exponential backoff

- Implemented RetryPolicy value object
- Added retry logic to JobWorkerService
- Handles max retries and backoff calculation

Closes #42
```

### Branch Naming
- Feature: `feature/description`
- Fix: `fix/issue-description`
- Docs: `docs/update-readme`

## Documentation

- Update README.md for user-facing changes
- Update ARCHITECTURE.md for structural changes
- Add XML comments to public methods/classes
- Include swagger attributes on API endpoints

## Pull Request Checklist

- [ ] Code follows project style
- [ ] Self-review completed
- [ ] Comments added for complex logic
- [ ] No new warnings introduced
- [ ] Tests added/updated
- [ ] Documentation updated
- [ ] All tests pass locally
- [ ] Commit messages are clear

## Questions?

- Create a GitHub Discussion
- Open an issue for clarification
- Contact maintainers

---

Thank you for contributing! 🙏
