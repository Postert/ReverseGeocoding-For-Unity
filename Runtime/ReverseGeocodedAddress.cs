namespace ReverseGeocoding
{
    /// <summary>
    /// Contains address attributes and methods to return combined fields in a usefully formatted way.
    /// </summary>
    [System.Serializable]
    public class ReverseGeocodedAddress
    {
        public Feature[] features;

        public string GetFullAddress()
        {
            return (features.Length > 0) ? features[0].place_name : null;
        }

        public string GetStreet()
        {
            return (features.Length > 0 ? features[0].text + " " + features[0].address : null);
        }
    }


    [System.Serializable]
    public class Feature
    {
        /// <summary>
        /// Street name
        /// </summary>
        public string text;


        /// <summary>
        /// Full address
        /// </summary>
        public string place_name;

        /// <summary>
        /// House number
        /// </summary>
        public string address;
    }
}