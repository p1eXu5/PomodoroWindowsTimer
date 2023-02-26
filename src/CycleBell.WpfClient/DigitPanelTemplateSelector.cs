﻿using System.Windows.Controls;
using System.Windows;

namespace CycleBell.WpfClient;

public class DigitPanelTemplateSelector : DataTemplateSelector
{
    public override DataTemplate? SelectTemplate(object isBreak, DependencyObject container)
    {
        FrameworkElement? element = container as FrameworkElement;

        if (element != null && isBreak != null)
        {
            if (isBreak is bool b && b)
                return element.FindResource("gp_Red") as DataTemplate;

            return element.FindResource("gp_Green") as DataTemplate;
        }
        return null;
    }
}
