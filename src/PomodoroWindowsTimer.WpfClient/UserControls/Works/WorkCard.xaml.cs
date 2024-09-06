using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PomodoroWindowsTimer.WpfClient.UserControls.Works
{
    /// <summary>
    /// Interaction logic for WorkCard.xaml
    /// </summary>
    public partial class WorkCard : UserControl
    {
        public WorkCard()
        {
            InitializeComponent();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }


        public TextWrapping TitleWrapping
        {
            get { return (TextWrapping )GetValue(TitleWrappingProperty); }

            set { SetValue(TitleWrappingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for WorkTitleWrapping.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleWrappingProperty =
            DependencyProperty.Register(
                "TitleWrapping",
                typeof(TextWrapping ),
                typeof(WorkCard),
                new FrameworkPropertyMetadata(
                    TextWrapping.NoWrap,
                    FrameworkPropertyMetadataOptions.None
                )
            );


        public Thickness TitleMargin
        {
            get { return (Thickness)GetValue(TitleMarginProperty); }
            set { SetValue(TitleMarginProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TitleMargin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleMarginProperty =
            DependencyProperty.Register("TitleMargin", typeof(Thickness), typeof(WorkCard), new PropertyMetadata(new Thickness(0)));

        public double TitleHeight
        {
            get { return (double)GetValue(TitleHeightProperty); }
            set { SetValue(TitleHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TitleHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleHeightProperty =
            DependencyProperty.Register("TitleHeight", typeof(double), typeof(WorkCard), new PropertyMetadata(Double.NaN));

    }
}
