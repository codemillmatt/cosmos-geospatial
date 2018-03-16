using System;
using System.Collections.Generic;

using Xamarin.Forms;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Linq;
using Microsoft.Azure.Documents.Spatial;
using System.Text;
using Microsoft.Azure.Documents;

namespace CosmosGeo.Core
{
    public partial class GeoSpatialCalcPage : ContentPage
    {
        // Initialize with a read-only key
        DocumentClient client = new DocumentClient(new Uri("https://awesomecontactz.documents.azure.com:443/"), "tT95SR4OLebuny9cx9GxeghbBQhNoAEyWBpaSlOcQ5DdIdS4dN2e431r6GSwG6WM3lTKdz4djn7ldDSiUwytiQ==");
        Uri collectionUri = UriFactory.CreateDocumentCollectionUri("CDALocations", "CityState");

        public GeoSpatialCalcPage()
        {
            InitializeComponent();

            var featureList = new List<string>
            {
                "Madison","Milwaukee", "Chicago","Wisconsin","I-39","I-41","I-43","I-90","I-94"
            };

            foreach (var feature in featureList)
            {
                firstFeature.Items.Add(feature);
            }

            firstFeature.SelectedIndex = 0;

            operation.Items.Add("Intersects");
            operation.Items.Add("Within");
            operation.Items.Add("Distance");

            operation.SelectedIndex = 0;

            calculate.Clicked += async (sender, args) =>
            {
                var feature = await GetFeatureFromCosmos(firstFeature.SelectedItem.ToString());

                string operationResults = "";


                if (operation.SelectedItem.ToString() == "Intersects")
                    operationResults = await Intersects(feature);
                else if (operation.SelectedItem.ToString() == "Distance")
                    operationResults = await Distance(feature);
                else
                    operationResults = await Within(feature);

                results.Text = operationResults;
            };
        }

        async Task<Feature> GetFeatureFromCosmos(string featureName)
        {
            Feature feature = null;

            // Query for the feature name
            var featureQuery = client.CreateDocumentQuery<Feature>(collectionUri)
                                     .Where(f => f.LocationName == featureName)
                                     .Take(1)
                                     .AsDocumentQuery();

            // For this example - only interested in the first one
            if (featureQuery.HasMoreResults)
            {
                var featureResult = await featureQuery.ExecuteNextAsync<Feature>();

                return featureResult.FirstOrDefault();
            }

            return feature;
        }

        async Task<string> Within(Feature feature)
        {
            var withinBuilder = new StringBuilder();

            var withinQuery = client.CreateDocumentQuery<Feature>(collectionUri)
                  .Where(f => feature.Id != f.Id) // Ignore passed in
                  .Where(f => f.Location.Within(feature.Location))
                                    .Select(f => f.LocationName)
                  .AsDocumentQuery();

            while (withinQuery.HasMoreResults)
            {
                var withinResults = await withinQuery.ExecuteNextAsync<string>();

                foreach (var item in withinResults)
                {
                    withinBuilder.AppendLine(item);
                }
            }

            return withinBuilder.ToString();
        }

        async Task<string> Intersects(Feature feature)
        {
            var intersectBuilder = new StringBuilder();

            var intersectingQuery = client.CreateDocumentQuery<Feature>(collectionUri)
                  .Where(f => feature.Id != f.Id) // Ignore passed in
                  .Where(f => feature.Location.Intersects(f.Location))
                                          .Select(f => f.LocationName)
                  .AsDocumentQuery();

            while (intersectingQuery.HasMoreResults)
            {
                var intersectsResults = await intersectingQuery.ExecuteNextAsync<string>();

                foreach (var item in intersectsResults)
                {
                    intersectBuilder.AppendLine(item);
                }
            }

            return intersectBuilder.ToString();
        }


        async Task<string> Distance(Feature feature)
        {
            var cheeseFestival = await GetFeatureFromCosmos("CheeseFest");

            // Geometry property is Microsoft.Azure.Documents.Spatial.Geometry
            var distanceQuery = client.CreateDocumentQuery<Feature>(collectionUri)
                                      .Where(f => f.Id == feature.Id)
                                      .Select(f => cheeseFestival.Location.Distance(f.Location))
                                      .AsDocumentQuery();

            var distanceBuilder = new StringBuilder();

            while (distanceQuery.HasMoreResults)
            {
                var distanceResults = await distanceQuery.ExecuteNextAsync<double>();

                foreach (var item in distanceResults)
                {
                    distanceBuilder.AppendLine($"Distance to the Cheese Fest is {Math.Round(item.MeterToMiles(), 2)} mi");
                }
            }

            return distanceBuilder.ToString();
        }
    }

    public static class MileConversion
    {
        public static double MeterToMiles(this double meters)
        {
            return meters * 0.000621371;
        }
    }
}
