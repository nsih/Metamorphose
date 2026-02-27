using R3;
using Common;


public class RunResultModel
{
    public ReactiveProperty<RunEndReason> LastResult { get; } =
        new ReactiveProperty<RunEndReason>(RunEndReason.None);
}