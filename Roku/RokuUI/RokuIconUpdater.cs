/*using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.UI; // For BasicTriList, CrestronOne
using RokuUI;
using System;
using System.Net.Http;
using System.Threading.Tasks;

public class RokuIconUpdater

{

    private readonly BasicTriList _basicTriListUi;

    private readonly CrestronOne _crestronOneUi;

    private readonly HttpClient _client = new HttpClient();

    // Constructor for BasicTriList UI (e.g. XpanelForHtml5)

    public RokuIconUpdater(BasicTriList ui)

    {

        _basicTriListUi = ui;

    }

    // Constructor for CrestronOne UI

    public RokuIconUpdater(CrestronOne ui)

    {

        _crestronOneUi = ui;

    }

    public async Task UpdateButtonIconAsync(string appId, ushort serialJoin)

    {

        if (string.IsNullOrEmpty(ip))

        {

            UiLogic.WriteLog("Cannot update icon. Roku IP is null.");

            return;

        }

        string imageUrl = $"http://{ip}:8060/query/icon/{appId}";

        try

        {

            HttpResponseMessage response = await _client.GetAsync(imageUrl);

            if (response.IsSuccessStatusCode)

            {

                // Set StringInput on the correct UI type

                if (_basicTriListUi != null)

                {

                    _basicTriListUi.StringInput[serialJoin].StringValue = imageUrl;

                }

                else if (_crestronOneUi != null)

                {

                    _crestronOneUi.StringInput[serialJoin].StringValue = imageUrl;

                }

                UiLogic.WriteLog($"Updated icon for app {appId} on join {serialJoin}");

            }

            else

            {

                UiLogic.WriteLog($"Failed to get icon for {appId}. Status: {response.StatusCode}");

            }

        }

        catch (Exception ex)

        {

            UiLogic.WriteLog($"Exception while getting icon: {ex.Message}");

        }

    }

}*/

