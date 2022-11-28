﻿using ReactiveUI;
using ReactiveUI.Fody.Helpers;
namespace CreationEditor.WPF.Services;

public class BusyService : ReactiveObject, IBusyService { 
    [Reactive] public bool IsBusy { get; set; }
}
