using FluentValidation;
using HelixPortal.Application.DTOs.Request;

namespace HelixPortal.Application.Validators;

public class CreateRequestDtoValidator : AbstractValidator<CreateRequestDto>
{
    public CreateRequestDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(5000).WithMessage("Description must not exceed 5000 characters");

        RuleFor(x => x.Priority)
            .Must(p => p == "Low" || p == "Medium" || p == "High" || p == "Critical")
            .WithMessage("Priority must be Low, Medium, High, or Critical");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.DueDate.HasValue)
            .WithMessage("Due date must be in the future");
    }
}

