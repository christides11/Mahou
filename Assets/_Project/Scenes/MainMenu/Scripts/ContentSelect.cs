using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Mahou.Managers;
using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;
using Mahou.Helpers;
using Mahou.Content;

namespace Mahou.Menus
{
    public class ContentSelect : MonoBehaviour
    {
        public delegate void EmptyAction();
        public delegate void ContentAction(Content.ModObjectReference gamemodeReference);
        public event EmptyAction OnMenuClosed;
        public event ContentAction OnContentSelected;

        public GameObject gamemodeContentPrefab;
        public Transform gamemodeContentHolder;

        public TextMeshProUGUI gamemodeName;

        private Content.ModObjectReference currentSelectedGamemode;

        private ContentType contentType;

        public void CloseMenu()
        {
            gameObject.SetActive(false);
            OnMenuClosed?.Invoke();
        }

        public async UniTask OpenMenu(ContentType contentType)
        {
            this.contentType = contentType;
            await FillContentHolder();
            gameObject.SetActive(true);
        }

        private async UniTask FillContentHolder()
        {
            foreach(Transform child in gamemodeContentHolder)
            {
                Destroy(child.gameObject);
            }

            bool loadResult = await ContentManager.instance.LoadContentDefinitions(contentType);

            List<Content.ModObjectReference> contentReferences = ContentManager.instance.GetContentDefinitionReferences(contentType);

            foreach (Content.ModObjectReference contentRef in contentReferences)
            {
                Content.ModObjectReference cref = contentRef;
                GameObject gamemodeContentObject = GameObject.Instantiate(gamemodeContentPrefab, gamemodeContentHolder, false);
                gamemodeContentObject.GetComponentInChildren<TextMeshProUGUI>().text = cref.ToString();
                gamemodeContentObject.GetComponent<EventTrigger>().AddOnSubmitListeners((data) => { SelectContent(cref); });
            }

            if (contentReferences.Count > 0)
            {
                SelectContent(contentReferences[0]);
            }
        }

        private void SelectContent(Content.ModObjectReference contentReference)
        {
            currentSelectedGamemode = contentReference;
            gamemodeName.text = contentReference.ToString();
        }

        public void SelectContent()
        {
            OnContentSelected?.Invoke(currentSelectedGamemode);
        }
    }
}