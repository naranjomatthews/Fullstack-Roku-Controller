
using Crestron.SimplSharp;//For Basic SIMPL# Classes like threading, errorlog, and basic system functions
using Crestron.SimplSharp.CrestronLogger;
using Crestron.SimplSharp.Net;
using Crestron.SimplSharpPro; // Inclues the main classes forr working with hardware, such as ControlSystem, XpanelForHtml5, CrestronOne, and Tsw770
using Crestron.SimplSharpPro.DeviceSupport;//contains classes for device support
using Crestron.SimplSharpPro.UI;//adds suppport for UI devices like XpanelForHtml5, CrestronOne, and Tsw770
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;//for async tasks, needed for async and await keywords

namespace RokuUI
{

    public class UiLogic
    {
        static public XpanelForHtml5 localUiEmulator; //used for browser based emulation
        static public CrestronOne localUiIpad; //used for the Ipad
        static Tsw770 localUiTsw770; //used for a physical Crestron display

        ControlSystem _cs; //stores reference to main control system
        private static ControlRoku _rokuController = new ControlRoku(); // Instance of Roku controller

        bool[] buttonState; //array of boolean values that tracking toggle buttons

        string imageUrl;

        string input; //users input into the IP textbox, this is constantly changing as they type
        string finalIP; //stores the user inputted IP address as soon as they press the enter button

        string tubiName;
        string youtubeName;
        string fishName;

        string tubiID = "41468";
        string youtubeID = "837";
        string fishID = "758888";

        bool IPValid = false; //stores the value of whether or not the IP is valid, false by default

        private static LinkedList<string> _logTable = new LinkedList<string>();


        public UiLogic() //default constructor
        {

        }

        public UiLogic(ControlSystem cs) : this()
        { //constructor that takes one parameter
            _cs = cs; //updates the reference to the main control system
            buttonState = new bool[] { false, false }; //boolean array with 2 default states of false.
        }


        public void RegisterUserInterface()
        {
            localUiEmulator = new XpanelForHtml5(0x05, _cs);
            //RegisterJoinsForCH5UI(localUiEmulator);
            localUiEmulator.SigChange += UiSigChange;
            localUiEmulator.OnlineStatusChange += OnlineSigChange;
            localUiEmulator.Register();

            localUiIpad = new CrestronOne(0x03, _cs);
            //RegisterJoinsForCH5UI(localUiIpad);
            localUiIpad.ParameterProjectName.Value = "ch5-vanilla-js-template";
            localUiIpad.SigChange += UiSigChange;
            localUiIpad.OnlineStatusChange += OnlineSigChange;
            localUiIpad.Register();
            

            // Await icon updates so you know when they're done
            /*await UpdateButtonIconAsync(localUiIpad, youtubeID, 5);
            await UpdateButtonIconAsync(localUiIpad, tubiID, 4);
            await UpdateButtonIconAsync(localUiEmulator, youtubeID, 5);
            await UpdateButtonIconAsync(localUiEmulator, tubiID, 4);*/
        }

        private void OnlineSigChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            
            WriteLog($"{currentDevice.Name} with ID: {currentDevice.ID:x} status = {(args.DeviceOnLine ? "on": "off")}");
        }

        private void UiSigChange(BasicTriList device, SigEventArgs args)
        {
            if(args.Sig.Type == eSigType.Bool)
            {
                //corresponds to BooleanOutput join that the UI pressed
                if (args.Sig.BoolValue)
                {
                    //checks if the signal is BooleanInput (construct) or BooleanOutput (CH5)
                    if (device.BooleanInput.Contains(args.Sig) || device.BooleanOutput.Contains(args.Sig))
                    {


                        switch (args.Sig.Number) //what button number was pressed
                        {
                            case 16: 
                                _ = TriggerRokuAsync(_rokuController, "Rev");
                                break;
                            case 17: 
                                _ = TriggerRokuAsync(_rokuController, "Play");
                                break;
                            case 18: 
                                _ = TriggerRokuAsync(_rokuController, "Fwd");
                                break;
                            case 19:
                                _ = TriggerRokuAsync(_rokuController, "Info");
                                break;
                            case 20: 
                                _ = TriggerRokuAsync(_rokuController, "InstantReplay");
                                break;
                            case 21: //dpad back press
                                _ = TriggerRokuAsync(_rokuController, "Back");
                                break;

                            case 22: //dpad home press
                                _ = TriggerRokuAsync(_rokuController, "Home");
                                break;

                            case 23: //dpad up press
                                _ = TriggerRokuAsync(_rokuController, "Up");
                                break;

                            case 24: //dpad down press
                                _ = TriggerRokuAsync(_rokuController, "Down");
                                break;
                            case 25://dpad left press
                                _ = TriggerRokuAsync(_rokuController, "Left");
                                break;

                            case 26: //dpad right press
                                _ = TriggerRokuAsync(_rokuController, "Right");
                                break;

                            case 27: //select press
                                _ = TriggerRokuAsync(_rokuController, "Select");
                                break;
                            case 28: //tubi shortcut press
                                _ = TriggerRokuLaunchAsync(_rokuController, tubiID);
                                break;
                            case 29: //youtube shortcut press
                                _ = TriggerRokuLaunchAsync(_rokuController, youtubeID);
                                break;
                            case 30: //fish shortcut press
                                _ = TriggerRokuLaunchAsync(_rokuController, fishID);
                                break;
                            case 31: //enter press
                                finalIP = CleanInput(input); //updates the IP to what the user enters

                                WriteLog($"Attempting to connect to the roku device with IP: {finalIP}");
                                //updates IPValid to true or false depending on if the IP is valid
                                WriteLog($"IP valid before validation: {IPValid}");
                                ValidateIP(_rokuController, finalIP).GetAwaiter().GetResult();
                                WriteLog($"IP valid after validation: {IPValid}");
                                if (IPValid)
                                {
                                    localUiIpad.StringInput[2].StringValue = finalIP; //join number 2 just shows what is being typed
                                    localUiIpad.StringInput[1].StringValue = finalIP; //join number 1 what is actually being typed
                                    localUiEmulator.StringInput[2].StringValue = finalIP; //join number 2 just shows what is being typed
                                    localUiEmulator.StringInput[1].StringValue = finalIP; //join number 1 what is actually being typed

                                    WriteLog("You are connected to the Roku device");
                                    string tubiImageUrl = $"http://{finalIP}:8060/query/icon/{tubiID}";
                                    string youtubeImageUrl = $"http://{finalIP}:8060/query/icon/{youtubeID}";
                                    string fishImageUrl = $"http://{finalIP}:8060/query/icon/{fishID}";
                                    TriggerRokuAppNameRetrieve(_rokuController).GetAwaiter().GetResult();
                                    try
                                    {
                                        //sets 
                                        localUiIpad.StringInput[4].StringValue = tubiImageUrl;
                                        localUiEmulator.StringInput[4].StringValue = tubiImageUrl;
                                        localUiIpad.StringInput[40].StringValue = tubiName;
                                        localUiEmulator.StringInput[40].StringValue = tubiName;
                                        localUiIpad.StringInput[5].StringValue = youtubeImageUrl;
                                        localUiEmulator.StringInput[5].StringValue = youtubeImageUrl;
                                        localUiIpad.StringInput[41].StringValue = youtubeName;
                                        localUiEmulator.StringInput[41].StringValue = youtubeName;
                                        localUiIpad.StringInput[6].StringValue = fishImageUrl;
                                        localUiEmulator.StringInput[6].StringValue = fishImageUrl;
                                        localUiIpad.StringInput[42].StringValue = fishName;
                                        localUiEmulator.StringInput[42].StringValue = fishName;
                                        WriteLog("Shortcut icons have been successfully set up");

                                    }
                                    catch
                                    {
                                        //if a name or image URL is not set up correctly, this will be thrown
                                        WriteLog("Could not set up the shortcut icons correctly");
                                    }
                                }
                                else
                                {
                                    //IP is not valid, output error message to log
                                    WriteLog($"could not connect to Roku device with IP Address: {finalIP}"); //invalid ip address
                                    finalIP = string.Empty; //reset the finalIP to empty so that it can be used again
                                }
                                break;

                            default:
                                break;
                        }//end of switch statement for button presses
                    }
                }
            }
            else if (args.Sig.Type == eSigType.String)
            {
                {
                    switch (args.Sig.Number) //handles text boxes, when clicked
                    {
                        case 1: // Serial Join 1, the IP address text box
                            input = args.Sig.StringValue;

                            //localUiIpad.StringInput[2].StringValue = input; //join number 2 just shows what is being typed
                            localUiIpad.StringInput[1].StringValue = input; //join number 1 what is actually being typed
                            //localUiEmulator.StringInput[2].StringValue = input; //join number 2 just shows what is being typed
                            localUiEmulator.StringInput[1].StringValue = input; //join number 1 what is actually being typed

                            break;

                        case 3: // Serial Join 3, the language display textbox gets updated (meaning the language changed)
                                //if the display textbox gets updated to spanish (happens through publish event happens in Theme.js), update isSpanish variable
                            
                            var detectedLanguage = args.Sig.StringValue;
                            
                            if (detectedLanguage == "Spanish")
                            {
                                detectedLanguage = "Español"; //update the language to spanish
                            }
                            localUiIpad.StringInput[3].StringValue = detectedLanguage;
                            localUiEmulator.StringInput[3].StringValue = detectedLanguage;
                            //localUiEmulator.StringInput[3].StringValue = $"language set to: {detectedLanguage}";
                            //localUiIpad.StringInput[3].StringValue = $"language set to: {detectedLanguage}";

                            break;
                        
                        default:
                            break;
                    }//end of switch statement for text boxes
                }
            }
        }

        //outputs log to textbox with join code: 8
        public static void WriteLog(string message)
        {
            //formats what goes into the log table to include the time and message
            string time = DateTime.Now.ToString("h:mm:ss:tt");
            string log = $"{time} => {message}";

            //adds new log message to the table
            _logTable.AddFirst(log);
            localUiIpad.StringInput[8].StringValue = log;
            localUiEmulator.StringInput[8].StringValue = log;

            if (_logTable.Count > 1) //if there are items already in the log, update it
            {
                UpdateLogTable(_logTable.First.Next, 1); //start update at the next log entry (log entry that used to be the first)
            }
        }

        public static void UpdateLogTable(LinkedListNode<string> head, uint distanceFromTop)//log will be the earliest log message, number of shifts starts at 1 
        {
            if(head == null) //head being null is our base case since it means we have updated the entire log and are now at an empty part of the table 
            {
                return; //end the recursion
            }
            else //the logtable is not completely full, recursion case
            {
                if (_logTable.Count > 10) //if the table is at its max capacity, remove the last (oldest) entry
                {
                    _logTable.RemoveLast();
                }
                //update the next log slot with the current log message
                localUiIpad.StringInput[8 + distanceFromTop].StringValue = head.Value;
                localUiEmulator.StringInput[8 + distanceFromTop].StringValue = head.Value;
                //call recursively with the next log string becoming the new head, and increment the distance from the most recent log entry.
                UpdateLogTable(head.Next, distanceFromTop + 1);
            }

            //Mickey Notes: look into list
            //list would make us do a 1-d for/while loop to update each value of log table (O(n)) =~ 10 
            //This linked-list does use a recursive function, but it uses short recursion depth - always ends after 10 calls (MAX), thus O(n=10) =~ 10 (I THINK).

        }

        public static string CleanInput(string input)
        {
            return input?.Trim().Replace("\0", "").Replace("\n", "").Replace("\r", "");
        }

        public async Task ValidateIP(ControlRoku control, string IPInput)
        {
            IPValid = await control.IsValidIP(IPInput);
        }

        private async Task TriggerRokuAsync(ControlRoku control, string command)
        {
            await control.KeyPress(command);
        }

        private async Task TriggerRokuLaunchAsync(ControlRoku control, string appID)
        {
            await control.Launch(appID);
        }

        private async Task TriggerRokuImageAsync(ControlRoku control, string appID)
        {
            imageUrl = await control.GetImageURL(appID);
        }

        private async Task TriggerRokuAppNameRetrieve(ControlRoku control)
        {
            Dictionary<string, string> appDictionary = await control.GetAppDictionary();
            tubiName  = control.GetAppName(appDictionary, tubiID);
            youtubeName = control.GetAppName(appDictionary, youtubeID); 
            fishName = control.GetAppName(appDictionary, fishID);
            WriteLog($"App names: {tubiName}, {youtubeName}, {fishName}");

        }

        
    }
}