﻿using CreationEditor.GUI.Skyrim.ViewModels.Record;
namespace CreationEditor.GUI.Skyrim.Views.Record; 

public partial class FactionList {
    public FactionList(FactionListVM recordListVM) {
        InitializeComponent();

        DataContext = ViewModel = recordListVM;
    }
}
