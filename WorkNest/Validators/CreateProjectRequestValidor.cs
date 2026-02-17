using FluentValidation;
using WorkNest.Api.Controllers;

namespace WorkNest.Api.Validators;

public class CreateProjectRequestValidator : AbstractValidator<ProjectsController.CreateProjectRequest>
{
    public CreateProjectRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required.")
            .MinimumLength(3).WithMessage("Project name must be at least 3 characters.")
            .MaximumLength(50).WithMessage("Project name must be at most 50 characters.");
    }
}
