namespace SMLHelper.V2.Patchers
{
    internal class SmlJsonValue : JsonValue
    {
        public SmlJsonValue()
        {
        }

        public SmlJsonValue(JsonValue jsonValue)
        {
            this.propertyType = jsonValue.propertyType;            
            this.dataBool = jsonValue.dataBool;
            this.dataDouble = jsonValue.dataDouble;
            this.dataInt = jsonValue.dataInt;
            this.dataLong = jsonValue.dataLong;
            this.dataArray = jsonValue.dataArray;
            this.dataObject = jsonValue.dataObject;            
        }

        public bool HasPendingChanges { get; set; } = true;
    }
}
