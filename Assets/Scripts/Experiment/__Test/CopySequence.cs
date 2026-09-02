using Peripherals.Logging.Core;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TGP.Helpers.Test
{
    public class CopySequence : MonoBehaviour
    {
        private List<string> namesToCopy = new List<string>()
        {
"3294",
"4980",
"4652",
"4882",
"0394",
"3462",
"1338",
"4224",
"3122",
"0230",
"1186",
"3742",
"4262",
"3202",
"2694",
"0426",
"0800",
"1964",
"2700",
"1120",
"4298",
"1256",
"1164",
"2126",
"1692",
"1220",
"0142",
"2430",
"4156",
"3484",
"3076",
"0816",
"4776",
"4870",
"2946",
"2501",
"0479",
"4675",
"1493",
"4003",
"0049",
"1405",

        };

        private void Awake()
        {
            string originalPath = @"C:\Users\mrkon\Downloads\[201113] 5000xFMRI\Queues\";
            string targetPath = Path.Combine(originalPath, @"_SelectedMixed\");

            Debug.Log("Begining copy from {0} of\n{1}"._Format(originalPath, namesToCopy.ToReadableString()));

            int copied = 0;
            foreach (string s in FileWrapper.GetFolderContents(originalPath, FilesFolders.Folders, false))
                if (namesToCopy.Contains(s))
                {
                    copied++;
                    FileWrapper.CopyFolder(Path.Combine(originalPath, s), Path.Combine(targetPath, s), true);
                }

            if (copied == namesToCopy.Count)
                Debug.Log("Copied over all the {0} that were requested"._Format(copied));
            else if (copied < namesToCopy.Count)
                Debug.LogWarning("Copied over less folders ({0}) than requested ({1}); check that all the requested exist"._Format(copied, namesToCopy.Count));
            else
                Debug.LogError("Copied over more folders ({0}) than requested ({1}) - Should never happen"._Format(copied, namesToCopy.Count));

        }
    }
}