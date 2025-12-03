using FluentValidation;
using HelixPortal.Application.DTOs.Request;

namespace HelixPortal.Application.Validators;

public class AddCommentDtoValidator : AbstractValidator<AddCommentDto>
{
    public AddCommentDtoValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required")
            .MaximumLength(2000).WithMessage("Message must not exceed 2000 characters");
    }
}

