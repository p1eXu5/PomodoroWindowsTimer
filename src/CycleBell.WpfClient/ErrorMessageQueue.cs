using System;
using CycleBell.ElmishApp.Abstractions;

namespace CycleBell.WpfClient;

public class ErrorMessageQueue : MaterialDesignThemes.Wpf.SnackbarMessageQueue, IErrorMessageQueue
{
    public void EnqueuError(string value)
    {
        Enqueue(
            value,
            "Clear",
            _ => Clear(),
            null,
            false,
            true,
            TimeSpan.FromSeconds(15));
    }
}
