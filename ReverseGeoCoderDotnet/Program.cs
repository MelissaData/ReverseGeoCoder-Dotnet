using Newtonsoft.Json;

namespace ReverseGeoCoderDotnet
{
  static class Program
  {
    static void Main(string[] args)
    {
      string baseServiceUrl = @"https://reversegeo.melissadata.net/";
      string serviceEndpoint = @"v3/web/ReverseGeoCode/doLookup"; //please see https://www.melissa.com/developer/reverse-geocoder for more endpoints
      string license = "";
      string latitude = "";
      string longitude = "";
      string maxRecords = "";

      ParseArguments(ref license, ref latitude, ref longitude, ref maxRecords, args);
      CallAPI(baseServiceUrl, serviceEndpoint, license, latitude, longitude, maxRecords);
    }

    static void ParseArguments(ref string license, ref string latitude, ref string longitude, ref string maxRecords, string[] args)
    {
      for (int i = 0; i < args.Length; i++)
      {
        if (args[i].Equals("--license") || args[i].Equals("-l"))
        {
          if (args[i + 1] != null)
          {
            license = args[i + 1];
          }
        }
        if (args[i].Equals("--lat"))
        {
          if (args[i + 1] != null)
          {
            latitude = args[i + 1];
          }
        }
        if (args[i].Equals("--long"))
        {
          if (args[i + 1] != null)
          {
            longitude = args[i + 1];
          }
        }
        if (args[i].Equals("--max"))
        {
          if (args[i + 1] != null)
          {
            maxRecords = args[i + 1];
          }
        }
      }
    }

    public static async Task GetContents(string baseServiceUrl, string requestQuery)
    {
      HttpClient client = new HttpClient();
      client.BaseAddress = new Uri(baseServiceUrl);
      HttpResponseMessage response = await client.GetAsync(requestQuery);

      string text = await response.Content.ReadAsStringAsync();

      var obj = JsonConvert.DeserializeObject(text);
      var prettyResponse = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);

      // Print output
      Console.WriteLine("\n==================================== OUTPUT ====================================\n");
      
      Console.WriteLine("API Call: ");
      string APICall = Path.Combine(baseServiceUrl, requestQuery);
      for (int i = 0; i < APICall.Length; i += 70)
      {
        try
        {
          Console.WriteLine(APICall.Substring(i, 70));
        }
        catch
        {
          Console.WriteLine(APICall.Substring(i, APICall.Length - i));
        }
      }

      Console.WriteLine("\nAPI Response:");
      Console.WriteLine(prettyResponse);
    }
    
    static void CallAPI(string baseServiceUrl, string serviceEndPoint, string license, string latitude, string longitude, string maxRecords)
    {
      Console.WriteLine("\n================= WELCOME TO MELISSA REVERSE GEOCODER CLOUD API ================\n");
      
      bool shouldContinueRunning = true;
      while (shouldContinueRunning)
      {
        string inputLatitude = "";
        string inputLongitude = "";
        string inputMaxRecords = "";

        if (string.IsNullOrEmpty(latitude) && string.IsNullOrEmpty(longitude))
        {
          Console.WriteLine("\nFill in each value to see results");

          Console.Write("Latitude: ");
          inputLatitude = Console.ReadLine();

          Console.Write("Longitude: ");
          inputLongitude = Console.ReadLine();

          Console.Write("MaxRecords: ");
          inputMaxRecords = Console.ReadLine();
        }
        else
        {
          inputLatitude = latitude;
          inputLongitude = longitude;
          inputMaxRecords = maxRecords;
        }

        while (string.IsNullOrEmpty(inputLatitude) || string.IsNullOrEmpty(inputLongitude) || string.IsNullOrEmpty(inputMaxRecords))
        {
          Console.WriteLine("\nFill in missing required parameter");

          if (string.IsNullOrEmpty(inputLatitude))
          {
            Console.Write("Latitude: ");
            inputLatitude = Console.ReadLine();
          }

          if (string.IsNullOrEmpty(inputLongitude))
          {
            Console.Write("Longitude: ");
            inputLongitude = Console.ReadLine();
          }

          if (string.IsNullOrEmpty(inputMaxRecords))
          {
            Console.Write("MaxRecords: ");
            inputMaxRecords = Console.ReadLine();
          }
        }

        Dictionary<string, string> inputs = new Dictionary<string, string>()
        {
            { "format", "json"},
            { "lat", inputLatitude},
            { "long", inputLongitude},
            { "recs", inputMaxRecords}
        };

        Console.WriteLine("\n===================================== INPUTS ===================================\n");
        Console.WriteLine($"\t   Base Service Url: {baseServiceUrl}");
        Console.WriteLine($"\t  Service End Point: {serviceEndPoint}");
        Console.WriteLine($"\t           Latitude: {inputLatitude}");
        Console.WriteLine($"\t          Longitude: {inputLongitude}");
        Console.WriteLine($"\t         MaxRecords: {inputMaxRecords}");

        // Create Service Call
        // Set the License String in the Request
        string RESTRequest = "";

        RESTRequest += @"&id=" + Uri.EscapeDataString(license);

        // Set the Input Parameters
        foreach (KeyValuePair<string, string> kvp in inputs)
          RESTRequest += @"&" + kvp.Key + "=" + Uri.EscapeDataString(kvp.Value);

        // Build the final REST String Query
        RESTRequest = serviceEndPoint + @"?" + RESTRequest;

        // Submit to the Web Service. 
        bool success = false;
        int retryCounter = 0;

        do
        {
          try //retry just in case of network failure
          {
            GetContents(baseServiceUrl, $"{RESTRequest}").Wait();
            Console.WriteLine();
            success = true;
          }
          catch (Exception ex)
          {
            retryCounter++;
            Console.WriteLine(ex.ToString());
            return;
          }
        } while ((success != true) && (retryCounter < 5));

        bool isValid = false;
        if (!string.IsNullOrEmpty(latitude + longitude + maxRecords))
        {
          isValid = true;
          shouldContinueRunning = false;
        }

        while (!isValid)
        {
          Console.WriteLine("\nTest another record? (Y/N)");
          string testAnotherResponse = Console.ReadLine();

          if (!string.IsNullOrEmpty(testAnotherResponse))
          {
            testAnotherResponse = testAnotherResponse.ToLower();
            if (testAnotherResponse == "y")
            {
              isValid = true;
            }
            else if (testAnotherResponse == "n")
            {
              isValid = true;
              shouldContinueRunning = false;
            }
            else
            {
              Console.Write("Invalid Response, please respond 'Y' or 'N'");
            }
          }
        }
      }
      
      Console.WriteLine("\n===================== THANK YOU FOR USING MELISSA CLOUD API ====================\n");
    }
  }
}