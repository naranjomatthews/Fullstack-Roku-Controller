using Crestron.SimplSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using static System.Collections.Specialized.BitVector32;

namespace RokuUI
{
    public class ControlRoku
    {
        //"172.21.1.168"; //IP address of Mickey's Roku Device
        //"172.30.4.140" JJs roku address
        private static readonly HttpClient _client = new HttpClient(); //creates http client used to send web requests.

        public string Roku_IP;


        /// <summary>
        /// isValidIP uses Regular Expression (Regex) to check if the IP string being passed is of valid IP adddress format
        /// a number from 255-250 or 249-200 or 199-100 or 99-0, ending with a '.' seperator, is accepted 3 times.
        /// the same is accepted for the last part of the address, minus the '.' seperator
        /// Thus IPs from 255.255.255.255 to 0.0.0.0 are accepted
        /// 
        /// We also create an instance of HttpClient to send a basic request to the Roku device
        /// if the request is successful, we return true, indicating that the IP is valid and reachable.
        /// </summary>
        public async Task<bool> IsValidIP(string ip)
        {
            string strRegex = @"^((25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)\.){3}(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)$";
            Regex re = new Regex(strRegex);
            //if the IP does not match the regex, return false
            if (re.IsMatch(ip) != true)
            {
                UiLogic.WriteLog($"could not set new IP Address of: {ip}   Check if format is valid");
                Roku_IP = string.Empty; //clears the IP address if the format is not valid
                return false; //this avoids creating a client and making an API call if the IP is not valid
            }
            else
            {
                try
                {
                    // Optional: set a timeout
                    using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3)))
                    {
                        HttpResponseMessage response = await _client.GetAsync(
                            $"http://{ip}:8060/query/device-info",
                            cts.Token
                        );

                        if (response.IsSuccessStatusCode)
                        {
                            UiLogic.WriteLog($"Roku reachable at {ip}");
                            Roku_IP = ip;
                            return true;
                        }
                        else
                        {
                            UiLogic.WriteLog(
                                $"Roku unavailable, device at {ip} responded with status {response.StatusCode}."
                            );
                            Roku_IP = string.Empty;
                            return false;
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    UiLogic.WriteLog($"Timeout when trying to reach {ip}");
                    Roku_IP = string.Empty;
                    return false;
                }
                catch (HttpRequestException ex)
                {
                    UiLogic.WriteLog($"Network error when reaching {ip}: {ex.Message}");
                    Roku_IP = string.Empty;
                    return false;
                }
                catch (Exception ex)
                {
                    UiLogic.WriteLog($"Unexpected error when validating IP {ip}: {ex.Message}");
                    Roku_IP = string.Empty;
                    return false;
                }
            }
        }

        //async allows method to run in the background, useful for calls to another webservice or sending requests
        //we return type Task or Task<T> depending on if there is return value as a result of the async method executing.
        //there is no need for Keypress to return a boolean since the command will never fail since there is no way for there to be an invalid input/button press (every button will be configured)
        public async Task KeyPress(string command) //when remote key is pressed, perform the key action (up, down, left, etc).
        {
            //VALID COMMANDS: Home, Back, Up, Down, Left, Right, Select, Rev, Play, Fwd, Info, InstantReplay
            //'$" symbol used for string interpolation in C#
            string url = $"http://{Roku_IP}:8060/keypress/{command}";//Base URL for Roku commands, Roku's API listens on port 8060 using HTTP requests

            UiLogic.WriteLog($"the url is: {url}");

            // Send the actual command
            HttpResponseMessage response = await _client.PostAsync(url, null);  //null because no extra data in the body is expected/required
            if (response.IsSuccessStatusCode) //if only the response is a success, but not the int cast, then we are dealing with a key press command
            {
                UiLogic.WriteLog($"Sucessfully sent Roku command: {command}"); //logs that the key press command was successful
            }
            else
            {
                UiLogic.WriteLog($"Error sending Roku command: '{command}'. Status: {response.StatusCode}"); //logs a warning
            }
        }

        public async Task Launch(string ID) //when shortcut is pressed it will launch the app id (netflix, tubi)
        {
            //VALID COMMANDS: [(tubi, 41468), (youtube,837)]
            //'$" symbol used for string interpolation in C#           
            string launchUrl = $"http://{Roku_IP}:8060/launch/{ID}"; //Base URL for Roku commands, Roku's API listens on port 8060 using HTTP requests
            UiLogic.WriteLog($"the launch url is {launchUrl}");

            // Send the actual command
            HttpResponseMessage response = await _client.PostAsync(launchUrl, null);//null because no extra data in the body is expected/required
            if (response.IsSuccessStatusCode)
            {
                UiLogic.WriteLog($"Sucessfully launched channel with ID: {ID}"); //logs that the launch action was successful
            }
            else
            {
                UiLogic.WriteLog($"Error launching channel with ID: '{ID}'. Status: {response.StatusCode}"); //logs a warning
            }
        }

        public async Task<string> GetImageURL(string ID)
        {
            string imageUrl = $"http://{Roku_IP}:8060/query/icon/{ID}";
            HttpResponseMessage response = await _client.GetAsync(imageUrl);

            if (response.IsSuccessStatusCode)
            {
                UiLogic.WriteLog($"successfully got image url"); // Optional logging
                return imageUrl; // Send URL to panel
            }
            else
            {
                UiLogic.WriteLog("image url invalid");
                return null;
            }
        }

        public async Task<Dictionary<string, string>>GetAppDictionary() //async, return Dictionary
        {
            Dictionary<string, string> apps = new Dictionary<string, string>(); //creates new dictionary everytime function is called (when enter is pressed)
            string url = $"http://{Roku_IP}:8060/query/apps"; //stores the url used to fetch app query

            UiLogic.WriteLog($"attempting to make app dictionary");

            //HTTP get-request using an instance of HttpClient
            //HttpCompletionOption.ResponseHeadersRead tells HttpClient to return once the headers are available(useful when you plan to stream the content)
            //'using' ensures the HttpResponseMessage is properly disposed after use.
            using (HttpResponseMessage response = await _client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode(); //helps avoid silently failing requests

                //reads the response body as a stream asynchronously
                //here stream is used to parse XML data without loading the entire string in memory (better for memory and efficiency)
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    //creates settings to configure XMLReader & sets async to true so reader can be used asynchronously
                    XmlReaderSettings settings = new XmlReaderSettings { Async = true };
                    //creates an xml reader that parses the stream with the given settings, disposes the reader properly with 'using'
                    using (XmlReader reader = XmlReader.Create(stream, settings))
                    {
                        while (await reader.ReadAsync()) //reads the XML file node by node in async manner, continues until out of nodes
                        {
                            //checks if the current node is of type "app"
                            if (reader.NodeType == XmlNodeType.Element && reader.Name == "app")
                            {
                                string id = reader.GetAttribute("id"); //retrieves the ID attribute from the current <app> node
                                string name = await reader.ReadElementContentAsStringAsync(); //reads inner text of the <app> element asynchronously
                                apps.Add(id, name); //adds both to the app dictionary, with the ID as the key
                            }
                        }
                    }
                }
            }
            UiLogic.WriteLog($"getting the app dictionary");
            return apps; 
        }

        public string GetAppName(Dictionary<string, string> apps, string appID)
        {
            if (apps.TryGetValue(appID, out string appName))
            {
                UiLogic.WriteLog($"found app name"); // Optional logging
                return appName; // returns the app name based on the ID
            }
            else
            {
                UiLogic.WriteLog("could not find the app name");
                return null;
            }

        }

    }
}