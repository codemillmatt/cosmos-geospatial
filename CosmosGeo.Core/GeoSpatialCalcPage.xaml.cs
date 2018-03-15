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
                "Madison","Chicago","Wisconsin","I-39","I-41","I-43","I-90","I-94"
            };

            foreach (var feature in featureList)
            {
                firstFeature.Items.Add(feature);
            }

            firstFeature.SelectedIndex = 0;

            operation.Items.Add("Intersects");
            operation.Items.Add("Within");
            operation.SelectedIndex = 0;

            calculate.Clicked += async (sender, args) =>
            {
                var feature = await GetFeatureFromCosmos(firstFeature.SelectedItem.ToString());

                var foundFeatures = new List<Feature>();

                if (operation.SelectedItem.ToString() == "Intersects")
                    foundFeatures = await Intersects(feature);
                else
                    foundFeatures = await Within(feature);

                StringBuilder theResults = new StringBuilder();
                foreach (var found in foundFeatures)
                {
                    theResults.AppendLine(found.LocationName);
                }

                results.Text = theResults.ToString();
            };
        }

        async Task<Feature> GetFeatureFromCosmos(string featureName)
        {
            //DocumentCollection dc = await client.ReadDocumentCollectionAsync(collectionUri);
            //dc.IndexingPolicy = new IndexingPolicy(new SpatialIndex(DataType.Point));
            //await client.ReplaceDocumentCollectionAsync(dc);

            //await Task.Delay(TimeSpan.FromSeconds(10));

            Feature feature = null;
            // Query for the feature name
            var featureQuery = client.CreateDocumentQuery<Feature>(collectionUri).Where(f => f.LocationName == featureName).Take(1).AsDocumentQuery();

            // For this example - only interested in the first one
            if (featureQuery.HasMoreResults)
            {
                var featureResult = await featureQuery.ExecuteNextAsync<Feature>();

                feature = featureResult.FirstOrDefault();
            }

            return feature;
        }

        async Task<List<Feature>> Within(Feature feature)
        {
            List<Feature> containedFeatures = new List<Feature>();

            var feedOptions = new FeedOptions { EnableScanInQuery = true, MaxItemCount = -1 };
            var containsQuery = client.CreateDocumentQuery<Feature>(collectionUri, feedOptions)
                                      //.Where(f => feature.Id != f.Id)
                                      .Where(f => f.Geometry.Within(feature.Geometry))
                                      .AsDocumentQuery();

            while (containsQuery.HasMoreResults)
            {
                var containsResults = await containsQuery.ExecuteNextAsync<Feature>();

                containedFeatures.AddRange(containsResults);
            }

            return containedFeatures;
        }

        async Task<List<Feature>> Intersects(Feature feature)
        {
            List<Feature> intersectingFeatures = new List<Feature>();

            var feedOptions = new FeedOptions { EnableScanInQuery = true, MaxItemCount = -1 };

            Microsoft.Azure.Documents.Spatial.Point p = new Microsoft.Azure.Documents.Spatial.Point(-87.655, 41.948);

            var ls = feature.Geometry as LineString;

            var intersectingQuery = client.CreateDocumentQuery<Feature>(collectionUri, feedOptions)
                                          //.Where(f => feature.Id != f.Id)
                                          .Where(f => ls.Intersects(f.Geometry))
                                          //.Where(f => f.Geometry.Intersects(ls))
                                          //.Where(f => feature.Geometry.Intersects(f.Geometry))
                                          .AsDocumentQuery();

            while (intersectingQuery.HasMoreResults)
            {
                var intersectsResults = await intersectingQuery.ExecuteNextAsync<Feature>();

                intersectingFeatures.AddRange(intersectsResults);
            }

            return intersectingFeatures;
        }
    }
}
