using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using MRADS2.Ships.ViewModel;
using MRADS2.Ships.CCM;
using MRADS2.Controls;

namespace MRADS2.Panels.CCM
{
    /// <summary>
    /// Interaction logic for PDMInputsPanel.xaml
    /// </summary>
    public partial class PDMInputsPanel : BasePanel
    {
        public PDMInputsPanel(MRADSDataVM vmdata, DefaultBindVM vmship) : base(vmdata, vmship)
        {
            InitializeComponent();

            CreateTabs((CCMVM)vmship);
        }

        public override string HeaderText => "PDM Inputs";

        void CreateTabs(CCMVM vmship)
        {
            foreach (var pdmvm in vmship.PDMs)
            {
                if (pdmvm.InputVariables.Count > 0)
                    tabInputs.Items.Add(CreateTab(pdmvm));
            }
        }

        TabItem CreateTab(PDMVM pdmvm)
        {
            int i;
            TabItem ret = new TabItem();

            ret.Header = pdmvm.Name.Substring(4);

            Grid grid = new Grid();

            for (i = 0; i < 6; i++)
                grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

            for (i = 0; i < 4; i++)
            {
                var cd = new ColumnDefinition();

                if ((i % 2) == 0)
                    cd.Width = new GridLength(1, GridUnitType.Auto);
                else
                    cd.Width = new GridLength(1, GridUnitType.Star);

                grid.ColumnDefinitions.Add(cd);
            }

            i = 0;

            foreach (var output in pdmvm.InputVariables)
            {
                int row = i % 6;
                int col = i / 6 * 2;

                Label lbl = new Label();
                lbl.Content = output.Variable.Name;
                lbl.HorizontalContentAlignment = HorizontalAlignment.Right;
                lbl.VerticalContentAlignment = VerticalAlignment.Center;
                lbl.SetValue(Grid.RowProperty, row);
                lbl.SetValue(Grid.ColumnProperty, col);

                grid.Children.Add(lbl);

                DataBox db = new DataBox();
                db.SetBinding(DataBox.ValueProperty, new Binding() { Source = output, Path = new PropertyPath("Value") });
                db.SetDataVM(VMData);
                db.MinWidth = 100;
                db.SetValue(Grid.RowProperty, row);
                db.SetValue(Grid.ColumnProperty, col + 1);

                grid.Children.Add(db);

                i++;
            }

            ret.Content = grid;

            return (ret);
        }
    }
}
