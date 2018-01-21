using System;

using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Devices.Geolocation;
using System.Diagnostics;
using Windows.UI.Xaml.Controls.Maps;
using Windows.Services.Maps;
using Windows.UI;
using System.Net.Http;
using Windows.Data.Json;

namespace ApiMaps
{
    public sealed partial class MainPage : Page
    {
    	// URLs para obtener información sobre la ruta a seguir
        private String bingMapsToken = "Ly1T1huj5T9bh4yE4GBy~Yi7HgpZa5kicWBIcQYzxuA~AnUJ2QNURcR4sfwM2KJ2xE9Y4JQ8P10pTvyyml5nAH0wgK_Hs2gzzCza8XSErvtH";
        private String apiDirectionsGoogle = "https://maps.googleapis.com/maps/api/geocode/json?address=";
        private String apiMapsKey = "&region=es&key=AIzaSyC_J850zSjycDuLFmwjSB7RfQWrD5GpD-o";

        // TAGS para obtener los elementos necesarios en el JSON
		static String TAG_ERRORAPI = "ZERO_RESULTS";
        static String TAG_LOCATION = "location";        
        static String TAG_RESULTS = "results";
        static String TAG_GEO = "geometry";
        static String TAG_LAT = "lat";
        static String TAG_LNG = "lng";

        Geolocator GPS = new Geolocator();		// Permite ubicarnos en el mapa
        double latitudBusqueda = 0.0;			// Nos da la latitud de la ciudad buscada por el usuario
        double longitudBusqueda = 0.0;			// Nos da la longitud de la ciudad buscada por el usuario
        String ruta = "";						// Almacena el itinerario a seguir hasta el objetivo


        public MainPage()
        {
            this.InitializeComponent();

            // Inicializamos el mapa con el token de Bing
            Mapa.MapServiceToken = bingMapsToken;

            // Asignamos la mayor precision
            GPS.DesiredAccuracy = PositionAccuracy.High;
            GPS.MovementThreshold = 10;

            // Permite mostrar puntos en el mapa
            Mapa.LandmarksVisible = true;
        }

        // Asociado al pulsar el botón de buscar el recorrido
        private async void button_Click(object sender, RoutedEventArgs e)
        {
            /* Iniciamos todo el proceso
                1. Preguntamos a la api de google cual es la latitud y longitud del lugar seleccionado por el usuario
                2. Cuando tenemos valores, obtenemos el recorrido.
                3. Cuando tenemos el recorrido, lo pintamos sobre el mapa (indicando punto de salida, llegada, tiempo y distancia)
            */

            // Si no tenemos elementos para buscar
            if (lugarBusqueda.Text == null || lugarBusqueda.Text == "")
            {
                Distancia.Text = "Introduce un lugar válido";
                Tiempo.Text = "";

            } else {
            	// SI el usuario ha indicado alguna ciudad
                obtenDatosLugar(lugarBusqueda.Text);
            }
        }

        private async void obtenerInformacionTrayecto()
        {
            // Empezamos siempre desde la ciudad de Madrid
            BasicGeoposition startLocation = new BasicGeoposition() { Latitude = 40.4188556, Longitude = -3.7149356 };

            // La localizacion final dependera de la ciudad indicada por el usuario
            BasicGeoposition endLocation = new BasicGeoposition() { Latitude = latitudBusqueda, Longitude = longitudBusqueda };

            // Obtenemos la ruta entre los puntos
            MapRouteFinderResult routeResult = await MapRouteFinder.GetDrivingRouteAsync( new Geopoint(startLocation), new Geopoint(endLocation),
                  MapRouteOptimization.Time,
                  MapRouteRestrictions.None);

            // Si podemos hacer la ruta
            if (routeResult.Status == MapRouteFinderStatus.Success)
            {
            	// Creamos dos Geopoint con las localizaciones mencionadas antes
                var geopoint = new Geopoint(startLocation);
                var geopoint2 = new Geopoint(endLocation);

                // Creamos un icono en el mapa en dichas localizaciones
                setIconMap(geopoint, "Inicio");
                setIconMap(geopoint2, "Fin");

                // Hacemos que el mapa se centre en la localizacion de salida con un zoom de 15
                Mapa.Center = geopoint;
                Mapa.ZoomLevel = 15;

                // Mostramos al usuario la distancia y tiempo hasta el recorrido
                Distancia.Text = "Distancia al objetivo: " + (routeResult.Route.LengthInMeters / 1000).ToString() + " KM";
                Tiempo.Text = "Tiempo del viaje: " + routeResult.Route.EstimatedDuration.TotalMinutes.ToString() + " minutos";

                // Para cada ruta obtenida en el mapa
                foreach (MapRouteLeg leg in routeResult.Route.Legs)
                {
                    foreach (MapRouteManeuver maneuver in leg.Maneuvers)
                    {
                    	// Guardamos las instrucciones (itinerario) hasta el objetivo
                        ruta = ruta + maneuver.InstructionText + "\n";
                    }
                }

                // Mostramos la ruta hasta el objetivo
                ShowRouteOnMap();
            }
            else { }
        }

        // Funcion que permite crear un icono en el mapa segun el Geopoint dado
        private void setIconMap(Geopoint geo, String titulo)
        {
            MapIcon iconLocated = new MapIcon
            {
                Location = geo,
                NormalizedAnchorPoint = new Point(0.5, 1.0),
                ZIndex = 0,
                Title = titulo
            };

            Mapa.MapElements.Add(iconLocated);
        }

        // Nos muestra el recorrido a seguir hasta el objetivo
        private async void ShowRouteOnMap()
        {
            // Empezamos siempre desde la ciudad de Madrid
            BasicGeoposition startLocation = new BasicGeoposition() { Latitude = 40.4188556, Longitude = -3.7149356 };

            // La localizacion final dependera de la ciudad indicada por el usuario
            BasicGeoposition endLocation = new BasicGeoposition() { Latitude = latitudBusqueda, Longitude = longitudBusqueda };

            // Obtenemos la ruta entre los puntos
            MapRouteFinderResult routeResult =
                  await MapRouteFinder.GetDrivingRouteAsync(
                  new Geopoint(startLocation),
                  new Geopoint(endLocation),
                  MapRouteOptimization.Time,
                  MapRouteRestrictions.None);

            if (routeResult.Status == MapRouteFinderStatus.Success)
            {
                // Creamos una ruta con el resultado obtenido
                MapRouteView viewOfRoute = new MapRouteView(routeResult.Route);
                viewOfRoute.RouteColor = Colors.Yellow;
                viewOfRoute.OutlineColor = Colors.Black;

              	// Añadimos el MapRouteView al controlador del mapa
                Mapa.Routes.Add(viewOfRoute);

                // Esperamos a que termine de pintarlo
                await Mapa.TrySetViewBoundsAsync(
                      routeResult.Route.BoundingBox,
                      null,
                      MapAnimationKind.None);
            }
        }

        // Accede a la API de google maps para devolvernos en formato json informacion sobre la ciudad buscada
        private async void obtenDatosLugar(String lugar)
        {
        	// Creamos un nuevo httpclient
            var httpClient = new HttpClient();

            // Creamos la peticion
            HttpResponseMessage httpResponse = await httpClient.GetAsync(new Uri(apiDirectionsGoogle + lugar + apiMapsKey));
            httpResponse.EnsureSuccessStatusCode();
            
            // Esperamos el resultado y lo alamacenamos en un string
            string httpResponseBody = await httpResponse.Content.ReadAsStringAsync();

            // Si la respuesta obtenida no es un error, por lo tanto la ciudad existe
            if (!httpResponseBody.Contains(TAG_ERRORAPI))
            {
            	// Obtenemos las cordenadas y mostramos al usuario el trayecto
                obtenCoordenadas(httpResponseBody);
                obtenerInformacionTrayecto();
            }
            else
            {	// Indicamos al usuario que no existe esa ciduad
                Distancia.Text = "Introduce un lugar válido";
            }
        }

        // Formate el json para obtener la latitud y longitud de la ciudad buscada
        private void obtenCoordenadas(String httpResponse)
        {
        	// Creamos un JsonObject con el string 
            JsonObject httpObj = JsonObject.Parse(httpResponse);

            // Accedemos al array
            JsonArray res = httpObj.GetNamedArray(TAG_RESULTS);

            // Para cada elemento del array obtenemos solo la Latitud y Longitud de la localizacion
            for (uint i = 0; i < res.Count; i++)
            {
                JsonObject c = res.GetObjectAt(i);

                JsonObject loc = c.GetNamedObject(TAG_GEO).GetNamedObject(TAG_LOCATION);

                latitudBusqueda = loc.GetNamedNumber(TAG_LAT);
                longitudBusqueda = loc.GetNamedNumber(TAG_LNG);
            }

            Debug.Write("Busqueda latitud: " + latitudBusqueda + ", y longitud: " + longitudBusqueda);
        }

        // Nos permite acceder a la siguiente vista y ver el itinerario a seguir
        private void verItinerario_Click(object sender, RoutedEventArgs e)
        {
            var parameters = new DatosIt();
            parameters.Itinerario = ruta;

            Frame.Navigate(typeof(VistaItinerario),parameters);
        }
    }
}
