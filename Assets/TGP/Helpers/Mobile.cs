using UnityEngine;

namespace TGP
{
    namespace Helpers
    {
        public static class Mobile_Helper
        {
            public static void Vibrate(int v)
            {
#if UNITY_ANDROID
        VibrationManager.Vibrate(v);
#elif UNITY_IOS
        Handheld.Vibrate();
#elif UNITY_WINRT_8_1
        TGP_UnityPlugins.VibrationManager.Vibrate(v);
#endif
            }

            public static int ANDROID_SDK_VERSION
            {
                get
                {
                    int version = -1;
#if UNITY_ANDROID && !UNITY_EDITOR
            using (var buildVersion = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                version = buildVersion.GetStatic<int>("SDK_INT");
            }
#elif UNITY_ANDROID && UNITY_EDITOR
            version = 9000;
#endif
                    return version;
                }
            }

            private static AndroidJavaObject Android_getPackageManager()
            {
                AndroidJavaObject pM = null;

#if UNITY_ANDROID
        if (!Application.isEditor)
        {
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");
            // int flag = new AndroidJavaClass("android.content.pm.PackageManager").GetStatic<int>("GET_META_DATA");
            pM = currentActivity.Call<AndroidJavaObject>("getPackageManager");
        }
#endif
                return pM;
            }

            /// <summary>
            /// Where did this app come from? (Market)
            /// </summary>
            public static AndroidMarketPlace Android_getMarketPlace()
            {
                // Setting installer name when developing
                /*
                http://pixplicity.com/setting-install-vendor-debug-app/
                adb shell pm uninstall com.tallguyproductions.agameofcoins
                adb push C:/app.apk /sdcard/app.apk
                adb shell pm install -i "com.android" -r /sdcard/app.apk
                adb shell rm /sdcard/app.apk

                */

                AndroidMarketPlace source = AndroidMarketPlace.Unknown;
                string installerPackageName = "";

#if UNITY_ANDROID
        if (!Application.isEditor)
        {
            string packageName = Application.bundleIdentifier;

            // Application.bundleIdentifier
            AndroidJavaObject pm = Android_getPackageManager();
            installerPackageName = pm.Call<string>("getInstallerPackageName", packageName);
            if (installerPackageName == null)
                installerPackageName = "";
        }
#endif

                if (installerPackageName.CompareTo("") != 0)
                {
                    Debug_Helper.OnGUI_AddMessage(installerPackageName, 10);

                    string processedPackageName = installerPackageName.RemoveSpaces();

                    Debug_Helper.OnGUI_AddMessage(processedPackageName, 10);

                    // Differentiate based on name
                    // http://stackoverflow.com/questions/17629787/how-android-app-can-detect-what-store-installed-it

                    // Samsung contains android in the name
                    if (processedPackageName.ContainsInvariant("samsung"))
                        source = AndroidMarketPlace.SamsungStore;
                    else if (processedPackageName.ContainsInvariant("amazon"))
                        source = AndroidMarketPlace.Amazon;
                    else if (processedPackageName.ContainsInvariant("android"))
                        source = AndroidMarketPlace.GooglePlay;

                    Debug_Helper.OnGUI_AddMessage(source.ToString());
                }
                else
                    Debug_Helper.OnGUI_AddMessage("Couldn't get installer package name", 10);

                return source;
            }

            /// <summary>
            /// Opens the corresponding store, based on the device we're running on
            /// </summary>
            public static void OpenAppRateURL()
            {
                // If by the end of the function this doesn't have a value, we won't open an empty URL.
                string url = "";

                // Set to a default value for all systems which do not support rating
                string defaultURL = "http://tall-guy-productions.com";

                // Don't want any nagging for platform-dependent IDs
#pragma warning disable 0219

                // If your app isn't live yet and this is a debug build, 
                // dummy app details can be used (to test functionality)
                // This will link directly to existing apps in the stores
                bool useDummyDetails_DuringDebug_GooglePlay = true;
                bool useDummyDetails_DuringDebug_Amazon = true;
                bool useDummyDetails_DuringDebug_SamsungStore = true;
                bool useDummyDetails_DuringDebug_WindowsStore = true;
                bool useDummyDetails_DuringDebug_AppleAppStore = true;

                // -------------------- [SOS] --------------------
                // If these strings are not empty, a rating will be attempted!
                // If you are not sure what values to put there before releasing the first version
                // of your app, it's better to leave them empty!
                // -------------------- [SOS] --------------------

                // This is the package name 
                string android_AppID = Application.identifier;

                // App management -> App identity -> URL for Windows Phone 8.1 and earlier
                string windows_GUID = "86bc0cb0-bc5a-4c47-bbaa-38654f6950bd";

                // App management -> App identity -> URL for Windows 10
                string windows_AppID = "9nblggh5pckw";

                // App Identity -> Bottom
                string apple_AppID = "1090931634";

#pragma warning restore 0219

                // [NOTES]
                // windowsGUID only appears after submitting a package and selecting for availability:
                // Only available to specified people on Windows Phone 8.x devices, 
                // or people with a promotional code on Windows 10 devices. 

                // Google Play Dev Portal
                // https://play.google.com/apps/publish

                // Amazon Dev Portal
                // https://developer.amazon.com/home.html

                // Samsung Dev Portal
                // http://seller.samsungapps.com/content/common/summaryContentList.as

                // Windows Store Dev Portal:
                // https://dev.windows.com/en-us/dashboard/apps/ 

                // Apple Dev Portal
                // https://itunesconnect.apple.com/WebObjects/iTunesConnect.woa/ra/ng/app

#if UNITY_ANDROID
        AndroidMarketPlace market = Android_getMarketPlace();

        // Use this to simulate behavior
        if (Debug.isDebugBuild)
            market = AndroidMarketPlace.GooglePlay;        
        
        if (Debug.isDebugBuild)
        {
            // [GOOGLE PLAY] 
            // Use maps as dummy 
            // https://play.google.com/store/apps/details?id=com.google.android.apps.maps
            if (market == AndroidMarketPlace.GooglePlay && useDummyDetails_DuringDebug_GooglePlay)
                android_AppID = "com.google.android.apps.maps";

            // [AMAZON]
            // Use angry birds as dummy (will someday people use our packages as dummies? :D)
            // http://www.amazon.com/gp/mas/dl/android?p=com.rovio.angrybirds
            else if (market == AndroidMarketPlace.Amazon && useDummyDetails_DuringDebug_Amazon)
                android_AppID = "com.rovio.angrybirds";

            // [SAMSUNG STORE]
            // Use Samsung app itself as reference
            // samsungapps://ProductDetail/com.sec.android.app.samsungapps
            else if (market == AndroidMarketPlace.SamsungStore && useDummyDetails_DuringDebug_SamsungStore)
                android_AppID = "com.sec.android.app.samsungapps";
        }

        if (market == AndroidMarketPlace.GooglePlay)
            url = "market://details?id=" + android_AppID;
        else if (market == AndroidMarketPlace.Amazon)
            url = "amzn://apps/android?p=" + android_AppID;
        else if (market == AndroidMarketPlace.SamsungStore)
            url = "samsungapps://ProductDetail/" + android_AppID;

        if (android_AppID.CompareTo("") == 0)
            url = "";        
        
#elif UNITY_WINRT_8_0 || UNITY_WINRT_8_1 || UNITY_WINRT_10
        // Windows Phone 7.x / 8.x & Windows 8.x
        if (SystemInfo.operatingSystem.Contains("Windows Phone 7") ||
            SystemInfo.operatingSystem.Contains("Windows Phone 8") ||
            SystemInfo.operatingSystem.Contains("Windows 8"))
        {

            // Use OneNote as dummy
            // https://msdn.microsoft.com/en-us/windows/uwp/launch-resume/launch-store-app
            if (Debug.isDebugBuild && useDummyDetails_DuringDebug_WindowsStore)
                windows_GUID = "ca05b3ab-f157-450c-8c49-a1f127f5e71d";

            if (SystemInfo.operatingSystem.Contains("Phone"))
                url = "ms-windows-store:reviewapp?appid=" + windows_GUID;
            else
                url = "ms-windows-store:review?appid=" + windows_GUID;

            if (windows_GUID.CompareTo("") == 0)
                url = "";
        }
        // Windows 10 / Windows Phone 10
        else if (SystemInfo.operatingSystem.Contains("Windows Phone 10") ||
                 SystemInfo.operatingSystem.Contains("Windows 10"))
        {
            // Use OneNote as dummy
            // https://msdn.microsoft.com/en-us/windows/uwp/launch-resume/launch-store-app
            if (Debug.isDebugBuild && useDummyDetails_DuringDebug_WindowsStore)
                windows_AppID = "9WZDNCRFHVJL";

            url = "ms-windows-store://review/?ProductId=" + windows_AppID;

            if (windows_AppID.CompareTo("") == 0)
                url = "";
        }

#elif UNITY_IOS

        // Use Skype as dummy
        // https://itunes.apple.com/us/app/skype-for-iphone/id304878510?mt=8
		if (Debug.isDebugBuild && useDummyDetails_DuringDebug_AppleAppStore)
            apple_AppID = "304878510";

        // Pre iOS 7.x
        if (SystemInfo.operatingSystem.Contains("6."))
            url = "itms-apps://itunes.apple.com/WebObjects/MZStore.woa/wa/"
                + "viewContentsUserReviews?type=Purple+Software&id=" + apple_AppID;
		// Post iOS 7.x (deep link support dropped, using http to reach review directly)
        else     
			url = "http://itunes.apple.com/WebObjects/MZStore.woa/wa/viewContentsUserReviews?id=" + apple_AppID 
				+ "&pageNumber=0&sortOrdering=2&type=Purple+Software&mt=8";
            //url = "itms-apps://itunes.apple.com/app/id" + apple_AppID;   
        
        if (apple_AppID.CompareTo("") == 0)
            url = "";
#endif

                // Nothing yet, go for default
                if (url.CompareTo("") == 0)
                    url = defaultURL;

                // Don't open empty URLs
                if (url.CompareTo("") != 0)
                    Application.OpenURL(url);
            }
        }

        public enum AndroidMarketPlace { Unknown = 0, GooglePlay, Amazon, SamsungStore }
    }
}
