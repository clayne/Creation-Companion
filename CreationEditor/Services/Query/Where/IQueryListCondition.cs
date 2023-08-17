using DynamicData.Binding;
namespace CreationEditor.Services.Query.Where;

public sealed record QueryConditionListMemento(
    string FullTypeName,
    string SelectedFunctionOperator,
    List<QueryConditionEntryMemento> SubConditions) : IQueryConditionMemento;

public interface IQueryListCondition : IQueryCondition {
    IObservableCollection<IQueryConditionEntry> SubConditions { get; }
}
