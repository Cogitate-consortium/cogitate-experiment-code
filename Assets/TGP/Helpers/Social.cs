// NS_ABSORB
using Helpers.Engine;

using UnityEngine;

namespace TGP
{
    namespace Helpers
    {
        public static class Social_Helper
        {
            static float timeLast_Call_Facebook = -Mathf.Infinity;
            static float timeLast_Call_Twitter = -Mathf.Infinity;
            static float timeLast_Call_GooglePlus = -Mathf.Infinity;
            // We attempt to start the app -> If nothing happens the user will probably click on it again
            // This time we want to try the URL instead. This also works well if the app does launch
            // but with problems (directs to store / doesn't find your product). While the user
            // is not in your app (given it doesn't run on background) time doesn't pass, so he'll also be
            // redirected to the url.
            static readonly float minDurationForAppAttempts = 3f;

            public static void GoToFacebook()
            {
                string facebookAddress = "http://www.facebook.com/TallGuyProds";
                string facebookApp = "fb://facewebmodal/f?href=" + facebookAddress;

                if (TimeWrapper.time_NotTS - timeLast_Call_Facebook > minDurationForAppAttempts)
                    Application.OpenURL(facebookApp);
                else
                    Application.OpenURL(facebookAddress);

                timeLast_Call_Facebook = TimeWrapper.time_NotTS;
            }

            public static void GoToTwitter()
            {
                string profileID = "706546285302644737";
                string profileName = "TallGuyProds";


                string twitterApp = "twitter://user?user_id=" + profileID;
                string twitterAddress = "http://twitter.com/" + profileName;


                if (TimeWrapper.time_NotTS - timeLast_Call_Twitter > minDurationForAppAttempts)
                    Application.OpenURL(twitterApp);
                else
                    Application.OpenURL(twitterAddress);

                timeLast_Call_Twitter = TimeWrapper.time_NotTS;
            }

            public static void GoToGooglePlus()
            {
                string profileID = "u/0/communities/111734634818768957747";

                // User ID
                //  "117004778634926368759";
                // App link - though it's handled by the browser too
                // "gplus://plus.google.com/"
                string googlePlusApp = "http://plus.google.com/" + profileID;
                string googlePlusAddress = "http://plus.google.com/" + profileID;

                if (TimeWrapper.time_NotTS - timeLast_Call_GooglePlus > minDurationForAppAttempts)
                    Application.OpenURL(googlePlusApp);
                else
                    Application.OpenURL(googlePlusAddress);

                timeLast_Call_GooglePlus = TimeWrapper.time_NotTS;
            }

            public static void GoToTallGuyProductions()
            {
                string webpage = "http://tall-guy-productions.com/";
                Application.OpenURL(webpage);
            }
        }
    }
}
