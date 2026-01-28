using FluentValidation;
using WorkNest.Api.Controllers;

namespace WorkNest.Api.Validators;

public class CreateTaskRequestValidator : AbstractValidator<TasksController.CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}
