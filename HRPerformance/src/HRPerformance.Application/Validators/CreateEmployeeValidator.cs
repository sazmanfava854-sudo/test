using FluentValidation;
using HRPerformance.Application.DTOs.Employees;

namespace HRPerformance.Application.Validators;
public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeRequest>
{
    public CreateEmployeeValidator()
    {
        RuleFor(x => x.PersonnelCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.NationalCode).NotEmpty().Length(10);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EmploymentDate).NotEmpty();
    }
}
