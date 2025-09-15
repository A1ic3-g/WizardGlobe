using System;
using Avalonia.Controls;
using WizardPlanet.ViewModels;

namespace WizardPlanet.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
    
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        
        if (DataContext is MainWindowViewModel mwvm)
        {
            mwvm.SetStorageProviderCommand.Execute(StorageProvider);
        }
    }

}