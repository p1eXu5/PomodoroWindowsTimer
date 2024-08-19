using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using Microsoft.Xaml.Behaviors;

namespace PomodoroWindowsTimer.WpfClient.Behaviors;

/// <summary>
/// Applying to button. This button must invoke `materialDesign:DialogHost.OpenDialogCommand` and pass in command parameter
/// dialog template content. This behavion sets <see cref="DialogOpenedEventHandler"/> and <see cref="DialogClosedEventHandler"/>
/// with delegates to load dialog models and process ok and cancel commands.
/// </summary>
internal sealed class DialogEventHandlerBehavior : Behavior<Button>
{
    private DialogOpenedEventHandler? _openedEventHandler;
    private DialogClosedEventHandler? _closedEventHandler;

    protected override void OnAttached()
    {
        base.OnAttached();
    }


    # region LoadDialogModelCommandProperty -----------------------------

    public static ICommand GetLoadDialogModelCommand(DependencyObject obj)
    {
        return (ICommand)obj.GetValue(LoadDialogModelCommandProperty);
    }

    public static void SetLoadDialogModelCommand(DependencyObject obj, ICommand value)
    {
        obj.SetValue(LoadDialogModelCommandProperty, value);
    }

    // Using a DependencyProperty as the backing store for LoadDialogModelCommand.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty LoadDialogModelCommandProperty =
        DependencyProperty.RegisterAttached(
            "LoadDialogModelCommand",
            typeof(ICommand),
            typeof(DialogEventHandlerBehavior),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, OnLoadDialogModelCommandCommandChanged));

    private static void OnLoadDialogModelCommandCommandChanged(object d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DialogEventHandlerBehavior behavior)
        {
            return;
        }

        if (e.NewValue is ICommand loadDialogModelCommand)
        {
            behavior._openedEventHandler = (sender, eventArgs) =>
            {
                eventArgs.Handled = true;
                object? parameter = GetLoadDialogModelCommandParameter(behavior);
                if (loadDialogModelCommand.CanExecute(parameter))
                {
                    loadDialogModelCommand.Execute(parameter);
                }
            };
        }
        else
        {
            behavior._openedEventHandler = (sender, eventArgs) =>
            {
                eventArgs.Handled = true;
            };
        }

        DialogHost.SetDialogOpenedAttached(behavior.AssociatedObject, behavior._openedEventHandler);
    }

    #endregion --------------------------------------------------------


    #region LoadDialogModelCommandParameterProperty -------------------

    public static object? GetLoadDialogModelCommandParameter(DependencyObject obj)
    {
        return (object?)obj.GetValue(LoadDialogModelCommandParameterProperty);
    }

    public static void SetLoadDialogModelCommandParameter(DependencyObject obj, object? value)
    {
        obj.SetValue(LoadDialogModelCommandParameterProperty, value);
    }

    // Using a DependencyProperty as the backing store for LoadDialogModelCommandParameter.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty LoadDialogModelCommandParameterProperty =
        DependencyProperty.RegisterAttached(
            "LoadDialogModelCommandParameter",
            typeof(object),
            typeof(DialogEventHandlerBehavior),
            new PropertyMetadata(null));

    #endregion --------------------------------------------------------


    #region OkCommandProperty -----------------------------------------

    public static ICommand? GetOkCommand(DependencyObject obj)
    {
        return (ICommand?)obj.GetValue(OkCommandProperty);
    }

    public static void SetOkCommand(DependencyObject obj, ICommand value)
    {
        obj.SetValue(OkCommandProperty, value);
    }

    // Using a DependencyProperty as the backing store for OkCommand.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty OkCommandProperty =
        DependencyProperty.RegisterAttached(
            "OkCommand",
            typeof(ICommand),
            typeof(DialogEventHandlerBehavior),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, OnOkCommandChanged));

    private static void OnOkCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => SetCloseCommand(d, e, true);

    #endregion --------------------------------------------------------


    #region CancelCommandProperty -------------------------------------

    public static ICommand? GetCancelCommand(DependencyObject obj)
    {
        return (ICommand?)obj.GetValue(CancelCommandProperty);
    }

    public static void SetCancelCommand(DependencyObject obj, ICommand? value)
    {
        obj.SetValue(CancelCommandProperty, value);
    }

    // Using a DependencyProperty as the backing store for CancelCommand.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty CancelCommandProperty =
        DependencyProperty.RegisterAttached(
            "CancelCommand",
            typeof(ICommand),
            typeof(DialogEventHandlerBehavior),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, OnCancelCommandChanged));

    public static void OnCancelCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => SetCloseCommand(d, e, false);

    #endregion --------------------------------------------------------

    /// <summary>
    /// 
    /// </summary>
    /// <param name="d"></param>
    /// <param name="e"></param>
    /// <param name="isOkCommand"></param>
    public static void SetCloseCommand(DependencyObject d, DependencyPropertyChangedEventArgs e, bool isOkCommand)
    {
        if (d is not DialogEventHandlerBehavior behavior)
        {
            return;
        }

        ICommand? okCommand = isOkCommand ? e.NewValue as ICommand : GetOkCommand(behavior);
        ICommand? cancelCommand = isOkCommand ? GetCancelCommand(behavior) : e.NewValue as ICommand;

        if (okCommand is null && cancelCommand is null)
        {
            behavior._closedEventHandler = (sender, eventArgs) =>
            {
                eventArgs.Handled = true;
            };

        }
        else
        {
            behavior._closedEventHandler = (sender, eventArgs) =>
            {
                eventArgs.Handled = true;

                if (eventArgs.Parameter is bool v)
                {
                    if (v && okCommand is not null && okCommand.CanExecute(null))
                    {
                        okCommand.Execute(null);
                        return;
                    }

                    if (!v && cancelCommand is not null && cancelCommand.CanExecute(null))
                    {
                        cancelCommand.Execute(null);
                    }

                    return;
                }

                if (eventArgs.Parameter is not null && okCommand is not null && okCommand.CanExecute(null))
                {
                    okCommand.Execute(null);
                    return;
                }
            };
        }

        DialogHost.SetDialogClosedAttached(behavior.AssociatedObject, behavior._closedEventHandler);
    }
}
