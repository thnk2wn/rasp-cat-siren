namespace CatSiren.TrainModel
{
    public class TrainingOptions
    {
        public class AzureVisionOptions
        {
            public string ApiKey {get; set;}
            public string Endpoint {get; set;}
        }

        public AzureVisionOptions AzureVision {get; set;}
    }
}