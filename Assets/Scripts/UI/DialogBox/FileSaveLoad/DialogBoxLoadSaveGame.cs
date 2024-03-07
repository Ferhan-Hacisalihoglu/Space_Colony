using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Xml.Serialization;
using System.IO;
using System.Linq;

// Object -> MonoBehavior -> DialogBox -> DialogBoxLoadSaveGame ->
//                                                                 DialogBoxSaveGame
//                                                                 DialogBoxLoadGame
//

namespace MyNamespace
{
   public class DialogBoxLoadSaveGame : DialogBox 
   {

        public GameObject fileListItemPrefab;
        public Transform fileList;

        public override void ShowDialog()
        {
            base.ShowDialog();

            string directoryPath = WorldController.Instance.FileSaveBasePath();

            DirectoryInfo saveDir = new DirectoryInfo( directoryPath );

            FileInfo[] saveGames = saveDir.GetFiles().OrderByDescending( f => f.CreationTime ).ToArray();

            InputField inputField = gameObject.GetComponentInChildren<InputField>();

            foreach(FileInfo file in saveGames) 
            {
                GameObject go = (GameObject)Instantiate(fileListItemPrefab);

                go.transform.SetParent( fileList );

                go.GetComponentInChildren<Text>().text = Path.GetFileNameWithoutExtension( file.FullName );

                go.GetComponent<DialogListItem>().inputField = inputField;
            }
        }

        public override void CloseDialog() 
        {
            while(fileList.childCount > 0) {
                Transform c = fileList.GetChild(0);
                c.SetParent(null);	
                Destroy(c.gameObject);
            }

            base.CloseDialog();
        }

    }
}
