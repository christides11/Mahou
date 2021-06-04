using Mahou.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mahou.Content;
using Mahou.Helpers;
using UnityEngine.EventSystems;

namespace Mahou.Menus
{
    public class CharacterSelectMenu : MonoBehaviour
    {
        public delegate void SubmitAction(ModObjectReference fighter);
        public event SubmitAction OnCharacterSubmit;

        [SerializeField] private GameObject textContentItem;

        [SerializeField] private Transform modContentHolder;
        [SerializeField] private Transform characterContentHolder;

        public ModObjectReference selectedFighter;

        public virtual void CloseMenu()
        {
            gameObject.SetActive(false);
        }

        public virtual void OpenMenu()
        {
            FillModMenu();
            FillCharacterMenu("core");
        }

        public virtual void Submit()
        {
            OnCharacterSubmit?.Invoke(selectedFighter);
        }

        private void FillModMenu()
        {
            foreach(Transform child in modContentHolder)
            {
                GameObject.Destroy(child.gameObject);
            }

            ContentManager modManager = ContentManager.instance;

            foreach(var mod in ModLoader.instance.loadedMods)
            {
                GameObject go = GameObject.Instantiate(textContentItem, modContentHolder, false);
                go.GetComponent<TextMeshProUGUI>().text = mod.Key;
                string modIdentifier = mod.Key;
                go.GetComponent<EventTrigger>().AddOnSubmitListeners((data) => { FillCharacterMenu(modIdentifier); });
            }
        }

        private async void FillCharacterMenu(string modIdentifier)
        {
            foreach (Transform child in characterContentHolder)
            {
                GameObject.Destroy(child.gameObject);
            }

            ContentManager modManager = ContentManager.instance;
            await modManager.LoadContentDefinitions(ContentType.Fighter, modIdentifier);
            List<ModObjectReference> fighters = modManager.GetContentDefinitionReferences(ContentType.Fighter, modIdentifier);

            foreach(var fighter in fighters)
            {
                IFighterDefinition fd = (IFighterDefinition)modManager.GetContentDefinition(ContentType.Fighter, fighter);
                if(fd == null)
                {
                    Debug.Log($"No fighter for {fighter}");
                    continue;
                }
                GameObject go = GameObject.Instantiate(textContentItem, characterContentHolder, false);
                go.GetComponent<TextMeshProUGUI>().text = fd.Name;
                ModObjectReference f = fighter;
                go.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => { selectedFighter = f; });
            }
        }
    }
}