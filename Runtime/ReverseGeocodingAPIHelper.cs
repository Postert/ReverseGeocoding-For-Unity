using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Net;
using GeocoordinateTransformer;

namespace ReverseGeocoding
{

    /// <summary>
    /// Contains methods to request the address associated with geographical coordinates from Mapbox. Note that the results are not checked for plausibility by this library.
    /// </summary>
    public class ReverseGeocodingAPIHelper : MonoBehaviour
    {
        /// <summary>
        /// Use <see cref="CoordinateTransformer"/> instead.
        /// </summary>
        [field: SerializeField]
        private CoordinateTransformer _coordinateTransformer;

        /// <summary>
        /// Contains the <see cref="GeocoordinateTransformer.CoordinateTransformer"/> that was created in the Unity Scene.
        /// </summary>
        /// <exception cref="System.ApplicationException">Thrown when there was no <see cref="GeocoordinateTransformer.CoordinateTransformer"/> could be found in the Unity Scene.</exception>
        public CoordinateTransformer CoordinateTransformer
        {
            get
            {
                if (!_coordinateTransformer)
                {
                    _coordinateTransformer = GameObject.FindAnyObjectByType<CoordinateTransformer>();
                    if (!_coordinateTransformer)
                    {
                        throw new ApplicationException("CoordinateTransformer could not be found in scene. Please add one CoordinateTransformer to each szene and provide UTM coordinates for the representing Unity's origin.");
                    }
                }

                return _coordinateTransformer;
            }
            set => _coordinateTransformer = value;
        }

        // Awake is called by Unity
        private void Awake()
        {
            if (!_coordinateTransformer)
            {
                _coordinateTransformer = GameObject.FindAnyObjectByType<CoordinateTransformer>();
                if (!_coordinateTransformer)
                {
                    Debug.LogError("CoordinateTransformer could not be found in scene. Please add one CoordinateTransformer to each szene and provide UTM coordinates for the representing Unity's origin.");
                }
            }
        }


        /// <summary>
        /// Mapbox endpoint defined according to https://docs.mapbox.com/api/search/geocoding/#reverse-geocoding
        /// </summary>
        public string MapBoxEndpoint = "https://api.mapbox.com/geocoding/v5/mapbox.places";

        /// <summary>
        /// Mapbox access token that can be set up here: https://account.mapbox.com/access-tokens/
        /// Costs may be charged as a perquisite of the requests.
        /// </summary>
        public string AccessToken = "Paste Mapbox Token here";

        /// <summary>
        /// If set to true, the UnityWebRequest result will be printed in the console.
        /// </summary>
        public bool PrintResponseInConsole = false;


        /// <summary>
        /// Asynchronuly requests the adress associated based on the provided geographic coordinates.
        /// </summary>
        /// <returns>ReverseGeocodedAddress with values provided by Mapbox. Note that the results are not checked for plausibility.</returns>
        /// <exception cref="WebException">Thrown if the data request failed. To handle error codes specifically, check out https://docs.mapbox.com/api/search/geocoding/#geocoding-api-errors.</exception>
        /// <exception cref="JsonReaderException">Thrown if the retrieved data could not be deserialized.</exception>
        private async Task<ReverseGeocodedAddress> GetAddressAsync(double lat, double lon)
        {
            string url = $"{MapBoxEndpoint}{(MapBoxEndpoint.EndsWith("/") ? "" : "/")}{lon},{lat}.json?types=address&access_token={AccessToken}";

            using UnityWebRequest request = UnityWebRequest.Get(url);

            request.SetRequestHeader("Content-Type", "application/json");

            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new WebException($"Web request failed: {request.error}");
            }

            string jsonResponse = request.downloadHandler.text;

            if (PrintResponseInConsole)
            {
                Debug.Log($"Success request for lon {lon} and lat {lat}: {request.downloadHandler.text}");
            }

            ReverseGeocodedAddress result = JsonConvert.DeserializeObject<ReverseGeocodedAddress>(jsonResponse);
            return result;
        }


        /// <summary>
        /// Asynchronuly requests the adress associated based on the provided geographic coordinates.
        /// </summary>
        /// /// <param name="geographicCoordinates">Geographical coordinates</param>
        /// <returns>ReverseGeocodedAddress with values provided by Mapbox. Note that the results are not checked for plausibility.</returns>
        /// <exception cref="WebException">Thrown if the data request failed. To handle error codes specifically, check out https://docs.mapbox.com/api/search/geocoding/#geocoding-api-errors.</exception>
        /// <exception cref="JsonReaderException">Thrown if the retrieved data could not be deserialized.</exception>
        public async Task<ReverseGeocodedAddress> GetAddressForGeographicCoordinatesAsync(GeographicCoordinates geographicCoordinates)
        {
            return await GetAddressAsync(lat: geographicCoordinates.latitude, lon: geographicCoordinates.longitude);
        }


        /// <summary>
        /// Asynchronuly requests the adress associated based on the provided UTM coordinates.
        /// </summary>
        /// /// <param name="utmCoordinates">WGS84/UTM coordinates</param>
        /// <returns>ReverseGeocodedAddress with values provided by Mapbox. Note that the results are not checked for plausibility.</returns>
        /// <exception cref="WebException">Thrown if the data request failed. To handle error codes specifically, check out https://docs.mapbox.com/api/search/geocoding/#geocoding-api-errors.</exception>
        /// <exception cref="JsonReaderException">Thrown if the retrieved data could not be deserialized.</exception>
        public async Task<ReverseGeocodedAddress> GetAddressForUTMCoordinatesAsync(UTMCoordinates utmCoordinates)
        {
            GeographicCoordinates geographicCoordinates = CoordinateTransformer.GetGeographicCoordinates(utmCoordinates);
            return await GetAddressAsync(lat: geographicCoordinates.latitude, lon: geographicCoordinates.longitude);
        }


        /// <summary>
        /// Asynchronuly requests the adress associated based on the provided Unity coordinates.
        /// </summary>
        /// /// <param name="unityCoordinates">Unity coordinates</param>
        /// <returns>ReverseGeocodedAddress with values provided by Mapbox. Note that the results are not checked for plausibility.</returns>
        /// <exception cref="WebException">Thrown if the data request failed. To handle error codes specifically, check out https://docs.mapbox.com/api/search/geocoding/#geocoding-api-errors.</exception>
        /// <exception cref="JsonReaderException">Thrown if the retrieved data could not be deserialized.</exception>
        public async Task<ReverseGeocodedAddress> GetAddressForUnityCoordinatesAsync(Vector3 unityCoordinates)
        {
            GeographicCoordinates geographicCoordinates = CoordinateTransformer.GetGeographicCoordinates(unityCoordinates);
            return await GetAddressAsync(lat: geographicCoordinates.latitude, lon: geographicCoordinates.longitude);
        }


        /// <summary>
        /// Test Method that can be called from the Unity Inspector (by clicking on the component's  “…” menu and the TestMapBoxRequest button generated below).
        /// </summary>
        [ContextMenu("TestMapBoxRequest")]
        private async Task<ReverseGeocodedAddress> TestMapBoxRequest()
        {
            Vector3 unityCoordinates = this.gameObject.transform.position;
            Debug.LogFormat("Given Unity coordinates of placed object -- x: {0}, y: {1}, z:{2}", unityCoordinates.x, unityCoordinates.y, unityCoordinates.z);

            UTMCoordinates utmCoordinates = CoordinateTransformer.GetUTMCoordinates(unityCoordinates);
            Debug.LogFormat("UTM coordinates -- east: {0}, north: {1}", utmCoordinates.east, utmCoordinates.north);

            GeographicCoordinates geographicCoordinates = CoordinateTransformer.GetGeographicCoordinates(unityCoordinates);

            Debug.LogFormat("Geographic coordinates -- latitude: {0}, longitude: {1}", geographicCoordinates.latitude, geographicCoordinates.longitude);

            ReverseGeocodedAddress address = await GetAddressAsync(lat: geographicCoordinates.latitude, lon: geographicCoordinates.longitude);

            if (PrintResponseInConsole)
            {
                Debug.Log("Full address: " + address.GetFullAddress() ?? "NO DATA");
                Debug.Log("Street: " + address.GetStreet() ?? "NO DATA");
            }

            return address;
        }
    }
}
