using Newtonsoft.Json;

namespace ReverseGeoCoderDotnet
{
  /// <summary>
  /// Reverse GeoCoder takes a latitude/longitude coordinate and returns the nearest
  /// addresses to that point, up to a requested maximum number of records.
  ///
  /// <para>High-level flow of this sample:</para>
  /// <list type="number">
  ///   <item><description>ARGS    - ParseArguments reads any --flag values off the command line.</description></item>
  ///   <item><description>INPUT   - CallAPI fills in whatever wasn't supplied via interactive prompts.</description></item>
  ///   <item><description>REQUEST - CallAPI builds the REST query string (license + latitude, longitude, max records).</description></item>
  ///   <item><description>CALL    - GetContents issues the GET request and pretty-prints the JSON response.</description></item>
  /// </list>
  ///
  /// <para>This sample is a thin HTTP client: it builds a query string, sends a GET
  /// request to the Reverse GeoCoder Cloud API, and prints the JSON response.</para>
  ///
  /// <para>Reference:</para>
  /// <list type="bullet">
  ///   <item><description>Documentation: https://docs.melissa.com/cloud-api/reverse-geocoder/reverse-geocoder-index.html</description></item>
  ///   <item><description>Release notes: https://releasenotes.melissa.com/cloud-api/reverse-geocoder/</description></item>
  ///   <item><description>Result codes: https://docs.melissa.com/melissa/result-codes/result-codes-index.html</description></item>
  /// </list>
  /// </summary>
  static class Program
  {
    /// <summary>
    /// Entry point. Reads the optional command-line arguments, then hands control to
    /// CallAPI, which performs the actual request/response cycle.
    /// </summary>
    /// <param name="args">The raw command-line arguments.</param>
    static void Main(string[] args)
    {
      string baseServiceUrl = @"https://reversegeo.melissadata.net/";
      string serviceEndpoint = @"v3/web/ReverseGeoCode/doLookup"; //please see https://www.melissa.com/developer/reverse-geocoder for more endpoints
      string license = "";
      string latitude = "";
      string longitude = "";
      string maxRecords = "";

      // Populate any values passed on the command line, then run the lookup.
      ParseArguments(ref license, ref latitude, ref longitude, ref maxRecords, args);
      CallAPI(baseServiceUrl, serviceEndpoint, license, latitude, longitude, maxRecords);
    }

    /// <summary>
    /// Reads the supported command-line options and writes each recognized value into
    /// its matching by-ref parameter. Any parameter left unset here falls back to an
    /// interactive prompt later in <see cref="CallAPI"/>.
    ///
    /// <para>Recognized flags (each followed by its value, e.g. "--lat 33.637520"):
    /// --license/-l, --lat, --long, --max.</para>
    /// </summary>
    /// <param name="license">Receives the Melissa license string, if supplied.</param>
    /// <param name="latitude">Receives the latitude to look up, if supplied.</param>
    /// <param name="longitude">Receives the longitude to look up, if supplied.</param>
    /// <param name="maxRecords">Receives the maximum number of records to return, if supplied.</param>
    /// <param name="args">The raw command-line arguments to parse.</param>
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

    /// <summary>
    /// Issues the GET request against the Reverse GeoCoder endpoint and pretty-prints
    /// the API call and the JSON response to the console.
    /// </summary>
    /// <param name="baseServiceUrl">The Reverse GeoCoder Cloud API base URL.</param>
    /// <param name="requestQuery">The endpoint path plus query string built by <see cref="CallAPI"/>.</param>
    public static async Task GetContents(string baseServiceUrl, string requestQuery)
    {
      HttpClient client = new HttpClient();
      client.BaseAddress = new Uri(baseServiceUrl);
      HttpResponseMessage response = await client.GetAsync(requestQuery);

      string text = await response.Content.ReadAsStringAsync();

      // Re-serialize with indentation so the raw response is easier to read.
      var obj = JsonConvert.DeserializeObject(text);
      var prettyResponse = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);

      // Print output
      Console.WriteLine("\n==================================== OUTPUT ====================================\n");
      
      Console.WriteLine("API Call: ");
      string APICall = Path.Combine(baseServiceUrl, requestQuery);
      for (int i = 0; i < APICall.Length; i += 70)
      {
        if (i + 70 < APICall.Length)
        {
          Console.WriteLine(APICall.Substring(i, 70));
        }
        else
        {
          Console.WriteLine(APICall.Substring(i, APICall.Length - i));
        }
      }

      Console.WriteLine("\nAPI Response:");
      Console.WriteLine(prettyResponse);
    }
    
    /// <summary>
    /// Drives the interactive/CLI loop: gathers the latitude, longitude, and max records,
    /// builds and submits the REST query, prints the result, and optionally repeats for
    /// another record.
    ///
    /// <para>In interactive mode (no lookup args) it loops, asking for a new record each pass
    /// until the user answers "N". In one-shot mode (any of latitude, longitude, or max records
    /// supplied) it runs a single pass and exits.</para>
    /// </summary>
    /// <param name="baseServiceUrl">The Reverse GeoCoder Cloud API base URL.</param>
    /// <param name="serviceEndPoint">The specific Reverse GeoCoder endpoint path to call.</param>
    /// <param name="license">The Melissa license string sent with every request.</param>
    /// <param name="latitude">A latitude to look up in one-shot mode; if latitude, longitude, and max records are all empty, the program prompts interactively.</param>
    /// <param name="longitude">A longitude to look up in one-shot mode.</param>
    /// <param name="maxRecords">The maximum number of records to return in one-shot mode.</param>
    static void CallAPI(string baseServiceUrl, string serviceEndPoint, string license, string latitude, string longitude, string maxRecords)
    {
      Console.WriteLine("\n================= WELCOME TO MELISSA REVERSE GEOCODER CLOUD API ================\n");
      
      bool shouldContinueRunning = true;
      while (shouldContinueRunning)
      {
        string inputLatitude = "";
        string inputLongitude = "";
        string inputMaxRecords = "";

        // No values were supplied via command line, so prompt for every field.
        if (string.IsNullOrEmpty(latitude) && string.IsNullOrEmpty(longitude) && string.IsNullOrEmpty(maxRecords))
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
          // At least one field was supplied via command line; use those values as-is.
          inputLatitude = latitude;
          inputLongitude = longitude;
          inputMaxRecords = maxRecords;
        }

        // Prompt individually for any still-missing required field.
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

        // Map input fields to the API's expected query parameter names ("recs" is the
        // max number of records) and request a JSON response.
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

        // If latitude, longitude, or max records came from the command line, treat this
        // as a one-shot run rather than looping for additional records.
        bool isValid = false;
        if (!string.IsNullOrEmpty(latitude + longitude + maxRecords))
        {
          isValid = true;
          shouldContinueRunning = false;
        }

        // Otherwise ask whether to test another record. Keep prompting until we get a
        // valid Y/N. "N" ends the program; "Y" falls through to another pass.
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
