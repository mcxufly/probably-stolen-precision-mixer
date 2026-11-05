using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ProbablyStolenMixer
{
    // Hosts the game's native TMP dropdown in a pixel-based machinery window.
    internal sealed class ComponentDropdown : PixelElement
    {
        private readonly TMP_Dropdown dropdown;
        private readonly RectTransform dropdownRect;
        private readonly List<string> labels = new List<string>();
        public bool canResize => false;
        public int widthPixels { get; private set; } = 180;
        public int heightPixels => 18;
        public TreeNodeRender handler { get; }
        public RectTransform rectTransform => handler.rectTransform;
        public TreeNodeRender inner => handler;

        public ComponentDropdown(Action<int> onChanged)
        {
            handler = TreeNodeRender.Instantiate(this, "Mixer component dropdown");
            handler.image.enabled = false;
            var prefab = CustomUIManager.Instance.GetPrefab(CustomUIManager.ElementKind.Dropdown, false);
            var control = UnityEngine.Object.Instantiate(prefab, rectTransform);
            // Options belong to the mixer, not any fixed localization keys on the prefab.
            foreach (var localizer in control.GetComponentsInChildren<DropdownLocalizer>(true))
                localizer.enabled = false;
            dropdown = control.GetComponentInChildren<TMP_Dropdown>(true);
            dropdownRect = (RectTransform)control.transform;
            dropdown.onValueChanged = new TMP_Dropdown.DropdownEvent();
            dropdown.onValueChanged.AddListener(index => onChanged(index));
            dropdownRect.anchorMin = dropdownRect.anchorMax = new Vector2(0, 1);
            dropdownRect.pivot = new Vector2(0, 1);
            dropdownRect.anchoredPosition = Vector2.zero;
            // Native CustomUI controls render at twice the machinery pixel scale.
            dropdownRect.localScale = new Vector3(0.5f, 0.5f, 1f);
            Validate();
        }

        public void SetOptions(List<string> options, int selected)
        {
            bool changed = labels.Count != options.Count;
            for (int i = 0; !changed && i < options.Count; i++) changed = labels[i] != options[i];
            if (changed)
            {
                dropdown.Hide();
                labels.Clear();
                labels.AddRange(options);
                dropdown.ClearOptions();
                dropdown.AddOptions(options);
            }
            dropdown.SetValueWithoutNotify(selected);
            dropdown.RefreshShownValue();
        }

        public void Validate()
        {
            rectTransform.sizeDelta = new Vector2(widthPixels, heightPixels);
            dropdownRect.sizeDelta = new Vector2(widthPixels * 2, heightPixels * 2);
        }

        public void ResizePixels(int width, int height) { widthPixels = Math.Max(180, width); Validate(); }
        public bool Attach(PixelElement element) => false;
        public bool Detach(PixelElement element) => false;
        public int WidthPixelsClamp(int width) => widthPixels;
        public int HeightPixelsClamp(int height) => heightPixels;
        public int XClamp(int x) => 0;
        public int YClamp(int y) => 0;
    }
}
