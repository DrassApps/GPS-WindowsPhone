using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace ApiMaps
{
    // Vista que nos muestra el itinerario a seguir hasta la ciudad indicada en la vista anterior
    public sealed partial class VistaItinerario : Page
    {
        public VistaItinerario()
        {
            this.InitializeComponent();
        }

        // Volvemos a la vista anterior
        private void volverInicio(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        // Al acceder a ka vista obtenemos la ruta 
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var parameters = (DatosIt)e.Parameter;
            rutaP.Text = parameters.Itinerario;
        }
    }
}
