# GPS-WindowsPhone
La aplicación para dispositivos WindowsPhone se ha desarrollado con VisaulStudio y lenguaje C#, siendo una app tipo GPS indicándote la ruta a seguir desde tu ubicación al destino, así como tiempo del trayecto y distancia a recorrer.

* Integrada con los mapas de Bing para obtener la ruta, puedes crear tu token para tu mapa en [BingMapsApi](https://www.bingmapsportal.com/)
```c#
private String bingMapsToken = "Ly1T1huj5T9bh4yE4GBy~Yi7HgpZa5kicWBIcQYzxuA~AnUJ2QNURcR4sfwM2KJ2xE9Y4JQ8P10pTvyyml5nAH0wgK_Hs2gzzCza8XSErvtH";

// Inicializamos el mapa con el token de Bing
Mapa.MapServiceToken = bingMapsToken;

// Asignamos la mayor precision
GPS.DesiredAccuracy = PositionAccuracy.High;
GPS.MovementThreshold = 10;

// Permite mostrar puntos en el mapa
Mapa.LandmarksVisible = true;
```

* _setIconMap()_ nos permite añadir una marca o chincheta en la ubicación que le pasemos por parámetro
* _ShowRouteOnMap()_ nos permite pintar el recorrido en el mapa

```c#
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
```
