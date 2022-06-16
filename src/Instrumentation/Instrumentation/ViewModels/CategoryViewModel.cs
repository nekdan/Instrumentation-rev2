using Instrumentation.Model;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Instrumentation.ViewModels;

public sealed class CategoryViewModel : ViewModelBase
{
    [Reactive]
    public InstrumentCategory Category { get; set; }
    [Reactive]
    public IRoutableViewModel ViewModel { get; set; }
    [Reactive]
    public double Opacity { get; set; } = 1;
}