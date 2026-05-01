using FluentValidation;

public class OrderValidator : AbstractValidator<Order>
{
    public OrderValidator()
    {
        RuleFor(x => x.SenderCity)
            .NotNull().NotEmpty();
        RuleFor(x => x.SenderAddress)
            .NotNull().NotEmpty();
        RuleFor(x => x.RecieverCity)
            .NotNull().NotEmpty();
        RuleFor(x => x.RecieverAddress)
            .NotNull().NotEmpty();
        RuleFor(x => x.Weight)
            .InclusiveBetween(0.1M, 250)
            .WithMessage("Допустимый вес - от 100 г. (0.1 кг.) до 250 кг.");
    }
}
